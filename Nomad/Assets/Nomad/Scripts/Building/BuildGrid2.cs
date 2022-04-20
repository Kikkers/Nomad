using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

public class BuildGrid2 : MonoBehaviour
{
	private static readonly ContextLogger log = ContextLogger.Get<BuildGrid2>();

	[SerializeField] private BuildGrid2 mergeTarget;
	[SerializeField, Range(1, 3)] private int rotateAmount;
	[SerializeField] private int offsetX;
	[SerializeField] private int offsetY;

	private void Rotate(Quaternion rotation)
	{
		int sizeX = cells.Length;
		int sizeY = cells[0].Length;

		RotateSingleCell(sizeX-1, sizeY-1, rotation, out int rotatedSizeX, out int rotatedSizeY, out _);
		int offsetX = rotatedSizeX < 0 ? -rotatedSizeX : 0;
		int offsetY = rotatedSizeY < 0 ? -rotatedSizeY : 0;
		rotatedSizeX = Mathf.Abs(rotatedSizeX) + 1;
		rotatedSizeY = Mathf.Abs(rotatedSizeY) + 1;

		Cell2DData[][] newCells = MakeCells(rotatedSizeX, rotatedSizeY);
		for (int x = 0; x < sizeX; ++x)
		{
			for (int y = 0; y < sizeY; ++y)
			{
				RotateSingleCell(x, y, rotation, out int rotatedX, out int rotatedY, out Cell2DData data);
				newCells[rotatedX + offsetX][rotatedY + offsetY] = data;
			}
		}
		cells = newCells;
	}

	private void RotateOffset(Quaternion rotation, Vector3 translation)
	{
		int sizeX = cells.Length;
		int sizeY = cells[0].Length;

		CalcTransformedBounds(rotation, translation, out int minX, out int maxX, out int minY, out int maxY);
		int newSizeX = maxX - minX;
		int newSizeY = maxY - minY;
		int offsetX = minX < 0 ? -minX : 0;
		int offsetY = minY < 0 ? -minY : 0;

		Cell2DData[][] newCells = MakeCells(newSizeX, newSizeY);
		for (int x = 0; x < sizeX; ++x)
		{
			for (int y = 0; y < sizeY; ++y)
			{
				TransformSingleCell(x, y, translation, rotation, out int transformedX, out int transformedY, out Cell2DData data);
				newCells[transformedX + offsetX][transformedY + offsetY] = data;
			}
		}
		cells = newCells;
	}

	private void CalcTransformedBounds(Quaternion rotation, Vector3 translation, 
		out int minX, out int maxX, out int minY, out int maxY)
	{
		int sizeX = cells.Length;
		int sizeY = cells[0].Length;

		minX = int.MaxValue;
		maxX = int.MinValue;
		minY = int.MaxValue;
		maxY = int.MinValue;
		for(int x = 0; x < sizeX; x += sizeX - 1)
		{
			for (int y = 0; y < sizeY; y += sizeY - 1)
			{
				TransformSingleCell(x, y, translation, rotation, out int transformedX, out int transformedY, out _);
				minX = Mathf.Min(transformedX, minX);
				maxX = Mathf.Max(transformedX, maxX);
				minY = Mathf.Min(transformedY, minY);
				maxY = Mathf.Max(transformedY, maxY);
			}
		}
	}

	private void TransformSingleCell(int x, int y, Vector3 translation, Quaternion rotation,
		out int rotatedX, out int rotatedY, out Cell2DData data)
	{
		CoordToData(x, y, out Vector3 localPos, out data);
		Vector3 rotatedPos = rotation * localPos + translation;
		LocalPosToCoord(rotatedPos, out rotatedX, out rotatedY);
	}

	private void RotateSingleCell(int x, int y, Quaternion rotation, out int rotatedX, out int rotatedY, out Cell2DData data)
	{
		CoordToData(x, y, out Vector3 localPos, out data);
		Vector3 rotatedPos = rotation * localPos;
		LocalPosToCoord(rotatedPos, out rotatedX, out rotatedY);
	}

	[Button]
	private void Merge()
	{
		var otherCells = mergeTarget.cells;
		//TODO
	}

