namespace ShellShockers.Core.Utilities.Networking.CommunicationProtocols;

public enum MessageType
{
	None = 0,
	Invalid,

	Disconnect,

	// Client server login register
	LoginRequest, LoginReponse,
	RegisterRequest, RegisterReponse,
	TwoFARequest, TwoFAResponse,
	ForgotPasswordRequest, ForgotPasswordReponse,

	DoSResponse,

	LobbiesFetchRequest, LobbiesFetchReponse,
	CreateLobbyRequest, CreateLobbyResponse,
	JoinLobbyRequest, JoinLobbyResponse,
	NotARobotRequest, NotARobotResponse,
}