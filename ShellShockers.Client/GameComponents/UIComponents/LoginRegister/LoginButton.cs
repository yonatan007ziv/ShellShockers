using GameEngine.Components.UIComponents;

namespace ShellShockers.Client.GameComponents.UIComponents.LoginRegister;

internal class LoginButton : UIButton
{
	public LoginButton()
		: base("Blue.mat")
	{
		Text = "Login";
	}
}