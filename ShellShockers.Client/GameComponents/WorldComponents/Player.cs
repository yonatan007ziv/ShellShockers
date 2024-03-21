using GameEngine.Core.Components.Objects;

namespace ShellShockers.Client.GameComponents.WorldComponents;

internal class Player : WorldObject
{
	public Player(/* PlayerControlSet */)
	{
		// Add components
		// Such as controllers

		// Egg
		Meshes.Add(new GameEngine.Core.Components.MeshData("PlayerEgg.obj", "Red.mat"));
	}
}