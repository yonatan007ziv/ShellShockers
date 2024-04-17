using GameEngine.Components;
using GameEngine.Components.UIComponents;
using GameEngine.Core.Components;
using ShellShockers.Client.Components;
using ShellShockers.Client.Components.Networking;
using ShellShockers.Core.Utilities.Models;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;
using System.Net;

namespace ShellShockers.Client.Scenes;

internal class CreateLobbyScene : Scene
{
	private UITextBox lobbyNameTextBox;
	private UITextBox maxPlayerCountTextBox;
	private UIButton createLobbyButton;

	public CreateLobbyScene()
	{
		UICameras.Add((new UICamera(), new ViewPort(0.5f, 0.5f, 1, 1)));

		UILabel lobbyNameLabel = new UILabel();
		lobbyNameLabel.Text = "Lobby Name:";
		lobbyNameLabel.TextColor = System.Drawing.Color.White;
		lobbyNameLabel.Transform.Scale /= 5;
		lobbyNameLabel.Transform.Position = new System.Numerics.Vector3(-0.25f, 0.75f, 0);
		UIObjects.Add(lobbyNameLabel);

		UILabel maxPlayerCountLabel = new UILabel();
		maxPlayerCountLabel.Text = "Max Player Count:";
		maxPlayerCountLabel.TextColor = System.Drawing.Color.White;
		maxPlayerCountLabel.Transform.Scale /= 5;
		maxPlayerCountLabel.Transform.Position = new System.Numerics.Vector3(-0.25f, 0.25f, 0);
		UIObjects.Add(maxPlayerCountLabel);

		lobbyNameTextBox = new UITextBox();
		lobbyNameTextBox.TextColor = System.Drawing.Color.White;
		lobbyNameTextBox.Transform.Scale /= 5;
		lobbyNameTextBox.Transform.Position = new System.Numerics.Vector3(0.25f, 0.75f, 0);
		UIObjects.Add(lobbyNameTextBox);

		maxPlayerCountTextBox = new UITextBox();
		maxPlayerCountTextBox.TextColor = System.Drawing.Color.White;
		maxPlayerCountTextBox.Transform.Scale /= 5;
		maxPlayerCountTextBox.Transform.Position = new System.Numerics.Vector3(0.25f, 0.25f, 0);
		UIObjects.Add(maxPlayerCountTextBox);

		createLobbyButton = new UIButton();
		createLobbyButton.Text = "Create Lobby";
		createLobbyButton.TextColor = System.Drawing.Color.White;
		createLobbyButton.Transform.Scale /= 5;
		createLobbyButton.Transform.Position = new System.Numerics.Vector3(0, -0.2f, 0);
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

	private async void OnCreateButton()
	{
		createLobbyButton.Enabled = false;
		await CreateLobbyProcedure();
		createLobbyButton.Enabled = true;
	}

	private async Task CreateLobbyProcedure()
	{
		if (!Factories.ClientFactory.Create(out TcpClientHandler client)
			|| !int.TryParse(maxPlayerCountTextBox.Text, out int maxPlayersCount)
			|| !await client.Connect(IPAddress.Parse(ServerAddresses.GameplayServerAddress), ServerAddresses.GameplayServerPort))
			return;

		MessagePacket<GameplayRequestModel> message = new MessagePacket<GameplayRequestModel>(MessageType.CreateLobbyRequest, new GameplayRequestModel(SessionHolder.AuthenticationToken));
		message.Payload!.CreateLobbyModel = new LobbyModel() { HostName = SessionHolder.Username, Name = lobbyNameTextBox.Text, MaxPlayerCount = maxPlayersCount };
		await client.WriteMessage(message);

		MessagePacket<GameplayResponseModel> response = await client.ReadMessage<GameplayResponseModel>();

		if (response.Payload!.SuccessCreatingLobby!.Value)
			new ShootingScene(client).LoadScene();
		else
			client.Disconnect();
	}
}