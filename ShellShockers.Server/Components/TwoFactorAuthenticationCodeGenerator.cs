using System.Text;

namespace ShellShockers.Server.Components;

internal static class TwoFactorAuthenticationCodeGenerator
{
	private static readonly Random random = new Random();

	public static string Generate2FACode()
	{
		const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
		StringBuilder res = new StringBuilder();
		for (int i = 0; i < 5; i++)
			res.Append(valid[random.Next(valid.Length)]);
		return res.ToString();
	}
}