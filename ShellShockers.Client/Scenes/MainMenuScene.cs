using GameEngine.Components;
using GameEngine.Components.UIComponents;
using GameEngine.Core.Components;
using System.Numerics;

namespace ShellShockers.Client.Scenes;

internal class MainMenuScene : Scene
{
	private UIButton _switchToLobbySelection;
	private UIButton _switchToGameSettings;

	public MainMenuScene()
	{
		Vector3 buttonScale = new Vector3(0.25f, 0.25f, 0);

		UICameras.Add((new UICamera(), new ViewPort(0.5f, 0.5f, 1, 1)));

		_switchToLobbySelection = new UIButton();
		_switchToLobbySelection.Text = "Lobby Selection";
		_switchToLobbySelection.TextColor = System.Drawing.Color.White;
		_switchToLobbySelection.Transform.Scale = buttonScale;
		_switchToLobbySelection.Transform.Position = new Vector3(-0.5f, 0, 0);
		_switchToLobbySelection.OnFullClicked += SwitchToLobbySelectionScene;
		UIObjects.Add(_switchToLobbySelection);

		_switchToGameSettings = new UIButton();
		_switchToGameSettings.Text = "Game Settings";
		_switchToGameSettings.TextColor = System.Drawing.Color.White;
		_switchToGameSettings.Transform.Scale = buttonScale;
		_switchToGameSettings.Transform.Position = new Vector3(0.5f, 0, 0);
		_switchToGameSettings.OnFullClicked += SwitchToGameSettingsScene;
		UIObjects.Add(_switchToGameSettings);
	}

	private void SwitchToLobbySelectionScene()
	{
		new LobbySelectionScene().LoadScene();
	}

	private void SwitchToGameSettingsScene()
	{
		new GameSettingsScene().LoadScene();
	}
}