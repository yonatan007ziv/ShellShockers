using GameEngine;
using GameEngine.Services.Interfaces;
using ShellShockers.Client.Scenes;

namespace ShellShockers.Client;

internal class Program
{
	public static void Main()
	{
		IGameEngine GameEngine = new GameEngineProvider().UseSilkOpenGL().BuildEngine();

		GameEngine.Title = "ShellShockers";
		GameEngine.SetResourceFolder(@$"{Directory.GetCurrentDirectory()}\Resources");
		GameEngine.SetBackgroundColor(System.Drawing.Color.CadetBlue);

		GameEngine.LogRenderingLogs = false;

		new LoginRegisterScene().LoadScene();

		GameEngine.Run();
	}
}