using GameEngine.Core.Components.Objects;

namespace ShellShockers.Client.GameComponents.UIComponents.Views.Base;

internal class BaseView : IView
{
	private bool _registered;
	protected readonly List<UIObject> uiObjects = new List<UIObject>();


	public void RegisterView(ICollection<UIObject> uiObjects)
	{
		if (_registered)
			return;

		_registered = true;
		foreach (UIObject obj in this.uiObjects)
			uiObjects.Add(obj);
	}

	public void UnregisterView(ICollection<UIObject> uiObjects)
	{
		if (!_registered)
			return;

		_registered = false;
		foreach (UIObject obj in this.uiObjects)
			uiObjects.Remove(obj);
	}
}