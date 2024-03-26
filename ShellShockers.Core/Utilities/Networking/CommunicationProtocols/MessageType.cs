namespace ShellShockers.Core.Utilities.Networking.CommunicationProtocols;

public enum MessageType
{
	None = 0,
	Invalid,

	Disconnect,

	// Client server login register
	LoginRequest, LoginReponse,
	RegisterRequest, RegisterReponse, TwoFACode

}