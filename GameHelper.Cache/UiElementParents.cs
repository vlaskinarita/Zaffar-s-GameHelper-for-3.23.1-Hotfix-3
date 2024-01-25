using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coroutine;
using GameHelper.CoroutineEvents;
using GameHelper.RemoteEnums;
using GameHelper.RemoteObjects.UiElement;
using ImGuiNET;

namespace GameHelper.Cache;

internal class UiElementParents
{
	private readonly UiElementParents grandparent;

	private readonly GameStateTypes ownerState1;

	private readonly GameStateTypes ownerState2;

	private readonly Dictionary<nint, UiElementBase> cache;

	public UiElementParents(UiElementParents grandparent, GameStateTypes ownerStateA, GameStateTypes ownerStateB)
	{
		ownerState1 = ownerStateA;
		ownerState2 = ownerStateB;
		cache = new Dictionary<nint, UiElementBase>();
		this.grandparent = grandparent;
		CoroutineHandler.Start(OnGameClose());
		CoroutineHandler.Start(OnStateChange());
	}

	public void AddIfNotExists(nint address)
	{
		if (address != IntPtr.Zero && (grandparent == null || !grandparent.cache.ContainsKey(address)) && !cache.ContainsKey(address))
		{
			try
			{
				cache.Add(address, new UiElementBase(address, this));
			}
			catch (Exception e)
			{
				Console.WriteLine($"Failed to add the UiElement Parent in the cache. 0x{((IntPtr)address).ToInt64():X} due to {e}");
			}
		}
	}

	public UiElementBase GetParent(nint address)
	{
		if (cache.TryGetValue(address, out var parent))
		{
			return parent;
		}
		if (grandparent.cache.TryGetValue(address, out var gParent))
		{
			return gParent;
		}
		throw new Exception($"UiElementBase with adress {((IntPtr)address).ToInt64():X} not found.");
	}

	public void UpdateAllParentsParallel()
	{
		Parallel.ForEach(cache, delegate(KeyValuePair<nint, UiElementBase> data)
		{
			try
			{
				data.Value.Address = data.Key;
			}
			catch (Exception value)
			{
				Console.WriteLine($"Failed to update the UiElement Parent in the cache. 0x{((IntPtr)data.Key).ToInt64():X} due to {value}");
			}
		});
	}

	public void Clear()
	{
		cache.Clear();
	}

	public void ToImGui()
	{
		ImGui.Text($"Total Size: {cache.Count}");
		if (!ImGui.TreeNode("Parent UiElements"))
		{
			return;
		}
		foreach (var (key, value) in cache)
		{
			if (ImGui.TreeNode($"0x{((IntPtr)key).ToInt64():X}"))
			{
				value.ToImGui();
				ImGui.TreePop();
			}
		}
		ImGui.TreePop();
	}

	private IEnumerable<Wait> OnGameClose()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.OnClose);
			cache.Clear();
		}
	}

	private IEnumerable<Wait> OnStateChange()
	{
		while (true)
		{
			yield return new Wait(RemoteEvents.StateChanged);
			if (Core.States.GameCurrentState != ownerState1 && Core.States.GameCurrentState != ownerState2)
			{
				cache.Clear();
			}
		}
	}
}
