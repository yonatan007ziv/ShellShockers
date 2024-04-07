using GameEngine.Core.Components.Objects;
using ShellShockers.Client.Components;
using ShellShockers.Client.Components.Networking;
using ShellShockers.Client.Scenes;
using ShellShockers.Core.Utilities.Models;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;
using System.Net;

namespace ShellShockers.Client.GameComponents.UIComponents.Views.MainMenu.LobbySelection;

internal class LobbiesWindowViewControl : UIObject
{
	private readonly List<LobbySelectionEntry> lobbyEntries = new List<LobbySelectionEntry>();

	public LobbiesWindowViewControl()
	{
		Meshes.Add(new GameEngine.Core.Components.MeshData("UIRect.obj", "Gray.mat"));

		_ = PopulateLobbies();
	}

	private async Task PopulateLobbies()
	{
		if (!Factories.ClientFactory.Create(out TcpClientHandler clientHandler)
			|| !await clientHandler.Connect(IPAddress.Parse(ServerAddresses.GameplayServerAddress), ServerAddresses.GameplayServerPort))
			return;

		await clientHandler.WriteMessage(new MessagePacket<GameplayRequestModel>(MessageType.LobbiesFetchRequest, new GameplayRequestModel(SessionHolder.AuthenticationToken)));
		GameplayResponseModel? responseModel = (await clientHandler.ReadMessage<GameplayResponseModel>()).Payload;
		if (responseModel is null)
			return;

		// Delete previous lobbies
		int k = 0;
		for (int i = 0; i < Children.Count; i++)
			if (Children[i] == lobbyEntries[k])
			{
				Children.RemoveAt(i--);
				k++;
			}

		float yScale = 1f / responseModel.LobbiesListArray!.Length;
		float yOffset = 1;
		foreach (LobbyModel lobby in responseModel.LobbiesListArray)
		{
			LobbySelectionEntry lobbyEntry = new LobbySelectionEntry(lobby);
			lobbyEntry.TextColor = System.Drawing.Color.White;
			lobbyEntry.Transform.Position = new System.Numerics.Vector3(0, yOffset - yScale, 1);
			lobbyEntry.Transform.Scale = new System.Numerics.Vector3(1, yScale, 1);
			lobbyEntry.OnFullClicked += () => OnJoinLobby(lobbyEntry.LobbyId);

			lobbyEntries.Add(lobbyEntry);
			Children.Add(lobbyEntry);

			yOffset -= yScale * 2;
		}

		clientHandler.Disconnect();
	}

	private async void OnJoinLobby(int lobbyId)
	{
		Console.WriteLine($"Joining lobby: {lobbyId}");

		foreach (LobbySelectionEntry lobbyEntry in lobbyEntries)
			lobbyEntry.Enabled = false;

		await JoinLobbyProcedure(lobbyId);

		foreach (LobbySelectionEntry lobbyEntry in lobbyEntries)
			lobbyEntry.Enabled = true;
	}

	private async Task JoinLobbyProcedure(int lobbyId)
	{
		if (!Factories.ClientFactory.Create(out TcpClientHandler client)
			|| !await client.Connect(IPAddress.Parse(ServerAddresses.GameplayServerAddress), ServerAddresses.GameplayServerPort))
			return;

		await client.WriteMessage(new MessagePacket<GameplayRequestModel>(MessageType.JoinLobbyRequest, new GameplayRequestModel(SessionHolder.AuthenticationToken) { JoinLobbyId = lobbyId }));
		MessagePacket<GameplayResponseModel> response = await client.ReadMessage<GameplayResponseModel>();

		if (response.Payload!.SuccessJoiningLobby.HasValue && response.Payload!.SuccessJoiningLobby.Value)
			new ShootingScene(client).LoadScene();
		else
			client.Disconnect();
	}
}