using GameEngine.Components.UIComponents;
using ShellShockers.Core.Utilities.Models;
using System.Drawing;
using System.Numerics;

namespace ShellShockers.Client.GameComponents.UIComponents.Views.MainMenu.LobbySelection;

internal class LobbySelectionEntry : UIButton
{
	public int LobbyId { get; }

	public LobbySelectionEntry(LobbyModel lobby)
	{
		LobbyId = lobby.Id;

		// Lobby name label
		UILabel lobbyNameLabel = new UILabel();
		lobbyNameLabel.Text = lobby.Name;
		lobbyNameLabel.TextColor = Color.White;
		lobbyNameLabel.Transform.Scale = new Vector3(0.2f, 1, 1);
		lobbyNameLabel.Transform.Position = new Vector3(-1 + 0.2f, 0, 0);
		Children.Add(lobbyNameLabel);

		// Host name label
		UILabel hostNameLabel = new UILabel();
		hostNameLabel.Text = lobby.HostName;
		hostNameLabel.TextColor = Color.White;
		hostNameLabel.Transform.Scale = new Vector3(0.2f, 1, 1);
		hostNameLabel.Transform.Position = new Vector3(-1 + 0.6f, 0, 0);
		Children.Add(hostNameLabel);

		// Current player count label
		UILabel currentPlayerCountLabel = new UILabel();
		currentPlayerCountLabel.Text = lobby.CurrentPlayerCount.ToString();
		currentPlayerCountLabel.TextColor = Color.White;
		currentPlayerCountLabel.Transform.Scale = new Vector3(0.2f, 1, 1);
		currentPlayerCountLabel.Transform.Position = new Vector3(-1 + 0.8f, 0, 0);
		Children.Add(currentPlayerCountLabel);

		// Max player count label
		UILabel maxPlayerCountLabel = new UILabel();
		maxPlayerCountLabel.Text = lobby.MaxPlayerCount.ToString();
		maxPlayerCountLabel.TextColor = Color.White;
		maxPlayerCountLabel.Transform.Scale = new Vector3(0.2f, 1, 1);
		maxPlayerCountLabel.Transform.Position = new Vector3(-1 + 1, 0, 0);
		Children.Add(maxPlayerCountLabel);
	}
}