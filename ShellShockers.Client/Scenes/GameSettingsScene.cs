using GameEngine.Components;
using GameEngine.Components.UIComponents;

namespace ShellShockers.Client.Scenes;

internal class GameSettingsScene : Scene
{
	private readonly UIButton backButton;

	public GameSettingsScene()
	{
		UICameras.Add((new UICamera(), new GameEngine.Core.Components.ViewPort(0.5f, 0.5f, 1, 1)));

		// Back button
		backButton = new UIButton();
		backButton.Text = "Back to Main Menu";
		backButton.TextColor = System.Drawing.Color.White;
		backButton.Transform.Scale /= 5;
		backButton.Transform.Position = new System.Numerics.Vector3(-0.75f, -0.75f, 0);
		backButton.OnFullClicked += new MainMenuScene().LoadScene;
		UIObjects.Add(backButton);
	}
}