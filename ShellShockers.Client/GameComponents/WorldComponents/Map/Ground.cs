using GameEngine.Core.Components;
using GameEngine.Core.Components.Objects;
using System.Numerics;

namespace ShellShockers.Client.GameComponents.WorldComponents.Map;

internal class Ground : WorldObject
{
	public Ground(float groundSize)
	{
		Vector2 size = Vector2.One * groundSize;
		Meshes.Add(new MeshData("Cube.obj", "Ground.mat"));
		Transform.Scale = new Vector3(size.X, 1, size.Y);
		BoxCollider = new BoxCollider(true, new Vector3(-size.X, -1, -size.Y), new Vector3(size.X, 1, size.Y));
	}
}