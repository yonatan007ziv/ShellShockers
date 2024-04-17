using GameEngine.Components;
using GameEngine.Core.Components;
using GameEngine.Core.Components.Input.Buttons;
using ShellShockers.Client.Components.Networking;
using ShellShockers.Client.GameComponents.WorldComponents.Map;
using ShellShockers.Client.GameComponents.WorldComponents.Player;
using System.Numerics;

namespace ShellShockers.Client.Scenes;

internal class ShootingScene : Scene
{
	private readonly TcpClientHandler playerClient;
	private readonly RotatedCamera rotatingCamera;
	private bool rotateCamera;

	public ShootingScene(TcpClientHandler playerClient)
	{
		// Input
		MapKeyboardAxis("HorizontalMovement", KeyboardButton.D, KeyboardButton.A, -1, 0);
		MapKeyboardAxis("VerticalMovement", KeyboardButton.W, KeyboardButton.S, 1, 0);
		MapMouseAxis("HorizontalCamera", MouseAxis.MouseHorizontal, -1, 0);
		MapMouseAxis("VerticalCamera", MouseAxis.MouseVertical, -1, 0);

		// Scene starts with rotation camera around map
		// rotatingCamera = new RotatedCamera(Vector3.Zero, 150, 20, 0.2f);
		// WorldCameras.Add((rotatingCamera, new ViewPort(0.5f, 0.25f, 1, 0.5f)));

		// Player egg
		Player player = new Player(true);
		WorldObjects.Add(player.playerWorld);
		WorldCameras.Add((player.playerWorld.playerWorldCamera!, new ViewPort(0.5f, 0.5f, 1, 1)));
		UIObjects.Add(player.playerUI);
		UICameras.Add((player.playerUI.playerUICamera!, new ViewPort(0.5f, 0.5f, 1, 1)));

		// Map
		WorldObjects.Add(new PlayMap());
		this.playerClient = playerClient;
	}
}