using Microsoft.Data.Sqlite;

namespace ShellShockers.Server.Components.Database;

internal static class SqlLiteDatabaseHandler
{
	private const string connString = @"Data Source=D:\Code\VS Community\ShellShockers\ShellShockers.Server\Components\Database\ShellShockersDatabase.db";
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

	public static string GetSaltedPasswordHash(string username)
	{
		string sql = @"SELECT PasswordHash FROM [Users] WHERE Username = @Username";

		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		cmd.Parameters.AddWithValue("@Username", username);

		conn.Open();
		string result = (string)(cmd.ExecuteScalar() ?? "");
		conn.Close();
		return result;
	}

	public static string GetPasswordSalt(string username)
	{
		string sql = @"SELECT PasswordSalt FROM [Users] WHERE Username = @Username";

		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		cmd.Parameters.AddWithValue("@Username", username);

		conn.Open();
		string result = (string)(cmd.ExecuteScalar() ?? "");
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

	public static void InsertUser(string username, string email, string saltedPasswordHash, string passwordSalt)
	{
		string sql = @"INSERT INTO [Users] (Username, PasswordHash, PasswordSalt, Email, EmailConfirmed) VALUES (@Username, @SaltedPasswordHash, @PasswordSalt, @Email, 0)";

		SqliteCommand cmd = conn.CreateCommand();
		cmd.CommandText = sql;

		cmd.Parameters.AddWithValue("@Username", username);
		cmd.Parameters.AddWithValue("@SaltedPasswordHash", saltedPasswordHash);
		cmd.Parameters.AddWithValue("@PasswordSalt", passwordSalt);
		cmd.Parameters.AddWithValue("@Email", email);

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

	public static bool CheckPassword(string username, string password)
	{
		string salt = GetPasswordSalt(username);

		// Hash and then salt the password
		password = PasswordHasherSalter.SaltHash(PasswordHasherSalter.HashPassword(password), salt);

		string saltedPasswordHash = GetSaltedPasswordHash(username);

		return password == saltedPasswordHash;
	}
}