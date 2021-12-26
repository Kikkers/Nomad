using Sirenix.OdinInspector;
using UnityEngine;

public class EquipmentState : MonoBehaviour
{
	[SerializeField, Required] private EquipmentStateHandle handle;

	public int NumBuildDrones { get; set; } = 400;

	private void Awake()
	{
		handle.Register(this);
	}
}
