using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

public class BuildGrid : MonoBehaviour
{
	private static readonly ContextLogger log = ContextLogger.Get<BuildGrid>();

	[SerializeField] private BuildGrid joinTarget;
	[SerializeField, Range(1, 3)] private int rotateAmount;
	[SerializeField] private int offsetX;
	[SerializeField] private int offsetY;

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
		joinTarget.LineUpToOtherTransform(transform);

		// iterate over outer bounds grid positions to determine any size growth and/or origin offset
		// TODO

		// iterate over all grid cells on the other grid and add them to this grid
	}

	private void LineUpToOtherTransform(Transform other)
	{
		// position
		Vector3 otherPos = other.localPosition;
		Vector3 thisPos = other.InverseTransformPoint(transform.position);
		Vector3 diff = otherPos - thisPos;
		diff.x = Mathf.Round(diff.x);
		diff.y = Mathf.Round(diff.y);
		diff.z = Mathf.Round(diff.z);
		Vector3 matchedPosition = other.TransformPoint(otherPos - diff);

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

		Quaternion matchedRotation = other.rotation;
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
		Vector3 halfTranslation = translation * 0.5f;

		BoundsInt newBounds = CalcTransformedBounds(halfTranslation, rotation);
		Vector3Int min = newBounds.min;
		Vector3Int max = newBounds.max;
		int newSizeX = max.x - min.x;
		int newSizeY = max.y - min.y;
		int offsetX = min.x < 0 ? -min.x : 0;
		int offsetY = min.y < 0 ? -min.y : 0;

		log.Info($"{sizeX},{sizeY} => {newSizeX},{newSizeY} ({offsetX},{offsetY})");

		Cell2DData[][] newCells = MakeCells(newSizeX, newSizeY);
		for (int x = 0; x < sizeX; ++x)
		{
			for (int y = 0; y < sizeY; ++y)
			{
				TransformSingleCell(x, y, halfTranslation, rotation, out int transformedX, out int transformedY, out Cell2DData data);

				log.Info($"{(transformedX)},{(transformedY)} ({offsetX},{offsetY})");

				newCells[transformedX + offsetX][transformedY + offsetY] = data;
			}
		}
		cells = newCells;
	}

	private BoundsInt CalcTransformedBounds(Vector3 translation, Quaternion rotation)
	{
		int sizeX = cells.Length;
		int sizeY = cells[0].Length;

		Vector3Int min = new Vector3Int(int.MaxValue, int.MaxValue, 0);
		Vector3Int max = new Vector3Int(int.MinValue, int.MinValue, 0);
		for(int x = 0; x < sizeX; x += sizeX - 1)
		{
			for (int y = 0; y < sizeY; y += sizeY - 1)
			{
				TransformSingleCell(x, y, translation, rotation, out int transformedX, out int transformedY, out _);
				min.x = Mathf.Min(transformedX, min.x);
				min.y = Mathf.Min(transformedY, min.y);
				max.x = Mathf.Max(transformedX, max.x);
				max.y = Mathf.Max(transformedY, max.y);
			}
		}
		BoundsInt bounds = new BoundsInt();
		bounds.SetMinMax(min, max + Vector3Int.one);
		return bounds;
	}

	private BoundsInt MergeBounds(BoundsInt a, BoundsInt b)
	{
		return new BoundsInt(
			Vector3Int.Min(a.min, b.min),
			Vector3Int.Max(a.max, b.max));
	}

	private void TransformSingleCell(int x, int y, Vector3 halfTranslation, Quaternion rotation,
		out int transformedX, out int transformedY, out Cell2DData data)
	{
		CoordToData(x, y, out Vector3 localPos, out data);
		Vector3 rotatedPos = rotation * localPos + halfTranslation;
		LocalPosToCoord(rotatedPos, out transformedX, out transformedY);
	}

	private void RotateSingleCell(int x, int y, Quaternion rotation, out int rotatedX, out int rotatedY, out Cell2DData data)
	{
		CoordToData(x, y, out Vector3 localPos, out data);
		Vector3 rotatedPos = rotation * localPos;
		LocalPosToCoord(rotatedPos, out rotatedX, out rotatedY);
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
