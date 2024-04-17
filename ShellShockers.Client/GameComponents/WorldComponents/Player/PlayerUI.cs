using GameEngine.Components;
using GameEngine.Core.Components.Objects;

namespace ShellShockers.Client.GameComponents.WorldComponents.Player;

internal class PlayerUI : UIObject
{
	public readonly UICamera? playerUICamera;
	private readonly PlayerUICrosshair? crosshair;

	public PlayerUI(bool localPlayer)
	{
		if (localPlayer)
		{
			Children.Add(playerUICamera = new UICamera());

			Children.Add(crosshair = new PlayerUICrosshair());
		}
	}
}