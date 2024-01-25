using GameHelper.Utils;
using GameOffsets.Objects.Components;
using ImGuiNET;

namespace GameHelper.RemoteObjects.Components;

public class Player : ComponentBase
{
	public string Name { get; private set; }

	public Player(nint address)
		: base(address)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text("Player Name: " + Name);
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		PlayerOffsets data = reader.ReadMemory<PlayerOffsets>(base.Address);
		OwnerEntityAddress = data.Header.EntityPtr;
		if (hasAddressChanged)
		{
			Name = reader.ReadStdWString(data.Name);
		}
	}
}
