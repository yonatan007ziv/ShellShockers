using ShellShockers.Core.Utilities.Networking.CommunicationProtocols;
using ShellShockers.Core.Utilities.Networking.CommunicationProtocols.Models;

namespace ShellShockers.Server.Components.Networking;

internal static class DoSChecker
{
	// 10 requests per 5 minutes to the user
	private const int NumOfAllowedRequests = 10;
	private static readonly TimeSpan TimeUntilForgetsRequests = TimeSpan.FromMinutes(5);
	private static readonly Dictionary<TcpClientHandler, int> clientTracker = new Dictionary<TcpClientHandler, int>();

	public static bool CheckHealthy(TcpClientHandler tcpHandler)
	{
		if (clientTracker.ContainsKey(tcpHandler))
		{
			clientTracker[tcpHandler]++;

			if (clientTracker[tcpHandler] > NumOfAllowedRequests)
			{
				_ = tcpHandler.WriteMessage(new MessagePacket<EmptyMessageModel>(MessageType.DoSResponse, null));
				ReduceAfterTime(tcpHandler);
				return false;
			}
		}
		else
			clientTracker[tcpHandler] = 1;

		ReduceAfterTime(tcpHandler);
		return true;
	}

	private static async void ReduceAfterTime(TcpClientHandler tcpToReduce)
	{
		await Task.Delay(TimeUntilForgetsRequests);
		clientTracker[tcpToReduce]--;
	}
}