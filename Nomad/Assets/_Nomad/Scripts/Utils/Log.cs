using UnityEngine;

namespace Utils
{
	/// <summary>
	/// Like <see cref="Logger"/> but more compact
	/// </summary>
	public class Log
	{
		public readonly static Log Systems = new(nameof(Systems));
		public readonly static Log Fade = new(nameof(Fade));
		public readonly static Log Messaging = new(nameof(Messaging));
		public readonly static Log Grid = new(nameof(Grid));
		public readonly static Log Player = new(nameof(Player));

		private readonly string header;

		private Log(string name)
		{
			header = $"[{name}] ";
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
