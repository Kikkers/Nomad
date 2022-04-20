using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Utils;

public class BuildGrid : MonoBehaviour
{
	private static readonly ContextLogger log = ContextLogger.Get<BuildGrid>();

	[Required, SerializeField] private BuildGridConfig config;

	[Required, SerializeField] private BoxCollider boundsCollider;
	[SerializeField] private int xOffset;
	[SerializeField] private int yOffset;

	public struct Cell2DData
	{
		public int floorData;
		public int edgeXData;
		public int edgeYData;
		public int cornerData;

		public Cell2DData(int floorData, int edgeXData, int edgeYData, int cornerData)
		{
			this.floorData = floorData;
			this.edgeXData = edgeXData;
			this.edgeYData = edgeYData;
			this.cornerData = cornerData;
		}

		public Cell2DData(string serialized)
		{
			string[] splitted = serialized.Split(';');
			floorData = splitted.Length > 0 ? int.Parse(splitted[0]) : 0;
			edgeXData = splitted.Length > 1 ? int.Parse(splitted[1]) : 0;
			edgeYData = splitted.Length > 2 ? int.Parse(splitted[2]) : 0;
			cornerData = splitted.Length > 3 ? int.Parse(splitted[3]) : 0;
		}

		public override string ToString()
		{
			return $"{floorData};{edgeXData};{edgeYData};{cornerData}";
		}
	}

	private static readonly string[] variants = new string[]
	{
		"4,4,0;0;0;0,0;0;0;0,0;0;0;0,0;0;0;0,4,0;0;0;0,1;1;1;1,1;1;1;1,0;0;0;0,4,0;0;0;0,0;0;0;0,1;1;1;1,0;0;0;0,4,0;0;0;0,0;0;0;0,0;0;0;0,0;0;0;0,",

	};

	private Cell2DData[][] cells;

	private static Cell2DData[][] MakeCells(int newSizeX, int newSizeY)
	{
		Cell2DData[][] newCells = new Cell2DData[newSizeX][];
		for (int x = 0; x < newSizeX; ++x)
		{
			newCells[x] = new Cell2DData[newSizeY];
		}
		return newCells;
	}

	private static Cell2DData[][] MakeCopy(Cell2DData[][] cells)
	{
		int newSizeX = cells.Length;
		int newSizeY = cells[0].Length;
		Cell2DData[][] newCells = MakeCells(newSizeX, newSizeY);
		for (int x = 0; x < newSizeX; ++x)
		{
			for (int y = 0; y < newSizeY; ++y)
			{
				newCells[x][y] = cells[x][y];
			}
		}
		return newCells;
	}

	private static Cell2DData[][] MakeRotatedCopy(Cell2DData[][] cells, int rotateAmount)
	{
		rotateAmount %= 4;
		if (rotateAmount == 0)
		{
			return MakeCopy(cells);
		}

		int sizeX = cells.Length;
		int sizeY = cells[0].Length;
		Cell2DData[][] newCells;
		if (rotateAmount == 2)
		{
			int halfSizeX = sizeX / 2;
			newCells = MakeCopy(cells);
			for (int x = 0, xOpposite = sizeX - 1; x < halfSizeX; ++x, --xOpposite)
			{
				for (int y = 0, yOpposite = sizeY - 1; y < sizeY; ++y, --yOpposite)
				{
					Cell2DData tmp = newCells[x][y];
					newCells[x][y] = newCells[xOpposite][yOpposite];
					newCells[xOpposite][yOpposite] = tmp;
				}
			}
			return newCells;
		}

		int newSizeX = sizeY;
		int newSizeY = sizeX;
		newCells = MakeCells(newSizeX, newSizeY);
		if (rotateAmount == 1)
		{
			for (int newX = 0, oldY = sizeY - 1; newX < newSizeX; ++newX, --oldY)
			{
				for (int newY = 0, oldX = 0; newY < newSizeY; ++newY, ++oldX)
				{
					newCells[newX][newY] = cells[oldX][oldY];
				}
			}
		}
		else if (rotateAmount == 3)
		{
			for (int newX = 0, oldY = 0; newX < newSizeX; ++newX, ++oldY)
			{
				for (int newY = 0, oldX = sizeX - 1; newY < newSizeY; ++newY, --oldX)
				{
					newCells[newX][newY] = cells[oldX][oldY];
				}
			}
		}
		return newCells;
	}

