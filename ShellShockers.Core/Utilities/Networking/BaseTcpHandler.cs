using Microsoft.Extensions.Logging;
using ShellShockers.Core.Utilities.Encryption;
using ShellShockers.Core.Utilities.Exceptions.Encryption;
using ShellShockers.Core.Utilities.Exceptions.Networking;
using ShellShockers.Core.Utilities.Interfaces;
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
	protected TcpClient client;

	protected CancellationTokenSource disconnectedCts;
	protected TaskCompletionSource encryptionTask;

	public BaseTcpHandler(TcpClient client, ISerializer messageSerializer, ILogger logger)
	{
		this.client = client;
		this.messageSerializer = messageSerializer;
		this.logger = logger;

		writeTimeout = TimeSpan.FromSeconds(15);
		readTimeout = TimeSpan.FromSeconds(15);

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
	public async Task WriteMessage<T>(T deserializedMessage) where T : class
	{
		await encryptionTask.Task;
		NetworkedExceptionType exceptionType;

		try
		{
			string serializedMessage = messageSerializer.Serialize(deserializedMessage)
				?? throw new SerializationException(deserializedMessage.ToString());
			byte[] decryptedWriteBuffer = Encoding.UTF8.GetBytes(serializedMessage);
			byte[] writeBuffer = encryptionHandler.EncryptAes(decryptedWriteBuffer);
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
		catch (Exception ex) { logger.LogCritical($"MISHANDLED EXCEPTION: {ex.Message}"); throw new Exception("MISHANDLED EXCEPTION"); }

		disconnectedCts.Cancel();
		throw new NetworkedException(exceptionType);
	}
	public async Task<T> ReadMessage<T>() where T : class
	{
		await encryptionTask.Task;
		NetworkedExceptionType exceptionType;

		try
		{
			byte[] encryptedReadBuffer = await ReadBytes();
			byte[] readBuffer = encryptionHandler.DecryptAes(encryptedReadBuffer);
			string serializedMessage = Encoding.UTF8.GetString(readBuffer);

			return messageSerializer.Deserialize<T>(serializedMessage)
				?? throw new SerializationException(serializedMessage);
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
		catch (Exception ex) { logger.LogCritical($"MISHANDLED EXCEPTION: {ex.Message}"); throw new Exception("MISHANDLED EXCEPTION"); }

		disconnectedCts.Cancel();
		throw new NetworkedException(exceptionType);
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
			await client.GetStream().WriteAsync(prefixedBuffer, joinedCts.Token);
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
			await client.GetStream().ReadAsync(lengthBuffer, joinedCts.Token);

			int length = BitConverter.ToInt32(lengthBuffer);
			readBufer = new byte[length];
			bytesRead = await client.GetStream().ReadAsync(readBufer, joinedCts.Token);
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

	public void Dispose()
	{
		disconnectedCts.Cancel();
		client.Close();

		client = new TcpClient();
		encryptionHandler = new EncryptionHandler();
		disconnectedCts = new CancellationTokenSource();
		encryptionTask = new TaskCompletionSource();
	}
}