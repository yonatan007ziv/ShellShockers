using GameEngine.Components;
using GameEngine.Components.UIComponents;
using ShellShockers.Client.GameComponents.UIComponents.Views.MainMenu.LobbySelection;

namespace ShellShockers.Client.Scenes;

internal class LobbySelectionScene : Scene
{
	public LobbySelectionScene()
	{
		UICameras.Add((new UICamera(), new GameEngine.Core.Components.ViewPort(0.5f, 0.5f, 1, 1)));

		// Lobbies window
		LobbiesWindowViewControl _lobbiesWindowViewControl = new LobbiesWindowViewControl();
		_lobbiesWindowViewControl.Transform.Position = new System.Numerics.Vector3(0, 0.2f, 1);
		_lobbiesWindowViewControl.Transform.Scale = new System.Numerics.Vector3(0.7f, 0.7f, 1);
		UIObjects.Add(_lobbiesWindowViewControl);

		// Back button
		UIButton backButton = new UIButton();
		backButton.Text = "Back";
		backButton.TextColor = System.Drawing.Color.White;
		backButton.Transform.Scale /= 5;
		backButton.Transform.Position = new System.Numerics.Vector3(-0.75f, -0.75f, 0);
		backButton.OnFullClicked += new MainMenuScene().LoadScene;
		UIObjects.Add(backButton);

		// Create lobby button
		UIButton createLobbyButton = new UIButton();
		createLobbyButton.Text = "Create Lobby";
		createLobbyButton.TextColor = System.Drawing.Color.White;
		createLobbyButton.Transform.Scale /= 5;
		createLobbyButton.Transform.Position = new System.Numerics.Vector3(0.75f, -0.75f, 0);
		createLobbyButton.OnFullClicked += new CreateLobbyScene().LoadScene;
		UIObjects.Add(createLobbyButton);
	}
}