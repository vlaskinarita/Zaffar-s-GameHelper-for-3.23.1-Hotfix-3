using GameOffsets.Objects.Components;
using ImGuiNET;

namespace GameHelper.RemoteObjects.Components;

public class TriggerableBlockage : ComponentBase
{
	public bool IsBlocked { get; private set; }

	public TriggerableBlockage(nint address)
		: base(address)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text($"Is Blocked: {IsBlocked}");
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		TriggerableBlockageOffsets data = Core.Process.Handle.ReadMemory<TriggerableBlockageOffsets>(base.Address);
		OwnerEntityAddress = data.Header.EntityPtr;
		IsBlocked = data.IsBlocked;
	}
}
