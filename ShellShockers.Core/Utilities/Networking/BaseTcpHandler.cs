using Microsoft.Extensions.Logging;
using ShellShockers.Core.Utilities.Encryption;
using ShellShockers.Core.Utilities.Exceptions.Encryption;
using ShellShockers.Core.Utilities.Exceptions.Networking;
using ShellShockers.Core.Utilities.Interfaces;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace ShellShockers.Core.Utilities.Networking;

public abstract class BaseTcpHandler
{
	protected const string EncryptionTestWord = "Success";

	private readonly ILogger logger;
	private readonly ISerializer messageSerializer;

	// Timeouts
	private readonly TimeSpan writeTimeout;
	private readonly TimeSpan readTimeout;

	// Encryption handler
	protected EncryptionHandler encryptionHandler;

	// Underlying socket
	public TcpClient Socket { get; private set; }

	protected CancellationTokenSource disconnectedCts;
	protected TaskCompletionSource encryptionTask;

	public event Action? OnDisconnected;

	public BaseTcpHandler(TcpClient client, ISerializer messageSerializer, ILogger logger)
	{
		this.Socket = client;
		this.messageSerializer = messageSerializer;
		this.logger = logger;

		writeTimeout = TimeSpan.FromSeconds(30);
		readTimeout = TimeSpan.FromSeconds(30);

		encryptionHandler = new EncryptionHandler();

		disconnectedCts = new CancellationTokenSource();
		encryptionTask = new TaskCompletionSource();
	}

	protected abstract Task<bool> EstablishEncryption();

	protected async Task InitializeEncryption()
	{
		if (!await EstablishEncryption())
			throw new EncryptionException();
		encryptionTask.SetResult();
	}

	#region Read write message
	public async Task WriteMessage<T>(MessagePacket<T> message) where T : class
	{
		await encryptionTask.Task;
		NetworkedExceptionType exceptionType;

		try
		{
			// Serializes the payload
			string serializedPayload = messageSerializer.Serialize(message.Payload!)
				?? throw new SerializationException(message.Payload!.ToString());

			// Constructs a string representation of the Type and Payload of the message
			string encodedMessage = $"{(int)message.Type}:{serializedPayload}";

			// Get bytes from the representation
			byte[] decryptedWriteBuffer = Encoding.UTF8.GetBytes(encodedMessage);

			// Encrypt the bytes
			byte[] writeBuffer = encryptionHandler.EncryptAes(decryptedWriteBuffer);

			// Write them to the endpoint
			await WriteBytes(writeBuffer);
			return;
		}
		catch (SerializationException ex) { logger.LogError($"Serialization Exception: {ex.Message}"); exceptionType = NetworkedExceptionType.SerializationFailed; }
		catch (EncoderFallbackException ex) { logger.LogError($"Encoding Exception: {ex.Message}"); exceptionType = NetworkedExceptionType.TextEncodingFailed; }
		catch (CryptographicException ex) { logger.LogError($"Cryptographic Exception: {ex.Message}"); exceptionType = NetworkedExceptionType.EncryptionFailed; }
		catch (IOException ex) { logger.LogError($"IO Exception: {ex.Message}"); exceptionType = NetworkedExceptionType.IO; }
		catch (ObjectDisposedException ex) { logger.LogError($"Disposed Exception: {ex.Message}"); exceptionType = NetworkedExceptionType.Disposed; }
		catch (TimeoutException ex) { logger.LogError($"Timeout Exception: {ex.Message}"); exceptionType = NetworkedExceptionType.Timedout; }
		catch (OperationCanceledException ex) { logger.LogError($"Operation Cancelled Exception: {ex.Message}"); exceptionType = NetworkedExceptionType.OperationCancelled; }
		catch (Exception ex) { logger.LogCritical("MISHANDLED EXCEPTION: {exceptionMessage}", ex.Message); exceptionType = NetworkedExceptionType.Other; }

		disconnectedCts.Cancel();
		logger.LogError("Error writing message of type: {type}, Error: {errorCode}", typeof(T).Name, exceptionType.ToString());
	}
	public async Task<MessagePacket<T>> ReadMessage<T>() where T : class
	{
		await encryptionTask.Task;
		NetworkedExceptionType exceptionType;

		try
		{
			byte[] encryptedReadBuffer = await ReadBytes();
			byte[] readBuffer = encryptionHandler.DecryptAes(encryptedReadBuffer);
			string serializedMessage = Encoding.UTF8.GetString(readBuffer);

			// Get the two message parts: "type:payload"
			string[] messageParts = serializedMessage.Split(':', 2);

			// Get the Type
			if (!int.TryParse(messageParts[0], out int typeNumber))
				throw new FormatException("Failed converting the message type to a valid number");
			MessageType type = (MessageType)typeNumber;

			// Desrialized the payload
			T? deserialized = messageSerializer.Deserialize<T>(messageParts[1])
				?? null;

			return new MessagePacket<T>(type, deserialized);
		}
		// catch (NetworkedReadException ex) when (ex.Type == NetworkedExceptionType.Disconnected) { throw; }
		catch (IOException ex) { logger.LogError($"IO Exception: {ex.Message}"); exceptionType = NetworkedExceptionType.IO; }
		catch (TimeoutException ex) { logger.LogError($"Timeout Exception: {ex.Message}"); exceptionType = NetworkedExceptionType.Timedout; }
		// catch (DisconnectedException ex) { logger.LogError($"Disconnected Exception: {ex.Message}"); exceptionType = NetworkedExceptionType.Disconnected; }
		catch (NetworkedException ex) { logger.LogError($"Read Exception: {ex.Message}"); exceptionType = ex.Type; }
		catch (CryptographicException ex) { logger.LogError($"Cryptographic Exception: {ex.Message}"); exceptionType = NetworkedExceptionType.EncryptionFailed; }
		catch (DecoderFallbackException ex) { logger.LogError($"Decoding Exception: {ex.Message}"); exceptionType = NetworkedExceptionType.TextDecodingFailed; }
		catch (SerializationException ex) { logger.LogError($"Deserialization Exception: {ex.Message}"); exceptionType = NetworkedExceptionType.DeserializationFailed; }
		catch (ObjectDisposedException ex) { logger.LogError($"Disposed Exception: {ex.Message}"); exceptionType = NetworkedExceptionType.Disposed; }
		catch (FormatException ex) { logger.LogError($"Format exception: {ex.Message}"); exceptionType = NetworkedExceptionType.DeserializationFailed; }
		catch (Exception ex) { logger.LogCritical("MISHANDLED EXCEPTION: {exceptionMessage}", ex.Message); exceptionType = NetworkedExceptionType.Other; }

		disconnectedCts.Cancel();
		logger.LogError("Error reading message of type: {type}, Error: {errorCode}", typeof(T).Name, exceptionType.ToString());
		return new MessagePacket<T>(MessageType.Invalid, default);
	}
	#endregion

