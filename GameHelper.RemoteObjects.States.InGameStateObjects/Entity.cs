using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using GameHelper.RemoteEnums.Entity;
using GameHelper.RemoteObjects.Components;
using GameHelper.Utils;
using GameOffsets.Objects.States.InGameState;
using ImGuiNET;

namespace GameHelper.RemoteObjects.States.InGameStateObjects;

public class Entity : RemoteObjectBase
{
	private static readonly int MaxComponentsInAnEntity = 50;

	private static readonly string DeliriumHiddenMonsterStarting = "Metadata/Monsters/LeagueAffliction/DoodadDaemons/DoodadDaemon";

	private static readonly string DeliriumUselessMonsterStarting = "Metadata/Monsters/LeagueAffliction/Volatile/";

	private static readonly string LegionLeagueMonsterStarting = "Metadata/Monsters/LegionLeague/";

	private static readonly string BestiaryLeagueMonsterStarting = "Metadata/Monsters/LeagueBestiary/";

	private static readonly string HarbingerLeagueMonsterStarting = "Metadata/Monsters/Avatar/";

	private static readonly string TormentedSpirit = "Metadata/Monsters/Spirit/Tormented";

	private static readonly string BetrayalLeagueMonsterStarting = "Metadata/Monsters/LeagueBetrayal/Betrayal";

	private static readonly string ArchnemesisUselessMonster = "Metadata/Monsters/LeagueArchnemesis/LivingCrystal";

	private static readonly string AzmeriMiscellaneousObjectStarting = "Metadata/MiscellaneousObjects/Azmeri/";

	private static readonly string AbyssMiscellaneousObjectStarting = "Metadata/MiscellaneousObjects/Abyss/Abyss";

	private static readonly List<string> MavenUselessMonsters = new List<string> { "Metadata/Monsters/InvisibleFire/MavenLaserBarrageTarget", "Metadata/Monsters/MavenBoss/MavenBrainOrbitDaemon", "Metadata/Monsters/MavenBoss/MavenBrainVoidsandDaemon", "Metadata/Monsters/MavenBoss/TheMavenMap", "Metadata/Monsters/MavenBoss/TheMavenProving" };

	private static readonly List<string> Tier17UselessMonsters = new List<string> { "Metadata/Monsters/Daemon/DaemonElderTentacle", "Metadata/Monsters/AtlasBosses/TheShaperBossProjectiles", "Metadata/Monsters/AtlasExiles/AtlasExile5Wild", "Metadata/Monsters/AtlasExiles/AtlasExile5Apparition", "Metadata/Monsters/AtlasExiles/AtlasExile4Apparition", "Metadata/Monsters/AtlasExiles/AtlasExile3Apparition", "Metadata/Monsters/AtlasExiles/AtlasExile2Apparition", "Metadata/Monsters/AtlasExiles/AtlasExile1Apparition" };

	private readonly ConcurrentDictionary<string, nint> componentAddresses;

	private readonly ConcurrentDictionary<string, ComponentBase> componentCache;

	private NearbyZones zone;

	private int customGroup;

	private EntitySubtypes oldSubtypeWithoutPOI;

	public string Path { get; private set; }

	public uint Id { get; private set; }

	public NearbyZones Zones
	{
		get
		{
			if (!IsValid)
			{
				return NearbyZones.None;
			}
			return zone;
		}
	}

	public bool IsValid { get; set; }

	public EntityTypes EntityType { get; protected set; }

	public EntitySubtypes EntitySubtype { get; protected set; }

	public int EntityCustomGroup
	{
		get
		{
			if (EntitySubtype != EntitySubtypes.POIMonster)
			{
				return 0;
			}
			return customGroup;
		}
	}

	public EntityStates EntityState { get; protected set; }

	public bool CanExplodeOrRemovedFromGame
	{
		get
		{
			if (EntityState != EntityStates.Useless && EntityType != EntityTypes.Renderable && EntityType != EntityTypes.ImportantMiscellaneousObject && EntityType != EntityTypes.DeliriumSpawner && EntityType != EntityTypes.DeliriumBomb)
			{
				if (EntityType == EntityTypes.Monster)
				{
					return EntityState != EntityStates.LegionStage1Dead;
				}
				return false;
			}
			return true;
		}
	}

	internal Entity(nint address)
		: this()
	{
		base.Address = address;
	}

