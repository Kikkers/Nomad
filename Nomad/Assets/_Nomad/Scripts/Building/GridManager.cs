using System.Collections.Generic;
using UnityEngine;
using Utils;

public interface IGridManager
{
	IReadOnlyList<SimGrid> ActiveGrids { get; }

	void Add(SimGrid grid);
	void Remove(SimGrid grid);
}

public class GridManager : MonoBehaviour, IGridManager
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
