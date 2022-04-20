using UnityEngine;
using Utils;

public class Boot
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void OnAfterAssembliesLoaded()
	{
		Messaging.Initialize();
		Systems.Register<IInputManager>(new InputManager());
		IsInitialized = true;
	}

	public static bool IsInitialized { get; private set; }
}
