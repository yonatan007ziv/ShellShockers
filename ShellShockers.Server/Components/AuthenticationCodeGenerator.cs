using System.Text;

namespace ShellShockers.Server.Components;

internal class AuthenticationCodeGenerator
{
	private static readonly Random random = new Random();

	public static string GenerateAuthenticationCode()
	{
		const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
		StringBuilder res = new StringBuilder();
		for (int i = 0; i < 15; i++)
			res.Append(valid[random.Next(valid.Length)]);
		return res.ToString();
	}
}