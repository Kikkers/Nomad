using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
	public struct Data
	{
		public Vector3 position;
		public Vector3 direction;

		public Vector3 flockHeading;
		public Vector3 flockCentre;
		public Vector3 avoidanceHeading;
		public int numFlockmates;

		public static int Size => sizeof(float) * 3 * 5 + sizeof(int);
	}

	private Vector3 velocity;

	private BoidConfig config;
	private Transform target;

	public void Initialize(BoidConfig config, Transform target, ref Data boidData)
	{
		this.target = target;
		this.config = config;

		boidData.position = transform.position;
		boidData.direction = transform.forward;

		float startSpeed = (config.minSpeed + config.maxSpeed) / 2;
		velocity = transform.forward * startSpeed;
	}

	public void PostComputeUpdate(ref Data boidData)
	{
		Vector3 acceleration = Vector3.zero;

		// steer towards a target
		if (target != null)
		{
			Vector3 offsetToTarget = (target.position - boidData.position);
			acceleration = SteerTowards(offsetToTarget) * config.targetWeight;
		}

		// steer along flock
		if (boidData.numFlockmates != 0)
		{
			Vector3 centreOfFlockmates = boidData.flockCentre / boidData.numFlockmates;

			Vector3 offsetToFlockmatesCentre = centreOfFlockmates - boidData.position;
			
			var alignmentForce = SteerTowards(boidData.flockHeading) * config.alignWeight;
			var cohesionForce = SteerTowards(offsetToFlockmatesCentre) * config.cohesionWeight;
			var seperationForce = SteerTowards(boidData.avoidanceHeading) * config.seperateWeight;

			acceleration += alignmentForce;
			acceleration += cohesionForce;
			acceleration += seperationForce;
		}

		if (IsHeadingForCollision(ref boidData))
		{
			Vector3 collisionAvoidDir = ObstacleRays(ref boidData);
			Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * config.avoidCollisionWeight;
			acceleration += collisionAvoidForce;
		}

		velocity += acceleration * Time.deltaTime;
		float speed = velocity.magnitude;
		Vector3 dir = velocity / speed;
		speed = Mathf.Clamp(speed, config.minSpeed, config.maxSpeed);
		velocity = dir * speed;

		transform.position += velocity * Time.deltaTime;
		transform.forward = dir;
		boidData.position = transform.position;
		boidData.direction = dir;
	}

	private bool IsHeadingForCollision(ref Data boidData)
	{
		RaycastHit hit;
		if (Physics.SphereCast(boidData.position, config.boundsRadius, boidData.direction, out hit, config.collisionAvoidDst, config.obstacleMask))
		{
			return true;
		}
		else { }
		return false;
	}

	private Vector3 ObstacleRays(ref Data boidData)
	{
		IReadOnlyList<Vector3> directions = config.HitTestDirections;

		foreach(Vector3 direction in directions)
		{
			Vector3 dir = transform.TransformDirection(direction);
			Ray ray = new Ray(boidData.position, dir);
			if (!Physics.SphereCast(ray, config.boundsRadius, config.collisionAvoidDst, config.obstacleMask))
			{
				return dir;
			}
		}

		return boidData.direction;
	}

	Vector3 SteerTowards(Vector3 vector)
	{
		Vector3 v = vector.normalized * config.maxSpeed - velocity;
		return Vector3.ClampMagnitude(v, config.maxSteerForce);
	}

}
