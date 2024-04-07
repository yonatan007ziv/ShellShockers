using ShellShockers.Core.Utilities.InputValidators;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Enums;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;
using ShellShockers.Server.Components.Database;
using System.Text;

namespace ShellShockers.Server.Components.Networking.ClientHandlers;

internal class LoginRegisterClientHandler : BaseClientHandler
{
	private readonly bool[] notARobotTiles = new bool[9];

	public override async void StartRead()
	{
		while (Connected)
		{
			MessagePacket<LoginRegisterRequestModel> message = await TcpClientHandler.ReadMessage<LoginRegisterRequestModel>();

			if (!Connected)
				return;

			if (message.Type == MessageType.Disconnect || message.Type == MessageType.Invalid || message.Type == MessageType.None)
			{
				Disconnect();
				return;
			}

			await InterpretMessage(message);
		}
	}

	public async Task InterpretMessage(MessagePacket<LoginRegisterRequestModel> message)
	{
		LoginRegisterRequestModel requestModel = message.Payload!;
		if (message.Type == MessageType.LoginRequest)
			await LoginRequest(requestModel);
		else if (message.Type == MessageType.RegisterRequest)
			await RegisterRequest(requestModel);
		else if (message.Type == MessageType.TwoFARequest)
			await TwoFARequest(requestModel);
		else if (message.Type == MessageType.ForgotPasswordRequest)
			await ForgotPasswordRequestRequest(requestModel);
		else if (message.Type == MessageType.NotARobotRequest)
			await NotARobotRequest();
	}

	private async Task LoginRequest(LoginRegisterRequestModel requestModel)
	{
		await Console.Out.WriteLineAsync($"Login request: {requestModel.Username} {requestModel.Password}");

		LoginRegisterResponseModel responseModel = new LoginRegisterResponseModel();
		MessagePacket<LoginRegisterResponseModel> responsePacket = new MessagePacket<LoginRegisterResponseModel>(MessageType.LoginReponse, responseModel);

		if (!SqlLiteDatabaseHandler.UsernameExists(requestModel.Username))
			responseModel.Status = LoginRegisterResponse.UsernameDoesNotExist;
		else if (!SqlLiteDatabaseHandler.CheckPassword(requestModel.Username, Encoding.ASCII.GetBytes(requestModel.Password)))
			responseModel.Status = LoginRegisterResponse.WrongPassword;
		else if (!SqlLiteDatabaseHandler.GetEmailConfirmed(requestModel.Username))
		{
			responseModel.Status = LoginRegisterResponse.EmailNotConfirmed;
			Send2FA(SqlLiteDatabaseHandler.GetEmail(requestModel.Username), requestModel.Username);
		}
		else
		{
			responseModel.Status = LoginRegisterResponse.Success;
		}

		await TcpClientHandler.WriteMessage(responsePacket);
	}

	private async Task NotARobotRequest()
	{
		Random rand = new Random();
		for (int i = 0; i < notARobotTiles.Length; i++)
			notARobotTiles[i] = rand.Next(3) == 0;
		notARobotTiles[4] = true;

		MessagePacket<NotARobotModel> messagePacket = new MessagePacket<NotARobotModel>(MessageType.NotARobotResponse, new NotARobotModel() { SelectedSquares = notARobotTiles });
		await TcpClientHandler.WriteMessage(messagePacket);
		try
		{
			MessagePacket<NotARobotModel> receivedNotARobot = await TcpClientHandler.ReadMessage<NotARobotModel>();
			bool[] selectedClientTiles = receivedNotARobot.Payload!.SelectedSquares;

			bool success = true;
			for (int i = 0; i < selectedClientTiles.Length; i++)
			{
				await Console.Out.WriteLineAsync(selectedClientTiles[i].ToString());
				if (!selectedClientTiles[i])
					success = false;
			}

			MessagePacket<NotARobotModel> resultResponse = new MessagePacket<NotARobotModel>(MessageType.NotARobotResponse, new NotARobotModel() { Success = success });
			if (success)
			{
				resultResponse.Payload!.AuthenticationToken = ClientAuthenticator.GenerateAuthenticationToken();
				ClientAuthenticator.AddAuthenticationKey(resultResponse.Payload!.AuthenticationToken, receivedNotARobot.Payload!.Username);

				await Console.Out.WriteLineAsync(resultResponse.Payload!.AuthenticationToken);
				await Console.Out.WriteLineAsync(receivedNotARobot.Payload!.Username);
			}
			await TcpClientHandler.WriteMessage(resultResponse);
		}
		catch
		{
			MessagePacket<NotARobotModel> resultResponse = new MessagePacket<NotARobotModel>(MessageType.NotARobotResponse, new NotARobotModel() { Success = false });
			await TcpClientHandler.WriteMessage(resultResponse);
		}
	}

