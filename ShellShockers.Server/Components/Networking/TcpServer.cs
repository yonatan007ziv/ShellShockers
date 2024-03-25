using ShellShockers.Core.Services;
using ShellShockers.Core.Utilities.Serializers;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;

namespace ShellShockers.Server.Components.Networking;

internal class TcpServer
{
	private readonly TcpListener listener;

	public bool Run { get; set; }
	public ObservableCollection<TcpClientHandler> Clients { get; }

    public TcpServer(System.Net.IPAddress address, int port)
    {
		listener = new TcpListener(address, port);
		Clients = new ObservableCollection<TcpClientHandler>();
	}

	public async Task Start()
	{
		Run = true;

		listener.Start();
		await AcceptClients();
	}

	private async Task AcceptClients()
	{
		ClientStatusPoller();

		while (Run)
		{
			// TODO: DDoS and DoS blocker

			TcpClientHandler tcpClientHandler = new TcpClientHandler(await listener.AcceptTcpClientAsync(), new JsonSerializer(), new ConsoleLogger());
			Clients.Add(tcpClientHandler);
		}
	}

	private async void ClientStatusPoller()
	{
		while (Run)
		{
            await Console.Out.WriteLineAsync("Polling connections...");

            for (int i = 0; i < Clients.Count; i++)
				if (Clients[i].Socket.Client.Poll(1, SelectMode.SelectRead) && !Clients[i].Socket.GetStream().DataAvailable)
					Clients.RemoveAt(i--);

			await Task.Delay(1000);
		}
	}
}