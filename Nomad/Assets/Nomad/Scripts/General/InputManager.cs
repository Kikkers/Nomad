using Utils;

public interface IInputManager : ISystem
{
	Inputs.PlayerActions Player { get; }
	Inputs.UIActions UI { get; }
}

public class InputManager : IInputManager
{
	private readonly Inputs autoGenerated;

	public Inputs.PlayerActions Player => autoGenerated.Player;
	public Inputs.UIActions UI => autoGenerated.UI;


	public InputManager()
	{
		autoGenerated = new Inputs();
		autoGenerated.Enable();
	}
}
