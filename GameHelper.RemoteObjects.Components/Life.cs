using GameHelper.Utils;
using GameOffsets.Objects.Components;
using ImGuiNET;

namespace GameHelper.RemoteObjects.Components;

public class Life : ComponentBase
{
	public bool IsAlive { get; private set; } = true;


	public VitalStruct Health { get; private set; }

	public VitalStruct EnergyShield { get; private set; }

	public VitalStruct Mana { get; private set; }

	public Life(nint address)
		: base(address)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		if (ImGui.TreeNode("Health"))
		{
			VitalToImGui(Health);
			ImGui.TreePop();
		}
		if (ImGui.TreeNode("Energy Shield"))
		{
			VitalToImGui(EnergyShield);
			ImGui.TreePop();
		}
		if (ImGui.TreeNode("Mana"))
		{
			VitalToImGui(Mana);
			ImGui.TreePop();
		}
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		LifeOffset data = Core.Process.Handle.ReadMemory<LifeOffset>(base.Address);
		OwnerEntityAddress = data.Header.EntityPtr;
		Health = data.Health;
		EnergyShield = data.EnergyShield;
		Mana = data.Mana;
		IsAlive = data.Health.Current > 0;
	}

	private void VitalToImGui(VitalStruct data)
	{
		ImGuiHelper.IntPtrToImGui("PtrToSelf", data.PtrToLifeComponent);
		ImGui.Text($"Regeneration: {data.Regeneration}");
		ImGui.Text($"Total: {data.Total}");
		ImGui.Text($"ReservedFlat: {data.ReservedFlat}");
		ImGui.Text($"Current: {data.Current}");
		ImGui.Text($"Reserved(%%): {data.ReservedPercent}");
		ImGui.Text($"Current(%%): {data.CurrentInPercent()}");
	}
}
