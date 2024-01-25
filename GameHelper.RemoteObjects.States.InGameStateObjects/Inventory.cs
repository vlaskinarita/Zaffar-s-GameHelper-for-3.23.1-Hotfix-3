using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Coroutine;
using GameHelper.Utils;
using GameOffsets.Natives;
using GameOffsets.Objects.States.InGameState;
using ImGuiNET;

namespace GameHelper.RemoteObjects.States.InGameStateObjects;

public class Inventory : RemoteObjectBase
{
	private nint[] itemsToInventorySlotMapping;

	public StdTuple2D<int> TotalBoxes { get; private set; }

	public int ServerRequestCounter { get; private set; }

	public ConcurrentDictionary<nint, Item> Items { get; } = new ConcurrentDictionary<nint, Item>();


	[SkipImGuiReflection]
	public Item this[int y, int x]
	{
		get
		{
			if (y >= TotalBoxes.Y || x >= TotalBoxes.X)
			{
				return new Item(IntPtr.Zero);
			}
			int index = y * TotalBoxes.X + x;
			if (index >= itemsToInventorySlotMapping.Length)
			{
				return new Item(IntPtr.Zero);
			}
			nint itemAddr = itemsToInventorySlotMapping[index];
			if (itemAddr == IntPtr.Zero)
			{
				return new Item(IntPtr.Zero);
			}
			if (Items.TryGetValue(itemAddr, out var item))
			{
				return item;
			}
			return new Item(IntPtr.Zero);
		}
	}

	internal Inventory(nint address, string name)
		: base(address)
	{
		Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(OnTimeTick(), "[Inventory] Update " + name, 2147483643));
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text($"Total Boxes: {TotalBoxes}");
		ImGui.Text($"Server Request Counter: {ServerRequestCounter}");
		if (ImGui.TreeNode("Inventory Slots"))
		{
			for (int y = 0; y < TotalBoxes.Y; y++)
			{
				string data = string.Empty;
				for (int x = 0; x < TotalBoxes.X; x++)
				{
					data = ((itemsToInventorySlotMapping[y * TotalBoxes.X + x] == IntPtr.Zero) ? (data + " 0") : (data + " 1"));
				}
				ImGui.Text(data);
			}
			ImGui.TreePop();
		}
		if (!ImGui.TreeNode("Items"))
		{
			return;
		}
		foreach (KeyValuePair<nint, Item> item in Items)
		{
			if (ImGui.TreeNode($"{item.Value.Path}##{((IntPtr)item.Value.Address).ToInt64()}"))
			{
				item.Value.ToImGui();
				ImGui.TreePop();
			}
		}
		ImGui.TreePop();
	}

	protected override void CleanUpData()
	{
		TotalBoxes = default(StdTuple2D<int>);
		ServerRequestCounter = 0;
		itemsToInventorySlotMapping = null;
		Items.Clear();
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		InventoryStruct invInfo = reader.ReadMemory<InventoryStruct>(base.Address);
		TotalBoxes = invInfo.TotalBoxes;
		ServerRequestCounter = invInfo.ServerRequestCounter;
		itemsToInventorySlotMapping = reader.ReadStdVector<nint>(invInfo.ItemList);
		if (hasAddressChanged)
		{
			Items.Clear();
		}
		foreach (KeyValuePair<nint, Item> item3 in Items)
		{
			item3.Value.IsValid = false;
		}
		Parallel.ForEach(itemsToInventorySlotMapping.Distinct(), delegate(nint invItemPtr)
		{
			if (invItemPtr != IntPtr.Zero)
			{
				InventoryItemStruct inventoryItemStruct = reader.ReadMemory<InventoryItemStruct>(invItemPtr);
				if (Items.ContainsKey(invItemPtr))
				{
					Items[invItemPtr].Address = inventoryItemStruct.Item;
				}
				else
				{
					Item item2 = new Item(inventoryItemStruct.Item);
					if (!string.IsNullOrEmpty(item2.Path) && !Items.TryAdd(invItemPtr, item2))
					{
						throw new Exception("Failed to add item into the Inventory Item Dict.");
					}
				}
			}
		});
		foreach (KeyValuePair<nint, Item> item in Items)
		{
			if (!item.Value.IsValid)
			{
				Items.TryRemove(item.Key, out var _);
			}
		}
	}

	private IEnumerable<Wait> OnTimeTick()
	{
		while (true)
		{
			yield return new Wait(0.02);
			if (base.Address != IntPtr.Zero)
			{
				UpdateData(hasAddressChanged: false);
			}
		}
	}
}
