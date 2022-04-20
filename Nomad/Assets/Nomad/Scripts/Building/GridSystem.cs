using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public interface IGridSystem : ISystem
{
	IReadOnlyList<BuildGrid> ActiveGrids { get; }

	void Add(BuildGrid grid);
	void Remove(BuildGrid grid);
}

public class GridSystem : MonoBehaviour, IGridSystem
{

	private List<BuildGrid> activeGrids = new List<BuildGrid>();

	public IReadOnlyList<BuildGrid> ActiveGrids => activeGrids.AsReadOnly();

	public void Add(BuildGrid grid)
	{
		activeGrids.Add(grid);
	}

	public void Remove(BuildGrid grid)
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
