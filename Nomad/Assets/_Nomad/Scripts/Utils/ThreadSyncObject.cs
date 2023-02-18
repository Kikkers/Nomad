using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;

namespace Utils
{
	internal class ThreadSyncObject : MonoBehaviour
	{
		internal static readonly ConcurrentDictionary<ICollection, Action<ICollection>> UpdateActions
			= new ConcurrentDictionary<ICollection, Action<ICollection>>();
		internal static readonly ConcurrentDictionary<ICollection, Action<ICollection>> LateUpdateActions
			 = new ConcurrentDictionary<ICollection, Action<ICollection>>();
		internal static readonly ConcurrentDictionary<ICollection, Action<ICollection>> FixedUpdateActions
			 = new ConcurrentDictionary<ICollection, Action<ICollection>>();

		internal static void Initialize()
		{
			GameObject obj = new GameObject("Hidden_ThreadSyncObject", typeof(ThreadSyncObject))
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			DontDestroyOnLoad(obj);
		}

		private void Update()
		{
			foreach (var pair in UpdateActions)
				pair.Value.Invoke(pair.Key);
		}

		private void LateUpdate()
		{
			foreach (var pair in LateUpdateActions)
				pair.Value.Invoke(pair.Key);
		}

		private void FixedUpdate()
		{
			foreach (var pair in FixedUpdateActions)
				pair.Value.Invoke(pair.Key);
		}
	}
}
