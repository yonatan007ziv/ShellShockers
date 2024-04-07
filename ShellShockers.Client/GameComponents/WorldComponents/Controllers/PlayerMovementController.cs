using GameEngine.Components.ScriptableObjects;
using GameEngine.Core.Components.Input.Buttons;
using GameEngine.Extensions;
using System.Numerics;

namespace ShellShockers.Client.GameComponents.WorldComponents.Controllers;

internal class PlayerMovementController : ScriptableWorldObject
{
	public PlayerMovementController(/* Controls Data ,*/)
	{

	}

	public override void Update(float deltaTime)
	{
		if (Parent is null)
			return;

		float horizontalMovement = GetAxis("HorizontalMovement");
		float verticalMovement = GetAxis("VerticalMovement");

		Parent.Transform.Position += new Vector3(horizontalMovement, 0, verticalMovement).RotateVectorByAxis(Vector3.UnitY, Parent.Transform.Rotation.Y) * deltaTime;
	}
}