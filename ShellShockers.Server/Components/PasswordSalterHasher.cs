using System.Security.Cryptography;
using System.Text;

namespace ShellShockers.Server.Components;

internal static class HasherSalter
{
	private static readonly MD5 hasher = MD5.Create();
	private static readonly Random random = new Random();
	private static readonly int saltLength;

	static HasherSalter()
	{
		saltLength = hasher.HashSize;
	}

	public static byte[] RandomSalt()
	{
		const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
		StringBuilder res = new StringBuilder();

		for (int i = 0; i < saltLength; i++)
			res.Append(valid[random.Next(valid.Length)]);

		return Encoding.ASCII.GetBytes(res.ToString());
	}

	public static byte[] HashArray(byte[] arr)
	{
		return hasher.ComputeHash(arr);
	}

	public static byte[] SaltHash(byte[] hash, byte[] salt)
	{
		HashAlgorithm algorithm = SHA256.Create();

		byte[] hashWithSaltBytes = new byte[hash.Length + salt.Length];

		for (int i = 0; i < hash.Length; i++)
			hashWithSaltBytes[i] = hash[i];
		for (int i = 0; i < salt.Length; i++)
			hashWithSaltBytes[hash.Length + i] = salt[i];

		return algorithm.ComputeHash(hashWithSaltBytes);
	}
}