using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentStateHandle", menuName = "Handles/EquipmentStateHandle")]
public class EquipmentStateHandle : ScriptableObject
{
	public EquipmentState Instance { get; private set; }

	public void Register(EquipmentState instance)
	{
		Instance = instance;
	}
}
