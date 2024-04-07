using GameEngine.Components.UIComponents;
using GameEngine.Core.Components.Objects;
using ShellShockers.Client.Components;
using ShellShockers.Client.Components.Networking;
using ShellShockers.Core.Utilities.InputValidators;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;
using System.Drawing;
using System.Net;
using System.Numerics;

namespace ShellShockers.Client.GameComponents.UIComponents.Views.LoginRegisterViews;

internal class RegisterViewControl : UIObject
{
	public event Action? OnEmailNotConfirmed;

	public UIButton switchToLoginButton;

	private readonly UIButton registerButton;
	private readonly UITextBox usernameTextBox;
	private readonly UITextBox passwordTextBox;
	private readonly UITextBox emailTextBox;

	public RegisterViewControl()
	{
		Vector3 fieldScale = new Vector3(0.25f, 0.15f, 1);
		float difference = 0.01f;

		// Username label
		UILabel usernameLabel = new UILabel();
		usernameLabel.Text = "Username:";
		usernameLabel.Transform.Scale = fieldScale;
		usernameLabel.Transform.Position = new Vector3(-(fieldScale.X + difference), 0.75f, 5f);
		Children.Add(usernameLabel);

		// Password label
		UILabel passwordLabel = new UILabel();
		passwordLabel.Text = "Password:";
		passwordLabel.Transform.Scale = fieldScale;
		passwordLabel.Transform.Position = new Vector3(-(fieldScale.X + difference), 0.25f, 5f);
		Children.Add(passwordLabel);

		// Email label
		UILabel emailLabel = new UILabel();
		emailLabel.Text = "Email:";
		emailLabel.Transform.Scale = fieldScale;
		emailLabel.Transform.Position = new Vector3(-(fieldScale.X + difference), -0.25f, 5f);
		Children.Add(emailLabel);

		// Register button
		registerButton = new UIButton();
		registerButton.TextColor = Color.White;
		registerButton.Text = "Register";
		registerButton.Transform.Scale = fieldScale;
		registerButton.Transform.Position = new Vector3(fieldScale.X + difference, -0.75f, 5f);
		Children.Add(registerButton);

		// Username text box
		usernameTextBox = new UITextBox();
		usernameTextBox.TextColor = Color.White;
		usernameTextBox.Transform.Scale = fieldScale;
		usernameTextBox.Transform.Position = new Vector3(fieldScale.X + difference, 0.75f, 5f);
		Children.Add(usernameTextBox);

		// Password text box
		passwordTextBox = new UITextBox();
		passwordTextBox.TextColor = Color.White;
		passwordTextBox.Transform.Scale = fieldScale;
		passwordTextBox.Transform.Position = new Vector3(fieldScale.X + difference, 0.25f, 5f);
		Children.Add(passwordTextBox);

		// Email text box
		emailTextBox = new UITextBox();
		emailTextBox.TextColor = Color.White;
		emailTextBox.Transform.Scale = fieldScale;
		emailTextBox.Transform.Position = new Vector3(fieldScale.X + difference, -0.25f, 5f);
		Children.Add(emailTextBox);

		// Switch button
		switchToLoginButton = new UIButton();
		switchToLoginButton.TextColor = Color.White;
		switchToLoginButton.OnFullClicked += () => { ResetTextBoxes(); Visible = false; };
		switchToLoginButton.Text = "Login?";
		switchToLoginButton.Transform.Scale = fieldScale;
		switchToLoginButton.Transform.Position = new Vector3(-(fieldScale.X + difference), -0.75f, 5f);
		Children.Add(switchToLoginButton);

		// Register button clicked
		registerButton.OnFullClicked += OnRegisterButtonClicked;
	}

	private void ResetTextBoxes()
	{
		usernameTextBox.Text = "";
		passwordTextBox.Text = "";
		emailTextBox.Text = "";
	}

	private async void OnRegisterButtonClicked()
	{
		registerButton.Enabled = false;
		await RegisterProcedure();
		registerButton.Enabled = true;
	}

	private async Task RegisterProcedure()
	{
		if (!Factories.ClientFactory.Create(out TcpClientHandler clientHandler))
		{
			registerButton.Text = "Error creating a TcpClientHandler";
			return;
		}

		if (!LoginRegisterInputPredicates.UsernameValid(usernameTextBox.Text))
		{
			registerButton.Text = "Invalid username";
			return;
		}

		if (!LoginRegisterInputPredicates.PasswordValid(passwordTextBox.Text))
		{
			registerButton.Text = "Invalid password";
			return;
		}

		if (!LoginRegisterInputPredicates.EmailValid(emailTextBox.Text))
		{
			registerButton.Text = "Invalid email";
			return;
		}

		if (!await clientHandler.Connect(IPAddress.Parse(ServerAddresses.LoginRegisterServerAddress), ServerAddresses.LoginRegisterServerPort))
		{
			registerButton.Text = "Error connecting to server";
			return;
		}

		// Write login request to server 
		MessagePacket<LoginRegisterRequestModel> loginRequestPacket = new MessagePacket<LoginRegisterRequestModel>(MessageType.RegisterRequest, new LoginRegisterRequestModel() { Username = usernameTextBox.Text, Password = passwordTextBox.Text, Email = emailTextBox.Text });
		await clientHandler.WriteMessage(loginRequestPacket);

		// Read login response from server
		MessagePacket<LoginRegisterResponseModel> response = await clientHandler.ReadMessage<LoginRegisterResponseModel>();

		if (response.Type == MessageType.Invalid)
			return;

		bool unsuccessfulRequest = true;
		if (response.Type == MessageType.RegisterReponse)
		{
			switch (response.Payload!.Status)
			{
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.None:
					registerButton.Text = "Critical: response - None";
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.Success:
					unsuccessfulRequest = false;
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.UnknownError:
					registerButton.Text = "Unknown error occurred";
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.EmailInUse:
					registerButton.Text = "Email already exists";
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.UsernameDoesNotExist:
					registerButton.Text = "Username does not exist";
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.WrongPassword:
					registerButton.Text = "Wrong password";
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.TwoFactorAuthenticationSent:
					OnEmailNotConfirmed?.Invoke();
					registerButton.Text = "Two factor authentication needed";
					break;
			}
		}

		if (unsuccessfulRequest)
			ResetRegisterButtonTextAfterSeconds(5);

		// Disconnect at the end of the request
		clientHandler.Disconnect();
	}

	private async void ResetRegisterButtonTextAfterSeconds(int seconds)
	{
		await Task.Delay(TimeSpan.FromSeconds(seconds));
		registerButton.Text = "Register";
	}
}