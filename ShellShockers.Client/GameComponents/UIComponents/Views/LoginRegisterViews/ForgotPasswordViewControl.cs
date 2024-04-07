using GameEngine.Components.UIComponents;
using GameEngine.Core.Components.Objects;
using ShellShockers.Client.Components;
using ShellShockers.Client.Components.Networking;
using ShellShockers.Core.Utilities.InputValidators;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;
using System.Net;
using System.Numerics;

namespace ShellShockers.Client.GameComponents.UIComponents.Views.LoginRegisterViews;

internal class ForgotPasswordViewControl : UIObject
{
	public UIButton switchToLoginButton;
	private UILabel emailLabel;
	private UITextBox emailTextBox;
	private UIButton confirmForgotPasswordButton;

	public ForgotPasswordViewControl()
	{
		Vector3 fieldScale = new Vector3(0.25f, 0.15f, 1);
		float difference = 0.01f;

		// Email label
		emailLabel = new UILabel();
		emailLabel.Text = "Email:";
		emailLabel.Transform.Scale = fieldScale;
		emailLabel.Transform.Position = new Vector3(-(fieldScale.X + difference), 0.75f, 0);
		Children.Add(emailLabel);

		// Email text box
		emailTextBox = new UITextBox();
		emailTextBox.TextColor = System.Drawing.Color.White;
		emailTextBox.Transform.Scale = fieldScale;
		emailTextBox.Transform.Position = new Vector3(fieldScale.X + difference, 0.75f, 0);
		Children.Add(emailTextBox);

		// Confirm forgot password button
		confirmForgotPasswordButton = new UIButton();
		confirmForgotPasswordButton.TextColor = System.Drawing.Color.White;
		confirmForgotPasswordButton.Text = "Confirm forgot password";
		confirmForgotPasswordButton.Transform.Scale = fieldScale;
		confirmForgotPasswordButton.Transform.Position = new Vector3(0, 0, 5f);
		Children.Add(confirmForgotPasswordButton);

		// Switch to login button
		switchToLoginButton = new UIButton();
		switchToLoginButton.TextColor = System.Drawing.Color.White;
		switchToLoginButton.OnFullClicked += () => { ResetTextBoxes(); Visible = false; };
		switchToLoginButton.Text = "Return to login";
		switchToLoginButton.Transform.Scale = fieldScale;
		switchToLoginButton.Transform.Position = new Vector3(0, -0.75f, 5f);
		Children.Add(switchToLoginButton);

		// Forgot password button clicked
		confirmForgotPasswordButton.OnFullClicked += OnForgotPasswordButtonClicked;
	}

	private void ResetTextBoxes()
	{
		emailTextBox.Text = "";
	}

	private async void OnForgotPasswordButtonClicked()
	{
		confirmForgotPasswordButton.Enabled = false;
		await ForgotPasswordProcedure();
		confirmForgotPasswordButton.Enabled = true;
	}

	private async Task ForgotPasswordProcedure()
	{
		if (!Factories.ClientFactory.Create(out TcpClientHandler clientHandler))
		{
			switchToLoginButton.Text = "Error creating a TcpClientHandler";
			return;
		}

		if (!LoginRegisterInputPredicates.EmailValid(emailTextBox.Text))
		{
			switchToLoginButton.Text = "Invalid email";
			return;
		}

		if (!await clientHandler.Connect(IPAddress.Parse(ServerAddresses.LoginRegisterServerAddress), ServerAddresses.LoginRegisterServerPort))
		{
			switchToLoginButton.Text = "Error connecting to server";
			return;
		}

		// Write login request to server 
		MessagePacket<LoginRegisterRequestModel> forgotPasswordRequestPacket = new MessagePacket<LoginRegisterRequestModel>(MessageType.ForgotPasswordRequest, new LoginRegisterRequestModel() { Email = emailTextBox.Text });
		await clientHandler.WriteMessage(forgotPasswordRequestPacket);

		// Read login response from server
		MessagePacket<LoginRegisterResponseModel> response = await clientHandler.ReadMessage<LoginRegisterResponseModel>();

		if (response.Type == MessageType.Invalid)
			return;

		bool unsuccessfulRequest = true;
		if (response.Type == MessageType.ForgotPasswordReponse)
		{
			switch (response.Payload!.Status)
			{
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.None:
					switchToLoginButton.Text = "Critical: response - None";
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.Success:
					unsuccessfulRequest = false;
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.UnknownError:
					switchToLoginButton.Text = "Unknown error occurred";
					break;
			}
		}

		//if (unsuccessfulRequest)
		//	ResetRegisterButtonTextAfterSeconds(5);

		if (unsuccessfulRequest)
			switchToLoginButton.Text = "Successfully sent a password-reset email";

		// Disconnect at the end of the request
		clientHandler.Disconnect();
	}
}