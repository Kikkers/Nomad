using UnityEngine;
using UnityEngine.InputSystem;

public class AnyPlayerController : MonoBehaviour
{
	private void Start()
	{
		IInputManager input = Systems.Input;
		input.Player.OpenPauseMenu.performed += OnConfirm;
	}

	private void OnDestroy()
	{
		IInputManager input = Systems.Input;
		input.Player.OpenPauseMenu.performed -= OnConfirm;
	}

	private void OnConfirm(InputAction.CallbackContext obj)
	{
	}

}
