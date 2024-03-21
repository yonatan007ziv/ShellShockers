using GameEngine.Components;
using GameEngine.Core.Components;
using ShellShockers.Client.Components.Networking;
using ShellShockers.Client.GameComponents.UIComponents.Views;

namespace ShellShockers.Client.Scenes;

internal class LoginRegisterScene : Scene
{
	private readonly LoginViewControl _loginView;
	private readonly RegisterViewControl _registerView;

	public LoginRegisterScene()
	{
		// Client factory
		TcpClientFactory clientFactory = new TcpClientFactory();

		// Add ui camera
		UICameras.Add((new UICamera(), new ViewPort(0.5f, 0.5f, 1, 1)));

		// Add login view
		_loginView = new LoginViewControl(clientFactory);
		_loginView.RegisterView(UIObjects);

		// Add register view
		_registerView = new RegisterViewControl(clientFactory);
		_registerView.RegisterView(UIObjects);
	}
}