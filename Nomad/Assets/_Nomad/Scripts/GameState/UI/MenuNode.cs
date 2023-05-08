using UnityEngine;

public class MenuNode : MonoBehaviour
{
	public enum State
	{
		Disabled,
		Background,
		Foreground,
	}

	public State CurrentState { get; private set; }

	private void Awake()
	{
		CurrentState = State.Disabled;
	}

	public void MoveToForeground()
	{
		CurrentState = State.Foreground;
	}

	public void MoveToBackground()
	{
		CurrentState = State.Background;
	}

	public void MoveToDisabled()
	{
		CurrentState = State.Disabled;
	}

}
