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

	public static bool LobbyIdExists(int id)
		=> lobbies.ContainsKey(id);

	public static bool AddPlayerToLobby(GameplayClientHandler player, int lobbyId)
	{
		if (!lobbies.TryGetValue(lobbyId, out LobbyHandler? value) || value.LobbyFull())
			return false;

		value.AddPlayer(player);
		return true;
	}

	public static bool CreateLobby(LobbyModel createLobbyModel, GameplayClientHandler host)
	{
		if (LobbyIdExists(createLobbyModel.Id))
			return false;

		lobbies.Add(createLobbyModel.Id, new LobbyHandler(createLobbyModel, host));
		return true;
	}
}