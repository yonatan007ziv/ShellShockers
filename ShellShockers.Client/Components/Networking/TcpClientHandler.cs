using Microsoft.Extensions.Logging;
using ShellShockers.Core.Utilities.Exceptions.Encryption;
using ShellShockers.Core.Utilities.Interfaces;
using ShellShockers.Core.Utilities.Networking;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ShellShockers.Client.Components.Networking;

internal class TcpClientHandler : BaseTcpHandler
{
	private readonly ILogger logger;

	private string AuthenticationToken { get; set; } = "";

	public TcpClientHandler(TcpClient client, ISerializer messageSerializer, ILogger logger)
		: base(client, messageSerializer, logger)
	{
		this.logger = logger;
	}

	public async Task<bool> Connect(IPAddress address, int port)
	{
		try
		{
			await Socket.ConnectAsync(address, port);
			await InitializeEncryption();
			return true;
		}
		catch (Exception ex) { logger.LogError("Error connecting: {exceptionMessage}", ex.Message); }
		return false;
	}

	public async new Task WriteMessage<T>(MessagePacket<T> message) where T : class
	{
		try
		{
			await base.WriteMessage(message);
		}
		catch (Exception ex) { logger.LogCritical("Unhandled write exception: {exMessage}", ex.Message); }
	}


	public async new Task<MessagePacket<T>> ReadMessage<T>() where T : class
	{
		try
		{
			return await base.ReadMessage<T>();
		}
		catch (Exception ex) { logger.LogCritical("Unhandled read exception: {exMessage}", ex.Message); }
		return new MessagePacket<T>(MessageType.Invalid, default);
	}

	protected override async Task<bool> EstablishEncryption()
	{
		try
		{
			// Send Rsa Details
			byte[] rsaPublicKey = encryptionHandler.ExportRsa();
			_ = WriteBytes(rsaPublicKey);

			// Import Aes Details
			byte[] encryptedAesPrivateKey = await ReadBytes();
			byte[] aesPrivateKey = encryptionHandler.DecryptRsa(encryptedAesPrivateKey);
			encryptionHandler.ImportAesPrivateKey(aesPrivateKey);
			byte[] encryptedAesIv = await ReadBytes();
			byte[] aesIv = encryptionHandler.DecryptRsa(encryptedAesIv);
			encryptionHandler.ImportAesIv(aesIv);

			// Test Encryption: Send
			string msgTest = EncryptionTestWord;
			byte[] decryptedTest = Encoding.UTF8.GetBytes(msgTest);
			byte[] encryptedTest = encryptionHandler.EncryptAes(decryptedTest);
			_ = WriteBytes(encryptedTest);

			// Test Encryption: Receive
			encryptedTest = await ReadBytes();
			decryptedTest = encryptionHandler.DecryptAes(encryptedTest);
			msgTest = Encoding.UTF8.GetString(decryptedTest);

			if (msgTest != EncryptionTestWord)
				throw new EncryptionException($"Encryption error, Expected {EncryptionTestWord}, got {msgTest} ");
		}
		catch (Exception ex)
		{
			logger.LogError("Failed Establishing Encryption: {exceptionMessage}", ex.Message);
			disconnectedCts.Cancel();
			return false;
		}

		logger.LogInformation("End-to-end encryption was Successful");
		return true;
	}

	public async void Disconnect()
	{
		Socket.Close();
		disconnectedCts.Cancel();
	}
}