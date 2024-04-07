using GameEngine.Core.Components;
using GameEngine.Core.Components.Objects;

namespace ShellShockers.Client.GameComponents.WorldComponents.Player;

internal class PlayerEggVisual : WorldObject
{
	public PlayerEggVisual()
	{
		// Egg
		Meshes.Add(new MeshData("PlayerEgg.obj", "White.mat"));

		// Shadow
		Meshes.Add(new MeshData("PlayerEggShadow.obj", "Black.mat"));
	}
}