	private async Task RegisterRequest(LoginRegisterRequestModel requestModel)
	{
		await Console.Out.WriteLineAsync($"Register request: {requestModel.Username} {requestModel.Password} {requestModel.Email}");

		LoginRegisterResponseModel responseModel = new LoginRegisterResponseModel();
		MessagePacket<LoginRegisterResponseModel> responsePacket = new MessagePacket<LoginRegisterResponseModel>(MessageType.RegisterReponse, responseModel);

		string twoFAToken = TwoFactorAuthenticationCodeGenerator.Generate2FACode();
		if (SqlLiteDatabaseHandler.UsernameExists(requestModel.Username))
			responseModel.Status = LoginRegisterResponse.UsernameExists;
		else if (!LoginRegisterInputPredicates.UsernameValid(requestModel.Username))
			responseModel.Status = LoginRegisterResponse.InvalidUsername;
		else if (!LoginRegisterInputPredicates.PasswordValid(requestModel.Password))
			responseModel.Status = LoginRegisterResponse.InvalidPassword;
		else if (!LoginRegisterInputPredicates.EmailValid(requestModel.Email))
			responseModel.Status = LoginRegisterResponse.InvalidEmail;
		else if (SqlLiteDatabaseHandler.EmailExists(requestModel.Email))
			responseModel.Status = LoginRegisterResponse.EmailInUse;
		else if (!Send2FA(requestModel.Email, requestModel.Username))
			responseModel.Status = LoginRegisterResponse.InvalidEmail;
		else
			responseModel.Status = LoginRegisterResponse.TwoFactorAuthenticationSent;

		// Send password reset key to email
		if (responseModel.Status == LoginRegisterResponse.TwoFactorAuthenticationSent)
		{
			byte[] passwordBytes = Encoding.ASCII.GetBytes(requestModel.Password);
			byte[] hashedPasswordArray = HasherSalter.HashArray(passwordBytes);

			byte[] salt = HasherSalter.RandomSalt();
			byte[] saltedHash = HasherSalter.SaltHash(hashedPasswordArray, salt);

			// Insert user without confirming the email
			SqlLiteDatabaseHandler.InsertUser(requestModel.Username, requestModel.Email, saltedHash, salt, HasherSalter.HashArray(Encoding.UTF8.GetBytes(twoFAToken)));
		}

		await TcpClientHandler.WriteMessage(responsePacket);
	}

	private async Task TwoFARequest(LoginRegisterRequestModel requestModel)
	{
		await Console.Out.WriteLineAsync($"2FA request: {requestModel.Username} {requestModel.TwoFACode}");

		LoginRegisterResponseModel responseModel = new LoginRegisterResponseModel();
		MessagePacket<LoginRegisterResponseModel> responsePacket = new MessagePacket<LoginRegisterResponseModel>(MessageType.TwoFAResponse, responseModel);


		if (!SqlLiteDatabaseHandler.UsernameExists(requestModel.Username))
			responseModel.Status = LoginRegisterResponse.InvalidUsername;
		else if (!SqlLiteDatabaseHandler.Get2FATime(requestModel.Username, out DateTime twoFASentTime))
			responseModel.Status = LoginRegisterResponse.UnknownError;
		else if (DateTime.Now.Subtract(twoFASentTime) > TimeSpan.FromMinutes(5))
			responseModel.Status = LoginRegisterResponse.TwoFACodeExpired;
		else if (CheckAgainstStored2FA(requestModel.Username, requestModel.TwoFACode))
		{
			SqlLiteDatabaseHandler.ValidateEmail(requestModel.Username);
			responseModel.Status = LoginRegisterResponse.Success;
		}
		else
			responseModel.Status = LoginRegisterResponse.Wrong2FACode;

		Console.WriteLine(responseModel.Status.ToString());

		await TcpClientHandler.WriteMessage(responsePacket);
	}

	private bool CheckAgainstStored2FA(string username, string twoFACode)
	{
		byte[] stored2FAHash = SqlLiteDatabaseHandler.Get2FAHash(username);

		byte[] twoFABytes = Encoding.UTF8.GetBytes(twoFACode);
		byte[] twoFAHash = HasherSalter.HashArray(twoFABytes);

		if (twoFAHash.Length != stored2FAHash.Length)
			return false;
		for (int i = 0; i < twoFAHash.Length; i++)
			if (stored2FAHash[i] != twoFAHash[i])
				return false;

		return true;
	}

	private async Task ForgotPasswordRequestRequest(LoginRegisterRequestModel requestModel)
	{
		await Console.Out.WriteLineAsync($"Forgot password request: {requestModel.Email} {requestModel.TwoFACode}");

		LoginRegisterResponseModel responseModel = new LoginRegisterResponseModel();
		MessagePacket<LoginRegisterResponseModel> responsePacket = new MessagePacket<LoginRegisterResponseModel>(MessageType.ForgotPasswordReponse, responseModel);

		await TcpClientHandler.WriteMessage(responsePacket);
	}

	private bool Send2FA(string email, string username)
	{
		string twoFAToken = TwoFactorAuthenticationCodeGenerator.Generate2FACode();
		SqlLiteDatabaseHandler.Set2FATime(username, DateTime.Now);
		SqlLiteDatabaseHandler.Set2FAHash(username, HasherSalter.HashArray(Encoding.UTF8.GetBytes(twoFAToken)));
		return EmailSender.SendEmail(email, "2FA Token", $"Your 2FA token is: {twoFAToken}");
	}
}