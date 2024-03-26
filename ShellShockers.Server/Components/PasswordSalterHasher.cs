using System.Security.Cryptography;
using System.Text;

namespace ShellShockers.Server.Components;

internal static class PasswordHasherSalter
{
	private static readonly MD5 hasher = MD5.Create();
	private static readonly Random random = new Random();
	private static readonly int saltLength;

	static PasswordHasherSalter()
	{
		saltLength = hasher.HashSize;
	}

	public static string RandomSalt()
	{
		const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
		StringBuilder res = new StringBuilder();
		for (int i = 0; i < saltLength; i++)
			res.Append(valid[random.Next(valid.Length)]);
		return res.ToString();
	}

	public static string HashPassword(string password)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(password);
		byte[] hash = hasher.ComputeHash(bytes);
		return Encoding.UTF8.GetString(hash);
	}

	public static string SaltHash(string hash, string salt)
	{
		HashAlgorithm algorithm = SHA256Managed.Create();

		byte[] hashBytes = Encoding.UTF8.GetBytes(hash);
		byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

		byte[] hashWithSaltBytes = new byte[hashBytes.Length + saltBytes.Length];

		for (int i = 0; i < hashBytes.Length; i++)
		{
			hashWithSaltBytes[i] = hashBytes[i];
		}
		for (int i = 0; i < saltBytes.Length; i++)
		{
			hashWithSaltBytes[hashBytes.Length + i] = saltBytes[i];
		}

		return Encoding.UTF8.GetString(algorithm.ComputeHash(hashWithSaltBytes));
	}
}