	internal Entity()
		: base(IntPtr.Zero, forceUpdate: true)
	{
		componentAddresses = new ConcurrentDictionary<string, nint>();
		componentCache = new ConcurrentDictionary<string, ComponentBase>();
		zone = NearbyZones.None;
		Path = string.Empty;
		Id = 0u;
		IsValid = false;
		EntityType = EntityTypes.Unidentified;
		EntitySubtype = EntitySubtypes.Unidentified;
		oldSubtypeWithoutPOI = EntitySubtypes.None;
		EntityState = EntityStates.None;
		customGroup = 0;
	}

	public int DistanceFrom(Entity other)
	{
		if (TryGetComponent<Render>(out var myPosComp) && other.TryGetComponent<Render>(out var otherPosComp))
		{
			float num = myPosComp.GridPosition.X - otherPosComp.GridPosition.X;
			float dy = myPosComp.GridPosition.Y - otherPosComp.GridPosition.Y;
			return (int)Math.Sqrt(num * num + dy * dy);
		}
		return 0;
	}

	public bool TryGetComponent<T>(out T component) where T : ComponentBase
	{
		component = null;
		string componenName = typeof(T).Name;
		if (componentCache.TryGetValue(componenName, out var comp))
		{
			component = (T)comp;
			return true;
		}
		if (componentAddresses.TryGetValue(componenName, out var compAddr) && compAddr != IntPtr.Zero)
		{
			component = Activator.CreateInstance(typeof(T), compAddr) as T;
			if (component != null)
			{
				componentCache[componenName] = component;
				return true;
			}
		}
		return false;
	}

	public bool IsOrWasMonsterSubType(EntitySubtypes subType)
	{
		return ((EntitySubtype == EntitySubtypes.POIMonster) ? oldSubtypeWithoutPOI : EntitySubtype) == subType;
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text("Path: " + Path);
		ImGui.Text($"Id: {Id}");
		ImGui.Text($"Is Valid: {IsValid}");
		ImGui.Text($"Nearby Zone: {Zones}");
		ImGui.Text($"Entity Type: {EntityType}");
		ImGui.Text($"Entity SubType: {EntitySubtype}");
		if (EntitySubtype == EntitySubtypes.POIMonster)
		{
			ImGui.Text($"Entity Old SubType: {oldSubtypeWithoutPOI}");
		}
		ImGui.Text($"Entity Custom Group: {EntityCustomGroup}");
		ImGui.Text($"Entity State: {EntityState}");
		if (!ImGui.TreeNode("Components"))
		{
			return;
		}
		foreach (KeyValuePair<string, nint> kv in componentAddresses)
		{
			if (componentCache.TryGetValue(kv.Key, out var value))
			{
				if (ImGui.TreeNode(kv.Key ?? ""))
				{
					value.ToImGui();
					ImGui.TreePop();
				}
				continue;
			}
			Type componentType = Type.GetType(typeof(NPC).Namespace + "." + kv.Key);
			if (componentType != null)
			{
				if (ImGui.SmallButton("Load##" + kv.Key))
				{
					LoadComponent(componentType);
				}
				ImGui.SameLine();
			}
			ImGuiHelper.IntPtrToImGui(kv.Key, kv.Value);
		}
		ImGui.TreePop();
	}

	internal IEnumerable<string> GetComponentNames()
	{
		return componentAddresses.Keys;
	}

	internal void UpdateNearby(Entity player)
	{
		if (EntityState != EntityStates.Useless)
		{
			int distance = DistanceFrom(player);
			if (distance < Core.GHSettings.InnerCircle.Meaning)
			{
				zone = NearbyZones.InnerCircle | NearbyZones.OuterCircle;
				return;
			}
			if (distance < Core.GHSettings.OuterCircle.Meaning)
			{
				zone = NearbyZones.OuterCircle;
				return;
			}
		}
		zone = NearbyZones.None;
	}

