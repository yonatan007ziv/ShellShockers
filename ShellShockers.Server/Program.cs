using ShellShockers.Server.Servers;
using System.Net;

namespace ShellShockers.Server;

internal class Program
{
	public static async Task Main()
	{
		Task loginServer = new LoginRegisterServer(IPAddress.Parse("127.0.0.1"), 5001).Start();
		Task gameplayServer = new GameplayServer(IPAddress.Parse("127.0.0.1"), 5000).Start();

		// Wait for both servers to execute to completion
		await Task.WhenAll(loginServer, gameplayServer);
	}
}