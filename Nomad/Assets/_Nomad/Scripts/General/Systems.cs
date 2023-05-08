using UnityEngine;
using UnityEngine.AddressableAssets;
using Utils;

public class Systems : MonoBehaviour
{
	[SerializeField] private CanvasUIFader screenFade;

	public static IInputManager Input { get; private set; }
	public static SessionState Session { get; private set; }
	public static ICanvasUIFader ScreenFade { get; private set; }


	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void OnAfterAssembliesLoaded()
	{
		Log.Systems.Info("Initializing Systems...");

		var handle = Addressables.LoadAssetAsync<GameObject>("Systems");
		handle.WaitForCompletion();
		Instantiate(handle.Result);
		Addressables.Release(handle.Result);

		Log.Systems.Info("Initializing Systems "+ handle.Status);
	}

	private void Awake()
	{
		Messaging.Initialize();
		Input = new InputManager();
		Session = new SessionState();
		ScreenFade = screenFade;
		ScreenFade.FadeNow(1);

		DontDestroyOnLoad(gameObject);
	}
}
