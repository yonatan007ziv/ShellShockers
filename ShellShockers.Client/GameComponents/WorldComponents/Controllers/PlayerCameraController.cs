using GameEngine.Components.ScriptableObjects;
using System.Numerics;

namespace ShellShockers.Client.GameComponents.WorldComponents.Controllers;

internal class PlayerCameraController : ScriptableWorldObject
{
	private float clampAngleX = 80;

	public float Sensitivity { get; set; } = 20;

	public PlayerCameraController(/* Axis Data ,*/)
	{

	}

	public override void Update(float deltaTime)
	{
		if (MouseLocked && Parent is not null)
		{
			Vector2 mouseVector = new Vector2(GetAxis("HorizontalCamera"), GetAxis("VerticalCamera"));

			// Clamp camera
			float clampedX = Math.Clamp(Parent.Transform.Rotation.X - mouseVector.Y * deltaTime * Sensitivity, -clampAngleX, clampAngleX);
			Parent.Transform.Rotation = new Vector3(clampedX, Parent.Transform.Rotation.Y + mouseVector.X * deltaTime * Sensitivity, 0);
		}
	}
}