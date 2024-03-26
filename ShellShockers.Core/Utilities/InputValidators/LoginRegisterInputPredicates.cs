namespace ShellShockers.Core.Utilities.InputValidators;

public static class LoginRegisterInputPredicates
{
	public static bool UsernameValid(string username)
	{
		return username.Length > 0;
	}

	public static bool PasswordValid(string password)
	{
		return password.Length > 3;
	}

	public static bool EmailValid(string email)
	{
		return email.Length > 3 && email.Contains('@');
	}
}