using GameEngine.Components;
using GameEngine.Components.UIComponents;
using GameEngine.Core.Components;
using ShellShockers.Core.Utilities.Models;

namespace ShellShockers.Client.Scenes;

internal class CreateLobbyScene : Scene
{
	private UITextBox lobbyNameTextBox;
	private UITextBox maxPlayerCountTextBox;

	public CreateLobbyScene()
	{
		UICameras.Add((new UICamera(), new ViewPort(0.5f, 0.5f, 1, 1)));

		UILabel lobbyNameLabel = new UILabel();
		lobbyNameLabel.Text = "Lobby Name:";
		lobbyNameLabel.TextColor = System.Drawing.Color.White;
		lobbyNameLabel.Transform.Scale /= 5;
		lobbyNameLabel.Transform.Position = new System.Numerics.Vector3(-0.5f, 0.75f, 0);
		UIObjects.Add(lobbyNameLabel);

		UILabel maxPlayerCountLabel = new UILabel();
		maxPlayerCountLabel.Text = "Max Player Count:";
		maxPlayerCountLabel.TextColor = System.Drawing.Color.White;
		maxPlayerCountLabel.Transform.Scale /= 5;
		maxPlayerCountLabel.Transform.Position = new System.Numerics.Vector3(-0.5f, 0.25f, 0);
		UIObjects.Add(lobbyNameLabel);

		lobbyNameTextBox = new UITextBox();
		lobbyNameTextBox.Text = "Lobby Name:";
		lobbyNameTextBox.TextColor = System.Drawing.Color.White;
		lobbyNameTextBox.Transform.Scale /= 5;
		lobbyNameTextBox.Transform.Position = new System.Numerics.Vector3(0.5f, 0.75f, 0);
		UIObjects.Add(lobbyNameTextBox);

		maxPlayerCountTextBox = new UITextBox();
		maxPlayerCountTextBox.Text = "Max Player Count:";
		maxPlayerCountTextBox.TextColor = System.Drawing.Color.White;
		maxPlayerCountTextBox.Transform.Scale /= 5;
		maxPlayerCountTextBox.Transform.Position = new System.Numerics.Vector3(0.5f, 0.25f, 0);
		UIObjects.Add(maxPlayerCountTextBox);

		UIButton createLobbyButton = new UIButton();
		createLobbyButton.Text = "Max Player Count:";
		createLobbyButton.TextColor = System.Drawing.Color.White;
		createLobbyButton.Transform.Scale /= 5;
		createLobbyButton.Transform.Position = new System.Numerics.Vector3(0.5f, 0.25f, 0);
		createLobbyButton.OnFullClicked += OnCreateButton;
		UIObjects.Add(createLobbyButton);

		// Back button
		UIButton backButton = new UIButton();
		backButton.Text = "Back";
		backButton.TextColor = System.Drawing.Color.White;
		backButton.Transform.Scale /= 5;
		backButton.Transform.Position = new System.Numerics.Vector3(-0.75f, -0.75f, 0);
		backButton.OnFullClicked += new MainMenuScene().LoadScene;
		UIObjects.Add(backButton);
	}

	private void OnCreateButton()
	{
		throw new NotImplementedException();
	}
}