	public static void MakeMerged(
		Cell2DData[][] cells_A, Cell2DData[][] cells_B, int offsetB_X, int offsetB_Y, 
		out Cell2DData[][] newCells, out int newOriginOffsetX, out int newOriginOffsetY)
	{
		int sizeX_A = cells_A.Length;
		int sizeX_B = cells_B.Length;
		int newSizeX;
		int copyOffsetX_B;
		if (offsetB_X < 0)
		{
			copyOffsetX_B = 0;
			newOriginOffsetX = -offsetB_X;
			newSizeX = Mathf.Max(sizeX_A, sizeX_B + offsetB_X) - offsetB_X;
		}
		else if (offsetB_X > 0)
		{
			copyOffsetX_B = offsetB_X;
			newOriginOffsetX = 0;
			newSizeX = Mathf.Max(sizeX_A, sizeX_B + offsetB_X);
		}
		else
		{
			copyOffsetX_B = 0;
			newOriginOffsetX = 0;
			newSizeX = Mathf.Max(sizeX_A, sizeX_B);
		}

		int sizeY_A = cells_A[0].Length;
		int sizeY_B = cells_B[0].Length;
		int newSizeY;
		int copyOffsetY_B;
		if (offsetB_Y < 0)
		{
			copyOffsetY_B = 0;
			newOriginOffsetY = -offsetB_Y;
			newSizeY = Mathf.Max(sizeY_A, sizeY_B + offsetB_Y) - offsetB_Y;
		}
		else if (offsetB_Y > 0)
		{
			copyOffsetY_B = offsetB_Y;
			newOriginOffsetY = 0;
			newSizeY = Mathf.Max(sizeY_A, sizeY_B + offsetB_Y);
		}
		else
		{
			copyOffsetY_B = 0;
			newOriginOffsetY = 0;
			newSizeY = Mathf.Max(sizeY_A, sizeY_B);
		}

		newCells = MakeCells(newSizeX, newSizeY);

		for (int x = 0; x < sizeX_A; ++x)
		{
			for (int y = 0; y < sizeY_A; ++y)
			{
				newCells[x + newOriginOffsetX][y + newOriginOffsetY] = cells_A[x][y];
			}
		}

		for (int x = 0; x < sizeX_B; ++x)
		{
			for (int y = 0; y < sizeY_B; ++y)
			{
				newCells[x + copyOffsetX_B][y + copyOffsetY_B] = cells_B[x][y];
			}
		}

		cells_A = newCells;
	}

	private void Merge(Cell2DData[][] otherCells, int otherOffsetX, int otherOffsetY)
	{
		MakeMerged(
			cells, otherCells, otherOffsetX, otherOffsetY,
			out Cell2DData[][] newCells, out int newOriginOffsetX, out int newOriginOffsetY);
		cells = newCells;
		transform.Translate(new Vector3(-newOriginOffsetX, 0, -newOriginOffsetY));
	}

	[Button]
	public void Expand()
	{
		Merge(cells, xOffset, yOffset);
	}

	[Button]
	public void Rotate1()
	{
		cells = MakeRotatedCopy(cells, 1);
	}

