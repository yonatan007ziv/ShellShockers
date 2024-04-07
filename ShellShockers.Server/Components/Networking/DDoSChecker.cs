namespace ShellShockers.Server.Components.Networking;

internal class DDoSChecker
{
	// 50 requests per minute as a whole to the server
	private const int NumOfAllowedRequests = 50;
	private static readonly TimeSpan TimeUntilForgetsRequests = TimeSpan.FromMinutes(1);
	private static int currentRequestCount = 0;

	public static bool CheckHealthy()
	{
		currentRequestCount++;
		ReduceAfterTime();

		if (currentRequestCount > NumOfAllowedRequests)
			return false;
		return true;
	}

	private static async void ReduceAfterTime()
	{
		await Task.Delay(TimeUntilForgetsRequests);
		currentRequestCount--;
	}
}