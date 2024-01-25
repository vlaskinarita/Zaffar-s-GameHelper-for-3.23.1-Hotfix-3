using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Coroutine;
using GameHelper.CoroutineEvents;
using GameHelper.Utils;
using ImGuiNET;

namespace GameHelper.Cache;

internal class GgpkAddresses<T>
{
	private readonly ConcurrentDictionary<nint, T> cache;

	public GgpkAddresses()
	{
		cache = new ConcurrentDictionary<nint, T>();
		CoroutineHandler.Start(OnGameClose());
	}

	public T AddOrGetExisting(nint key, Func<nint, T> valueFactory)
	{
		if (key != IntPtr.Zero)
		{
			return cache.GetOrAdd(key, valueFactory);
		}
		throw new Exception($"Object tried to load 0x{((IntPtr)key).ToInt64():X} in the cache.");
	}

	public void ToImGui()
	{
		ImGui.Text($"Total Size: {cache.Count}");
		if (!ImGui.TreeNode("GGPK Addresses"))
		{
			return;
		}
		foreach (KeyValuePair<nint, T> item in cache)
		{
			item.Deconstruct(out var key2, out var value2);
			nint key = key2;
			T value = value2;
			string addr = $"0x{((IntPtr)key).ToInt64():X}";
			ImGuiHelper.DisplayTextAndCopyOnClick($"{addr} - {value}", addr);
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
}
