using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameSessionController : MonoBehaviour
{
	private void Awake()
	{
		IInputManager input = Systems.Input;
		input.Player.OpenPauseMenu.performed += OnOpenPauseMenu;
		input.UI.Confirm.performed += OnMenuConfirm;
		input.UI.Cancel.performed += OnMenuCancel;
	}

	private void OnDestroy()
	{
		IInputManager input = Systems.Input;
		input.Player.OpenPauseMenu.performed -= OnOpenPauseMenu;
		input.UI.Confirm.performed -= OnMenuConfirm;
		input.UI.Cancel.performed -= OnMenuCancel;
	}

	private IEnumerator Start()
	{
		yield return Systems.ScreenFade.FadeInDefaultRoutine();
		SetToCharacterControls();
	}

	public void SetToCharacterControls()
	{
		Systems.Input.Player.Enable();
		Systems.Input.UI.Disable();
	}

	public void SetToMenuControls()
	{
		Systems.Input.Player.Disable();
		Systems.Input.UI.Enable();
	}

	private void OnOpenPauseMenu(InputAction.CallbackContext obj)
	{
		Debug.Log("OnOpenPauseMenu");
	}

	private void OnMenuConfirm(InputAction.CallbackContext obj)
	{
		Debug.Log("OnMenuConfirm");
	}

	private void OnMenuCancel(InputAction.CallbackContext obj)
	{
		Debug.Log("OnMenuCancel");
	}
}
