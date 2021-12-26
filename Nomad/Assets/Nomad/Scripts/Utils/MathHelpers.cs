using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public static class MathHelper
{
	public static bool IsNearZero(Vector3 vector, float threshold = 0.0001f) => vector.sqrMagnitude < threshold;
	public static bool IsNearZero(Vector2 vector, float threshold = 0.0001f) => vector.sqrMagnitude < threshold;

	public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time)
	{
		if (Time.deltaTime < Mathf.Epsilon) return rot;
		// account for double-cover
		float dot = Quaternion.Dot(rot, target);
		float multi = dot > 0f ? 1f : -1f;
		target.x *= multi;
		target.y *= multi;
		target.z *= multi;
		target.w *= multi;
		// smooth damp (nlerp approx)
		var Result = new Vector4(
			Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
			Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
			Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
			Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
		).normalized;

		// ensure deriv is tangent
		var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
		deriv.x -= derivError.x;
		deriv.y -= derivError.y;
		deriv.z -= derivError.z;
		deriv.w -= derivError.w;

		return new Quaternion(Result.x, Result.y, Result.z, Result.w);
	}


	public static float3 BezierQuad(float3 p0, float3 p1, float3 p2, float t)
	{
		float tInv = 1f - t;

		return
			p0 * (tInv * tInv) +
			p1 * (2f * tInv * t) +
			p2 * (t * t);
	}

	public static float3 BezierCubic(float3 p0, float3 p1, float3 p2, float3 p3, float t)
	{
		float tInv = 1f - t;

		float tInv2 = tInv * tInv;
		float t2 = t * t;
		return
			p0 * (tInv2 * tInv) +
			p1 * (3f * tInv2 * t) +
			p2 * (3f * tInv * t2) +
			p3 * (t2 * t);
	}

	public static int Binomial(int n, int k)
	{
		return Factorial(n) /
			(Factorial(k) * Factorial(n - k));
	}

	public static int Factorial(int n)
	{
		int f = 1;
		for (int i = 2; i <= n; ++i)
			f *= i;
		return f;
	}

	public static float PowInt(float x, int pow)
	{
		float result = 1;
		for (int i = 0; i < pow; ++i)
			result *= x;
		return result;
	}

	public static float3 BezierGeneral(ref NativeArray<float3> px, float t)
	{
		float tInv = 1f - t;
		int n = px.Length - 1;

		float3 sum = float3.zero;
		for (int i = 0; i <= n; ++i)
		{
			sum += px[i] * Binomial(n, i) * PowInt(t, i) * PowInt(tInv, n - i);
		}
		return sum;
	}

	public static float3 BezierGeneral(float3[] px, float t)
	{
		float tInv = 1f - t;
		int n = px.Length - 1;

		float3 sum = float3.zero;
		for (int i = 0; i <= n; ++i)
		{
			sum += px[i] * Binomial(n, i) * PowInt(t, i) * PowInt(tInv, n - i);
		}
		return sum;
	}
}

public static class GeometryHelper
{
	public static Vector3[] GenerateDirections(int count)
	{
		return GenerateDirectionsInternal(count, count);
	}

	public static Vector3[] GenerateDirections(int density, float amountOccupied)
	{
		int cutoff = Mathf.CeilToInt(density * Mathf.Clamp(amountOccupied, 0, 1));
		return GenerateDirectionsInternal(density, cutoff);
	}

	private static Vector3[] GenerateDirectionsInternal(int density, int cutoff)
	{
		Vector3[] directions = new Vector3[cutoff];

		float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
		float angleIncrement = Mathf.PI * 2 * goldenRatio;

		for (int i = 0; i < cutoff; i++)
		{
			float t = (float)i / density;
			float inclination = Mathf.Acos(1 - 2 * t);
			float azimuth = angleIncrement * i;

			float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
			float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
			float z = Mathf.Cos(inclination);
			directions[i] = new Vector3(x, y, z);
		}
		return directions;
	}
}