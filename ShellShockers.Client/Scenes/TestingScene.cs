using GameEngine.Components;
using ShellShockers.Client.GameComponents.WorldComponents;

namespace ShellShockers.Client.Scenes;

internal class TestingScene : Scene
{
	public TestingScene()
	{
		WorldCameras.Add((new WorldCamera(), new GameEngine.Core.Components.ViewPort(0.5f, 0.5f, 1, 1)));

		Player player = new Player();
		WorldObjects.Add(player);
	}
}