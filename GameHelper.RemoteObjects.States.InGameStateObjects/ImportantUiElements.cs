using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coroutine;
using GameHelper.Cache;
using GameHelper.CoroutineEvents;
using GameHelper.RemoteEnums;
using GameHelper.RemoteObjects.UiElement;
using GameHelper.Utils;
using GameOffsets.Objects.States.InGameState;
using ImGuiNET;

namespace GameHelper.RemoteObjects.States.InGameStateObjects;

public class ImportantUiElements : RemoteObjectBase
{
	private readonly UiElementParents rootCache;

	private readonly UiElementParents passiveSkillTreeCache;

	private UiElementBase passiveskilltreenodes;

	public LargeMapUiElement LargeMap { get; }

	public MapUiElement MiniMap { get; }

	public ChatParentUiElement ChatParent { get; }

	public List<SkillTreeNodeUiElement> SkillTreeNodesUiElements { get; }

	internal ImportantUiElements(nint address)
		: base(address)
	{
		rootCache = new UiElementParents(null, GameStateTypes.InGameState, GameStateTypes.EscapeState);
		passiveSkillTreeCache = new UiElementParents(rootCache, GameStateTypes.InGameState, GameStateTypes.EscapeState);
		passiveskilltreenodes = new UiElementBase(IntPtr.Zero, rootCache);
		LargeMap = new LargeMapUiElement(IntPtr.Zero, rootCache);
		MiniMap = new MapUiElement(IntPtr.Zero, rootCache);
		ChatParent = new ChatParentUiElement(IntPtr.Zero, rootCache);
		SkillTreeNodesUiElements = new List<SkillTreeNodeUiElement>();
		Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(OnPerFrame(), "[InGameState] Update ImportantUiElements", 2147483644));
	}

	internal override void ToImGui()
	{
		displayParentsCache();
		base.ToImGui();
		ImGui.Text($"Passive Skill Tree Panel Visible: {passiveskilltreenodes.IsVisible}");
		ImGui.Text($"Total Skill Tree Nodes: {SkillTreeNodesUiElements.Count}");
		if (ImGui.TreeNode("Skill Tree Nodes"))
		{
			for (int i = 0; i < SkillTreeNodesUiElements.Count; i++)
			{
				int skillId = SkillTreeNodesUiElements[i].SkillGraphId;
				ImGuiHelper.DisplayTextAndCopyOnClick($"index: {i}, skillId: {skillId}", $"{skillId}");
				ImGui.GetForegroundDrawList().AddText(SkillTreeNodesUiElements[i].Postion, 4278190335u, $"{i}");
			}
			ImGui.TreePop();
		}
	}

	protected override void CleanUpData()
	{
		passiveskilltreenodes.Address = IntPtr.Zero;
		MiniMap.Address = IntPtr.Zero;
		LargeMap.Address = IntPtr.Zero;
		ChatParent.Address = IntPtr.Zero;
		SkillTreeNodesUiElements.Clear();
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		UpdateParentsCache();
		SafeMemoryHandle reader = Core.Process.Handle;
		ImportantUiElementsOffsets data1 = reader.ReadMemory<ImportantUiElementsOffsets>(base.Address);
		if (Core.GHSettings.EnableControllerMode)
		{
			LargeMap.Address = reader.ReadMemory<MapParentStruct>(data1.ControllerModeMapParentPtr).LargeMapPtr;
			MiniMap.Address = IntPtr.Zero;
			ChatParent.Address = IntPtr.Zero;
			passiveskilltreenodes.Address = IntPtr.Zero;
		}
		else
		{
			MapParentStruct data2 = reader.ReadMemory<MapParentStruct>(data1.MapParentPtr);
			PassiveSkillTreeStruct data3 = reader.ReadMemory<PassiveSkillTreeStruct>(data1.PassiveSkillTreePanel);
			LargeMap.Address = data2.LargeMapPtr;
			MiniMap.Address = data2.MiniMapPtr;
			ChatParent.Address = data1.ChatParentPtr;
			passiveskilltreenodes.Address = data3.SkillTreeNodeUiElements;
			updatePassiveSkillTreeData();
		}
	}

	private void updatePassiveSkillTreeData()
	{
		if (passiveskilltreenodes.IsVisible)
		{
			AddOrUpdateSkillNodes();
		}
		else
		{
			ClearSkillNodes();
		}
	}

	private void ClearSkillNodes()
	{
		SkillTreeNodesUiElements.Clear();
		passiveSkillTreeCache.Clear();
	}

	private void AddOrUpdateSkillNodes()
	{
		if (SkillTreeNodesUiElements.Count == 0)
		{
			UiElementBase currentChild = new UiElementBase(IntPtr.Zero, passiveSkillTreeCache);
			for (int j = 3; j < passiveskilltreenodes.TotalChildrens; j++)
			{
				currentChild.Address = passiveskilltreenodes[j].Address;
				if (currentChild.IsVisible)
				{
					AddSkillTreeNodeUiElementRecursive(currentChild);
					continue;
				}
				break;
			}
		}
		else
		{
			Parallel.For(0, SkillTreeNodesUiElements.Count, delegate(int i)
			{
				SkillTreeNodesUiElements[i].Address = SkillTreeNodesUiElements[i].Address;
			});
		}
	}

	private void AddSkillTreeNodeUiElementRecursive(UiElementBase uie)
	{
		if (uie.TotalChildrens > 0)
		{
			for (int i = 0; i < uie.TotalChildrens; i++)
			{
				AddSkillTreeNodeUiElementRecursive(uie[i]);
			}
			return;
		}
		SkillTreeNodeUiElement skillNode = new SkillTreeNodeUiElement(uie.Address, passiveSkillTreeCache);
		if (skillNode.SkillGraphId != 0)
		{
			SkillTreeNodesUiElements.Add(skillNode);
		}
	}

	private void displayParentsCache()
	{
		rootCache.ToImGui();
		passiveSkillTreeCache.ToImGui();
	}

	private void UpdateParentsCache()
	{
		rootCache.UpdateAllParentsParallel();
		passiveSkillTreeCache.UpdateAllParentsParallel();
	}

	private IEnumerator<Wait> OnPerFrame()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.PerFrameDataUpdate);
			if (base.Address != IntPtr.Zero && Core.States.GameCurrentState == GameStateTypes.InGameState)
			{
				UpdateData(hasAddressChanged: false);
			}
		}
	}
}
