using ShellShockers.Server.Components.Database;
using ShellShockers.Server.Components.Networking;
using ShellShockers.Server.Components.Networking.ClientHandlers;
using System.Net;

namespace ShellShockers.Server;

internal class Program
{
	public static async Task Main()
	{
		Task loginServer = new TcpServer<LoginRegisterClientHandler>(IPAddress.Parse("127.0.0.1"), 5001).Start();
		Task gameplayServer = new TcpServer<GameplayClientHandler>(IPAddress.Parse("127.0.0.1"), 5000).Start();

		// Wait for both servers to execute to completion
		await Task.WhenAll(loginServer, gameplayServer);
	}
}