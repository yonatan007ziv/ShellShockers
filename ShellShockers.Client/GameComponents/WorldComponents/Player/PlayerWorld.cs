using GameEngine.Components;
using GameEngine.Components.ScriptableObjects;
using GameEngine.Core.Components.Input.Buttons;
using GameEngine.Core.Components.Objects;
using ShellShockers.Client.GameComponents.WorldComponents.Controllers;
using ShellShockers.Client.GameComponents.WorldComponents.Weapons;
using System.Numerics;

namespace ShellShockers.Client.GameComponents.WorldComponents.Player;

internal class PlayerWorld : ScriptableWorldObject
{
	private readonly bool localPlayer;

	public readonly WorldCamera? playerWorldCamera;
	private readonly PlayerCameraController? playerCameraController;
	private readonly PlayerMovementController? playerMovementController;
	private readonly PlayerEggVisual eggVisual;
	private readonly PlayerHandsVisual handsVisual;

	public PlayerWorld(bool localPlayer)
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
			Children.Add(playerWorldCamera = new WorldCamera());

			Children.Add(playerCameraController = new PlayerCameraController());
			Children.Add(playerMovementController = new PlayerMovementController());

			eggVisual.Tag = "LocalPlayerEggVisual";
			eggVisual.UseRelativeRotation = false;

			playerWorldCamera.RenderingMaskTags.AddMask("LocalPlayerEggVisual");
		}
	}

	public override void Update(float deltaTime)
	{
		if (!localPlayer)
			return;

		playerWorldCamera!.Transform.Position = Transform.Position + GameEngine.Core.Components.Transform.GlobalUp * Transform.Scale.Y * 0.2f;
		playerWorldCamera!.Transform.Rotation = Transform.Rotation;
		eggVisual.Transform.Rotation = Vector3.UnitY * Transform.Rotation.Y;

		if (GetKeyboardButtonDown(KeyboardButton.One))
			SwitchToWeapon(Weapon.Hands);
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