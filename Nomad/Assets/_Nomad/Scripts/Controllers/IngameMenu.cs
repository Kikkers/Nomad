using UnityEngine;
using UnityEngine.InputSystem;

public class IngameMenu : MonoBehaviour
{
	private void Start()
	{
		IInputManager input = Systems.Input;
		input.UI.Confirm.performed += OnConfirm;
		input.UI.Cancel.performed += OnCancel;
	}

	private void OnDestroy()
	{
		IInputManager input = Systems.Input;
		input.UI.Confirm.performed -= OnConfirm;
		input.UI.Cancel.performed -= OnCancel;
	}

	private void OnConfirm(InputAction.CallbackContext obj)
	{

	}

	private void OnCancel(InputAction.CallbackContext obj)
	{

	}
}
