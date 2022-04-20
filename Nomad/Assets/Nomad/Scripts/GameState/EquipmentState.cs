using UnityEngine;
using Utils;

public interface IEquipmentState : ISystem
{
	int NumBuildDrones { get; set; }
}

public class EquipmentState : MonoBehaviour, IEquipmentState
{
	public int NumBuildDrones { get; set; } = 400;


	private void Awake()
	{
		Systems.Register<IEquipmentState>(this);
	}

	private void OnDestroy()
	{
		Systems.Unregister<IEquipmentState>(this);
	}
}
