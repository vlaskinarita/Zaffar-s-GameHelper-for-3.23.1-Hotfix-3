using System.Numerics;
using GameHelper.Cache;
using ImGuiNET;

namespace GameHelper.RemoteObjects.UiElement;

public class LargeMapUiElement : MapUiElement
{
	public override Vector2 Postion => new Vector2(Core.GameCull.Value, 0f);

	public override Vector2 Size => new Vector2(Core.Process.WindowArea.Width - Core.GameCull.Value * 2, Core.Process.WindowArea.Height);

	public Vector2 Center => base.Postion;

	internal LargeMapUiElement(nint address, UiElementParents parents)
		: base(address, parents)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text($"Center (without shift/default-shift) {Center}");
	}
}
