using GameEngine.Components;
using GameEngine.Components.ScriptableObjects;
using GameEngine.Core.Components.Input.Buttons;
using ShellShockers.Client.GameComponents.WorldComponents.Controllers;
using ShellShockers.Client.GameComponents.WorldComponents.Weapons;
using System.Numerics;

namespace ShellShockers.Client.GameComponents.WorldComponents.Player;

internal class Player : ScriptableWorldObject
{
	private readonly bool localPlayer;

	private readonly WorldCamera playerCamera;
	private readonly PlayerCameraController playerCameraController;
	private readonly PlayerMovementController playerMovementController;
	private readonly PlayerEggVisual eggVisual;
	private readonly PlayerHandsVisual handsVisual;

	public Player(/* PlayerControlSet */ bool localPlayer)
	{
		MouseLocked = true;

		this.localPlayer = localPlayer;

		// Egg visual
		Children.Add(eggVisual = new PlayerEggVisual());
		eggVisual.UseRelativeRotation = false;

		// Hands visual
		Children.Add(handsVisual = new PlayerHandsVisual());

		if (localPlayer)
		{
			Children.Add(playerCamera = new WorldCamera());
			Children.Add(playerCameraController = new PlayerCameraController());
			Children.Add(playerMovementController = new PlayerMovementController());

			eggVisual.Tag = "LocalPlayerEggVisual";
			playerCamera.RenderingMaskTags.AddMask("LocalPlayerEggVisual");

			WorldCameras.Add((playerCamera, new GameEngine.Core.Components.ViewPort(0.5f, 0.75f, 1, 0.5f)));
		}
	}

	public override void Update(float deltaTime)
	{
		if (!localPlayer)
			return;

		playerCamera.Transform.Position = Transform.Position + new Vector3(0, 0.05f, 0);
		playerCamera.Transform.Rotation = Transform.Rotation;
		eggVisual.Transform.Rotation = Vector3.UnitY * Transform.Rotation.Y;

		if (GetKeyboardButtonDown(KeyboardButton.One))
			SwitchToWeapon(Weapon.None);
		else if (GetKeyboardButtonDown(KeyboardButton.Two))
			SwitchToWeapon(Weapon.Pistol);
		else if (GetKeyboardButtonDown(KeyboardButton.Three))
			SwitchToWeapon(Weapon.Rifle);
		else if (GetKeyboardButtonDown(KeyboardButton.Four))
			SwitchToWeapon(Weapon.Sniper);
	}

	public void SwitchToWeapon(Weapon weapon)
	{
		handsVisual.SwitchWeaponVisual(weapon);
	}
}