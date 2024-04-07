namespace ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Enums;

public enum LoginRegisterResponse
{
	None = 0,
	Success,
	UnknownError,

	// Login responses
	UsernameDoesNotExist,
	WrongPassword,
	EmailNotConfirmed,

	// Register responses
	UsernameExists,
	InvalidUsername,
	InvalidPassword,
	InvalidEmail,
	EmailInUse,
	TwoFactorAuthenticationSent,

	// Forgot password responses
	EmailDoesNotExist,

	// Two FA
	Wrong2FACode,
	TwoFACodeExpired,

}