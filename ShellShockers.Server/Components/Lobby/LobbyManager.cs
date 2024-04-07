using ShellShockers.Core.Utilities.Models;
using ShellShockers.Server.Components.Networking.ClientHandlers;

namespace ShellShockers.Server.Components.Lobby;

internal class LobbyManager
{
	private static readonly Dictionary<int, LobbyHandler> lobbies = new Dictionary<int, LobbyHandler>();

	public static LobbyModel[] GetLobbyModels()
	{
		LobbyModel[] models = new LobbyModel[lobbies.Count];
		for (int i = 0; i < lobbies.Count; i++)
			models[i] = lobbies[i].lobbyModel;
		return models;
	}

	public static bool AddPlayerToLobby(GameplayClientHandler player, int lobbyId)
	{
		if (!lobbies.ContainsKey(lobbyId) || lobbies[lobbyId].LobbyFull())
			return false;

		lobbies[lobbyId].AddPlayer(player);
		return true;
	}
}