using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace Utils
{
	public interface IMessagingData { }

	public interface IMessage : IMessagingData { }

	public interface IRequest : IMessagingData { }

	public interface IHandler<TMessageData> where TMessageData : IMessagingData
	{
		void Handle(TMessageData data);
	}

	/// <summary>
	/// Messaging system by which otherwise disconnected systems can signal any other registered system 
	/// with data (<see cref="IMessage"/>), or request data from any system prepared to provide it <see cref="IRequest"/> 
	/// <para>
	/// For more tightly integrated systems, a more local event based system should be preferred (c# events and delegates)
	/// </para>
	/// </summary>
	public static class Messaging
	{
		public static bool IsInitialized { get; private set; }

		public enum Delay
		{
			Update,
			LateUpdate,
			FixedUpdate,
		}

		public static void Initialize()
		{
			if (IsInitialized)
			{
				Debug.LogError("Double Initialize detected");
				return;
			}

			ThreadSyncObject.Initialize();
			IsInitialized = true;
		}

		/// <summary> Starts keeping track of a handler to forward broadcasts/requests of this type to. </summary>
		public static void Register<T>([NotNull] IHandler<T> handler) 
			where T : IMessagingData
		{
			MessagingHelper<T>.Register(handler);
		}

		/// <summary> Stops keeping track of a handler to stop forwarding broadcasts/requests of this type to. </summary>
		public static void Unregister<T>([NotNull] IHandler<T> handler)
			where T : IMessagingData
		{
			MessagingHelper<T>.Unregister(handler);
		}

		/// <summary> Check to see if this handler is registered. </summary>
		public static bool IsRegistered<T>([NotNull] IHandler<T> handler)
			where T : class, IMessagingData
		{
			return MessagingHelper<T>.IsRegistered(handler);
		}

		/// <summary> Returns how many handlers are registered. </summary>
		public static int CountRegistered<T>()
			where T : class, IMessagingData
		{
			return MessagingHelper<T>.handlers.Count;
		}

		public static void Broadcast<T>(T data = default)
			where T : struct, IMessage
		{
			List<IHandler<T>> handlers = MessagingHelper<T>.handlers;
			for (int i = handlers.Count - 1; i >= 0; --i)
			{
				handlers[i].Handle(data);
			}
		}

		public static void Broadcast<T>(Delay delay, T data = default)
			where T : struct, IMessage
		{
			Assert.IsTrue(IsInitialized, "Messaging must be initialized before sending delayed data");

			MessagingHelper<T>.EnqueueDelayed(delay, data, null);
		}

		public static void Broadcast<T>(Delay delay, Action<T> callback, T data = default)
			where T : struct, IMessage
		{
			Assert.IsTrue(IsInitialized, "Messaging must be initialized before sending delayed data");

			MessagingHelper<T>.EnqueueDelayed(delay, data, callback);
		}

		public static void Request<T>([NotNull] T data)
			where T : class, IRequest
		{
			Assert.IsNotNull(data, "Request data must be non-null");

			List<IHandler<T>> handlers = MessagingHelper<T>.handlers;
			for (int i = handlers.Count - 1; i >= 0; --i)
			{
				handlers[i].Handle(data);
			}
		}

		public static void Request<T>(Delay delay, Action<T> callback, [NotNull] T data)
			where T : class, IRequest
		{
			Assert.IsTrue(IsInitialized, "Messaging must be initialized before sending delayed data");
			Assert.IsNotNull(data, "Request data must be non-null");

			MessagingHelper<T>.EnqueueDelayed(delay, data, callback);
		}
	}


	internal static class MessagingHelper<TMessageData> where TMessageData : IMessagingData
	{
		// List is used for cheap iteration at the cost of O(n) removal, which should happen less often.
		internal static readonly List<IHandler<TMessageData>> handlers = new List<IHandler<TMessageData>>();

		// Threads are a factor so safety is prioritized (though no additional allocation outside of should happen)
		private static readonly ConcurrentQueue<DelayedData> delayedUpdateData = new ConcurrentQueue<DelayedData>();
		private static readonly ConcurrentQueue<DelayedData> delayedLateUpdateData = new ConcurrentQueue<DelayedData>();
		private static readonly ConcurrentQueue<DelayedData> delayedFixedUpdateData = new ConcurrentQueue<DelayedData>();

		private readonly struct DelayedData
		{
			public readonly TMessageData Data;
			public readonly Action<TMessageData> CompletedAction;

			public DelayedData(TMessageData data, Action<TMessageData> callback) : this()
			{
				Data = data;
				CompletedAction = callback;
			}
		}

		internal static void Register([NotNull] IHandler<TMessageData> handler)
		{
			if (IsRegistered(handler))
			{
				Debug.LogWarning($"[Messaging] Supplied handler {handler} to be added is already registered");
				return;
			}

			handlers.Add(handler);
		}


		internal static bool IsRegistered([NotNull] IHandler<TMessageData> handler)
		{
			Assert.IsTrue(IsHandlerValid(handler, out string error), error);
			return handlers.Contains(handler);
		}

		internal static void Unregister([NotNull] IHandler<TMessageData> handler)
		{
			Assert.IsTrue(IsHandlerValid(handler, out string error), error);

			bool wasRemoved = handlers.Remove(handler);

			if (!wasRemoved && Application.isPlaying)
			{
				Debug.LogWarning($"[Messaging] Supplied handler {handler} to be removed was not registered");
			}
		}

		internal static void EnqueueDelayed(Messaging.Delay delay, TMessageData data, Action<TMessageData> callback)
		{
			switch (delay)
			{
				case Messaging.Delay.Update:
					delayedUpdateData.Enqueue(new DelayedData(data, callback));
					ThreadSyncObject.UpdateActions.TryAdd(delayedUpdateData, DelayedEmptyQueue);
					return;
				case Messaging.Delay.LateUpdate:
					delayedLateUpdateData.Enqueue(new DelayedData(data, callback));
					ThreadSyncObject.LateUpdateActions.TryAdd(delayedLateUpdateData, DelayedEmptyQueue);
					return;
				case Messaging.Delay.FixedUpdate:
					delayedFixedUpdateData.Enqueue(new DelayedData(data, callback));
					ThreadSyncObject.FixedUpdateActions.TryAdd(delayedFixedUpdateData, DelayedEmptyQueue);
					return;
			}
		}

		private static void DelayedEmptyQueue(ICollection delayBasedQueue)
		{
			ConcurrentQueue<DelayedData> queue = (ConcurrentQueue<DelayedData>)delayBasedQueue;
			while (queue.TryDequeue(out DelayedData delayData))
			{
				for (int i = handlers.Count - 1; i >= 0; --i)
				{
					handlers[i].Handle(delayData.Data);
				}
				delayData.CompletedAction?.Invoke(delayData.Data);
			}
		}

		private static bool IsHandlerValid(IHandler<TMessageData> handler, out string error)
		{
			if (handler == null)
			{
				error = "Supplied handler must be non-null";
				return false;
			}
			if (!handler.GetType().IsClass)
			{
				error = $"Supplied handler {handler} must be a class (reference type)";
				return false;
			}
			error = null;
			return true;
		}

	}

}
