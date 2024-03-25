using GameEngine.Components.UIComponents;

namespace ShellShockers.Client.GameComponents.UIComponents.LoginRegister;

internal class RegisterButton : UIButton
{
	public RegisterButton()
		: base("Green.mat")
	{
		Text = "Register";
	}
}