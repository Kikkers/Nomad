
public class SessionState
{
	public UIState UI { get; private set; } = new();
	public SimulationState Simulation { get; private set; } = new();
}
