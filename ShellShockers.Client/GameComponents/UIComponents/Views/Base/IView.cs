using GameEngine.Core.Components.Objects;

namespace ShellShockers.Client.GameComponents.UIComponents.Views.Base;

internal interface IView
{
	void RegisterView(ICollection<UIObject> uiObjects);
	void UnregisterView(ICollection<UIObject> uiObjects);
}