using System;
using GameHelper.Cache;
using GameHelper.Utils;
using GameOffsets.Objects.UiElement;
using ImGuiNET;

namespace GameHelper.RemoteObjects.UiElement;

public class SkillTreeNodeUiElement : UiElementBase
{
	public int SkillGraphId { get; private set; }

	internal SkillTreeNodeUiElement(nint address, UiElementParents parents)
		: base(address, parents)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text($"SkillGraphId = {SkillGraphId}");
	}

	protected override void CleanUpData()
	{
		base.CleanUpData();
		SkillGraphId = 0;
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		SafeMemoryHandle handle = Core.Process.Handle;
		SkillTreeNodeUiElementOffset data = handle.ReadMemory<SkillTreeNodeUiElementOffset>(base.Address);
		UpdateData(data.UiElementBase, hasAddressChanged);
		if (data.SkillInfo != IntPtr.Zero)
		{
			SkillGraphId = handle.ReadMemory<PassiveSkillsDatStruct>(handle.ReadMemory<SkillInfoStruct>(data.SkillInfo).PassiveSkillsDatRow).PassiveSkillGraphId;
		}
	}
}
