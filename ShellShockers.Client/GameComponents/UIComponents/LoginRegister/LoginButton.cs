using GameEngine.Components.UIComponents;

namespace ShellShockers.Client.GameComponents.UIComponents.LoginRegister;

internal class LoginButton : UIButton
{
	private readonly Action onLoginClicked;

	protected override void OnFullClicked()
	{
		base.OnFullClicked();
		onLoginClicked?.Invoke();
	}

	public LoginButton(Action onClicked)
		: base("Blue")
	{
		TextData.Text = "Login";
		this.onLoginClicked = onClicked;
	}
}