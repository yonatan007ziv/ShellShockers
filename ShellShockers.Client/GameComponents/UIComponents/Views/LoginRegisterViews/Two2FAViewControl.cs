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

internal class Two2FAViewControl : UIObject
{
	public UIButton switchToLoginButton;
	private UILabel usernameLabel;
	private UITextBox usernameTextBox;
	private UILabel twoFALabel;
	private UITextBox twoFATextBox;
	private UIButton confirm2FAButton;

	public Two2FAViewControl()
	{
		Vector3 fieldScale = new Vector3(0.25f, 0.15f, 1);
		float difference = 0.01f;

		// Username label
		usernameLabel = new UILabel();
		usernameLabel.Text = "Username:";
		usernameLabel.Transform.Scale = fieldScale;
		usernameLabel.Transform.Position = new Vector3(-(fieldScale.X + difference), 0.75f, 0);
		Children.Add(usernameLabel);

		// Username text box
		usernameTextBox = new UITextBox();
		usernameTextBox.TextColor = System.Drawing.Color.White;
		usernameTextBox.Transform.Scale = fieldScale;
		usernameTextBox.Transform.Position = new Vector3(fieldScale.X + difference, 0.75f, 0);
		Children.Add(usernameTextBox);

		// 2FA label
		twoFALabel = new UILabel();
		twoFALabel.Text = "Two FA Code:";
		twoFALabel.Transform.Scale = fieldScale;
		twoFALabel.Transform.Position = new Vector3(-(fieldScale.X + difference), 0.25f, 5f);
		Children.Add(twoFALabel);

		// 2FA text box
		twoFATextBox = new UITextBox();
		twoFATextBox.TextColor = System.Drawing.Color.White;
		twoFATextBox.Transform.Scale = fieldScale;
		twoFATextBox.Transform.Position = new Vector3(fieldScale.X + difference, 0.25f, 5f);
		Children.Add(twoFATextBox);

		// Confirm 2FA button
		confirm2FAButton = new UIButton();
		confirm2FAButton.TextColor = System.Drawing.Color.White;
		confirm2FAButton.OnFullClicked += OnConfirm2FAButton;
		confirm2FAButton.Text = "Confirm 2FA";
		confirm2FAButton.Transform.Scale = fieldScale;
		confirm2FAButton.Transform.Position = new Vector3(fieldScale.X + difference, -0.25f, 0);
		Children.Add(confirm2FAButton);

		// Back to login button
		switchToLoginButton = new UIButton();
		switchToLoginButton.TextColor = System.Drawing.Color.White;
		switchToLoginButton.OnFullClicked += () => { ResetTextBoxes(); Visible = false; };
		switchToLoginButton.Text = "Back to login";
		switchToLoginButton.Transform.Scale = fieldScale;
		switchToLoginButton.Transform.Position = new Vector3(-(fieldScale.X + difference), -0.25f, 0);
		Children.Add(switchToLoginButton);
	}

	private void ResetTextBoxes()
	{
		usernameTextBox.Text = "";
		twoFATextBox.Text = "";
	}

	private async void OnConfirm2FAButton()
	{
		confirm2FAButton.Enabled = false;
		await TwoFAProcedure();
		confirm2FAButton.Enabled = true;
	}

	private async Task TwoFAProcedure()
	{

		if (!Factories.ClientFactory.Create(out TcpClientHandler clientHandler))
		{
			confirm2FAButton.Text = "Error creating a TcpClientHandler";
			return;
		}

		if (!LoginRegisterInputPredicates.UsernameValid(usernameTextBox.Text))
		{
			confirm2FAButton.Text = "Invalid username";
			return;
		}

		if (!await clientHandler.Connect(IPAddress.Parse(ServerAddresses.LoginRegisterServerAddress), ServerAddresses.LoginRegisterServerPort))
		{
			confirm2FAButton.Text = "Error connecting to server";
			return;
		}

		// Write login request to server 
		MessagePacket<LoginRegisterRequestModel> loginRequestPacket = new MessagePacket<LoginRegisterRequestModel>(MessageType.TwoFARequest, new LoginRegisterRequestModel() { Username = usernameTextBox.Text, TwoFACode = twoFATextBox.Text });
		await clientHandler.WriteMessage(loginRequestPacket);

		// Read login response from server
		MessagePacket<LoginRegisterResponseModel> response = await clientHandler.ReadMessage<LoginRegisterResponseModel>();

		if (response.Type == MessageType.Invalid)
			return;

		bool unsuccessfulRequest = true;
		if (response.Type == MessageType.TwoFAResponse)
		{
			switch (response.Payload!.Status)
			{
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.None:
					confirm2FAButton.Text = "Critical: response - None";
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.Success:
					unsuccessfulRequest = false;
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.UnknownError:
					confirm2FAButton.Text = "Unknown error occurred";
					break;
				case Core.Utilities.Networking.CommunicationProtocols.Enums.LoginRegisterResponse.Wrong2FACode:
					confirm2FAButton.Text = "Wrong 2FA Code";
					break;
			}
		}

		if (!unsuccessfulRequest)
			confirm2FAButton.Text = "Email Confirmed";

		// Disconnect at the end of the request
		clientHandler.Disconnect();
	}
}