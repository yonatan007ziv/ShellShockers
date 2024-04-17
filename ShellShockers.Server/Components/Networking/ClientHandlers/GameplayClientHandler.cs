using ShellShockers.Core.Utilities.Models;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;
using ShellShockers.Server.Components.Lobby;

namespace ShellShockers.Server.Components.Networking.ClientHandlers;

internal class GameplayClientHandler : BaseClientHandler
{
	private LobbyHandler? joinedLobby;

	public override async void StartRead()
	{
		OnDisconnect += NotifyLobbyDisconnected;

		while (Connected)
		{
			MessagePacket<GameplayRequestModel> message = await TcpClientHandler.ReadMessage<GameplayRequestModel>();

			if (!Connected)
				return;

			if (message.Type == MessageType.Disconnect || message.Type == MessageType.Invalid || message.Type == MessageType.None)
			{
				Disconnect();
				return;
			}

			if (joinedLobby is not null)
				await joinedLobby.InterpretMessage(message); // Authenticated client
			else
				InterpretMessage(message);
		}
	}

	private void NotifyLobbyDisconnected()
	{
		joinedLobby?.RemovePlayer(this);
	}

	public void InterpretMessage(MessagePacket<GameplayRequestModel> message)
	{
		if (!ClientAuthenticator.CheckAuthenticationToken(message.Payload!.AuthenticationToken))
			return;

		GameplayRequestModel requestModel = message.Payload!;
		if (message.Type == MessageType.LobbiesFetchRequest)
			LobbiesFetchRequest();
		else if (message.Type == MessageType.JoinLobbyRequest)
		{
			if (message.Payload.JoinLobbyId.HasValue)
				JoinLobbyRequest(message.Payload.JoinLobbyId.Value);
		}
		else if (message.Type == MessageType.CreateLobbyRequest)
			CreateLobbyRequest(message.Payload.CreateLobbyModel!);
	}

	private void CreateLobbyRequest(LobbyModel createLobbyModel)
	{
        Console.WriteLine($"CREATING LOBBY: {createLobbyModel}");

		bool success = LobbyManager.CreateLobby(createLobbyModel, this);
		_ = TcpClientHandler.WriteMessage(new MessagePacket<GameplayResponseModel>(MessageType.CreateLobbyResponse, new GameplayResponseModel() { SuccessCreatingLobby = success }));
	}

	private void LobbiesFetchRequest()
	{
		LobbyModel[] lobbies = LobbyManager.GetLobbyModels();
		GameplayResponseModel responseModel = new GameplayResponseModel() { LobbiesListArray = lobbies };
		MessagePacket<GameplayResponseModel> message = new MessagePacket<GameplayResponseModel>(MessageType.LobbiesFetchReponse, responseModel);
		_ = TcpClientHandler.WriteMessage(message);
	}

	private async void JoinLobbyRequest(int lobbyId)
	{
		MessagePacket<GameplayResponseModel> message = new MessagePacket<GameplayResponseModel>(MessageType.JoinLobbyResponse, new GameplayResponseModel());
		message.Payload!.SuccessJoiningLobby = LobbyManager.AddPlayerToLobby(this, lobbyId);
		await TcpClientHandler.WriteMessage(message);
	}
}