	protected bool UpdateComponentData(ItemStruct idata, bool hasAddressChanged)
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		if (hasAddressChanged)
		{
			componentAddresses.Clear();
			componentCache.Clear();
			EntityDetails entityDetails = reader.ReadMemory<EntityDetails>(idata.EntityDetailsPtr);
			Path = reader.ReadStdWString(entityDetails.name);
			if (string.IsNullOrEmpty(Path))
			{
				return false;
			}
			ComponentLookUpStruct lookupPtr = reader.ReadMemory<ComponentLookUpStruct>(entityDetails.ComponentLookUpPtr);
			if (lookupPtr.ComponentsNameAndIndex.Capacity > MaxComponentsInAnEntity)
			{
				return false;
			}
			List<ComponentNameAndIndexStruct> namesAndIndexes = reader.ReadStdBucket<ComponentNameAndIndexStruct>(lookupPtr.ComponentsNameAndIndex);
			nint[] entityComponent = reader.ReadStdVector<nint>(idata.ComponentListPtr);
			for (int i = 0; i < namesAndIndexes.Count; i++)
			{
				ComponentNameAndIndexStruct nameAndIndex = namesAndIndexes[i];
				if (nameAndIndex.Index >= 0 && nameAndIndex.Index < entityComponent.Length)
				{
					string name = reader.ReadString(nameAndIndex.NamePtr);
					if (!string.IsNullOrEmpty(name))
					{
						componentAddresses.TryAdd(name, entityComponent[nameAndIndex.Index]);
					}
				}
			}
		}
		else
		{
			foreach (KeyValuePair<string, ComponentBase> kv in componentCache)
			{
				kv.Value.Address = kv.Value.Address;
				if (!kv.Value.IsParentValid(base.Address))
				{
					return false;
				}
			}
		}
		return true;
	}

	protected override void CleanUpData()
	{
		componentAddresses?.Clear();
		componentCache?.Clear();
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		EntityOffsets entityData = Core.Process.Handle.ReadMemory<EntityOffsets>(base.Address);
		IsValid = EntityHelper.IsValidEntity(entityData.IsValid);
		if (!IsValid)
		{
			return;
		}
		Id = entityData.Id;
		if (EntityState != EntityStates.Useless)
		{
			if (!UpdateComponentData(entityData.ItemBase, hasAddressChanged))
			{
				UpdateComponentData(entityData.ItemBase, hasAddressChanged: true);
			}
			if (EntityType == EntityTypes.Unidentified && !TryCalculateEntityType())
			{
				EntityState = EntityStates.Useless;
			}
			else if (EntitySubtype == EntitySubtypes.Unidentified && !TryCalculateEntitySubType())
			{
				EntityState = EntityStates.Useless;
			}
			else
			{
				CalculateEntityState();
			}
		}
	}

	private void LoadComponent(Type componentType)
	{
		if (componentAddresses.TryGetValue(componentType.Name, out var compAddr) && compAddr != IntPtr.Zero && Activator.CreateInstance(componentType, compAddr) is ComponentBase component)
		{
			componentCache[componentType.Name] = component;
		}
	}

	private bool TryCalculateEntityType()
	{
		if (!TryGetComponent<Render>(out var _))
		{
			return false;
		}
		Player component3;
		Shrine component4;
		Life component5;
		NPC component9;
		if (TryGetComponent<Chest>(out var _))
		{
			EntityType = EntityTypes.Chest;
		}
		else if (TryGetComponent<Player>(out component3))
		{
			EntityType = EntityTypes.Player;
		}
		else if (TryGetComponent<Shrine>(out component4))
		{
			EntityType = EntityTypes.Shrine;
		}
		else if (TryGetComponent<Life>(out component5))
		{
			if (TryGetComponent<TriggerableBlockage>(out var _))
			{
				EntityType = EntityTypes.Blockage;
			}
			else
			{
				if (!TryGetComponent<Positioned>(out var pos))
				{
					return false;
				}
				if (!TryGetComponent<ObjectMagicProperties>(out var _))
				{
					return false;
				}
				if (!pos.IsFriendly && TryGetComponent<DiesAfterTime>(out var _))
				{
					if (!TryGetComponent<Targetable>(out var tComp) || !tComp.IsTargetable)
					{
						return false;
					}
					EntityType = EntityTypes.Monster;
				}
				else if (Path.StartsWith(DeliriumHiddenMonsterStarting) || Path.StartsWith(DeliriumUselessMonsterStarting))
				{
					if (Path.Contains("BloodBag"))
					{
						EntityType = EntityTypes.DeliriumBomb;
					}
					else if (Path.Contains("EggFodder"))
					{
						EntityType = EntityTypes.DeliriumSpawner;
					}
					else
					{
						if (!Path.Contains("GlobSpawn"))
						{
							return false;
						}
						EntityType = EntityTypes.DeliriumSpawner;
					}
				}
				else
				{
					if (!componentAddresses.ContainsKey("Buffs"))
					{
						return false;
					}
					EntityType = EntityTypes.Monster;
				}
			}
		}
		else if (TryGetComponent<NPC>(out component9))
		{
			EntityType = EntityTypes.NPC;
		}
		else if (Path.StartsWith(AzmeriMiscellaneousObjectStarting))
		{
			if (!TryGetComponent<Targetable>(out var _))
			{
				EntityType = EntityTypes.ImportantMiscellaneousObject;
			}
			else if (Path.StartsWith("Metadata/MiscellaneousObjects/Azmeri/AzmeriDustConverter"))
			{
				EntityType = EntityTypes.ImportantMiscellaneousObject;
				EntitySubtype = EntitySubtypes.AzmeriDustConvertor;
			}
			else
			{
				EntityType = EntityTypes.Shrine;
			}
		}
		else if (Path.StartsWith(AbyssMiscellaneousObjectStarting))
		{
			EntityType = EntityTypes.ImportantMiscellaneousObject;
		}
		else
		{
			if (!Core.GHSettings.ProcessAllRenderableEntities || !TryGetComponent<Positioned>(out var _))
			{
				return false;
			}
			EntityType = EntityTypes.Renderable;
		}
		return true;
	}

	private bool TryCalculateEntitySubType()
	{
		switch (EntityType)
		{
		case EntityTypes.Unidentified:
			throw new Exception($"Entity with path ({Path}) and Id (${Id}) is unidentified.");
		case EntityTypes.Chest:
		{
			TryGetComponent<Chest>(out var chestComp);
			if (Path.StartsWith("Metadata/Chests/LeagueAzmeri/OmenChest"))
			{
				EntitySubtype = EntitySubtypes.ImportantStrongbox;
				break;
			}
			if (Path.StartsWith("Metadata/Chests/LeagueAzmeri/"))
			{
				EntitySubtype = EntitySubtypes.ChestWithLabel;
				break;
			}
			if (Path.StartsWith("Metadata/Chests/LeaguesExpedition"))
			{
				EntitySubtype = EntitySubtypes.ExpeditionChest;
				break;
			}
			if (TryGetComponent<MinimapIcon>(out var _))
			{
				return false;
			}
			if (Path.StartsWith("Metadata/Chests/LegionChests"))
			{
				return false;
			}
			if (Path.StartsWith("Metadata/Chests/DelveChests/"))
			{
				EntitySubtype = EntitySubtypes.DelveChest;
			}
			else if (Path.StartsWith("Metadata/Chests/Breach"))
			{
				EntitySubtype = EntitySubtypes.BreachChest;
			}
			else if (chestComp.IsStrongbox || Path.StartsWith("Metadata/Chests/SynthesisChests/SynthesisChestAmbush"))
			{
				if (Path.StartsWith("Metadata/Chests/StrongBoxes/Arcanist") || Path.StartsWith("Metadata/Chests/StrongBoxes/Cartographer") || Path.StartsWith("Metadata/Chests/StrongBoxes/StrongboxDivination") || Path.StartsWith("Metadata/Chests/StrongBoxes/StrongboxScarab"))
				{
					EntitySubtype = EntitySubtypes.ImportantStrongbox;
				}
				else
				{
					EntitySubtype = EntitySubtypes.Strongbox;
				}
			}
			else if (chestComp.IsLabelVisible)
			{
				EntitySubtype = EntitySubtypes.ChestWithLabel;
			}
			else
			{
				EntitySubtype = EntitySubtypes.None;
			}
			break;
		}
		case EntityTypes.Player:
			if (Id == Core.States.InGameStateObject.CurrentAreaInstance.Player.Id)
			{
				EntitySubtype = EntitySubtypes.PlayerSelf;
			}
			else
			{
				EntitySubtype = EntitySubtypes.PlayerOther;
			}
			break;
		case EntityTypes.ImportantMiscellaneousObject:
			if (Path.StartsWith("Metadata/MiscellaneousObjects/Azmeri/AzmeriLightBomb"))
			{
				EntitySubtype = EntitySubtypes.AzmeriLightBomb;
			}
			else if (Path.StartsWith("Metadata/MiscellaneousObjects/Azmeri/AzmeriFuelResupply"))
			{
				EntitySubtype = EntitySubtypes.AzmeriRefuel;
			}
			else if (Path.StartsWith("Metadata/MiscellaneousObjects/Azmeri/AzmeriResource"))
			{
				if (!TryGetComponent<Animated>(out var ani))
				{
					return false;
				}
				if (ani.Path.Contains("wisp_primal_sml"))
				{
					EntitySubtype = EntitySubtypes.AzmeriBlueWispSml;
					break;
				}
				if (ani.Path.Contains("wisp_primal_med"))
				{
					EntitySubtype = EntitySubtypes.AzmeriBlueWispMed;
					break;
				}
				if (ani.Path.Contains("wisp_primal_big"))
				{
					EntitySubtype = EntitySubtypes.AzmeriBlueWispBig;
					break;
				}
				if (ani.Path.Contains("wisp_warden_sml"))
				{
					EntitySubtype = EntitySubtypes.AzmeriYellowWispSml;
					break;
				}
				if (ani.Path.Contains("wisp_warden_med"))
				{
					EntitySubtype = EntitySubtypes.AzmeriYellowWispMed;
					break;
				}
				if (ani.Path.Contains("wisp_warden_big"))
				{
					EntitySubtype = EntitySubtypes.AzmeriYellowWispBig;
					break;
				}
				if (ani.Path.Contains("wisp_vodoo_sml"))
				{
					EntitySubtype = EntitySubtypes.AzmeriPurpleWispSml;
					break;
				}
				if (ani.Path.Contains("wisp_vodoo_med"))
				{
					EntitySubtype = EntitySubtypes.AzmeriPurpleWispMed;
					break;
				}
				if (!ani.Path.Contains("wisp_vodoo_big"))
				{
					return false;
				}
				EntitySubtype = EntitySubtypes.AzmeriPurpleWispBig;
			}
			else if (Path.StartsWith("Metadata/MiscellaneousObjects/Abyss/AbyssStartNode"))
			{
				EntitySubtype = EntitySubtypes.AbyssStartNode;
			}
			else if (Path.StartsWith("Metadata/MiscellaneousObjects/Abyss/AbyssFinalNode"))
			{
				EntitySubtype = EntitySubtypes.AbyssFinalNode;
			}
			else if (Path.StartsWith("Metadata/MiscellaneousObjects/Abyss/AbyssCrack") || Path.StartsWith("Metadata/MiscellaneousObjects/Abyss/AbyssNodeMini"))
			{
				EntitySubtype = EntitySubtypes.AbyssCrack;
			}
			else if (Path.StartsWith("Metadata/MiscellaneousObjects/Abyss/AbyssNode"))
			{
				EntitySubtype = EntitySubtypes.AbyssMidNode;
			}
			else
			{
				EntitySubtype = EntitySubtypes.None;
			}
			break;
		case EntityTypes.Shrine:
			if (Path.StartsWith("Metadata/MiscellaneousObjects/Azmeri/SacrificeAltarObjects/AzmeriSacrificeAltar"))
			{
				EntitySubtype = EntitySubtypes.AzmeriSacrificeAltar;
			}
			else if (Path.StartsWith("Metadata/MiscellaneousObjects/Azmeri/AzmeriFlaskRefill"))
			{
				EntitySubtype = EntitySubtypes.AzmeriWell;
			}
			else if (Path.StartsWith("Metadata/MiscellaneousObjects/Azmeri/AzmeriBuffEffigySmall"))
			{
				EntitySubtype = EntitySubtypes.AzmeriExperienceGainShrine;
			}
			else if (Path.StartsWith("Metadata/MiscellaneousObjects/Azmeri/AzmeriBuffEffigyMedium"))
			{
				EntitySubtype = EntitySubtypes.AzmeriIncreaseQuanityShrine;
			}
			else if (Path.StartsWith("Metadata/MiscellaneousObjects/Azmeri/AzmeriBuffEffigyLarge"))
			{
				EntitySubtype = EntitySubtypes.AzmeriCanNotBeDamagedShrine;
			}
			else if (Path.StartsWith(AzmeriMiscellaneousObjectStarting))
			{
				EntitySubtype = EntitySubtypes.AzmeriUnknownShrine;
			}
			else
			{
				EntitySubtype = EntitySubtypes.None;
			}
			break;
		case EntityTypes.Monster:
		{
			if (!TryGetComponent<ObjectMagicProperties>(out var omp))
			{
				return false;
			}
			if (Path.StartsWith(LegionLeagueMonsterStarting) && TryGetComponent<Buffs>(out var buffComp))
			{
				if (buffComp.StatusEffects.ContainsKey("legion_reward_display"))
				{
					EntitySubtype = EntitySubtypes.LegionChest;
				}
				else if (Path.Contains("ChestEpic"))
				{
					EntitySubtype = EntitySubtypes.LegionEpicChest;
				}
				else if (Path.Contains("Chest"))
				{
					EntitySubtype = EntitySubtypes.LegionChest;
				}
				else
				{
					EntitySubtype = EntitySubtypes.LegionMonster;
				}
			}
			else if (Path.StartsWith(BestiaryLeagueMonsterStarting) || omp.ModNames.Contains("BestiaryModYellowDifficulty") || omp.ModNames.Contains("BestiaryModYellowCannotBeDestroyed"))
			{
				EntitySubtype = EntitySubtypes.BestiaryMonster;
			}
			else if (Path.StartsWith(HarbingerLeagueMonsterStarting) && omp.ModNames.Contains("MonsterCannotBeDamaged"))
			{
				EntitySubtype = EntitySubtypes.HarbingerMonster;
			}
			else if (Path.StartsWith(TormentedSpirit))
			{
				EntitySubtype = EntitySubtypes.TormentedSpiritsMonster;
			}
			else if (Path.StartsWith(BetrayalLeagueMonsterStarting) && componentAddresses.ContainsKey("NPC"))
			{
				EntitySubtype = EntitySubtypes.BetrayalEnemyNPC;
			}
			else
			{
				if (Path.StartsWith(ArchnemesisUselessMonster))
				{
					return false;
				}
				if (MavenUselessMonsters.Any(Path.StartsWith))
				{
					return false;
				}
				if (Tier17UselessMonsters.Any(Path.StartsWith))
				{
					return false;
				}
				if (omp.ModNames.Contains("PinnacleAtlasBoss"))
				{
					EntitySubtype = EntitySubtypes.PinnacleBoss;
				}
				else
				{
					EntitySubtype = EntitySubtypes.None;
				}
			}
			for (int i = 0; i < Core.GHSettings.PoiMonstersCategories.Count; i++)
			{
				var (filtertype, filter, rarity, group) = Core.GHSettings.PoiMonstersCategories[i];
				if (filtertype switch
				{
					EntityFilterType.PATH => Path.StartsWith(filter), 
					EntityFilterType.PATHANDRARITY => omp.Rarity == rarity && Path.StartsWith(filter), 
					EntityFilterType.MOD => omp.ModNames.Contains(filter), 
					EntityFilterType.MODANDRARITY => omp.Rarity == rarity && omp.ModNames.Contains(filter), 
					_ => throw new Exception($"EntityFilterType {filtertype} added but not handled in Entity file."), 
				})
				{
					oldSubtypeWithoutPOI = EntitySubtype;
					EntitySubtype = EntitySubtypes.POIMonster;
					customGroup = group;
				}
			}
			break;
		}
		case EntityTypes.NPC:
			if (Core.GHSettings.SpecialNPCPaths.Any(Path.StartsWith))
			{
				EntitySubtype = EntitySubtypes.SpecialNPC;
			}
			else if (Path.StartsWith("Metadata/NPC/League/Azmeri/UniqueDealer"))
			{
				EntitySubtype = EntitySubtypes.AzmeriTraderNPC;
			}
			else if (Path.StartsWith("Metadata/NPC/League/Affliction/GlyphsHarvestTree"))
			{
				EntitySubtype = EntitySubtypes.AzmeriHarvestNPC;
			}
			else
			{
				EntitySubtype = EntitySubtypes.None;
			}
			break;
		case EntityTypes.Item:
			EntitySubtype = EntitySubtypes.WorldItem;
			break;
		default:
			throw new Exception($"Please update TryCalculateEntitySubType function to include {EntityType}.");
		case EntityTypes.Blockage:
		case EntityTypes.DeliriumBomb:
		case EntityTypes.DeliriumSpawner:
		case EntityTypes.Renderable:
			break;
		}
		return true;
	}

	private void CalculateEntityState()
	{
		Player playerComp;
		if (EntityType == EntityTypes.Chest)
		{
			if (TryGetComponent<Chest>(out var chestComp) && chestComp.IsOpened)
			{
				EntityState = EntityStates.Useless;
			}
		}
		else if (EntityType == EntityTypes.DeliriumBomb || EntityType == EntityTypes.DeliriumSpawner)
		{
			if (TryGetComponent<Life>(out var lifeComp) && !lifeComp.IsAlive)
			{
				EntityState = EntityStates.Useless;
			}
		}
		else if (EntityType == EntityTypes.Monster)
		{
			if (!TryGetComponent<Life>(out var lifeComp2))
			{
				return;
			}
			if (!lifeComp2.IsAlive)
			{
				EntityState = EntityStates.Useless;
			}
			else
			{
				if (!TryGetComponent<Positioned>(out var posComp))
				{
					return;
				}
				Buffs buffComp;
				if (posComp.IsFriendly)
				{
					EntityState = EntityStates.MonsterFriendly;
				}
				else if (EntityState == EntityStates.MonsterFriendly)
				{
					EntityState = EntityStates.None;
				}
				else if (IsBestiaryOrUsedToBe())
				{
					if (TryGetComponent<Buffs>(out var buffsComp) && buffsComp.StatusEffects.ContainsKey("capture_monster_trapped"))
					{
						EntityState = EntityStates.Useless;
					}
				}
				else if (IsBetrayalEnemyNPCOrUsedToBe())
				{
					if (TryGetComponent<Buffs>(out var buffsComp2) && buffsComp2.StatusEffects.ContainsKey("betrayal_target_safety_aura"))
					{
						EntityState = EntityStates.Useless;
					}
				}
				else if (IsPinnacleBossOrUsedToBe())
				{
					if (TryGetComponent<Buffs>(out var buffsComp3) && buffsComp3.StatusEffects.ContainsKey("hidden_monster"))
					{
						EntityState = EntityStates.PinnacleBossHidden;
					}
					else
					{
						EntityState = EntityStates.None;
					}
				}
				else if (IsLegionRelatedOrUsedToBe() && TryGetComponent<Buffs>(out buffComp))
				{
					bool isFrozenInTime = buffComp.StatusEffects.ContainsKey("frozen_in_time");
					bool isHidden = buffComp.StatusEffects.ContainsKey("hidden_monster");
					if (isFrozenInTime && isHidden)
					{
						EntityState = EntityStates.LegionStage0;
					}
					else if (isFrozenInTime)
					{
						EntityState = EntityStates.LegionStage1Alive;
					}
					else if (isHidden)
					{
						EntityState = EntityStates.LegionStage1Dead;
					}
					else
					{
						EntityState = EntityStates.None;
					}
				}
			}
		}
		else if (EntitySubtype == EntitySubtypes.PlayerOther && TryGetComponent<Player>(out playerComp))
		{
			if (playerComp.Name.Equals(Core.GHSettings.LeaderName))
			{
				EntityState = EntityStates.PlayerLeader;
			}
			else
			{
				EntityState = EntityStates.None;
			}
		}
	}

	private bool IsBestiaryOrUsedToBe()
	{
		return ((EntitySubtype == EntitySubtypes.POIMonster) ? oldSubtypeWithoutPOI : EntitySubtype) == EntitySubtypes.BestiaryMonster;
	}

	private bool IsBetrayalEnemyNPCOrUsedToBe()
	{
		return ((EntitySubtype == EntitySubtypes.POIMonster) ? oldSubtypeWithoutPOI : EntitySubtype) == EntitySubtypes.BetrayalEnemyNPC;
	}

	private bool IsPinnacleBossOrUsedToBe()
	{
		return ((EntitySubtype == EntitySubtypes.POIMonster) ? oldSubtypeWithoutPOI : EntitySubtype) == EntitySubtypes.PinnacleBoss;
	}

	private bool IsLegionRelatedOrUsedToBe()
	{
		EntitySubtypes toCheck = ((EntitySubtype == EntitySubtypes.POIMonster) ? oldSubtypeWithoutPOI : EntitySubtype);
		if (toCheck != EntitySubtypes.LegionChest && toCheck != EntitySubtypes.LegionEpicChest)
		{
			return toCheck == EntitySubtypes.LegionMonster;
		}
		return true;
	}
}
