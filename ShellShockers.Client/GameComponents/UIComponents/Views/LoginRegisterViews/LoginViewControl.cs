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

internal class LoginViewControl : UIObject
{
	public event Action? OnEmailNotConfirmed;
	public event Action? OnSuccessfulLogin;

	public UIButton switchToRegisterButton;
	public UIButton forgotPasswordButton;
	private UITextBox usernameTextBox;
	private UITextBox passwordTextBox;
	private UIButton loginButton;

	public LoginViewControl()
	{
		Vector3 fieldScale = new Vector3(0.25f, 0.15f, 1);
		float difference = 0.01f;

		// Username label
		UILabel usernameLabel = new UILabel();
		usernameLabel.Text = "Username:";
		usernameLabel.Transform.Scale = fieldScale;
		usernameLabel.Transform.Position = new Vector3(-(fieldScale.X + difference), 0.75f, 0);
		Children.Add(usernameLabel);

		// Password label
		UILabel passwordLabel = new UILabel();
		passwordLabel.Text = "Password:";
		passwordLabel.Transform.Scale = fieldScale;
		passwordLabel.Transform.Position = new Vector3(-(fieldScale.X + difference), 0.25f, 0);
		Children.Add(passwordLabel);

		// Username text box
		usernameTextBox = new UITextBox();
		usernameTextBox.TextColor = Color.White;
		usernameTextBox.Transform.Scale = fieldScale;
		usernameTextBox.Transform.Position = new Vector3(fieldScale.X + difference, 0.75f, 0);
		Children.Add(usernameTextBox);

		// Password text box
		passwordTextBox = new UITextBox();
		passwordTextBox.TextColor = Color.White;
		passwordTextBox.Transform.Scale = fieldScale;
		passwordTextBox.Transform.Position = new Vector3(fieldScale.X + difference, 0.25f, 0);
		Children.Add(passwordTextBox);

		// Login button
		loginButton = new UIButton();
		loginButton.TextColor = Color.White;
		loginButton.Text = "Login";
		loginButton.Transform.Scale = fieldScale;
		loginButton.Transform.Position = new Vector3(fieldScale.X + difference, -0.25f, 0);
		Children.Add(loginButton);

		// Forgot password button
		forgotPasswordButton = new UIButton();
		forgotPasswordButton.TextColor = Color.White;
		forgotPasswordButton.Text = "Forgot password?";
		forgotPasswordButton.TextData.TextColor = Color.White;
		forgotPasswordButton.Transform.Scale = fieldScale;
		forgotPasswordButton.Transform.Position = new Vector3(0, -0.75f, 5f);
		Children.Add(forgotPasswordButton);

		// Switch button
		switchToRegisterButton = new UIButton();
		switchToRegisterButton.TextColor = Color.White;
		switchToRegisterButton.OnFullClicked += () => { ResetTextBoxes(); Visible = false; };
		switchToRegisterButton.Text = "Register?";
		switchToRegisterButton.Transform.Scale = fieldScale;
		switchToRegisterButton.Transform.Position = new Vector3(-(fieldScale.X + difference), -0.25f, 5f);
		Children.Add(switchToRegisterButton);

		// Login button clicked
		loginButton.OnFullClicked += OnLoginButtonClicked;
	}

	private void ResetTextBoxes()
	{
		usernameTextBox.Text = "";
		passwordTextBox.Text = "";
	}

	private async void OnLoginButtonClicked()
	{
		loginButton.Enabled = false;
		await LoginProcedure();
		loginButton.Enabled = true;
	}

	private async Task LoginProcedure()
	{
		if (!Factories.ClientFactory.Create(out TcpClientHandler clientHandler))
		{
			loginButton.Text = "Error creating a TcpClientHandler";
			return;
		}

		if (!LoginRegisterInputPredicates.UsernameValid(usernameTextBox.Text))
		{
			loginButton.Text = "Invalid username";
			return;
		}

		if (!LoginRegisterInputPredicates.PasswordValid(passwordTextBox.Text))
		{
			loginButton.Text = "Invalid password";
			return;
		}

		if (!await clientHandler.Connect(IPAddress.Parse(ServerAddresses.LoginRegisterServerAddress), ServerAddresses.LoginRegisterServerPort))
		{
			loginButton.Text = "Error connecting to server";
			return;
		}

		// Write login request to server 
		MessagePacket<LoginRegisterRequestModel> loginRequestPacket = new MessagePacket<LoginRegisterRequestModel>(MessageType.LoginRequest, new LoginRegisterRequestModel() { Username = usernameTextBox.Text, Password = passwordTextBox.Text });
		await clientHandler.WriteMessage(loginRequestPacket);

		// Read login response from server
		MessagePacket<LoginRegisterResponseModel> response = await clientHandler.ReadMessage<LoginRegisterResponseModel>();

		if (response.Type == MessageType.Invalid)
			return;

		if (response.Type == MessageType.LoginReponse)
		{
			switch (response.Payload!.Status)
			{
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.None:
					loginButton.Text = "Critical: response - None";
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.Success:
					SuccessfulLogin(response.Payload!.AuthenticationToken);
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.UnknownError:
					loginButton.Text = "Unknown error occurred";
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.UsernameDoesNotExist:
					loginButton.Text = "Username does not exist";
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.WrongPassword:
					loginButton.Text = "Wrong password";
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.EmailNotConfirmed:
					OnEmailNotConfirmed?.Invoke();
					loginButton.Text = "Email not confirmed";
					break;
			}
		}

		// Disconnect at the end of the request
		clientHandler.Disconnect();
	}

	private void SuccessfulLogin(string authenticationToken)
	{
		OnSuccessfulLogin?.Invoke();
		AuthenticationTokenHolder.AuthenticationToken = authenticationToken;
		ResetLoginButtonTextAfterSeconds(5);
	}

	private async void ResetLoginButtonTextAfterSeconds(int seconds)
	{
		await Task.Delay(TimeSpan.FromSeconds(seconds));
		loginButton.Text = "Login";
	}
}