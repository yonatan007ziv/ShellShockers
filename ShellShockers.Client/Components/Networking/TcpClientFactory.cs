using GameEngine.Core.SharedServices.Implementations.Loggers;
using GameEngine.Core.SharedServices.Interfaces;
using ShellShockers.Core.Utilities.Serializers;
using System.Net.Sockets;

namespace ShellShockers.Client.Components.Networking;

internal class TcpClientFactory : IFactory<TcpClientHandler>
{
	public bool Create(out TcpClientHandler result)
	{
		result = new TcpClientHandler(new TcpClient(), new JsonSerializer(), new ConsoleLogger());
		return true;
	}
}