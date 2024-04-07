namespace ShellShockers.Core.Utilities.Models;

public class LobbyModel
{
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public string HostName { get; set; } = "";
	public int CurrentPlayerCount { get; set; }
	public int MaxPlayerCount { get; set; }
}