using System;
using GameOffsets.Objects.Components;
using ImGuiNET;

namespace GameHelper.RemoteObjects.Components;

public class Targetable : ComponentBase
{
	private uint targetableFlag;

	private uint hiddenFlag;

	public bool IsTargetable { get; private set; }

	public Targetable(nint address)
		: base(address)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text($"Targetable Flag: {Convert.ToString(targetableFlag, 2),8}");
		ImGui.Text($"Hidden Flag: {Convert.ToString(hiddenFlag, 2),8}");
		ImGui.Text($"Is Targetable {IsTargetable}");
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		TargetableOffsets data = Core.Process.Handle.ReadMemory<TargetableOffsets>(base.Address);
		OwnerEntityAddress = data.Header.EntityPtr;
		targetableFlag = data.TargetableFlag;
		hiddenFlag = data.HiddenFlag;
		IsTargetable = TargetableHelper.IsTargetable(data.TargetableFlag);
	}
}
