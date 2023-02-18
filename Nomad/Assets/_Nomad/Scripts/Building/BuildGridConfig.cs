using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Nomad/BuildGridConfig")]
public class BuildGridConfig : ScriptableObject
{
	[field: SerializeField, Required, AssetsOnly] public BuildPiece WallSocketPrefab { get; private set; }
	[field: SerializeField, Required, AssetsOnly] public BuildPiece WallPrefab { get; private set; }
	[field: SerializeField, Required, AssetsOnly] public BuildPiece FloorPrefab { get; private set; }
}
