namespace ShellShockers.Core.Utilities.Exceptions.Networking;

public class NetworkedException : Exception
{
	public NetworkedExceptionType Type { get; }

	public NetworkedException(NetworkedExceptionType type)
		: base($"Networked exception: {type}")
	{
		Type = type;
	}
}

public enum NetworkedExceptionType
{
	None,
	Other,

	IO,
	Disposed,
	Timedout,
	Disconnected,
	EncryptionFailed,
	TextDecodingFailed,
	DeserializationFailed,

	OperationCancelled,
	TextEncodingFailed,
	SerializationFailed
}