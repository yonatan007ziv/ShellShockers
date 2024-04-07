using GameEngine.Core.SharedServices.Implementations.Loggers;
using GameEngine.Core.SharedServices.Interfaces;
using ShellShockers.Client.Components.Networking;
using ShellShockers.Core.Utilities.Serializers;

namespace ShellShockers.Client.Components;

internal static class Factories
{
	public static IFactory<TcpClientHandler> ClientFactory { get; }

	static Factories()
	{
		ClientFactory = new TcpClientFactory(new JsonSerializer(), new ConsoleLogger());
	}
}