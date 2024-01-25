using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GameHelper.Utils;
using GameOffsets.Objects.Components;
using GameOffsets.Objects.FilesStructures;
using ImGuiNET;

namespace GameHelper.RemoteObjects.Components;

public class Buffs : ComponentBase
{
	public ConcurrentDictionary<string, StatusEffectStruct> StatusEffects { get; } = new ConcurrentDictionary<string, StatusEffectStruct>();


	public bool[] FlaskActive { get; private set; } = new bool[5];


	public Buffs(nint address)
		: base(address)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		if (!ImGui.TreeNode("Status Effect (Buffs/Debuffs)"))
		{
			return;
		}
		foreach (KeyValuePair<string, StatusEffectStruct> kv in StatusEffects)
		{
			if (ImGui.TreeNode(kv.Key ?? ""))
			{
				ImGuiHelper.DisplayTextAndCopyOnClick("Name: " + kv.Key, kv.Key);
				ImGuiHelper.IntPtrToImGui("BuffDefinationPtr", kv.Value.BuffDefinationPtr);
				ImGuiHelper.DisplayFloatWithInfinitySupport("Total Time:", kv.Value.TotalTime);
				ImGuiHelper.DisplayFloatWithInfinitySupport("Time Left:", kv.Value.TimeLeft);
				ImGui.Text($"Source Entity Id: {kv.Value.SourceEntityId}");
				ImGui.Text($"Charges: {kv.Value.Charges}");
				ImGui.Text($"Source FlaskSlot: {kv.Value.FlaskSlot}");
				ImGui.Text($"Source Effectiveness: {100 + kv.Value.Effectiveness} (raw value: {kv.Value.Effectiveness})");
				ImGui.Text($"Source UnknownIdAndEquipmentInfo: {kv.Value.UnknownIdAndEquipmentInfo:X}");
				ImGui.TreePop();
			}
		}
		ImGui.TreePop();
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		BuffsOffsets data = reader.ReadMemory<BuffsOffsets>(base.Address);
		OwnerEntityAddress = data.Header.EntityPtr;
		StatusEffects.Clear();
		nint[] statusEffects = reader.ReadStdVector<nint>(data.StatusEffectPtr);
		Array.Fill(FlaskActive, value: false);
		for (int i = 0; i < statusEffects.Length; i++)
		{
			StatusEffectStruct statusEffectData = reader.ReadMemory<StatusEffectStruct>(statusEffects[i]);
			if (statusEffectData.BuffDefinationPtr != IntPtr.Zero)
			{
				if (Core.States.InGameStateObject.CurrentAreaInstance.Player.Id != statusEffectData.SourceEntityId)
				{
					statusEffectData.FlaskSlot = -1;
				}
				(string, byte) obj = ((string, byte))Core.GgpkObjectCache.AddOrGetExisting(statusEffectData.BuffDefinationPtr, (nint key) => GetNameFromBuffDefination(key));
				var (effectName, _) = obj;
				if (obj.Item2 != 4)
				{
					statusEffectData.FlaskSlot = -1;
				}
				else if (statusEffectData.FlaskSlot >= 0 && statusEffectData.FlaskSlot < 5)
				{
					FlaskActive[statusEffectData.FlaskSlot] = true;
				}
				StatusEffects.AddOrUpdate(effectName, statusEffectData, delegate(string key, StatusEffectStruct oldValue)
				{
					statusEffectData.Charges = ++oldValue.Charges;
					return statusEffectData;
				});
			}
		}
	}

	private (string, byte) GetNameFromBuffDefination(nint addr)
	{
		SafeMemoryHandle handle = Core.Process.Handle;
		BuffDefinitionsOffset data = handle.ReadMemory<BuffDefinitionsOffset>(addr);
		return (handle.ReadUnicodeString(data.Name), data.BuffType);
	}
}
