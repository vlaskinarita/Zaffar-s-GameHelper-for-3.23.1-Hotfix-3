using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Coroutine;
using GameHelper.Cache;
using GameHelper.CoroutineEvents;
using GameHelper.RemoteEnums;
using GameHelper.RemoteEnums.Entity;
using GameHelper.RemoteObjects.Components;
using GameHelper.RemoteObjects.FilesStructures;
using GameHelper.Utils;
using GameOffsets.Natives;
using GameOffsets.Objects.Components;
using GameOffsets.Objects.States.InGameState;
using ImGuiNET;

namespace GameHelper.RemoteObjects.States.InGameStateObjects;

public class AreaInstance : RemoteObjectBase
{
	private int uselesssEntities;

	private int totalEntityRemoved;

	private string entityIdFilter;

	private string entityPathFilter;

	private Rarity entityRarityFilter;

	private byte filterBy;

	private StdVector environmentPtr;

	private readonly List<int> environments;

	public int CurrentAreaLevel { get; private set; }

	public string AreaHash { get; private set; }

	public ServerData ServerDataObject { get; }

	public Entity Player { get; }

	public ConcurrentDictionary<EntityNodeKey, Entity> AwakeEntities { get; }

	public List<DisappearingEntity> EntityCaches { get; }

	public int NetworkBubbleEntityCount { get; private set; }

	public int UselessAwakeEntities => uselesssEntities;

	public TerrainStruct TerrainMetadata { get; private set; }

	public float[][] GridHeightData { get; private set; }

	public byte[] GridWalkableData { get; private set; }

	public Dictionary<string, List<Vector2>> TgtTilesLocations { get; private set; }

	public float WorldToGridConvertor => TileStructure.TileToWorldConversion / (float)TileStructure.TileToGridConversion;

