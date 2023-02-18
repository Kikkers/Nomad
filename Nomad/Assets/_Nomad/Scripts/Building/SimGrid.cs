using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class SimGrid : MonoBehaviour
{
	private static readonly ContextLogger log = ContextLogger.Get<SimGrid>();

	[SerializeField] private SimGrid joinTarget;
	[SerializeField, Range(1, 3)] private int rotateAmount;

	private enum Dir
	{
		Fwd,
		Back,
		Left,
		Right
	}

	[Button]
	private void JoinOtherOntoThis()
	{
		if (joinTarget == null)
		{
			log.Info("NO");
		}
		
		// ensure grids line up properly
		joinTarget.LineUpToOther(transform, out _, out _);

		// iterate over outer bounds grid positions to determine any size growth and/or origin offset
		BoundsInt bounds = GetBounds();
		Vector3Int min = bounds.min;
		Vector3Int max = bounds.max;
		foreach (Vector3 otherWorldPos in joinTarget.EnumerateWorldBoundsCorners())
		{
			Vector3 localPos = transform.InverseTransformPoint(otherWorldPos);
			LocalPosToCoord(localPos, out int localIndexX, out int localIndexY);
			min.x = Mathf.Min(localIndexX, min.x);
			min.y = Mathf.Min(localIndexY, min.y);
			max.x = Mathf.Max(localIndexX, max.x);
			max.y = Mathf.Max(localIndexY, max.y);
		}

		// copy own cells to new cells
		int offsetX = -min.x;
		int offsetY = -min.y;
		Vector3Int newSize = max - min;
		int newSizeX = newSize.x;
		int newSizeY = newSize.y;
		if (newSizeX % 2 == 0)
			newSizeX++;
		if (newSizeY % 2 == 0)
			newSizeY++;
		if (offsetX % 2 != 0)
			offsetX++;
		if (offsetY % 2 != 0)
			offsetY++;
		
		int sizeX = bounds.size.x;
		int sizeY = bounds.size.y;
		Cell2DData[][] newCells = MakeCells(newSizeX, newSizeY);
		for (int x = 0; x < sizeX; ++x)
		{
			for (int y = 0; y < sizeY; ++y)
			{
				newCells[x + offsetX][y + offsetY] = cells[x][y];
			}
		}
		cells = newCells;

		// iterate over all grid cells on the other grid and add them to this grid
		// TODO: optimize this to not have to go through the world space transform, and directly copy based on transormed array index
		foreach (TransferData transfer in joinTarget.EnumerateTransferData())
		{
			Vector3 localPos = transform.InverseTransformPoint(transfer.worldPos);
			LocalPosToCoord(localPos, out int localIndexX, out int localIndexY);
			cells[localIndexX + offsetX][localIndexY + offsetY] = transfer.data;
		}
		
		transform.Translate(min.x * 0.5f, 0, min.y * 0.5f);
	}

	private struct TransferData
	{
		public Vector3 worldPos;
		public Cell2DData data;
	}

	private IEnumerable<TransferData> EnumerateTransferData()
	{
		int sizeX = cells.Length;
		int sizeY = cells[0].Length;
		for (int x = 0; x < sizeX; ++x)
		{
			for (int y = 0; y < sizeY; ++y)
			{
				CoordToData(x, y, out Vector3 localPos, out Cell2DData data);
				if (data.typeId == 0)
				{
					continue;
				}
				yield return new TransferData
				{
					data = data,
					worldPos = transform.TransformPoint(localPos)
				};
			}
		}
	}

	private IEnumerable<Vector3> EnumerateWorldBoundsCorners()
	{
		int sizeX = cells.Length;
		int sizeY = cells[0].Length;

		Vector3 localPos;

		CoordToPos(0, 0, out localPos);
		yield return transform.TransformPoint(localPos);
		CoordToPos(sizeX - 1, 0, out localPos);
		yield return transform.TransformPoint(localPos);
		CoordToPos(sizeX - 1, sizeY - 1, out localPos);
		yield return transform.TransformPoint(localPos);
		CoordToPos(0, sizeY - 1, out localPos);
		yield return transform.TransformPoint(localPos);
	}

	private void LineUpToOther(Transform other, out Vector3 matchedPosition, out Quaternion matchedRotation)
	{
		// position
		Vector3 offset = other.InverseTransformPoint(transform.position);
		offset.x = Mathf.Round(offset.x);
		offset.y = Mathf.Round(offset.y);
		offset.z = Mathf.Round(offset.z);
		matchedPosition = other.TransformPoint(offset);

		// rotation
		Vector3 otherForward = transform.forward;
		Dir bestDirection = Dir.Fwd;
		float bestAngle = -float.MaxValue;

		float dotForward = Vector3.Dot(otherForward, other.forward);
		if (bestAngle < dotForward)
		{
			bestDirection = Dir.Fwd;
			bestAngle = dotForward;
		}
		if (bestAngle < -dotForward)
		{
			bestDirection = Dir.Back;
			bestAngle = -dotForward;
		}

		float dotRight = Vector3.Dot(otherForward, other.right);
		if (bestAngle < dotRight)
		{
			bestDirection = Dir.Right;
			bestAngle = dotRight;
		}
		if (bestAngle < -dotRight)
		{
			bestDirection = Dir.Left;
			bestAngle = -dotRight;
		}

		matchedRotation = other.rotation;
		switch (bestDirection)
		{
			case Dir.Back:
				matchedRotation *= Quaternion.AngleAxis(180, Vector3.up);
				break;
			case Dir.Left:
				matchedRotation *= Quaternion.AngleAxis(270, Vector3.up);
				break;
			case Dir.Right:
				matchedRotation *= Quaternion.AngleAxis(90, Vector3.up);
				break;
		}
		transform.SetPositionAndRotation(matchedPosition, matchedRotation);
	}

	private BoundsInt GetBounds()
	{
		int sizeX = cells.Length;
		int sizeY = cells[0].Length;
		return new BoundsInt(0, 0, 0, sizeX, sizeY, 1);
	}

	private void TransformSingleCell(int x, int y, Vector3 halfTranslation, Quaternion rotation,
		out int transformedX, out int transformedY, out Cell2DData data)
	{
		CoordToData(x, y, out Vector3 localPos, out data);
		Vector3 transformedPos = rotation * localPos + halfTranslation;
		LocalPosToCoord(transformedPos, out transformedX, out transformedY);
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

	[Button]
	private void DoReset()
	{
		cells = new Cell2DData[][]
		{
			new Cell2DData[]{ new Cell2DData(0), new Cell2DData(0), new Cell2DData(0), new Cell2DData(0), new Cell2DData(0) },
			new Cell2DData[]{ new Cell2DData(0), new Cell2DData(1), new Cell2DData(1), new Cell2DData(1), new Cell2DData(0) },
			new Cell2DData[]{ new Cell2DData(0), new Cell2DData(1), new Cell2DData(2), new Cell2DData(3), new Cell2DData(0) },
			new Cell2DData[]{ new Cell2DData(0), new Cell2DData(1), new Cell2DData(1), new Cell2DData(4), new Cell2DData(0) },
			new Cell2DData[]{ new Cell2DData(0), new Cell2DData(0), new Cell2DData(0), new Cell2DData(0), new Cell2DData(0) },
		};
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

	private void CoordToPos(int x, int y, out Vector3 localPos)
	{
		localPos = new Vector3(x * 0.5f, 0, y * 0.5f);
	}

	private void CoordToData(int x, int y, out Vector3 localPos, out Cell2DData data)
	{
		CoordToPos(x, y, out localPos);
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
		for (int x = 0; x < sizeX; ++x)
		{
			bool xIsCenter = x % 2 == 0;
			for (int y = 0; y < sizeY; ++y)
			{
				bool yIsCenter = y % 2 == 0;
				if (xIsCenter && yIsCenter)
				{
					CoordToData(x, y, out Vector3 pos, out Cell2DData center);
					Gizmos.color = IdToColor(center.typeId);
					Gizmos.DrawWireCube(pos, new Vector3(0.95f, 0, 0.95f));
				}
				else if (xIsCenter)
				{
					CoordToData(x, y, out Vector3 pos, out Cell2DData xEdge);
					Gizmos.color = IdToColor(xEdge.typeId);
					Gizmos.DrawWireCube(pos, new Vector3(0.25f, 0.25f, 0.05f));
				}
				else if (yIsCenter)
				{
					CoordToData(x, y, out Vector3 pos, out Cell2DData yEdge);
					Gizmos.color = IdToColor(yEdge.typeId);
					Gizmos.DrawWireCube(pos, new Vector3(0.05f, 0.25f, 0.25f));
				}
				else
				{
					CoordToData(x, y, out Vector3 pos, out Cell2DData corner);
					Gizmos.color = IdToColor(corner.typeId);
					Gizmos.DrawWireCube(pos, new Vector3(0.1f, 1, 0.1f));
				}
			}
		}

		Gizmos.matrix = Matrix4x4.identity;
	}

}
