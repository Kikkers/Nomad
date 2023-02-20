using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentStateSettings", menuName = "Settings/EquipmentState")]
public class EquipmentStateSettings : ScriptableObject
{
	[field: SerializeField] public bool RenameMe { get; private set; }
}
