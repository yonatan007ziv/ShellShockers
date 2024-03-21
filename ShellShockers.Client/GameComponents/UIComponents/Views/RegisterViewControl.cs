using GameEngine.Components.UIComponents;
using GameEngine.Core.SharedServices.Interfaces;
using ShellShockers.Client.Components.Networking;
using ShellShockers.Client.GameComponents.UIComponents.LoginRegister;
using ShellShockers.Client.GameComponents.UIComponents.Views.Base;

namespace ShellShockers.Client.GameComponents.UIComponents.Views;

internal class RegisterViewControl : BaseView
{
	public RegisterViewControl(IFactory<TcpClientHandler> clientFactory)
	{
		UIButton registerButton = new RegisterButton();
		registerButton.Transform.Scale /= 5;
		registerButton.Transform.Position = new System.Numerics.Vector3(-0.5f, -0.25f, 5f);
		uiObjects.Add(registerButton);
	}
}