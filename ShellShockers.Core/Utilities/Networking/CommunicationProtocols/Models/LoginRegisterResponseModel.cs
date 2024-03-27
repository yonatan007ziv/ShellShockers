using ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Enums;

namespace ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;

public class LoginRegisterResponseModel
{
	public LoginRegisterResponse Status { get; set; }
	public string AuthenticationToken { get; set; } = null!;
}