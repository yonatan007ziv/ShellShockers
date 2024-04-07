

namespace ShellShockers.Server.Components.Networking;

internal class ClientAuthenticator
{
	// Authentication Token -> Username
	private static readonly Dictionary<string, string> authenticationTokens = new Dictionary<string, string>();

	public static void AddAuthenticationKey(string authenticationToken, string username)
	{
		authenticationTokens.Add(authenticationToken, username);
	}

	public static bool CheckAuthenticationToken(string authenticationToken)
	{
		return authenticationTokens.ContainsKey(authenticationToken);
	}

	public static string GenerateAuthenticationToken()
	{
		string code = AuthenticationCodeGenerator.GenerateAuthenticationCode();
		if (authenticationTokens.ContainsKey(code))
			return GenerateAuthenticationToken();
		return code;
	}
}