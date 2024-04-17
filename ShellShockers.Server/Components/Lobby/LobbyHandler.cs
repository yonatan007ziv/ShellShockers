using Microsoft.IdentityModel.Tokens;
using ShellShockers.Core.Utilities.Models;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;
using ShellShockers.Server.Components.Networking.ClientHandlers;

namespace ShellShockers.Server.Components.Lobby;

internal class LobbyHandler
{
	private readonly List<GameplayClientHandler> players = new List<GameplayClientHandler>();
	private readonly GameplayClientHandler host;

	public readonly LobbyModel lobbyModel;

	public LobbyHandler(LobbyModel lobbyModel, GameplayClientHandler host)
	{
		this.lobbyModel = lobbyModel;
		this.host = host;
	}

	public async Task InterpretMessage(MessagePacket<GameplayRequestModel> message)
	{
		await Console.Out.WriteLineAsync($"Got a lobby message, lobby id: {lobbyModel.Id}");
	}

	public bool LobbyFull()
		=> players.Count >= lobbyModel.MaxPlayerCount;

	public void AddPlayer(GameplayClientHandler player)
	{
		players.Add(player);
		lobbyModel.CurrentPlayerCount++;

		OnPlayerJoin(player);
	}

	public void RemovePlayer(GameplayClientHandler player)
	{
		players.Remove(player);
		lobbyModel.CurrentPlayerCount--;

		OnPlayerLeave(player);
	}

	private void OnPlayerJoin(GameplayClientHandler player)
	{
		// Send new player list to players and stuff, update scoreboard and such
	}

	private void OnPlayerLeave(GameplayClientHandler player)
	{
		// Send new player list to players and stuff, update scoreboard and such
	}
}