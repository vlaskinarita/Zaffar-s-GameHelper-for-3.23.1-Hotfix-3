using System;
using System.Collections.Generic;
using GameHelper.RemoteEnums;
using GameHelper.Utils;
using GameOffsets.Objects.Components;
using ImGuiNET;

namespace GameHelper.RemoteObjects.Components;

public class Stats : ComponentBase
{
	public Dictionary<GameStats, int> AllStats = new Dictionary<GameStats, int>();

	public Stats(nint address)
		: base(address)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		if (!ImGui.TreeNode("All Stats"))
		{
			return;
		}
		foreach (KeyValuePair<GameStats, int> stat in AllStats)
		{
			ImGuiHelper.DisplayTextAndCopyOnClick($"{stat.Key}: {stat.Value}", $"{stat.Key}");
		}
		ImGui.TreePop();
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		StatsOffsets data = reader.ReadMemory<StatsOffsets>(base.Address);
		OwnerEntityAddress = data.Header.EntityPtr;
		if (data.StatsDataPtr != IntPtr.Zero)
		{
			StatsStructInternal data2 = reader.ReadMemory<StatsStructInternal>(data.StatsDataPtr);
			AllStats.Clear();
			StatArrayStruct[] mystats = reader.ReadStdVector<StatArrayStruct>(data2.Stats);
			for (int i = 0; i < mystats.Length; i++)
			{
				StatArrayStruct p = mystats[i];
				AllStats[(GameStats)p.key] = p.value;
			}
		}
	}
}
