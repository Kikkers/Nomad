using System;
using UnityEngine.InputSystem;
using Utils;

public interface IInputManager
{
	Inputs.PlayerActions Player { get; }
	Inputs.UIActions UI { get; }

	void SetInputEvents(InputAction action, Action<InputAction.CallbackContext> function);
	void ClearInputEvents(InputAction action, Action<InputAction.CallbackContext> function);
}

public class InputManager : IInputManager
{
	private readonly static SingletonTracker instance = new();
	private readonly Inputs autoGenerated;

	public Inputs.PlayerActions Player => autoGenerated.Player;
	public Inputs.UIActions UI => autoGenerated.UI;

	public InputManager()
	{
		instance.Set(this);

		autoGenerated = new Inputs();
		autoGenerated.Enable();
	}

	public void SetInputEvents(InputAction action, Action<InputAction.CallbackContext> function)
	{
		action.performed += function;
		action.canceled += function;
		action.started += function;
	}

	public void ClearInputEvents(InputAction action, Action<InputAction.CallbackContext> function)
	{
		action.performed -= function;
		action.canceled -= function;
		action.started -= function;
	}
}
