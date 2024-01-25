using GameOffsets.Objects.Components;
using ImGuiNET;

namespace GameHelper.RemoteObjects.Components;

public class Shrine : ComponentBase
{
	public bool IsUsed { get; private set; }

	public Shrine(nint address)
		: base(address)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text($"Is Shrine Used: {IsUsed}");
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		ShrineOffsets data = Core.Process.Handle.ReadMemory<ShrineOffsets>(base.Address);
		OwnerEntityAddress = data.Header.EntityPtr;
		IsUsed = data.IsUsed;
	}
}
