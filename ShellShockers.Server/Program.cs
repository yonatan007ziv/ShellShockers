using ShellShockers.Core.Utilities.Networking.CommunicationProtocols;
using ShellShockers.Server.Components.Networking;
using ShellShockers.Server.Components.Networking.ClientHandlers;
using System.Net;

namespace ShellShockers.Server;

internal class Program
{
	public static async Task Main()
	{
		Task loginServer = new TcpServer<LoginRegisterClientHandler>(IPAddress.Parse(ServerAddresses.LoginRegisterServerAddress), ServerAddresses.LoginRegisterServerPort).Start();
		Task gameplayServer = new TcpServer<GameplayClientHandler>(IPAddress.Parse(ServerAddresses.GameplayServerAddress), ServerAddresses.GameplayServerPort).Start();

		// Wait for both servers to execute to completion
		await Task.WhenAll(loginServer, gameplayServer);
	}
}