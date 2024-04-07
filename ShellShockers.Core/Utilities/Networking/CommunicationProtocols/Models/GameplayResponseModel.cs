using ShellShockers.Core.Utilities.Models;

namespace ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;

public class GameplayResponseModel
{
	public LobbyModel[]? LobbiesListArray { get; set; } = null;
	public bool? SuccessJoiningLobby { get; set; } = null;
	public bool? SuccessCreatingLobby { get; set; } = null;
}