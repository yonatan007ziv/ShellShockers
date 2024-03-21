using Microsoft.Extensions.Logging;
using ShellShockers.Core.Utilities.Exceptions.Encryption;
using ShellShockers.Core.Utilities.Exceptions.Networking;
using ShellShockers.Core.Utilities.Interfaces;
using ShellShockers.Core.Utilities.Networking;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ShellShockers.Client.Components.Networking;

internal class TcpClientHandler : BaseTcpHandler
{
	private readonly ILogger logger;

	public TcpClientHandler(TcpClient client, ISerializer messageSerializer, ILogger logger)
		: base(client, messageSerializer, logger)
	{
		this.logger = logger;
	}

	public async Task<bool> Connect(IPAddress address, int port)
	{
		try
		{
			await client.ConnectAsync(address, port);
			await InitializeEncryption();
			return true;
		}
		catch (Exception ex) { logger.LogError("Error connecting: {exceptionMessage}", ex.Message); }
		return false;
	}

	public async new Task WriteMessage<T>(T message) where T : class
	{
		try
		{
			await base.WriteMessage(message);
		}
		catch (NetworkedException) { Disconnect(); }
	}

	public async new Task<T> ReadMessage<T>() where T : class
	{
		T result;
		try
		{
			result = await base.ReadMessage<T>();
		}
		catch (NetworkedException) { Disconnect(); throw; }

		return result;
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

		logger.LogInformation("Encryption Successful");
		return true;
	}

	public void Disconnect()
	{
		client.Close();
		disconnectedCts.Cancel();
	}
}