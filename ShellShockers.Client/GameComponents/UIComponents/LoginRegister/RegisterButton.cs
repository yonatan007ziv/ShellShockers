using GameEngine.Components.UIComponents;

namespace ShellShockers.Client.GameComponents.UIComponents.LoginRegister;

internal class RegisterButton : UIButton
{
	protected override void OnFullClicked()
	{
		base.OnFullClicked();
		Console.WriteLine("Clicked register");
	}

	public RegisterButton()
		: base("Green")
	{
		TextData.Text = "Register";
	}
}