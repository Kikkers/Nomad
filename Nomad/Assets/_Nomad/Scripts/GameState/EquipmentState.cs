using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public interface IEquipmentState 
{
	int NumBuildDrones { get; set; }

	IReadOnlyList<ItemConfig> ItemsOwned { get; }
}

public class EquipmentState : MonoBehaviour, IEquipmentState
{
	private readonly static SingletonTracker instance = new();

	public int NumBuildDrones { get; set; } = 400;

	public IReadOnlyList<ItemConfig> ItemsOwned => itemsOwned.AsReadOnly();

	private List<ItemConfig> itemsOwned = new List<ItemConfig>();

	private void Awake()
	{
		instance.Set(this);
	}
}
