using System;
using GameHelper.Utils;
using GameOffsets.Objects.Components;
using ImGuiNET;

namespace GameHelper.RemoteObjects.Components;

public class Chest : ComponentBase
{
	public bool IsOpened { get; private set; }

	public bool IsStrongbox { get; private set; }

	public bool IsLabelVisible { get; private set; }

	public Chest(nint address)
		: base(address)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text($"IsOpened: {IsOpened}");
		ImGui.Text($"IsStrongbox: {IsStrongbox}");
		ImGui.Text($"IsLabelVisible: {IsLabelVisible}");
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		ChestOffsets data = reader.ReadMemory<ChestOffsets>(base.Address);
		OwnerEntityAddress = data.Header.EntityPtr;
		IsOpened = data.IsOpened;
		if (hasAddressChanged)
		{
			ChestsStructInternal dataInternal = reader.ReadMemory<ChestsStructInternal>(data.ChestsDataPtr);
			IsStrongbox = dataInternal.StrongboxDatPtr != IntPtr.Zero;
			IsLabelVisible = dataInternal.IsLabelVisible;
		}
	}
}