	[Button]
	public void MergeOtherOntoThis()
	{
		IReadOnlyList<BuildGrid> grids = Systems.Get<IGridSystem>().ActiveGrids;
		BuildGrid other = null;
		foreach (var grid in grids)
		{
			if (grid == this)
				continue;
			other = grid;
			break;
		}
		if (other == null)
			return;

		float otherAngle = other.transform.rotation.eulerAngles.y;
		float thisAngle = transform.rotation.eulerAngles.y;
		float deltaAngle = thisAngle - otherAngle;
		if (deltaAngle > 180)
			deltaAngle -= 360;
		if (deltaAngle < -180)
			deltaAngle += 360;

		int rotateAmount;
		if (deltaAngle > 135 || deltaAngle < -135)
			rotateAmount = 2;
		else if (deltaAngle > 45)
			rotateAmount = 3;
		else if (deltaAngle < -45)
			rotateAmount = 1;
		else
			rotateAmount = 0;

		float targetAngle = rotateAmount * 90;
		other.transform.eulerAngles = new Vector3(0, thisAngle + targetAngle, 0);
	}

	[Button]
	public void Print()
	{
		string str = CellsToString(cells);
		Cell2DData[][] cells2 = StringToCells(str);
		string str2 = CellsToString(cells2);
		Debug.Log("res: " + (str == str2));
		Debug.Log(str);
		Debug.Log(str2);
	}

	private void Awake()
	{
		cells = StringToCells(variants[0]);
	}

	private static Cell2DData[][] StringToCells(string str)
	{
		try
		{
			string[] parts = str.Split(',');
			int index = 0;
			int sizeX = int.Parse(parts[index++]);
			Cell2DData[][] cells = new Cell2DData[sizeX][];
			for (int x = 0; x < sizeX; ++x)
			{
				int sizeY = int.Parse(parts[index++]);
				cells[x] = new Cell2DData[sizeY];
				Cell2DData[] row = cells[x];
				for (int y = 0; y < sizeY; ++y)
				{
					row[y] = new Cell2DData(parts[index++]);
				}
			}
			return cells;
		}
		catch(Exception ex)
		{
			Debug.LogError("Failed to parse cells: " + ex);
			return new Cell2DData[0][];
		}
	}

	private static string CellsToString(Cell2DData[][] cells)
	{
		StringBuilder sb = new StringBuilder();
		sb.Append(cells.Length).Append(',');
		for(int x = 0; x < cells.Length; ++x)
		{
			Cell2DData[] row = cells[x];
			sb.Append(row.Length).Append(',');
			for (int y = 0; y < row.Length; ++y)
			{
				sb.Append(row[y].ToString()).Append(',');
			}
		}
		return sb.ToString();
	}

	private void OnDrawGizmos()
	{
		if (cells == null)
			return;

		Gizmos.matrix = transform.localToWorldMatrix;

		for (int x = 0; x < cells.Length; ++x)
		{
			Cell2DData[] row = cells[x];
			for (int y = 0; y < row.Length; ++y)
			{
				switch (row[y].floorData)
				{
					case 0: Gizmos.color = Color.black; break;
					default: Gizmos.color = Color.blue; break;
				}
				Gizmos.DrawWireCube(new Vector3(0.5f + x, 0, 0.5f + y), new Vector3(0.95f, 0, 0.95f));

				switch (row[y].edgeXData)
				{
					case 0: Gizmos.color = Color.clear; break;
					default: Gizmos.color = Color.red; break;
				}
				Gizmos.DrawWireCube(new Vector3(0.5f + x, 1, y), new Vector3(0.95f, 1, 0));

				switch (row[y].edgeYData)
				{
					case 0: Gizmos.color = Color.clear; break;
					default: Gizmos.color = Color.green; break;
				}
				Gizmos.DrawWireCube(new Vector3(x, 1, 0.5f + y), new Vector3(0, 1, 0.95f));

				switch (row[y].cornerData)
				{
					case 0: Gizmos.color = Color.clear; break;
					default: Gizmos.color = Color.white; break;
				}
				Gizmos.DrawWireSphere(new Vector3(x, 0, y), 0.25f);
			}
		}

		Gizmos.matrix = Matrix4x4.identity;
	}

	private void OnEnable()
	{
		Systems.Get<IGridSystem>().Add(this);
	}

	private void OnDisable()
	{
		Systems.Get<IGridSystem>().Remove(this);
	}

}
