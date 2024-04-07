using GameEngine.Core.Components;
using GameEngine.Core.Components.Objects;
using ShellShockers.Client.GameComponents.WorldComponents.Weapons;

namespace ShellShockers.Client.GameComponents.WorldComponents.Player;

internal class PlayerHandsVisual : WorldObject
{
	private MeshData weaponMeshData;

	public PlayerHandsVisual()
	{
		// Hands
		Meshes.Add(new MeshData("PlayerEggHand.obj", "Red.mat"));
	}

	public void SwitchWeaponVisual(Weapon weapon)
	{
		Meshes.Remove(weaponMeshData);

		if (weapon == Weapon.None)
			return;

		weaponMeshData = new MeshData($"{weapon}.obj", "Blue.mat");
		Meshes.Add(weaponMeshData);
	}
}