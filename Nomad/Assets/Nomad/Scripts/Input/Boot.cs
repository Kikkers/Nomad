using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

public class Boot : MonoBehaviour
{
	public static bool IsInitialized { get; private set; }

	[SerializeField, Required, AssetsOnly] private InputHandle inputHandle;

	private void Awake()
	{
		if (IsInitialized)
		{
			Debug.LogError("Double Initialize detected");
			return;
		}

		Messaging.Initialize();
		inputHandle.Initialize();

		IsInitialized = true;
	}
}
