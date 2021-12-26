using UnityEngine;

[CreateAssetMenu(fileName = "InputHandle", menuName = "Handles/InputHandle")]
public class InputHandle : ScriptableObject
{
	public Inputs Instance { get; private set; }
	public Inputs.PlayerActions Player => Instance.Player;
	public Inputs.UIActions UI => Instance.UI;

	public void Initialize()
	{
		Instance = new Inputs();
		Instance.Enable();
	}
}
