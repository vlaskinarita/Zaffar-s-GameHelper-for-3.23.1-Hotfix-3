using System;
using System.Collections.Generic;
using System.Linq;
using GameHelper.RemoteEnums;
using GameHelper.Utils;
using GameOffsets.Natives;
using GameOffsets.Objects.Components;
using ImGuiNET;

namespace GameHelper.RemoteObjects.Components;

public class ObjectMagicProperties : ComponentBase
{
	public List<(string name, (float value0, float value1) values)> Mods = new List<(string, (float, float))>();

	public HashSet<string> ModNames = new HashSet<string>();

	public Rarity Rarity { get; private set; }

	public ObjectMagicProperties(nint address)
		: base(address)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text($"Rarity: {Rarity}");
		ModsToImGui("All Mods", Mods);
		if (!ImGui.TreeNode("All Mod names"))
		{
			return;
		}
		foreach (string mod in ModNames)
		{
			ImGuiHelper.DisplayTextAndCopyOnClick(mod ?? "", mod);
		}
		ImGui.TreePop();
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		ObjectMagicPropertiesOffsets data = reader.ReadMemory<ObjectMagicPropertiesOffsets>(base.Address);
		OwnerEntityAddress = data.Header.EntityPtr;
		Rarity = (Rarity)data.Details1.Rarity;
		if (hasAddressChanged)
		{
			AddToMods(Mods, reader.ReadStdVector<ModArrayStruct>(data.Details1.Mods.ImplicitMods));
			AddToMods(Mods, reader.ReadStdVector<ModArrayStruct>(data.Details1.Mods.ExplicitMods));
			AddToMods(Mods, reader.ReadStdVector<ModArrayStruct>(data.Details1.Mods.EnchantMods));
			AddToMods(Mods, reader.ReadStdVector<ModArrayStruct>(data.Details1.Mods.HellscapeMods));
			AddToMods(Mods, reader.ReadStdVector<ModArrayStruct>(data.Details1.Mods.CrucibleMods));
			Mods.All(((string name, (float value0, float value1) values) k) => ModNames.Add(k.name));
		}
	}

	internal static void ModsToImGui(string text, List<(string name, (float value0, float value1) values)> collection)
	{
		if (ImGui.TreeNode(text))
		{
			for (int i = 0; i < collection.Count; i++)
			{
				var (name, values) = collection[i];
				ImGuiHelper.DisplayTextAndCopyOnClick($"{name}: {values.Item1} - {values.Item2}", name);
			}
			ImGui.TreePop();
		}
	}

	internal static void AddToMods(List<(string name, (float value0, float value1) values)> collection, ModArrayStruct[] mods)
	{
		for (int i = 0; i < mods.Length; i++)
		{
			ModArrayStruct mod = mods[i];
			if (mod.ModsPtr != IntPtr.Zero)
			{
				collection.Add((GetModName(mod.ModsPtr), GetValue(mod.Values, mod.Value0)));
			}
		}
	}

	internal static string GetModName(nint modsDatRowAddress)
	{
		return Core.GgpkStringCache.AddOrGetExisting(modsDatRowAddress, delegate(nint key)
		{
			SafeMemoryHandle handle = Core.Process.Handle;
			return handle.ReadUnicodeString(handle.ReadMemory<nint>(key));
		});
	}

	internal static (float, float) GetValue(StdVector valuesPtr, int value0)
	{
		switch (valuesPtr.TotalElements(4))
		{
		case 0L:
			return (float.NaN, float.NaN);
		case 1L:
			return (value0, float.NaN);
		default:
		{
			int[] values = Core.Process.Handle.ReadStdVector<int>(valuesPtr);
			if (values.Length > 1)
			{
				return (values[0], values[1]);
			}
			return (float.NaN, float.NaN);
		}
		}
	}
}