	[Button]
	private void RotateAndOffset()
	{
		RotateOffset(Quaternion.AngleAxis(90 * rotateAmount, Vector3.up), new Vector3(offsetX, 0, offsetY));
	}

	[Button]
	private void Rotate90()
	{
		Rotate(Quaternion.AngleAxis(90, Vector3.up));
	}

	[Button]
	private void Rotate180()
	{
		Rotate(Quaternion.AngleAxis(180, Vector3.up));
	}

	[Button]
	private void Rotate270()
	{
		Rotate(Quaternion.AngleAxis(270, Vector3.up));
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
	private static Cell2DData[][] MakeCells(int newSizeX, int newSizeY)
	{
		Cell2DData[][] newCells = new Cell2DData[newSizeX][];
		for (int x = 0; x < newSizeX; ++x)
		{
			newCells[x] = new Cell2DData[newSizeY];
		}
		return newCells;
	}

	public struct Cell2DData
	{
		public int typeId;
		public int value;

		public Cell2DData(int typeId)
		{
			this.typeId = typeId;
			value = 0;
		}
	}

	private Cell2DData[][] cells = new Cell2DData[][]
	{
		new Cell2DData[]{ new Cell2DData(0), new Cell2DData(0), new Cell2DData(0), new Cell2DData(0), new Cell2DData(0) },
		new Cell2DData[]{ new Cell2DData(0), new Cell2DData(1), new Cell2DData(1), new Cell2DData(1), new Cell2DData(0) },
		new Cell2DData[]{ new Cell2DData(0), new Cell2DData(1), new Cell2DData(2), new Cell2DData(3), new Cell2DData(0) },
		new Cell2DData[]{ new Cell2DData(0), new Cell2DData(1), new Cell2DData(1), new Cell2DData(4), new Cell2DData(0) },
		new Cell2DData[]{ new Cell2DData(0), new Cell2DData(0), new Cell2DData(0), new Cell2DData(0), new Cell2DData(0) },
	};

	private void LocalPosToCoord(Vector3 pos, out int x, out int y)
	{
		x = Mathf.RoundToInt(pos.x * 2);
		y = Mathf.RoundToInt(pos.z * 2);
	}

	private void CoordToData(int x, int y, out Vector3 localPos, out Cell2DData data)
	{
		localPos = new Vector3(x * 0.5f, 0, y * 0.5f);
		data = cells[x][y];
	}

	private Color IdToColor(int id)
	{
		switch (id)
		{
			case 1: return Color.red;
			case 2: return Color.green;
			case 3: return Color.blue;
			case 4: return Color.white;
			default: return Color.black;
		}
	}

	private void OnDrawGizmos()
	{
		if (cells == null || cells.Length == 0)
			return;

		Gizmos.matrix = transform.localToWorldMatrix;

		// centers
		int sizeX = cells.Length;
		int sizeY = cells[0].Length;
		Vector3 pos;
		for (int x = 0; x < sizeX; x += 2)
		{
			for (int y = 0; y < sizeY; y += 2)
			{
				CoordToData(x, y, out pos, out Cell2DData center);
				Gizmos.color = IdToColor(center.typeId);
				Gizmos.DrawWireCube(pos, new Vector3(0.95f, 0, 0.95f));
			}
		}

		for (int x = 1; x < sizeX; x += 2)
		{
			for (int y = 1; y < sizeY; y += 2)
			{
				CoordToData(x, y - 1, out pos, out Cell2DData xEdge);
				Gizmos.color = IdToColor(xEdge.typeId);
				Gizmos.DrawWireCube(pos, new Vector3(0.05f, 0.25f, 0.25f));

				CoordToData(x - 1, y, out pos, out Cell2DData yEdge);
				Gizmos.color = IdToColor(yEdge.typeId);
				Gizmos.DrawWireCube(pos, new Vector3(0.25f, 0.25f, 0.05f));

				CoordToData(x, y, out pos, out Cell2DData corner);
				Gizmos.color = IdToColor(corner.typeId);
				Gizmos.DrawWireCube(pos, new Vector3(0.1f, 1, 0.1f));
			}
		}

		Gizmos.matrix = Matrix4x4.identity;
	}

}
