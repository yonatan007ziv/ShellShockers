using GameEngine.Components.UIComponents;
using GameEngine.Core.SharedServices.Interfaces;
using ShellShockers.Client.Components.Networking;
using ShellShockers.Client.GameComponents.UIComponents.LoginRegister;
using ShellShockers.Client.GameComponents.UIComponents.Views.Base;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols;
using System.Net;

namespace ShellShockers.Client.GameComponents.UIComponents.Views;

internal class RegisterViewControl : BaseView
{
	private IFactory<TcpClientHandler> clientFactory;

	private UILabel usernameLabel;
	private UITextBox usernameTextBox;
	private UILabel passwordLabel;
	private UITextBox passwordTextBox;
	private UILabel emailLabel;
	private UITextBox emailTextBox;
	private LoginButton registerButton;
	private UILabel registerResultLabel;

	public RegisterViewControl(IFactory<TcpClientHandler> clientFactory)
	{
		// Username label
		usernameLabel = new UILabel();
		usernameLabel.Text = "Username:";
		usernameLabel.Transform.Scale /= 5;
		usernameLabel.Transform.Position = new System.Numerics.Vector3(-1, 0.75f, 5f);
		uiObjects.Add(usernameLabel);

		// Username text box
		usernameTextBox = new UITextBox("TextBox.mat");
		usernameTextBox.Transform.Scale /= 5;
		usernameTextBox.Transform.Position = new System.Numerics.Vector3(-0.5f, 0.75f, 5f);
		uiObjects.Add(usernameTextBox);

		// Password label
		passwordLabel = new UILabel();
		passwordLabel.Text = "Password:";
		passwordLabel.Transform.Scale /= 5;
		passwordLabel.Transform.Position = new System.Numerics.Vector3(-1, 0.25f, 5f);
		uiObjects.Add(passwordLabel);

		// Password text box
		passwordTextBox = new UITextBox("TextBox.mat");
		passwordTextBox.Transform.Scale /= 5;
		passwordTextBox.Transform.Position = new System.Numerics.Vector3(-0.5f, 0.25f, 5f);
		uiObjects.Add(passwordTextBox);

		// Email label
		emailLabel = new UILabel();
		emailLabel.Text = "Email:";
		emailLabel.Transform.Scale /= 5;
		emailLabel.Transform.Position = new System.Numerics.Vector3(-1, -0.25f, 5f);
		uiObjects.Add(emailLabel);

		// Email text box
		emailTextBox = new UITextBox("TextBox.mat");
		emailTextBox.Transform.Scale /= 5;
		emailTextBox.Transform.Position = new System.Numerics.Vector3(-0.5f, -0.25f, 5f);
		uiObjects.Add(emailTextBox);

		// Login button
		registerButton = new LoginButton();
		registerButton.Transform.Scale /= 5;
		registerButton.Transform.Position = new System.Numerics.Vector3(-0.5f, -0.75f, 5f);
		uiObjects.Add(registerButton);

		// Login result label
		registerResultLabel = new UILabel();
		registerResultLabel.Transform.Scale /= 5;
		registerResultLabel.Transform.Position = new System.Numerics.Vector3(-0.5f, -0.5f, 5f);
		uiObjects.Add(registerResultLabel);

		// Button clicked
		registerButton.OnFullClicked += OnRegisterButtonClicked;

		this.clientFactory = clientFactory;
	}

	private async void OnRegisterButtonClicked()
	{
		registerButton.Enabled = false;
		await RegisterProcedure();
		registerButton.Enabled = true;
	}

	private async Task RegisterProcedure()
	{
		if (!clientFactory.Create(out TcpClientHandler clientHandler))
		{
			registerResultLabel.Text = "Error creating a TcpClientHandler";
			Console.WriteLine(registerResultLabel.Text);
			return;
		}

		if (usernameTextBox.Text.Length == 0)
		{
			registerResultLabel.Text = "Invalid username";
			Console.WriteLine(registerResultLabel.Text);
			return;
		}

		if (passwordTextBox.Text.Length == 0)
		{
			registerResultLabel.Text = "Invalid password";
			Console.WriteLine(registerResultLabel.Text);
			return;
		}

		if (emailTextBox.Text.Length == 0)
		{
			registerResultLabel.Text = "Invalid email";
			Console.WriteLine(registerResultLabel.Text);
			return;
		}

		if (!await clientHandler.Connect(IPAddress.Parse(ServerAddresses.LoginRegisterServerAddress), ServerAddresses.LoginRegisterServerPort))
		{
			registerResultLabel.Text = "Error connecting to server";
			Console.WriteLine(registerResultLabel.Text);
			return;
		}

		// Write login request to server 
		MessagePacket<LoginRegisterRequestModel> loginRequestPacket = new MessagePacket<LoginRegisterRequestModel>(MessageType.RegisterRequest, new LoginRegisterRequestModel() { Username = usernameTextBox.Text, Password = passwordTextBox.Text, Email = emailTextBox.Text });
		await clientHandler.WriteMessage<LoginRegisterRequestModel>(loginRequestPacket);

		// Read login response from server
		MessagePacket<LoginRegisterResponseModel> response = await clientHandler.ReadMessage<LoginRegisterResponseModel>();

		if (response.Type == MessageType.Invalid)
			return;

		if (response.Type == MessageType.LoginReponse)
		{
			switch (response.Payload!.Status)
			{
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.None:
					registerResultLabel.Text = "Critical: response - None";
					Console.WriteLine(registerResultLabel.Text);
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.Success:
					registerResultLabel.Text = "Error connecting to server";
					Console.WriteLine(registerResultLabel.Text);
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.UnknownError:
					registerResultLabel.Text = "Unknown error occurred";
					Console.WriteLine(registerResultLabel.Text);
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.UsernameDoesNotExist:
					registerResultLabel.Text = "Username does not exist";
					Console.WriteLine(registerResultLabel.Text);
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.WrongPassword:
					registerResultLabel.Text = "Wrong password";
					Console.WriteLine(registerResultLabel.Text);
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.EmailNotConfirmed:
					registerResultLabel.Text = "Email not confirmed";
					Console.WriteLine(registerResultLabel.Text);
					break;
			}
		}

		// Disconnect at the end of the request
		clientHandler.Disconnect();
	}
}