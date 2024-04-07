using Microsoft.Data.Sqlite;
using System.Globalization;

namespace ShellShockers.Server.Components.Database;

internal static class SqlLiteDatabaseHandler
{
	private const string connString = @"Data Source=C:\Code\VS Community\ShellShockers\ShellShockers.Server\Components\Database\ShellShockersDatabase.db";
	private static readonly SqliteConnection conn = new SqliteConnection(connString);

	public static bool UsernameExists(string username)
	{
		string sql = @"SELECT COUNT(*) FROM [Users] WHERE Username = @Username";

		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		cmd.Parameters.AddWithValue("@Username", username);

		conn.Open();
		long count = (long)(cmd.ExecuteScalar() ?? 0);
		conn.Close();
		return count > 0;
	}

	public static byte[] Get2FAHash(string username)
	{

		string sql = @"SELECT TwoFAHash FROM [Users] WHERE Username = @Username";

		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		cmd.Parameters.AddWithValue("@Username", username);

		conn.Open();
		byte[] arr = (byte[])(cmd.ExecuteScalar() ?? 0);
		conn.Close();
		return arr;
	}

	public static void Set2FAHash(string username, byte[] twoFAHash)
	{

		if (!UsernameExists(username))
			return;

		string sql = @"UPDATE [Users] SET TwoFAHash = @TwoFAHash WHERE Username = @Username";

		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		cmd.Parameters.AddWithValue("@Username", username);
		cmd.Parameters.AddWithValue("@TwoFAHash", twoFAHash);

		conn.Open();
		cmd.ExecuteNonQuery();
		conn.Close();
	}

	public static void Set2FATime(string username, DateTime lastTime)
	{
		if (!UsernameExists(username))
			return;

		string twoFADateTime = lastTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

		string sql = @"UPDATE [Users] SET TwoFADateTime = @TwoFADateTime WHERE Username = @Username";

		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		cmd.Parameters.AddWithValue("@Username", username);
		cmd.Parameters.AddWithValue("@TwoFADateTime", twoFADateTime);

		conn.Open();
		cmd.ExecuteNonQuery();
		conn.Close();
	}

	public static bool Get2FATime(string username, out DateTime dateTime)
	{
		string sql = @"SELECT TwoFADateTime FROM [Users] WHERE Username = @Username";

		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		cmd.Parameters.AddWithValue("@Username", username);

		conn.Open();
		string result = (string)(cmd.ExecuteScalar() ?? "");
		conn.Close();

		return DateTime.TryParseExact(result, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime);
	}

	public static bool EmailExists(string email)
	{
		string sql = @"SELECT COUNT(*) FROM [Users] WHERE Email = @Email";

		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		cmd.Parameters.AddWithValue("@Email", email);

		conn.Open();
		long count = (long)(cmd.ExecuteScalar() ?? 0);
		conn.Close();
		return count > 0;
	}

	public static string GetEmail(string username)
	{
		string sql = @"SELECT Email FROM [Users] WHERE Username = @Username";

		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		cmd.Parameters.AddWithValue("@Username", username);

		conn.Open();
		string count = (string)(cmd.ExecuteScalar() ?? 0);
		conn.Close();
		return count;
	}

	public static byte[] GetSaltedPasswordHash(string username)
	{
		string sql = @"SELECT PasswordHash FROM [Users] WHERE Username = @Username";

		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		cmd.Parameters.AddWithValue("@Username", username);

		conn.Open();
		byte[] result = (byte[])(cmd.ExecuteScalar() ?? "");
		conn.Close();
		return result;
	}

	public static byte[] GetPasswordSalt(string username)
	{
		string sql = @"SELECT PasswordSalt FROM [Users] WHERE Username = @Username";

		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		cmd.Parameters.AddWithValue("@Username", username);

		conn.Open();
		byte[] result = (byte[])(cmd.ExecuteScalar() ?? "");
		conn.Close();
		return result;
	}

	public static bool GetEmailConfirmed(string username)
	{
		string sql = @"SELECT EmailConfirmed FROM [Users] WHERE Username = @Username";

		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		cmd.Parameters.AddWithValue("@Username", username);

		conn.Open();
		long result = (long)(cmd.ExecuteScalar() ?? 0);
		conn.Close();
		return result == 1;
	}

	public static void InsertUser(string username, string email, byte[] saltedPasswordHash, byte[] passwordSalt, byte[] twoFAHash)
	{
		string sql = @"INSERT INTO [Users] (Username, PasswordHash, PasswordSalt, Email, EmailConfirmed, TwoFAHash, TwoFADateTime) VALUES (@Username, @SaltedPasswordHash, @PasswordSalt, @Email, 0, @TwoFAHash, @TwoFADateTime)";

		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		cmd.Parameters.AddWithValue("@Username", username);
		cmd.Parameters.AddWithValue("@SaltedPasswordHash", saltedPasswordHash);
		cmd.Parameters.AddWithValue("@PasswordSalt", passwordSalt);
		cmd.Parameters.AddWithValue("@Email", email);
		cmd.Parameters.AddWithValue("@TwoFAHash", twoFAHash);
		cmd.Parameters.AddWithValue("@TwoFADateTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

		conn.Open();
		cmd.ExecuteNonQuery();
		conn.Close();
	}

	public static void ValidateEmail(string username)
	{
		string sql = @"UPDATE [Users] SET EmailConfirmed = 1 WHERE Username = @Username";

		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		cmd.Parameters.AddWithValue("@Username", username);

		conn.Open();
		cmd.ExecuteNonQuery();
		conn.Close();
	}

	public static bool CheckPassword(string username, byte[] password)
	{
		byte[] salt = GetPasswordSalt(username);

		// Hash and then salt the password
		byte[] hashedPasswordArray = HasherSalter.HashArray(password);
		password = HasherSalter.SaltHash(hashedPasswordArray, salt);

		byte[] saltedPasswordHash = GetSaltedPasswordHash(username);

		if (password.Length != saltedPasswordHash.Length)
			return false;

		for (int i = 0; i < password.Length; i++)
			if (password[i] != saltedPasswordHash[i])
				return false;

		return true;
	}
}