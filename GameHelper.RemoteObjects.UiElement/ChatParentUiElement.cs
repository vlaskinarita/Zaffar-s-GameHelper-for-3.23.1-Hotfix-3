using GameHelper.Cache;
using ImGuiNET;

namespace GameHelper.RemoteObjects.UiElement;

public class ChatParentUiElement : UiElementBase
{
	public bool IsChatActive => backgroundColor.W * 255f >= 140f;

	internal ChatParentUiElement(nint address, UiElementParents parents)
		: base(address, parents)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text($"IsChatActive: {IsChatActive} ({backgroundColor.W * 255f})");
	}
}