	internal AreaInstance(nint address)
		: base(address)
	{
		entityIdFilter = string.Empty;
		entityPathFilter = string.Empty;
		entityRarityFilter = Rarity.Normal;
		filterBy = 0;
		environmentPtr = default(StdVector);
		environments = new List<int>();
		CurrentAreaLevel = 0;
		AreaHash = string.Empty;
		ServerDataObject = new ServerData(IntPtr.Zero);
		Player = new Entity();
		AwakeEntities = new ConcurrentDictionary<EntityNodeKey, Entity>();
		EntityCaches = new List<DisappearingEntity>
		{
			new DisappearingEntity("/LeagueAffliction/", 1124, 1124, AwakeEntities),
			new DisappearingEntity("Breach", 1114, 1118, AwakeEntities)
		};
		NetworkBubbleEntityCount = 0;
		TerrainMetadata = default(TerrainStruct);
		GridHeightData = Array.Empty<float[]>();
		GridWalkableData = Array.Empty<byte>();
		TgtTilesLocations = new Dictionary<string, List<Vector2>>();
		Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(OnPerFrame(), "[AreaInstance] Update Area Data", 2147483643));
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		if (ImGui.TreeNode("Environment Info"))
		{
			ImGuiHelper.IntPtrToImGui("Address", environmentPtr.First);
			if (ImGui.TreeNode($"All Environments ({environments.Count})###AllEnvironments"))
			{
				for (int i = 0; i < environments.Count; i++)
				{
					if (ImGui.Selectable($"{environments[i]}"))
					{
						ImGui.SetClipboardText($"{environments[i]}");
					}
				}
				ImGui.TreePop();
			}
			foreach (DisappearingEntity entityCache in EntityCaches)
			{
				entityCache.ToImGui();
			}
			ImGui.TreePop();
		}
		ImGui.Text("Area Hash: " + AreaHash);
		ImGui.Text($"Monster Level: {CurrentAreaLevel}");
		if (ImGui.TreeNode("Terrain Metadata"))
		{
			ImGui.Text($"Total Tiles: {TerrainMetadata.TotalTiles}");
			ImGui.Text($"Tiles Data Pointer: {TerrainMetadata.TileDetailsPtr}");
			ImGui.Text($"Tiles Height Multiplier: {TerrainMetadata.TileHeightMultiplier}");
			ImGui.Text($"Grid Walkable Data: {TerrainMetadata.GridWalkableData}");
			ImGui.Text($"Grid Landscape Data: {TerrainMetadata.GridLandscapeData}");
			ImGui.Text($"Data Bytes Per Row (for Walkable/Landscape Data): {TerrainMetadata.BytesPerRow}");
			ImGui.TreePop();
		}
		if (Player.TryGetComponent<Render>(out var pPos))
		{
			int y = (int)pPos.GridPosition.Y;
			int x = (int)pPos.GridPosition.X;
			if (y < GridHeightData.Length && x < GridHeightData[0].Length)
			{
				ImGui.Text($"Player Pos (y:{y / TileStructure.TileToGridConversion}, x:{x / TileStructure.TileToGridConversion}) to Terrain Height: {GridHeightData[y][x]}");
			}
		}
		ImGui.Text($"Total Entity Removed Per Area: {totalEntityRemoved}");
		ImGui.Text($"Entities in network bubble: {NetworkBubbleEntityCount}");
		EntitiesWidget("Awake", AwakeEntities);
	}

	protected override void CleanUpData()
	{
		Cleanup(isAreaChange: false);
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		AreaInstanceOffsets data = reader.ReadMemory<AreaInstanceOffsets>(base.Address);
		if (hasAddressChanged)
		{
			Cleanup(isAreaChange: true);
			TerrainMetadata = data.TerrainMetadata;
			CurrentAreaLevel = data.CurrentAreaLevel;
			AreaHash = $"{data.CurrentAreaHash:X}";
			GridWalkableData = reader.ReadStdVector<byte>(TerrainMetadata.GridWalkableData);
			GridHeightData = GetTerrainHeight();
			TgtTilesLocations = GetTgtFileData();
		}
		UpdateEnvironmentAndCaches(data.Environments);
		ServerDataObject.Address = data.PlayerInfo.ServerDataPtr;
		Player.Address = data.PlayerInfo.LocalPlayerPtr;
		UpdateEntities(data.Entities.AwakeEntities, AwakeEntities, addToCache: true);
	}

	private void UpdateEnvironmentAndCaches(StdVector environments)
	{
		this.environments.Clear();
		SafeMemoryHandle handle = Core.Process.Handle;
		environmentPtr = environments;
		EnvironmentStruct[] envData = handle.ReadStdVector<EnvironmentStruct>(environments);
		for (int i = 0; i < envData.Length; i++)
		{
			this.environments.Add(envData[i].Key);
		}
		EntityCaches.ForEach(delegate(DisappearingEntity eCache)
		{
			eCache.UpdateState(this.environments);
		});
	}

	private void AddToCacheParallel(EntityNodeKey key, string path)
	{
		for (int i = 0; i < EntityCaches.Count && ((Core.GHSettings.UsingUnendingNightmareDeliriumKeystone && i == 0) || !EntityCaches[i].TryAddParallel(key, path)); i++)
		{
		}
	}

	private void UpdateEntities(StdMap ePtr, ConcurrentDictionary<EntityNodeKey, Entity> data, bool addToCache)
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		bool dc = Core.GHSettings.DisableAllCounters;
		WorldAreaDat areaDetails = Core.States.InGameStateObject.CurrentWorldInstance.AreaDetails;
		if (Core.GHSettings.DisableEntityProcessingInTownOrHideout && (areaDetails.IsHideout || areaDetails.IsTown))
		{
			NetworkBubbleEntityCount = 0;
			return;
		}
		uselesssEntities = 0;
		Parallel.ForEach(data, delegate(KeyValuePair<EntityNodeKey, Entity> kv)
		{
			if (kv.Value.IsValid)
			{
				if (!dc && kv.Value.EntityState == EntityStates.Useless)
				{
					Interlocked.Increment(ref uselesssEntities);
				}
			}
			else if (kv.Value.EntityState == EntityStates.MonsterFriendly || (kv.Value.CanExplodeOrRemovedFromGame && Player.DistanceFrom(kv.Value) < 150))
			{
				data.TryRemove(kv.Key, out var _);
				if (!dc)
				{
					Interlocked.Increment(ref totalEntityRemoved);
				}
			}
			kv.Value.IsValid = false;
		});
		NetworkBubbleEntityCount = reader.ReadStdMap(ePtr, 100000, !dc, delegate(EntityNodeKey key, EntityNodeValue value)
		{
			if (!Core.GHSettings.ProcessAllRenderableEntities && !EntityFilter.IgnoreVisualsAndDecorations(key))
			{
				return false;
			}
			if (data.TryGetValue(key, out var value2))
			{
				value2.Address = value.EntityPtr;
			}
			else
			{
				value2 = new Entity(value.EntityPtr);
				if (!string.IsNullOrEmpty(value2.Path))
				{
					data[key] = value2;
					if (addToCache)
					{
						AddToCacheParallel(key, value2.Path);
					}
				}
				else
				{
					value2 = null;
				}
			}
			value2?.UpdateNearby(Player);
			return true;
		});
	}

	private Dictionary<string, List<Vector2>> GetTgtFileData()
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		TileStructure[] tileData = reader.ReadStdVector<TileStructure>(TerrainMetadata.TileDetailsPtr);
		Dictionary<string, List<Vector2>> ret = new Dictionary<string, List<Vector2>>();
		object mylock = new object();
		Parallel.For(0, tileData.Length, () => new Dictionary<string, List<Vector2>>(), delegate(int tileNumber, ParallelLoopState _, Dictionary<string, List<Vector2>> localstate)
		{
			TileStructure tileStructure = tileData[tileNumber];
			TgtFileStruct tgtFileStruct = reader.ReadMemory<TgtFileStruct>(tileStructure.TgtFilePtr);
			string text = reader.ReadStdWString(tgtFileStruct.TgtPath);
			if (string.IsNullOrEmpty(text))
			{
				return localstate;
			}
			text = ((tileStructure.RotationSelector % 2 != 0) ? (text + $"x:{tileStructure.tileIdY}-y:{tileStructure.tileIdX}") : (text + $"x:{tileStructure.tileIdX}-y:{tileStructure.tileIdY}"));
			Vector2 vector = default(Vector2);
			vector.Y = tileNumber / TerrainMetadata.TotalTiles.X * TileStructure.TileToGridConversion;
			vector.X = tileNumber % TerrainMetadata.TotalTiles.X * TileStructure.TileToGridConversion;
			Vector2 item = vector;
			if (localstate.ContainsKey(text))
			{
				localstate[text].Add(item);
			}
			else
			{
				localstate[text] = new List<Vector2> { item };
			}
			return localstate;
		}, delegate(Dictionary<string, List<Vector2>> finalresult)
		{
			lock (mylock)
			{
				foreach (KeyValuePair<string, List<Vector2>> current in finalresult)
				{
					if (!ret.TryGetValue(current.Key, out var value))
					{
						value = new List<Vector2>();
						ret[current.Key] = value;
					}
					value.AddRange(current.Value);
				}
			}
		});
		return ret;
	}

	private float[][] GetTerrainHeight()
	{
		byte[] rotationHelper = Core.RotationSelector.Values;
		byte[] rotatorMetrixHelper = Core.RotatorHelper.Values;
		SafeMemoryHandle reader = Core.Process.Handle;
		TileStructure[] tileData = reader.ReadStdVector<TileStructure>(TerrainMetadata.TileDetailsPtr);
		ConcurrentDictionary<nint, sbyte[]> subTileHeightCache = new ConcurrentDictionary<nint, sbyte[]>();
		Parallel.For(0, tileData.Length, delegate(int index)
		{
			TileStructure tileStructure2 = tileData[index];
			subTileHeightCache.AddOrUpdate(tileStructure2.SubTileDetailsPtr, delegate(nint addr)
			{
				SubTileStruct subTileStruct = reader.ReadMemory<SubTileStruct>(addr);
				return reader.ReadStdVector<sbyte>(subTileStruct.SubTileHeight);
			}, (nint addr, sbyte[] data) => data);
		});
		int gridSizeX = (int)TerrainMetadata.TotalTiles.X * TileStructure.TileToGridConversion;
		int gridSizeY = (int)TerrainMetadata.TotalTiles.Y * TileStructure.TileToGridConversion;
		float[][] result = new float[gridSizeY][];
		Parallel.For(0, gridSizeY, delegate(int y)
		{
			result[y] = new float[gridSizeX];
			for (int i = 0; i < gridSizeX; i++)
			{
				int num = y / TileStructure.TileToGridConversion * (int)TerrainMetadata.TotalTiles.X;
				num += i / TileStructure.TileToGridConversion;
				int num2 = 0;
				if (num < tileData.Length)
				{
					TileStructure tileStructure = tileData[num];
					if (subTileHeightCache.TryGetValue(tileStructure.SubTileDetailsPtr, out var value))
					{
						int num3 = i % TileStructure.TileToGridConversion;
						int num4 = y % TileStructure.TileToGridConversion;
						int num5 = ((tileStructure.RotationSelector < rotationHelper.Length) ? (rotationHelper[tileStructure.RotationSelector] * 3) : 24);
						num5 = ((num5 > 24) ? 24 : num5);
						int[] obj = new int[4]
						{
							TileStructure.TileToGridConversion - num3 - 1,
							num3,
							TileStructure.TileToGridConversion - num4 - 1,
							num4
						};
						int num6 = rotatorMetrixHelper[num5];
						int num7 = rotatorMetrixHelper[num5 + 1];
						int num8 = rotatorMetrixHelper[num5 + 2];
						int num9 = 0;
						if (num6 == 0)
						{
							num9 = 2;
						}
						int x = obj[num6 * 2 + num7];
						int y2 = obj[num8 + num9];
						num2 = GetSubTerrainHeight(value, y2, x);
						result[y][i] = (float)tileStructure.TileHeight * (float)TerrainMetadata.TileHeightMultiplier + (float)num2;
						result[y][i] = result[y][i] * TerrainStruct.TileHeightFinalMultiplier * -1f;
					}
				}
			}
		});
		return result;
	}

	private int GetSubTerrainHeight(sbyte[] subterrainheightarray, int y, int x)
	{
		if (x < 0 || y < 0 || x >= TileStructure.TileToGridConversion || y >= TileStructure.TileToGridConversion)
		{
			return 0;
		}
		int index = y * TileStructure.TileToGridConversion + x;
		if (subterrainheightarray.Length == 0)
		{
			return 0;
		}
		int arrayLength = subterrainheightarray.Length;
		switch (arrayLength)
		{
		case 1:
			return subterrainheightarray[0];
		case 69:
			return subterrainheightarray[((byte)subterrainheightarray[(index >> 3) + 2] >> (index & 7)) & 1];
		case 137:
			return subterrainheightarray[((byte)subterrainheightarray[(index >> 2) + 4] >> ((index & 3) << 1)) & 3];
		case 281:
			return subterrainheightarray[((byte)subterrainheightarray[(index >> 1) + 16] >> ((index & 1) << 2)) & 0xF];
		default:
			if (arrayLength > index)
			{
				return subterrainheightarray[index];
			}
			throw new Exception($"SubterrainHeightArray Length {arrayLength} less-than index {index}");
		}
	}

	private void Cleanup(bool isAreaChange)
	{
		totalEntityRemoved = 0;
		uselesssEntities = 0;
		AwakeEntities.Clear();
		EntityCaches.ForEach(delegate(DisappearingEntity e)
		{
			e.Clear();
		});
		if (!isAreaChange)
		{
			environmentPtr = default(StdVector);
			environments.Clear();
			CurrentAreaLevel = 0;
			AreaHash = string.Empty;
			ServerDataObject.Address = IntPtr.Zero;
			Player.Address = IntPtr.Zero;
			NetworkBubbleEntityCount = 0;
			TerrainMetadata = default(TerrainStruct);
			GridHeightData = Array.Empty<float[]>();
			GridWalkableData = Array.Empty<byte>();
			TgtTilesLocations.Clear();
		}
	}

	private void EntitiesWidget(string label, ConcurrentDictionary<EntityNodeKey, Entity> data)
	{
		if (!ImGui.TreeNode($"{label} Entities ({data.Count})###${label} Entities"))
		{
			return;
		}
		if (ImGui.RadioButton("Filter by Id           ", filterBy == 0))
		{
			filterBy = 0;
		}
		ImGui.SameLine();
		if (ImGui.RadioButton("Filter by Path           ", filterBy == 1))
		{
			filterBy = 1;
		}
		ImGui.SameLine();
		if (ImGui.RadioButton("Filter by Rarity", filterBy == 2))
		{
			filterBy = 2;
		}
		switch (filterBy)
		{
		case 0:
			ImGui.InputText("Entity Id Filter", ref entityIdFilter, 10u, ImGuiInputTextFlags.CharsDecimal);
			break;
		case 1:
			ImGui.InputText("Entity Path Filter", ref entityPathFilter, 100u);
			break;
		case 2:
			ImGuiHelper.EnumComboBox("Entity Rarity Filter", ref entityRarityFilter);
			break;
		}
		foreach (KeyValuePair<EntityNodeKey, Entity> entity in data)
		{
			switch (filterBy)
			{
			case 0:
				if (!string.IsNullOrEmpty(entityIdFilter) && !$"{entity.Key.id}".Contains(entityIdFilter))
				{
					continue;
				}
				break;
			case 1:
				if (!string.IsNullOrEmpty(entityPathFilter) && !entity.Value.Path.ToLower().Contains(entityPathFilter.ToLower()))
				{
					continue;
				}
				break;
			case 2:
			{
				if (!entity.Value.TryGetComponent<ObjectMagicProperties>(out var omp2) || omp2.Rarity != entityRarityFilter)
				{
					continue;
				}
				break;
			}
			}
			bool isClicked = ImGui.TreeNode($"{entity.Value.Id} {entity.Value.Path}");
			ImGui.SameLine();
			if (ImGui.SmallButton($"dump##{entity.Key}"))
			{
				string filename = entity.Value.Path.Replace("/", "_") + ".txt";
				string contentToWrite = "============Path============\n";
				contentToWrite = contentToWrite + entity.Value.Path + "\n";
				contentToWrite += "============OMP Mods========\n";
				if (entity.Value.TryGetComponent<ObjectMagicProperties>(out var omp))
				{
					foreach (var mod in omp.Mods)
					{
						string name = mod.name;
						contentToWrite = contentToWrite + name + "\n";
					}
				}
				contentToWrite += "============BUFF===========\n";
				if (entity.Value.TryGetComponent<Buffs>(out var buf))
				{
					foreach (KeyValuePair<string, StatusEffectStruct> statusEffect in buf.StatusEffects)
					{
						contentToWrite = contentToWrite + statusEffect.Key + "\n";
					}
				}
				contentToWrite += "=========Component List====\n";
				foreach (string compName in entity.Value.GetComponentNames())
				{
					contentToWrite = contentToWrite + compName + "\n";
				}
				contentToWrite += "===========================\n";
				Directory.CreateDirectory("entity_dumps");
				File.AppendAllText(Path.Join("entity_dumps", filename), contentToWrite);
			}
			ImGuiHelper.ToolTip("Dump entity mods and buffs to file (if they exists).");
			if (isClicked)
			{
				entity.Value.ToImGui();
				ImGui.TreePop();
			}
			if (entity.Value.IsValid && entity.Value.TryGetComponent<Render>(out var eRender))
			{
				switch (filterBy)
				{
				case 0:
					ImGuiHelper.DrawText(eRender.WorldPosition, $"ID: {entity.Key.id}");
					break;
				case 1:
					ImGuiHelper.DrawText(eRender.WorldPosition, "Path: " + entity.Value.Path);
					break;
				}
			}
		}
		ImGui.TreePop();
	}

	private IEnumerator<Wait> OnPerFrame()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.PerFrameDataUpdate);
			if (base.Address != IntPtr.Zero)
			{
				UpdateData(hasAddressChanged: false);
			}
		}
	}
}
