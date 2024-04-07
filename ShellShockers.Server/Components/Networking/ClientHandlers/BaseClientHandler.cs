namespace ShellShockers.Server.Components.Networking.ClientHandlers;

internal abstract class BaseClientHandler
{
	public TcpClientHandler TcpClientHandler { get; set; } = null!;
	public bool Connected { get; set; }
	public event Action? OnDisconnect;

	protected void Disconnect()
	{
		Connected = false;
		OnDisconnect?.Invoke();
	}

	public abstract void StartRead();
}