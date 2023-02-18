using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

[RequireComponent(typeof(Rigidbody))]
public class EnvironmentalPlayerController : MonoBehaviour
{
	private readonly static ContextLogger log = ContextLogger.Get(typeof(EnvironmentalPlayerController));

	[SerializeField, Required] private Rigidbody body;

	private Vector2 inputMove;
	private float inputMoveVertical;
	private Vector2 inputLook;
	private float inputLookRoll;

	private void Start()
	{
		IInputManager input = Systems.Input;
		input.SetInputEvents(input.Player.Move, OnMove);
		input.SetInputEvents(input.Player.MoveVertical, OnMoveVertical);
		input.SetInputEvents(input.Player.Look, OnLook);
		input.SetInputEvents(input.Player.LookRoll, OnLookRoll);
	}

	private void OnDestroy()
	{
		IInputManager input = Systems.Input;
		input.ClearInputEvents(input.Player.Move, OnMove);
		input.ClearInputEvents(input.Player.MoveVertical, OnMoveVertical);
		input.ClearInputEvents(input.Player.Look, OnLook);
		input.ClearInputEvents(input.Player.LookRoll, OnLookRoll);
	}

	private void OnMove(InputAction.CallbackContext obj) => inputMove = obj.ReadValue<Vector2>();
	private void OnMoveVertical(InputAction.CallbackContext obj) => inputMoveVertical = obj.ReadValue<float>();
	private void OnLook(InputAction.CallbackContext obj) => inputLook = obj.ReadValue<Vector2>();
	private void OnLookRoll(InputAction.CallbackContext obj) => inputLookRoll = obj.ReadValue<float>();

	private void FixedUpdate()
	{
		Vector3 delta = new(inputMove.x, inputMoveVertical, inputMove.y);
		log.Info(delta);
		body.AddForce(delta, ForceMode.VelocityChange);
	}
}
