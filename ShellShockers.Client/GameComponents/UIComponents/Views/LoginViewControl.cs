using GameEngine.Components.UIComponents;
using GameEngine.Core.SharedServices.Interfaces;
using ShellShockers.Client.Components.Networking;
using ShellShockers.Client.GameComponents.UIComponents.LoginRegister;
using ShellShockers.Client.GameComponents.UIComponents.Views.Base;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;
using System.Net;

namespace ShellShockers.Client.GameComponents.UIComponents.Views;

internal class LoginViewControl : BaseView
{
	private readonly IFactory<TcpClientHandler> clientFactory;

	private UILabel usernameLabel;
	private UITextBox usernameTextBox;
	private UILabel passwordLabel;
	private UITextBox passwordTextBox;
	private LoginButton loginButton;
	private UILabel loginResultLabel;

	public LoginViewControl(IFactory<TcpClientHandler> clientFactory)
	{
		// Username label
		usernameLabel = new UILabel();
		usernameLabel.Text = "Username:";
		usernameLabel.Transform.Scale /= 5;
		usernameLabel.Transform.Position = new System.Numerics.Vector3(0, 0.75f, 5f);
		uiObjects.Add(usernameLabel);

		// Username text box
		usernameTextBox = new UITextBox("TextBox.mat");
		usernameTextBox.Transform.Scale /= 5;
		usernameTextBox.Transform.Position = new System.Numerics.Vector3(0.5f, 0.75f, 5f);
		uiObjects.Add(usernameTextBox);

		// Password label
		passwordLabel = new UILabel();
		passwordLabel.Text = "Password:";
		passwordLabel.Transform.Scale /= 5;
		passwordLabel.Transform.Position = new System.Numerics.Vector3(0, 0.25f, 5f);
		uiObjects.Add(passwordLabel);

		// Password text box
		passwordTextBox = new UITextBox("TextBox.mat");
		passwordTextBox.Transform.Scale /= 5;
		passwordTextBox.Transform.Position = new System.Numerics.Vector3(0.5f, 0.25f, 5f);
		uiObjects.Add(passwordTextBox);

		// Login button
		loginButton = new LoginButton();
		loginButton.Transform.Scale /= 5;
		loginButton.Transform.Position = new System.Numerics.Vector3(0.5f, -0.25f, 5f);
		uiObjects.Add(loginButton);

		// Login result label
		loginResultLabel = new UILabel();
		loginResultLabel.Transform.Scale /= 5;
		loginResultLabel.Transform.Position = new System.Numerics.Vector3(0.5f, -0.5f, 5f);
		uiObjects.Add(loginResultLabel);

		// Button clicked
		loginButton.OnFullClicked += OnLoginButtonClicked;

		this.clientFactory = clientFactory;
	}

	private async void OnLoginButtonClicked()
	{
		loginButton.Enabled = false;
		await LoginProcedure();
		loginButton.Enabled = true;
	}

	private async Task LoginProcedure()
	{
		if (!clientFactory.Create(out TcpClientHandler clientHandler))
		{
			loginResultLabel.Text = "Error creating a TcpClientHandler";
			Console.WriteLine(loginResultLabel.Text);
			return;
		}

		if (usernameTextBox.Text.Length == 0)
		{
			loginResultLabel.Text = "Invalid username";
			Console.WriteLine(loginResultLabel.Text);
			return;
		}

		if (passwordTextBox.Text.Length == 0)
		{
			loginResultLabel.Text = "Invalid password";
			Console.WriteLine(loginResultLabel.Text);
			return;
		}

		if (!await clientHandler.Connect(IPAddress.Parse(ServerAddresses.LoginRegisterServerAddress), ServerAddresses.LoginRegisterServerPort))
		{
			loginResultLabel.Text = "Error connecting to server";
			Console.WriteLine(loginResultLabel.Text);
			return;
		}

		await clientHandler.WriteMessage<LoginRequestModel>(new MessagePacket<LoginRequestModel>(MessageType.LoginRequest, new LoginRequestModel() { Username = usernameTextBox.Text, Password = passwordTextBox.Text }));
		MessagePacket<LoginResponseModel> response = await clientHandler.ReadMessage<LoginResponseModel>();

		// Disconnect at the end of the request: temp
		clientHandler.Disconnect();
	}
}