	#region Read write bytes
	protected async Task WriteBytes(byte[] writeBuffer)
	{
		CancellationTokenSource writeTimeoutCts = new CancellationTokenSource();
		writeTimeoutCts.CancelAfter(writeTimeout);

		CancellationTokenSource joinedCts = CancellationTokenSource.CreateLinkedTokenSource(writeTimeoutCts.Token, disconnectedCts.Token);

		// Prefixes 4 Bytes Indicating Message Length
		byte[] length = BitConverter.GetBytes(writeBuffer.Length);
		byte[] prefixedBuffer = new byte[writeBuffer.Length + sizeof(int)];

		Array.Copy(length, 0, prefixedBuffer, 0, sizeof(int));
		Array.Copy(writeBuffer, 0, prefixedBuffer, sizeof(int), writeBuffer.Length);

		try
		{
			await Socket.GetStream().WriteAsync(prefixedBuffer, joinedCts.Token);
		}
		catch (OperationCanceledException)
		{
			if (writeTimeoutCts.IsCancellationRequested)
				throw new NetworkedException(NetworkedExceptionType.Timedout);
			throw new NetworkedException(NetworkedExceptionType.Disconnected);
		}
		catch { throw; }
	}
	protected async Task<byte[]> ReadBytes()
	{
		CancellationTokenSource readTimeoutCts = new CancellationTokenSource();
		readTimeoutCts.CancelAfter(readTimeout);

		CancellationTokenSource joinedCts = CancellationTokenSource.CreateLinkedTokenSource(readTimeoutCts.Token, disconnectedCts.Token);

		byte[] readBufer;
		int bytesRead;
		try
		{
			// Reads 4 Bytes Indicating Message Length
			byte[] lengthBuffer = new byte[4];
			await Socket.GetStream().ReadAsync(lengthBuffer, joinedCts.Token);

			int length = BitConverter.ToInt32(lengthBuffer);
			readBufer = new byte[length];
			bytesRead = await Socket.GetStream().ReadAsync(readBufer, joinedCts.Token);
		}
		catch (OperationCanceledException)
		{
			if (readTimeoutCts.IsCancellationRequested)
				throw new NetworkedException(NetworkedExceptionType.Timedout);
			throw new NetworkedException(NetworkedExceptionType.Disconnected);
		}
		catch { throw; }

		if (bytesRead == 0)
			throw new NetworkedException(NetworkedExceptionType.Disconnected);

		return readBufer;
	}
	#endregion

	public void Disconnected()
		=> OnDisconnected?.Invoke();
}