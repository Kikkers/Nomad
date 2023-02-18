using UnityEngine;
using Utils;

public static class Systems
{
	private readonly static ContextLogger log = ContextLogger.Get(typeof(Systems));

	public static IInputManager Input { get; private set; }
	public static bool IsStarted { get; private set; }


	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void OnAfterAssembliesLoaded()
	{
		Messaging.Initialize();
		Input = new InputManager();

		GameObject systemsGameObject = new GameObject("Systems");
		systemsGameObject.hideFlags = HideFlags.DontSave;
		Object.DontDestroyOnLoad(systemsGameObject);

		IsStarted = true;
		log.Info("Starting");
	}

}
