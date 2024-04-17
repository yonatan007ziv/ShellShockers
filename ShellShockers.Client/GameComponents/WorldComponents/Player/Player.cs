namespace ShellShockers.Client.GameComponents.WorldComponents.Player;

internal class Player
{
	public readonly PlayerWorld playerWorld;
	public readonly PlayerUI playerUI;

    public Player(bool localPlayer)
    {
        playerWorld = new PlayerWorld(localPlayer);
		playerUI = new PlayerUI(localPlayer);
    }
}