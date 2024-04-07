using GameEngine.Components;
using GameEngine.Core.Components;
using GameEngine.Extensions;
using System;
using System.Linq;
using System.Numerics;

namespace ShellShockers.Client.GameComponents.WorldComponents.Player;

internal class RotatedCamera : WorldCamera
{
	private readonly Vector3 rotateAroundPoint;
	private readonly float rotationRadius;
	private readonly float cameraHeight;
	private float rotationSpeed;
	private float t = 0;

	public RotatedCamera(Vector3 rotateAroundPoint, float rotationRadius, float cameraHeight, float rotationSpeed)
	{
		this.rotateAroundPoint = rotateAroundPoint;
		this.rotationRadius = 1;
		this.cameraHeight = 1;
		this.rotationSpeed = rotationSpeed;
	}

	public override void Update(float deltaTime)
	{
		// Transform.Position = new Vector3(-(float)Math.Sin(t * rotationSpeed) * rotationRadius, cameraHeight, -(float)Math.Cos(t * rotationSpeed) * rotationRadius);
		Transform.Position = new Vector3(0, 0.1f, -2);
		t += deltaTime;

		LookAt(rotateAroundPoint);
	}

	public void LookAt(Vector3 targetPosition)
	{
		// Calculate the direction vector from camera to target
		Vector3 direction = targetPosition - Transform.Position;

		// Calculate yaw angle (rotation around Y-axis)
		float yaw = (float)Math.Atan2(direction.Z, direction.X);

		// Calculate pitch angle (rotation around X-axis)
		float pitch = (float)Math.Asin(-direction.Y / direction.Length());

		// Set the rotation angles to the camera's Transform
		//Transform.Rotation = new Vector3(pitch, yaw, 0f) * (float)(Math.PI * 10);
	}
}