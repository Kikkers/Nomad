using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public interface IGridSystem : ISystem
{
	IReadOnlyList<SimGrid> ActiveGrids { get; }

	void Add(SimGrid grid);
	void Remove(SimGrid grid);
}

public class GridSystem : MonoBehaviour, IGridSystem
{

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
		Systems.Register<IGridSystem>(this);
	}

	private void OnDestroy()
	{
		Systems.Unregister<IGridSystem>(this);
	}
}
