using GameHelper.Utils;
using GameOffsets.Objects.Components;
using ImGuiNET;

namespace GameHelper.RemoteObjects.Components;

public class Charges : ComponentBase
{
	public int Current { get; private set; }

	public int PerUseCharge { get; private set; }

	public Charges(nint address)
		: base(address)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text($"Current Charges: {Current}");
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		ChargesOffsets data = reader.ReadMemory<ChargesOffsets>(base.Address);
		OwnerEntityAddress = data.Header.EntityPtr;
		Current = data.current;
		if (hasAddressChanged)
		{
			PerUseCharge = reader.ReadMemory<ChargesInternalStruct>(data.ChargesInternalPtr).PerUseCharges;
		}
	}
}
