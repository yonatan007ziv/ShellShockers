namespace ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;

public class LoginRequestModel
{
	public string Username { get; set; } = default!;
	public string Password { get; set; } = default!;
}