using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class BoidSystem : MonoBehaviour
{
	[SerializeField, Required, AssetsOnly] private BoidConfig config;

	public IReadOnlyList<Boid> CurrentBoids => currentBoids;
	public Transform CurrentTarget { get; private set; }

	private readonly List<Boid> currentBoids = new List<Boid>();
	private int numBoids;
	private int threadGroups;
	private Boid.Data[] boidData;
	private ComputeBuffer boidBuffer;

	public void SetBoids(IEnumerable<Boid> newBoids, Transform target)
	{
		if (boidBuffer != null)
		{
			boidBuffer.Release();
		}

		CurrentTarget = target;

		currentBoids.Clear();
		foreach (Boid boid in newBoids)
		{
			currentBoids.Add(boid);
		}

		numBoids = currentBoids.Count;
		const int threadGroupSize = 1024;
		threadGroups = Mathf.CeilToInt(numBoids / (float)threadGroupSize);

		boidData = new Boid.Data[numBoids];
		for(int i = 0; i < numBoids; ++i)
		{
			currentBoids[i].Initialize(config, target, ref boidData[i]);
		}
		boidBuffer = new ComputeBuffer(numBoids, Boid.Data.Size);
	}

	private void OnDestroy()
	{
		boidBuffer?.Release();
	}

	private void Update()
	{
		if (numBoids == 0)
		{
			return;
		}

		ComputeShader shader = config.StepShader;

		boidBuffer.SetData(boidData);

		shader.SetBuffer(0, "boids", boidBuffer);
		shader.SetInt("numBoids", numBoids);
		shader.SetFloat("viewRadius", config.perceptionRadius);
		shader.SetFloat("avoidRadius", config.avoidanceRadius);

		shader.Dispatch(0, threadGroups, 1, 1);

		boidBuffer.GetData(boidData);

		for (int i = 0; i < numBoids; i++)
		{
			currentBoids[i].PostComputeUpdate(ref boidData[i]);
		}

	}
}
