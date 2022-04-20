using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
	[SerializeField, Required] private Rigidbody body;

	private Vector2 inputMove;

	private void Start()
	{
		IInputManager input = Systems.Get<IInputManager>();
		input.Player.Move.performed += OnMove;
		input.Player.Move.canceled += OnMove;
		input.Player.Move.started += OnMove;
	}

	private void OnDestroy()
	{
		IInputManager input = Systems.Get<IInputManager>();
		input.Player.Move.performed -= OnMove;
		input.Player.Move.canceled -= OnMove;
		input.Player.Move.started -= OnMove;
	}

	private void OnMove(InputAction.CallbackContext obj)
	{
		inputMove = obj.ReadValue<Vector2>();
	}

	private void FixedUpdate()
	{
		Vector3 delta = new Vector3(inputMove.x, 0, inputMove.y);
		body.AddForce(delta, ForceMode.VelocityChange);
	}
}
