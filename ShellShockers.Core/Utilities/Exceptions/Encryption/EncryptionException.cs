namespace ShellShockers.Core.Utilities.Exceptions.Encryption;

public class EncryptionException : Exception
{
	public EncryptionException()
		: base("Ecryption exception occurred")
	{

	}

	public EncryptionException(string message)
		: base(message)
	{

	}
}