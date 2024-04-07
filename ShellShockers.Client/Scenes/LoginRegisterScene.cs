using GameEngine.Components;
using GameEngine.Core.Components;
using ShellShockers.Client.GameComponents.UIComponents.Views.LoginRegisterViews;

namespace ShellShockers.Client.Scenes;

internal class LoginRegisterScene : Scene
{
	private readonly LoginViewControl _loginView;
	private readonly RegisterViewControl _registerView;
	private readonly ForgotPasswordViewControl _forgotPasswordView;
	private readonly Two2FAViewControl _twoFAView;

	public LoginRegisterScene()
	{
		// Add ui camera
		UICameras.Add((new UICamera(), new ViewPort(0.5f, 0.5f, 1, 1)));

		// Login view
		_loginView = new LoginViewControl();
		_loginView.OnEmailNotConfirmed += () => { _loginView.Visible = false; SwitchTo2FA(); };
		_loginView.switchToRegisterButton.OnFullClicked += SwitchToRegister;
		_loginView.forgotPasswordButton.OnFullClicked += SwitchToForgotPassword;
		_loginView.OnSuccessfulLogin += SwitchToMainMenuScene;
		UIObjects.Add(_loginView);

		// Register view
		_registerView = new RegisterViewControl();
		_registerView.OnEmailNotConfirmed += () => { _registerView.Visible = false; SwitchTo2FA(); };
		_registerView.switchToLoginButton.OnFullClicked += SwitchToLogin;
		_registerView.Visible = false;
		UIObjects.Add(_registerView);

		// Forgot password view
		_forgotPasswordView = new ForgotPasswordViewControl();
		_forgotPasswordView.switchToLoginButton.OnFullClicked += SwitchToLogin;
		_forgotPasswordView.Visible = false;
		UIObjects.Add(_forgotPasswordView);

		// 2FA view
		_twoFAView = new Two2FAViewControl();
		_twoFAView.switchToLoginButton.OnFullClicked += SwitchToLogin;
		_twoFAView.Visible = false;
		UIObjects.Add(_twoFAView);
	}

	private void SwitchToMainMenuScene()
	{
		new MainMenuScene().LoadScene();
	}

	private void SwitchToLogin()
	{
		_loginView.Visible = true;
	}

	private void SwitchToRegister()
	{
		_registerView.Visible = true;
	}

	private void SwitchTo2FA()
	{
		_twoFAView.Visible = true;
	}

	private void SwitchToForgotPassword()
	{
		_forgotPasswordView.Visible = true;
	}
}