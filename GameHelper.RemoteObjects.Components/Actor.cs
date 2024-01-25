using System;
using System.Collections.Generic;
using GameHelper.RemoteEnums;
using GameHelper.Utils;
using GameOffsets.Objects.Components;
using GameOffsets.Objects.FilesStructures;
using ImGuiNET;

namespace GameHelper.RemoteObjects.Components;

public class Actor : ComponentBase
{
	private Dictionary<nint, VaalSoulStructure> ActiveSkillsVaalSouls { get; } = new Dictionary<nint, VaalSoulStructure>();


	private Dictionary<uint, ActiveSkillCooldown> ActiveSkillCooldowns { get; } = new Dictionary<uint, ActiveSkillCooldown>();


	private Dictionary<string, ActiveSkillDetails> ActiveSkills { get; } = new Dictionary<string, ActiveSkillDetails>();


	public Animation Animation { get; private set; }

	public Dictionary<string, ActiveSkillDetails>.KeyCollection ActiveSkillNames => ActiveSkills.Keys;

	public HashSet<string> IsSkillUsable { get; } = new HashSet<string>();


	public int[] DeployedEntities { get; private set; } = new int[256];


	public Actor(nint address)
		: base(address)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text($"AnimationId: {Animation}, Animation: {Animation}");
		if (ImGui.TreeNode("Vaal Souls"))
		{
			foreach (var (skillNamePtr, skillDetails2) in ActiveSkillsVaalSouls)
			{
				if (ImGui.TreeNode($"{((IntPtr)skillNamePtr).ToInt64():X}"))
				{
					ImGui.Text($"Required Souls: {skillDetails2.RequiredSouls}");
					ImGui.Text($"Current Souls: {skillDetails2.CurrentSouls}");
					ImGui.TreePop();
				}
			}
			ImGui.TreePop();
		}
		if (ImGui.TreeNode("Cooldowns"))
		{
			foreach (var (skillId, skillDetails) in ActiveSkillCooldowns)
			{
				if (ImGui.TreeNode($"{skillId:X}"))
				{
					ImGui.Text($"Active Skill Id: {skillDetails.ActiveSkillsDatId}");
					ImGuiHelper.IntPtrToImGui($"Cooldowns Vector (Length {skillDetails.TotalActiveCooldowns()})", skillDetails.CooldownsList.First);
					ImGui.Text($"Max Uses: {skillDetails.MaxUses}");
					ImGui.Text($"Total Cooldown Time (ms): {skillDetails.TotalCooldownTimeInMs}");
					ImGui.TreePop();
				}
			}
			ImGui.TreePop();
		}
		if (ImGui.TreeNode("Active Skills"))
		{
			foreach (KeyValuePair<string, ActiveSkillDetails> activeSkill in ActiveSkills)
			{
				object obj;
				ActiveSkillDetails skilldetails;
				(obj, skilldetails) = (KeyValuePair<string, ActiveSkillDetails>)(activeSkill);
				if (obj == null)
				{
					obj = "";
				}
				if (ImGui.TreeNode((string)obj))
				{
					ImGui.Text($"Use Stage: {skilldetails.UseStage}");
					ImGui.Text($"Cast Type: {skilldetails.CastType}");
					ImGui.Text($"Skill UnknownIdAndEquipmentInfo: {skilldetails.UnknownIdAndEquipmentInfo:X}");
					MiscHelper.ActiveSkillGemDataParser(skilldetails.UnknownIdAndEquipmentInfo, out var iue, out var iu, out var si, out var li, out var inv, out var uid);
					ImGui.Text($"Can skill be on player item: {iue}");
					ImGui.Text($"Not sure what this does (something related to vaal skill): {iu}");
					ImGui.Text($"Skill Gem link Number: {li}");
					ImGui.Text($"Skill Gem socket Number: {si}");
					ImGui.Text($"Skill Gem Inventory Slot: {inv}");
					ImGui.Text($"Skill Gem Name Hash: {uid:X}");
					ImGuiHelper.IntPtrToImGui("Granted Effects Per Level Ptr", skilldetails.GrantedEffectsPerLevelDatRow);
					ImGuiHelper.IntPtrToImGui("Active Skills Ptr", skilldetails.ActiveSkillsDatPtr);
					ImGuiHelper.IntPtrToImGui("Granted Effect Stat Sets Per Level Ptr", skilldetails.GrantedEffectStatSetsPerLevelDatRow);
					ImGui.Text($"Can be used with weapons: {skilldetails.CanBeUsedWithWeapon}");
					ImGui.Text($"Can not be used: {skilldetails.CannotBeUsed}");
					ImGui.Text($"Unknown0: {skilldetails.UnknownByte0}");
					ImGui.Text($"Unknown1: {skilldetails.UnknownByte1}");
					ImGui.Text($"Total Uses: {skilldetails.TotalUses}");
					ImGui.TreePop();
				}
			}
			ImGui.TreePop();
		}
		if (ImGui.TreeNode("Can use skills"))
		{
			foreach (string skill in IsSkillUsable)
			{
				ImGui.Text("Skill " + skill + " can be used.");
			}
			ImGui.TreePop();
		}
		if (!ImGui.TreeNode("Deployed Objects"))
		{
			return;
		}
		ImGui.Text("Please throw mines, totem, minons, traps, etc to populate the data over here.");
		for (int i = 0; i < DeployedEntities.Length; i++)
		{
			if (DeployedEntities[i] > 0)
			{
				ImGui.Text($"Object Type: {i}, Total Count: {DeployedEntities[i]}");
			}
		}
		ImGui.TreePop();
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		ActorOffset data = reader.ReadMemory<ActorOffset>(base.Address);
		OwnerEntityAddress = data.Header.EntityPtr;
		Animation = (Animation)data.AnimationId;
		IsSkillUsable.Clear();
		VaalSoulStructure[] skillsvaalsouls = reader.ReadStdVector<VaalSoulStructure>(data.VaalSoulsPtr);
		for (int l = 0; l < skillsvaalsouls.Length; l++)
		{
			ActiveSkillsVaalSouls[skillsvaalsouls[l].ActiveSkillsDatPtr] = skillsvaalsouls[l];
		}
		ActiveSkillCooldown[] cooldowns = reader.ReadStdVector<ActiveSkillCooldown>(data.CooldownsPtr);
		for (int k = 0; k < cooldowns.Length; k++)
		{
			ActiveSkillCooldowns[cooldowns[k].UnknownIdAndEquipmentInf0] = cooldowns[k];
		}
		ActiveSkillStructure[] activeSkills = reader.ReadStdVector<ActiveSkillStructure>(data.ActiveSkillsPtr);
		for (int j = 0; j < activeSkills.Length; j++)
		{
			ActiveSkillDetails skillDetails = reader.ReadMemory<ActiveSkillDetails>(activeSkills[j].ActiveSkillPtr);
			if (skillDetails.GrantedEffectsPerLevelDatRow != IntPtr.Zero)
			{
				ref nint activeSkillsDatPtr = ref skillDetails.ActiveSkillsDatPtr;
				string name;
				(name, activeSkillsDatPtr) = ((string, nint))Core.GgpkObjectCache.AddOrGetExisting(skillDetails.GrantedEffectsPerLevelDatRow, (nint key) => (reader.ReadUnicodeString(reader.ReadMemory<nint>(reader.ReadMemory<nint>(key))), reader.ReadMemory<GrantedEffectsDatOffset>(reader.ReadMemory<GrantedEffectsPerLevelDatOffset>(key).GrantedEffectDatPtr).ActiveSkillDatPtr));
				ActiveSkills[name] = skillDetails;
				if (!skillDetails.CannotBeUsed && (!ActiveSkillCooldowns.TryGetValue(skillDetails.UnknownIdAndEquipmentInfo, out var cooldownInfo) || !cooldownInfo.CannotBeUsed()) && (!ActiveSkillsVaalSouls.TryGetValue(skillDetails.ActiveSkillsDatPtr, out var vaalSoulInfo) || !vaalSoulInfo.CannotBeUsed()))
				{
					IsSkillUsable.Add(name);
				}
			}
		}
		Array.Fill(DeployedEntities, 0);
		DeployedEntityStructure[] deployedEntities = reader.ReadStdVector<DeployedEntityStructure>(data.DeployedEntityArray);
		for (int i = 0; i < deployedEntities.Length; i++)
		{
			if (deployedEntities[i].DeployedObjectType < DeployedEntities.Length)
			{
				DeployedEntities[deployedEntities[i].DeployedObjectType]++;
			}
		}
	}
}
