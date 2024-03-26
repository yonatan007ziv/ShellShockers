using ShellShockers.Core.Utilities.InputValidators;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Enums;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;
using ShellShockers.Server.Components.Database;

namespace ShellShockers.Server.Components.Networking.ClientHandlers;

internal class LoginRegisterClientHandler : BaseClientHandler
{
	private string stored2FA;

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
		else if (message.Type == MessageType.TwoFACode)
			await TwoFARegisterRequest(requestModel);
	}

	private async Task LoginRequest(LoginRegisterRequestModel requestModel)
	{
		await Console.Out.WriteLineAsync($"Login request: {requestModel.Username} {requestModel.Password}");

		LoginRegisterResponseModel responseModel = new LoginRegisterResponseModel();
		MessagePacket<LoginRegisterResponseModel> responsePacket = new MessagePacket<LoginRegisterResponseModel>(MessageType.LoginReponse, responseModel);

		if (!SqlLiteDatabaseHandler.UsernameExists(requestModel.Username))
			responseModel.Status = LoginRegisterResponse.UsernameDoesNotExist;
		else if (!SqlLiteDatabaseHandler.CheckPassword(requestModel.Username, requestModel.Password))
			responseModel.Status = LoginRegisterResponse.WrongPassword;
		else if (!SqlLiteDatabaseHandler.GetEmailConfirmed(requestModel.Username))
			responseModel.Status = LoginRegisterResponse.EmailNotConfirmed;
		else
			responseModel.Status = LoginRegisterResponse.Success;

		Console.WriteLine(responseModel.Status.ToString());
		await TcpClientHandler.WriteMessage(responsePacket);
	}

	private async Task RegisterRequest(LoginRegisterRequestModel requestModel)
	{
		await Console.Out.WriteLineAsync($"Register request: {requestModel.Username} {requestModel.Password} {requestModel.Email}");

		LoginRegisterResponseModel responseModel = new LoginRegisterResponseModel();
		MessagePacket<LoginRegisterResponseModel> responsePacket = new MessagePacket<LoginRegisterResponseModel>(MessageType.LoginReponse, responseModel);

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
		else if (!EmailSender.SendEmail(requestModel.Email, "2FA Token", $"Your 2FA token is: {twoFAToken}"))
			responseModel.Status = LoginRegisterResponse.InvalidEmail;
		else
			responseModel.Status = LoginRegisterResponse.TwoFactorAuthenticationSent;

		Console.WriteLine(responseModel.Status.ToString());

		stored2FA = twoFAToken;

		if (responseModel.Status == LoginRegisterResponse.TwoFactorAuthenticationSent)
		{
			// Insert user without confirming the email
			string salt = PasswordHasherSalter.RandomSalt();
			string saltedHash = PasswordHasherSalter.SaltHash(requestModel.Password, salt);
			SqlLiteDatabaseHandler.InsertUser(requestModel.Username, requestModel.Email, saltedHash, salt);
		}

		await TcpClientHandler.WriteMessage(responsePacket);
	}

	private async Task TwoFARegisterRequest(LoginRegisterRequestModel requestModel)
	{
		await Console.Out.WriteLineAsync($"2FA Request: {requestModel.Username} {requestModel.TwoFAToken}");

		LoginRegisterResponseModel responseModel = new LoginRegisterResponseModel();
		MessagePacket<LoginRegisterResponseModel> responsePacket = new MessagePacket<LoginRegisterResponseModel>(MessageType.RegisterReponse, responseModel);

		if (requestModel.TwoFAToken == stored2FA)
		{
			SqlLiteDatabaseHandler.ValidateEmail(requestModel.Username);
			responseModel.Status = LoginRegisterResponse.Success;
		}
		else
			responseModel.Status = LoginRegisterResponse.Wrong2FACode;

		Console.WriteLine(responseModel.Status.ToString());

		await TcpClientHandler.WriteMessage(responsePacket);
	}
}