using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentalPlayerControllerSettings", menuName = "Settings/EnvironmentalPlayerController")]
public class EnvironmentalPlayerControllerSettings : ScriptableObject
{
	[field: SerializeField] public Vector3 MoveStrength { get; private set; }
	[field: SerializeField] public Vector3 RotateStrength { get; private set; }
}
