using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Coroutine;
using GameHelper.RemoteObjects.States.InGameStateObjects;
using GameOffsets.Objects.States.InGameState;
using ImGuiNET;

namespace GameHelper.Cache;

public class DisappearingEntity
{
	private readonly string name;

	private readonly int envKeyMin;

	private readonly int envKeyMax;

	private readonly ConcurrentDictionary<EntityNodeKey, bool> cache;

	private readonly ConcurrentDictionary<EntityNodeKey, Entity> entities;

	private bool isActivated;

	private bool isCleanUpTriggered;

	public DisappearingEntity(string entityPathIdentifier, int environmentKeyMin, int environmentKeyMax, ConcurrentDictionary<EntityNodeKey, Entity> entities_)
	{
		name = entityPathIdentifier;
		envKeyMin = environmentKeyMin;
		envKeyMax = environmentKeyMax;
		isActivated = false;
		isCleanUpTriggered = false;
		cache = new ConcurrentDictionary<EntityNodeKey, bool>();
		entities = entities_;
	}

	internal void UpdateState(IReadOnlyList<int> environments)
	{
		UpdateActivation(environments);
		UpdateCleanUpJob();
	}

	internal bool TryAddParallel(EntityNodeKey entity, string path)
	{
		if (isActivated && path.Contains(name, StringComparison.Ordinal))
		{
			cache[entity] = true;
			return true;
		}
		return false;
	}

	internal void Clear()
	{
		isActivated = false;
		cache.Clear();
	}

	internal void ToImGui()
	{
		if (ImGui.TreeNode(name))
		{
			ImGui.Text($"Is Activated: {isActivated}");
			ImGui.Text($"Total Entities: {cache.Count}");
			ImGui.TreePop();
		}
	}

	public bool Contains(EntityNodeKey entity)
	{
		return cache.ContainsKey(entity);
	}

	public bool IsActive()
	{
		return isActivated;
	}

	private void UpdateActivation(IReadOnlyList<int> environments)
	{
		isActivated = false;
		foreach (int envKey in environments)
		{
			if (envKey >= envKeyMin && envKey <= envKeyMax)
			{
				isActivated = true;
				break;
			}
		}
	}

	private void UpdateCleanUpJob()
	{
		if (isCleanUpTriggered || isActivated || cache.IsEmpty)
		{
			return;
		}
		CoroutineHandler.InvokeLater(new Wait(0.5), delegate
		{
			foreach (KeyValuePair<EntityNodeKey, bool> current in cache)
			{
				entities.TryRemove(current.Key, out var _);
				cache.TryRemove(current.Key, out var _);
			}
			isCleanUpTriggered = false;
		});
		isCleanUpTriggered = true;
	}
}
