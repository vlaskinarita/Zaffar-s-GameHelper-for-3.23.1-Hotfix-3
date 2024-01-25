using System;
using System.Collections.Generic;
using Coroutine;
using GameHelper.RemoteEnums;
using GameHelper.Utils;
using GameOffsets.Objects.States.InGameState;
using ImGuiNET;

namespace GameHelper.RemoteObjects.States.InGameStateObjects;

public class ServerData : RemoteObjectBase
{
	private InventoryName selectedInvName;

	public Inventory FlaskInventory { get; } = new Inventory(IntPtr.Zero, "Flask");


	internal Inventory SelectedInv { get; } = new Inventory(IntPtr.Zero, "CurrentlySelected");


	internal Dictionary<InventoryName, nint> PlayerInventories { get; } = new Dictionary<InventoryName, nint>();


	internal ServerData(nint address)
		: base(address)
	{
	}

	internal override void ToImGui()
	{
		if ((int)selectedInvName > PlayerInventories.Count)
		{
			ClearCurrentlySelectedInventory();
		}
		ImGuiHelper.IntPtrToImGui("Address", base.Address);
		if (ImGui.TreeNode("FlaskInventory"))
		{
			FlaskInventory.ToImGui();
			ImGui.TreePop();
		}
		ImGui.Text("please click Clear Selected before leaving this window.");
		if (ImGuiHelper.IEnumerableComboBox("###Inventory Selector", PlayerInventories.Keys, ref selectedInvName))
		{
			SelectedInv.Address = PlayerInventories[selectedInvName];
		}
		ImGui.SameLine();
		if (ImGui.Button("Clear Selected"))
		{
			ClearCurrentlySelectedInventory();
		}
		if (selectedInvName != 0 && ImGui.TreeNode("Currently Selected Inventory"))
		{
			SelectedInv.ToImGui();
			ImGui.TreePop();
		}
	}

	protected override void CleanUpData()
	{
		ClearCurrentlySelectedInventory();
		PlayerInventories.Clear();
		FlaskInventory.Address = IntPtr.Zero;
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		if (hasAddressChanged)
		{
			ClearCurrentlySelectedInventory();
		}
		SafeMemoryHandle handle = Core.Process.Handle;
		InventoryArrayStruct[] inventoryData = handle.ReadStdVector<InventoryArrayStruct>(handle.ReadMemory<ServerDataStructure>(base.Address + 40704).PlayerInventories);
		PlayerInventories.Clear();
		for (int i = 0; i < inventoryData.Length; i++)
		{
			InventoryName invName = (InventoryName)inventoryData[i].InventoryId;
			nint invAddr = inventoryData[i].InventoryPtr0;
			PlayerInventories[invName] = invAddr;
			if (invName == InventoryName.Flask1)
			{
				FlaskInventory.Address = invAddr;
			}
		}
	}

	private void ClearCurrentlySelectedInventory()
	{
		selectedInvName = InventoryName.NoInvSelected;
		SelectedInv.Address = IntPtr.Zero;
	}

	private IEnumerable<Wait> OnTimeTick()
	{
		while (true)
		{
			yield return new Wait(0.2);
			if (base.Address != IntPtr.Zero)
			{
				UpdateData(hasAddressChanged: false);
			}
		}
	}
}
