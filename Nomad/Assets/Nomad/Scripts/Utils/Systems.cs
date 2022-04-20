using JetBrains.Annotations;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Assertions;

namespace Utils
{
	public interface ISystem { }

	public static class Systems
	{
		private readonly static ContextLogger log = ContextLogger.Get(typeof(Systems));
		private readonly static HashSet<ISystem> allRegistered = new HashSet<ISystem>();

		public static T Get<T>()
			where T : class, ISystem
		{
			Assert.IsTrue(VerifyType<T>(out string error), error);
			return SystemsHelper<T>.Instance;
		}

		public static void Register<T>([NotNull] T system)
			where T : class, ISystem
		{
			Assert.IsTrue(VerifyType<T>(out string error), error);
			T existing = SystemsHelper<T>.Instance;
			if (existing != null)
			{
				log.Error($"A system of type {typeof(T).Name} was already registered and should be unregistered first");
				allRegistered.Remove(existing);
			}
			allRegistered.Add(system);
			SystemsHelper<T>.Instance = system;
		}

		public static void Unregister<T>([NotNull] T system)
			where T : class, ISystem
		{
			Assert.IsTrue(VerifyType<T>(out string error), error);
			if (system != SystemsHelper<T>.Instance)
			{
				log.Warn($"Supplied system {system} to be removed was not the instance registered");
			}
			SystemsHelper<T>.Instance = null;
			allRegistered.Remove(system);
		}

		private static bool VerifyType<T>(out string error)
			where T : class, ISystem
		{
			if (!typeof(T).IsInterface)
			{
				error = $"{typeof(T).Name} can't be uses as a System as it's not an interface";
				return false;
			}
			error = null;
			return true;
		}

		public static int NumRegistered => allRegistered.Count;

		public static IEnumerator<ISystem> GetEnumerator() => allRegistered.GetEnumerator();

		public static string RegisteredToString()
		{
			StringBuilder builder = new StringBuilder();
			foreach (object system in allRegistered)
			{
				builder.AppendLine(system.ToString());
			}
			return builder.ToString();
		}

		private static class SystemsHelper<T>
				where T : class, ISystem
		{
			internal static T Instance { get; set; }
		}

	}

}
