namespace ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;

public class GameplayRequestModel
{
	public string AuthenticationToken { get; }
	public int JoinLobbyId { get; set; }

	public GameplayRequestModel(string authenticationToken)
	{
		AuthenticationToken = authenticationToken;
	}
}