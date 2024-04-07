using ShellShockers.Core.Services;
using ShellShockers.Core.Utilities.Serializers;
using ShellShockers.Server.Components.Networking.ClientHandlers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net.Sockets;

namespace ShellShockers.Server.Components.Networking;

internal class TcpServer<TClientHandler> where TClientHandler : BaseClientHandler, new()
{
	private readonly TcpListener listener;
	private readonly Dictionary<TcpClientHandler, BaseClientHandler> clients = new Dictionary<TcpClientHandler, BaseClientHandler>();

	private readonly TimeSpan DDosFreezeTime = TimeSpan.FromMinutes(1);

	public bool Run { get; set; }
	public ObservableCollection<TcpClientHandler> Clients { get; }

	public TcpServer(System.Net.IPAddress address, int port)
	{
		listener = new TcpListener(address, port);
		Clients = new ObservableCollection<TcpClientHandler>();
		Clients.CollectionChanged += ClientJoinedLeft;
	}


	private void ClientJoinedLeft(object? s, NotifyCollectionChangedEventArgs e)
	{
		// Client was added
		if (e.Action == NotifyCollectionChangedAction.Add)
			foreach (object clientHandler in e.NewItems!)
			{
				if (clientHandler is TcpClientHandler tcpClientHandler)
					OnClientJoin(tcpClientHandler);
			}
		// Client was Removed
		else if (e.Action == NotifyCollectionChangedAction.Remove)
			foreach (object clientHandler in e.OldItems!)
			{
				if (clientHandler is TcpClientHandler tcpClientHandler)
					OnClientLeave(tcpClientHandler);
			}
	}

	private void OnClientJoin(TcpClientHandler tcpClientHandler)
	{
		TClientHandler client = new TClientHandler();

		client.OnDisconnect += () => Clients.Remove(tcpClientHandler);

		client.TcpClientHandler = tcpClientHandler;

		client.Connected = true;
		client.StartRead();

		clients.Add(tcpClientHandler, client);
	}

	private void OnClientLeave(TcpClientHandler tcpClientHandler)
	{
		clients[tcpClientHandler].Connected = false;
		clients.Remove(tcpClientHandler);
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
			TcpClient tcpSocket = await listener.AcceptTcpClientAsync();
			TcpClientHandler tcpClientHandler = new TcpClientHandler(tcpSocket, new JsonSerializer(), new ConsoleLogger());

			if (!DDoSChecker.CheckHealthy())
				await FreezeServerForTime();

			if (DoSChecker.CheckHealthy(tcpClientHandler))
				Clients.Add(tcpClientHandler);
		}
	}

	private async Task FreezeServerForTime()
	{
		await Task.Delay(DDosFreezeTime);

		// Ignore all buffered requests
		while (listener.Pending())
		{
			TcpClient ignoredClient = await listener.AcceptTcpClientAsync();
			ignoredClient.Close();
		}
	}

	private async void ClientStatusPoller()
	{
		while (Run)
		{
			for (int i = 0; i < Clients.Count; i++)
				if (Clients[i].Socket.Client.Poll(1, SelectMode.SelectRead) && !Clients[i].Socket.GetStream().DataAvailable)
					try
					{
						Clients[i].Disconnect();
						Clients.RemoveAt(i--);
					}
					catch { }

			await Task.Delay(1000);
		}
	}
}