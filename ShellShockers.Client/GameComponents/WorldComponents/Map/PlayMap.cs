using GameEngine.Core.Components.Objects;

namespace ShellShockers.Client.GameComponents.WorldComponents.Map;

internal class PlayMap : WorldObject
{
    public PlayMap()
    {
        Children.Add(new Ground(50));

        Transform.Position = System.Numerics.Vector3.Zero;
        Transform.Scale = System.Numerics.Vector3.One;
	}
}