using GameEngine.Components.UIComponents;
using GameEngine.Core.Components;

namespace ShellShockers.Client.GameComponents.UIComponents.Views.LoginRegisterViews;

internal class NotARobotSquare : UIButton
{
	public bool SquareEnabled { get; private set; }

	public NotARobotSquare(bool predeterminedSelected)
	{
		OnFullClicked += OnClick;

		SquareEnabled = predeterminedSelected;
		Meshes.Add(new MeshData("UIRect.obj", $"{(predeterminedSelected ? "Green" : "Red")}.mat"));
	}

	public void OnClick()
	{
		Meshes.Clear();

		SquareEnabled = !SquareEnabled;
		Meshes.Add(new MeshData("UIRect.obj", $"{(SquareEnabled ? "Green" : "Red")}.mat"));
	}
}