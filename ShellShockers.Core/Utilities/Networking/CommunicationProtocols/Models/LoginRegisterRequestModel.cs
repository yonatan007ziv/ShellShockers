namespace ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;

public class LoginRegisterRequestModel
{
	// Login fields
	public string Username { get; set; } = default!;
	public string Password { get; set; } = default!;

	// Register fields
	public string Email { get; set; } = default!;
	public string TwoFAToken { get; set; } = default!;
}