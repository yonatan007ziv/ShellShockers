using GameEngine.Components.UIComponents;
using GameEngine.Core.Components.Objects;
using ShellShockers.Client.Components;
using ShellShockers.Client.Components.Networking;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;
using System.Drawing;
using System.Net;
using System.Numerics;

namespace ShellShockers.Client.GameComponents.UIComponents.Views.LoginRegisterViews;

internal class NotARobotViewControl : UIObject
{
	private UIButton? confirmNotARobot;
	private NotARobotSquare[] squares;
	private TcpClientHandler clientHandler;

	public event Action? OnSuccess;
	public event Action? OnFail;

	public NotARobotViewControl()
	{
		ResetView();
	}

	public void ResetView()
	{
		UILabel title = new UILabel();
		title.Transform.Position = new Vector3(0, 0.75f, 0);
		title.Transform.Scale = new Vector3(1, 0.5f, 0);
		title.Text = "Not a Robot\nplease make all of the squares green\nAnd then confirm";
		title.TextColor = Color.White;
		Children.Add(title);

		if (confirmNotARobot is not null)
			confirmNotARobot.OnFullClicked -= ConfirmNotARobotButton;
	}

	public async void RequestNotARobotPuzzle()
	{
		if (!Factories.ClientFactory.Create(out clientHandler)
			|| !await clientHandler.Connect(IPAddress.Parse(ServerAddresses.LoginRegisterServerAddress), ServerAddresses.LoginRegisterServerPort))
			return;

		MessagePacket<NotARobotModel> messagePacket = new MessagePacket<NotARobotModel>(MessageType.NotARobotRequest);
		await clientHandler.WriteMessage(messagePacket);

		NotARobotModel model = (await clientHandler.ReadMessage<NotARobotModel>()).Payload!;

		// Construct squares
		CreateSquares(model.SelectedSquares);
	}

	private void CreateSquares(bool[] predeterminedButtons)
	{
		Vector3 startPosition = new Vector3(-0.25f, 0.25f, 0);
		float horizontalJump = 0.25f;
		float verticalJump = -0.25f;

		Vector3 squareSize = new Vector3(0.1f, 0.1f, 0);
		Vector3 position = startPosition;
		squares = new NotARobotSquare[predeterminedButtons.Length];

		for (int i = 0; i < predeterminedButtons.Length; i++)
		{
			bool dropDown = i % 3 == 0 && i != 0;
			if (dropDown)
			{
				position += Vector3.UnitY * verticalJump;
				position = new Vector3(startPosition.X, position.Y, 0);
			}

			squares[i] = new NotARobotSquare(predeterminedButtons[i]);
			squares[i].Transform.Scale = squareSize;
			squares[i].Transform.Position = position;
			Children.Add(squares[i]);


			position += Vector3.UnitX * horizontalJump;
		}

		confirmNotARobot = new UIButton();
		confirmNotARobot.OnFullClicked += ConfirmNotARobotButton;
		confirmNotARobot.Transform.Scale /= 5;
		confirmNotARobot.Transform.Position = new Vector3(0, -0.75f, 0);
		confirmNotARobot.Text = "Confirm not a Robot";
		confirmNotARobot.TextColor = Color.White;
		Children.Add(confirmNotARobot);
	}

	private async void ConfirmNotARobotButton()
	{
		confirmNotARobot!.Enabled = false;

		bool[] selectedSquares = new bool[squares.Length];
		for (int i = 0; i < selectedSquares.Length; i++)
			selectedSquares[i] = squares[i].SquareEnabled;

		await clientHandler.WriteMessage(new MessagePacket<NotARobotModel>(MessageType.NotARobotResponse, new NotARobotModel() { SelectedSquares = selectedSquares, Username = SessionHolder.Username }));
		MessagePacket<NotARobotModel> messageNotARobotMessage = await clientHandler.ReadMessage<NotARobotModel>();

		if (messageNotARobotMessage.Payload!.Success)
		{
			SessionHolder.AuthenticationToken = messageNotARobotMessage.Payload.AuthenticationToken;
			OnSuccess?.Invoke();
		}
		else
			OnFail?.Invoke();

		Visible = false;
		confirmNotARobot.Enabled = true;
		clientHandler.Disconnect();
	}
}