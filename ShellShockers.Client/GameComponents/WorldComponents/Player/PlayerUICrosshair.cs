using GameEngine.Core.Components;
using GameEngine.Core.Components.Objects;
using ShellShockers.Client.GameComponents.WorldComponents.Weapons;

namespace ShellShockers.Client.GameComponents.WorldComponents.Player;

internal class PlayerUICrosshair : UIObject
{
    public PlayerUICrosshair()
    {
        Meshes.Add(new MeshData("UIRect.obj", $"{Weapon.Hands}Crosshair.mat"));
        Transform.Scale /= 5;
    }

    public void SwitchToWeaponCrosshair(Weapon newWeapon)
	{
		Meshes[0] = new MeshData("UIRect.obj", $"{newWeapon}Crosshair.mat");
	}
}