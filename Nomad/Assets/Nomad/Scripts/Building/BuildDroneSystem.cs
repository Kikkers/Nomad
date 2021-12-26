using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class BuildDroneSystem : MonoBehaviour
{
	[SerializeField, Required, AssetsOnly] private Boid boidPrefab;

	[SerializeField, Required, AssetsOnly] private EquipmentStateHandle equipmentStateHandle;

	[SerializeField, Required] private BoidSystem boidSystem;
	[SerializeField, Required] private Transform boidTarget;

	private readonly List<Boid> currentDrones = new List<Boid>();

	private void Start()
	{
		int numDrones = equipmentStateHandle.Instance.NumBuildDrones;
		Transform droneParent = boidSystem.transform;
		for (int i = 0; i < numDrones; ++i)
		{
			Vector3 pos = transform.position + Random.insideUnitSphere;
			Boid boid = Instantiate(boidPrefab, pos, Quaternion.identity, droneParent);
			currentDrones.Add(boid);
		}

		boidSystem.SetBoids(currentDrones, boidTarget);
	}
}
