using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public interface IGridSystem
{
	IReadOnlyList<SimGrid> ActiveGrids { get; }

	void Add(SimGrid grid);
	void Remove(SimGrid grid);
}

public class GridSystem : MonoBehaviour, IGridSystem
{
	private readonly static SingletonTracker instance = new();

	private List<SimGrid> activeGrids = new List<SimGrid>();

	public IReadOnlyList<SimGrid> ActiveGrids => activeGrids.AsReadOnly();

	public void Add(SimGrid grid)
	{
		activeGrids.Add(grid);
	}

	public void Remove(SimGrid grid)
	{
		activeGrids.Remove(grid);
	}

	private void Awake()
	{
		instance.Set(this);
		DontDestroyOnLoad(gameObject);
	}
}
