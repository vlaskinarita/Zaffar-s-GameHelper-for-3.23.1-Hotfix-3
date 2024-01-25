using System;
using GameHelper.Utils;
using GameOffsets.Objects.Components;
using GameOffsets.Objects.States.InGameState;
using ImGuiNET;

namespace GameHelper.RemoteObjects.Components;

public class Animated : ComponentBase
{
	public string Path { get; private set; }

	public uint Id { get; private set; }

	public Animated(nint address)
		: base(address)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text("Path: " + Path);
		ImGui.Text($"Id: {Id}");
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		if (hasAddressChanged)
		{
			SafeMemoryHandle reader = Core.Process.Handle;
			AnimatedOffsets data = reader.ReadMemory<AnimatedOffsets>(base.Address);
			OwnerEntityAddress = data.Header.EntityPtr;
			if (data.AnimatedEntityPtr != IntPtr.Zero)
			{
				EntityOffsets entity = reader.ReadMemory<EntityOffsets>(data.AnimatedEntityPtr);
				Path = reader.ReadStdWString(reader.ReadMemory<EntityDetails>(entity.ItemBase.EntityDetailsPtr).name);
				Id = entity.Id;
			}
		}
	}
}
