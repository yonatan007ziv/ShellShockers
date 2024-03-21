using ShellShockers.Server.Servers;
using System.Net;

namespace ShellShockers.Server;

internal class Program
{
	public static async void Main()
	{
		Task loginServer = new LoginRegisterServer(IPAddress.Parse("localhost"), 5001).Run();
		Task gameplayServer = new GameplayServer(IPAddress.Parse("localhost"), 5000).Run();

		// Wait for both servers to execute to completion
		await Task.WhenAll(loginServer, gameplayServer);
	}
}