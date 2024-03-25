using ShellShockers.Core.Utilities.Networking.CommunicationProtocols;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;
using ShellShockers.Server.Components.Networking;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;

namespace ShellShockers.Server.Servers;

internal class LoginRegisterServer
{
	private readonly TcpServer server;

	public LoginRegisterServer(IPAddress address, int port)
	{
		server = new TcpServer(address, port);
		server.Clients.CollectionChanged += ClientJoinedLeft;
	}

	public async Task Start()
		=> await server.Start();

	private void ClientJoinedLeft(object? s, NotifyCollectionChangedEventArgs e)
	{
		// Client was added
		if (e.Action == NotifyCollectionChangedAction.Add)
		{
			foreach (object clientHandler in e.NewItems!)
			{
				if (clientHandler is TcpClientHandler tcpClientHandler)
				{
					ClientRead(tcpClientHandler);
                    // tcpClientHandler joined
                    Console.WriteLine("Joined");
                }
			}
		}
		// Client was Removed
		else if (e.Action == NotifyCollectionChangedAction.Remove)
		{
			foreach (object clientHandler in e.OldItems!)
			{
				if (clientHandler is TcpClientHandler tcpClientHandler)
				{
					// tcpClientHandler left
					Console.WriteLine("Left");
				}
			}
		}
	}

	private async void ClientRead(TcpClientHandler tcpClientHandler)
	{
		MessagePacket<LoginRequestModel> request = await tcpClientHandler.ReadMessage<LoginRequestModel>();
        await Console.Out.WriteLineAsync(request.Payload.Username);
        await Console.Out.WriteLineAsync(request.Payload.Password);
	}
}