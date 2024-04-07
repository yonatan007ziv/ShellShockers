using Microsoft.Extensions.Logging;
using ShellShockers.Core.Utilities.Exceptions.Encryption;
using ShellShockers.Core.Utilities.Exceptions.Networking;
using ShellShockers.Core.Utilities.Interfaces;
using ShellShockers.Core.Utilities.Networking;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols;
using System.Net.Sockets;
using System.Text;

namespace ShellShockers.Server.Components.Networking;

internal class TcpClientHandler : BaseTcpHandler
{
	private readonly ILogger logger;
	public TcpClientHandler(TcpClient client, ISerializer messageSerializer, ILogger logger)
		: base(client, messageSerializer, logger)
	{
		this.logger = logger;
		_ = InitializeEncryption();
	}

	public async new Task WriteMessage<T>(MessagePacket<T> message) where T : class
	{
		try
		{
			await base.WriteMessage(message);
		}
		catch (NetworkedException) { Disconnect(); }
	}

	public async new Task<MessagePacket<T>> ReadMessage<T>() where T : class
	{
		try
		{
			return await base.ReadMessage<T>();
		}
		catch (NetworkedException) { Disconnect(); }

		return new MessagePacket<T>(MessageType.Invalid, default);
	}

	protected override async Task<bool> EstablishEncryption()
	{
		try
		{
			// Import Rsa Details
			byte[] rsaPublicKey = await ReadBytes();
			encryptionHandler.ImportRsa(rsaPublicKey);

			// Send Aes Details
			byte[] aesPrivateKey = encryptionHandler.ExportAesPrivateKey();
			byte[] encryptedRsaPrivateKey = encryptionHandler.EncryptRsa(aesPrivateKey);
			_ = WriteBytes(encryptedRsaPrivateKey);
			byte[] aesIv = encryptionHandler.ExportAesIv();
			byte[] encryptedRsaIv = encryptionHandler.EncryptRsa(aesIv);
			_ = WriteBytes(encryptedRsaIv);

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

	public void Disconnect()
	{
		Disconnected();
		Socket.Close();
		disconnectedCts.Cancel();
	}
}