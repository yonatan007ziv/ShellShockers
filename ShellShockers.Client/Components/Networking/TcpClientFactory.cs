using GameEngine.Core.SharedServices.Implementations.Loggers;
using GameEngine.Core.SharedServices.Interfaces;
using ShellShockers.Core.Utilities.Serializers;
using System.Net.Sockets;

namespace ShellShockers.Client.Components.Networking;

internal class TcpClientFactory : IFactory<TcpClientHandler>
{
	private readonly JsonSerializer jsonSerializer;
	private readonly ConsoleLogger consoleLogger;

	public TcpClientFactory(JsonSerializer jsonSerializer, ConsoleLogger consoleLogger)
	{
		this.jsonSerializer = jsonSerializer;
		this.consoleLogger = consoleLogger;
	}

	public bool Create(out TcpClientHandler result)
	{
		result = new TcpClientHandler(new TcpClient(), jsonSerializer, consoleLogger);
		return true;
	}
}