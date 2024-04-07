using ShellShockers.Core.Utilities.Models;

namespace ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;

public class GameplayRequestModel
{
	public string AuthenticationToken { get; }
	public int? JoinLobbyId { get; set; } = null;
	public LobbyModel? CreateLobbyModel { get; set; } = null;

	public GameplayRequestModel(string authenticationToken)
	{
		AuthenticationToken = authenticationToken;
	}
}