using GameEngine.Components.UIComponents;
using GameEngine.Core.Components.Objects;
using ShellShockers.Client.Components;
using ShellShockers.Client.Components.Networking;
using ShellShockers.Core.Utilities.InputValidators;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Enums;
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
	public UILabel resultLabel;
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

		// Result label
		resultLabel = new UILabel();
		resultLabel.Transform.Scale = fieldScale;
		resultLabel.Transform.Position = new Vector3(0, -0.1f, 0);
		Children.Add(resultLabel);

		// Login button
		loginButton = new UIButton();
		loginButton.TextColor = Color.White;
		loginButton.Text = "Login";
		loginButton.Transform.Scale = fieldScale;
		loginButton.Transform.Position = new Vector3(fieldScale.X + difference, -0.45f, 0);
		Children.Add(loginButton);

		// Switch button
		switchToRegisterButton = new UIButton();
		switchToRegisterButton.TextColor = Color.White;
		switchToRegisterButton.OnFullClicked += () => { ResetTextBoxes(); Visible = false; };
		switchToRegisterButton.Text = "Register?";
		switchToRegisterButton.Transform.Scale = fieldScale;
		switchToRegisterButton.Transform.Position = new Vector3(-(fieldScale.X + difference), -0.45f, 5f);
		Children.Add(switchToRegisterButton);

		// Forgot password button
		forgotPasswordButton = new UIButton();
		forgotPasswordButton.TextColor = Color.White;
		forgotPasswordButton.Text = "Forgot password?";
		forgotPasswordButton.TextData.TextColor = Color.White;
		forgotPasswordButton.Transform.Scale = fieldScale;
		forgotPasswordButton.Transform.Position = new Vector3(0, -0.80f, 5f);
		Children.Add(forgotPasswordButton);

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
			resultLabel.Text = "Error creating a TcpClientHandler";
			return;
		}

		if (!LoginRegisterInputPredicates.UsernameValid(usernameTextBox.Text))
		{
			resultLabel.Text = "Invalid username";
			return;
		}

		if (!LoginRegisterInputPredicates.PasswordValid(passwordTextBox.Text))
		{
			resultLabel.Text = "Invalid password";
			return;
		}

		if (!await clientHandler.Connect(IPAddress.Parse(ServerAddresses.LoginRegisterServerAddress), ServerAddresses.LoginRegisterServerPort))
		{
			resultLabel.Text = "Error connecting to server";
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
				case LoginRegisterResponse.None:
					resultLabel.Text = "Critical: response - None";
					break;
				case LoginRegisterResponse.Success:
					resultLabel.Text = "Success, Confirm not a Robot";
					break;
				case LoginRegisterResponse.UnknownError:
					resultLabel.Text = "Unknown error occurred";
					break;
				case LoginRegisterResponse.UsernameDoesNotExist:
					resultLabel.Text = "Username does not exist";
					break;
				case LoginRegisterResponse.WrongPassword:
					resultLabel.Text = "Wrong password";
					break;
				case LoginRegisterResponse.EmailNotConfirmed:
					OnEmailNotConfirmed?.Invoke();
					resultLabel.Text = "Email not confirmed";
					break;
			}
		}

		if (response.Payload!.Status == LoginRegisterResponse.Success)
		{
			SessionHolder.Username = usernameTextBox.Text;
			Visible = false;
			OnSuccessfulLogin?.Invoke();
		}

		clientHandler.Disconnect();
	}
}