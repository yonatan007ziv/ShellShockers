namespace ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;

public class NotARobotModel
{
	public string Username { get; set; } = null!;
	public string AuthenticationToken { get; set; } = null!;
	public bool Success { get; set; }
	public bool[] SelectedSquares { get; set; } = null!;
}