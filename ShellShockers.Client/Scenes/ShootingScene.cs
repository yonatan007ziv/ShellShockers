using GameEngine.Components;
using GameEngine.Core.Components;
using GameEngine.Core.Components.Input.Buttons;
using GameEngine.Core.Components.Objects;
using ShellShockers.Client.GameComponents.WorldComponents.Map;
using ShellShockers.Client.GameComponents.WorldComponents.Player;
using System.Numerics;

namespace ShellShockers.Client.Scenes;

internal class ShootingScene : Scene
{
    private readonly RotatedCamera rotatingCamera;
	private bool rotateCamera;

	public ShootingScene()
	{
		// Input
		MapKeyboardAxis("HorizontalMovement", KeyboardButton.D, KeyboardButton.A, 1, 0);
		MapKeyboardAxis("VerticalMovement", KeyboardButton.W, KeyboardButton.S, 1, 0);
		MapMouseAxis("HorizontalCamera", MouseAxis.MouseHorizontal, 1, 0);
		MapMouseAxis("VerticalCamera", MouseAxis.MouseVertical, 1, 0);
		
		// Scene starts with rotation camera around map
		rotatingCamera = new RotatedCamera(Vector3.Zero, 150, 20, 0.2f);

		WorldCameras.Add((rotatingCamera, new ViewPort(0.5f, 0.25f, 1, 0.5f)));

		// Player egg
		Player player = new Player(true);
		WorldObjects.Add(player);

		// Map
		WorldObjects.Add(new PlayMap());
	}
}