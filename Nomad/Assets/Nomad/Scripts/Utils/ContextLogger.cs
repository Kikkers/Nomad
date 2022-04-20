using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
	public class ContextLogger
	{
		private static ConcurrentDictionary<string, ContextLogger> activeLoggers =
			new ConcurrentDictionary<string, ContextLogger>();

		public static ContextLogger Get(string name)
		{
			return activeLoggers.GetOrAdd(name, (newName) => { return new ContextLogger(newName); });
		}

		public static ContextLogger Get(System.Type type) => Get(type.Name);
		public static ContextLogger Get<T>() => Get(typeof(T).Name);

		private readonly string header;

		public string Name { get; }

		private ContextLogger(string name)
		{
			Name = name;
			header = $"[{Name}] ";
		}

		public void Info(object message)
		{
			Debug.Log(header + message);
		}

		public void Warn(object message)
		{
			Debug.LogWarning(header + message);
		}

		public void Error(object message)
		{
			Debug.LogError(header + message);
		}
	}
}
