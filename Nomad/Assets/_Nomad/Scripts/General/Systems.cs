using UnityEngine;
using Utils;

public static class Systems
{
	private readonly static Log log = Log.Systems;

	public static IInputManager Input { get; private set; }
	public static IGridManager Grids { get; private set; }
	public static bool IsStarted { get; private set; }


	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void OnAfterAssembliesLoaded()
	{
		InitializeSystems();

		IsStarted = true;
		log.Info("Ready");
	}

	private static void InitializeSystems()
	{
		log.Info("Starting singletons");
		Messaging.Initialize();
		Input = new InputManager();

		GameObject systemsGameObject = new("Systems")
		{
			hideFlags = HideFlags.DontSave
		};
		Object.DontDestroyOnLoad(systemsGameObject);

		Grids = systemsGameObject.AddComponent<GridManager>();
	}

}
