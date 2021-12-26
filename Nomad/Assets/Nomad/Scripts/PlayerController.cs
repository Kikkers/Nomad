using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
	[SerializeField, Required, AssetsOnly] private InputHandle inputHandle;

	[SerializeField, Required] private Rigidbody body;

	private Vector2 inputMove;

	private void Start()
	{
		inputHandle.Player.Move.performed += OnMove;
		inputHandle.Player.Move.canceled += OnMove;
		inputHandle.Player.Move.started += OnMove;
	}

	private void OnDestroy()
	{
		inputHandle.Player.Move.performed -= OnMove;
		inputHandle.Player.Move.canceled -= OnMove;
		inputHandle.Player.Move.started -= OnMove;
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
