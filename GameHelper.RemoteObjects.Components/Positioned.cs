using GameOffsets.Objects.Components;
using GameOffsets.Objects.States.InGameState;
using ImGuiNET;

namespace GameHelper.RemoteObjects.Components;

public class Positioned : ComponentBase
{
	public byte Flags { get; private set; }

	public bool IsFriendly { get; private set; }

	public Positioned(nint address)
		: base(address)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text($"Flags: {Flags:X}");
		ImGui.Text($"IsFriendly: {IsFriendly}");
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		PositionedOffsets data = Core.Process.Handle.ReadMemory<PositionedOffsets>(base.Address);
		OwnerEntityAddress = data.Header.EntityPtr;
		Flags = data.Reaction;
		IsFriendly = EntityHelper.IsFriendly(data.Reaction);
	}
}
