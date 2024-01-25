using System.Collections.Generic;
using System.IO;
using ClickableTransparentOverlay;
using ClickableTransparentOverlay.Win32;
using GameHelper.RemoteEnums;
using GameHelper.RemoteEnums.Entity;
using Newtonsoft.Json;

namespace GameHelper.Settings;

internal class State
{
	[JsonIgnore]
	public static readonly FileInfo CoreSettingFile = new FileInfo("configs/core_settings.json");

	[JsonIgnore]
	public static readonly FileInfo PluginsMetadataFile = new FileInfo("configs/plugins.json");

	[JsonIgnore]
	public static readonly DirectoryInfo PluginsDirectory = new DirectoryInfo("Plugins");

	public bool UsingUnendingNightmareDeliriumKeystone = true;

	public bool DisableEntityProcessingInTownOrHideout;

	public int EntitiesToReadBeforeGoingParallel = 32;

	public bool DisableAllCounters;

	public bool HidePerfStatsWhenBg;

	public bool MinimumPerfStats = true;

	public bool HideSettingWindowOnStart;

	[JsonIgnore]
	public bool IsOverlayRunning = true;

	public int KeyPressTimeout = 80;

	public string FontPathName = "C:\\Windows\\Fonts\\msyh.ttc";

	public int FontSize = 18;

	public FontGlyphRangeType FontLanguage = FontGlyphRangeType.ChineseSimplifiedCommon;

	public string FontCustomGlyphRange = string.Empty;

	public VK MainMenuHotKey = VK.F12;

	public bool ShowDataVisualization;

	public bool ShowKrangledPassiveDetector;

	public bool ShowGameUiExplorer;

	public bool ShowPerfStats;

	public (int Meaning, bool IsVisible, bool FollowMouse) OuterCircle = (Meaning: 70, IsVisible: false, FollowMouse: false);

	public (int Meaning, bool IsVisible, bool FollowMouse) InnerCircle = (Meaning: 30, IsVisible: false, FollowMouse: false);

	public bool SkipPreloadedFilesInHideout = true;

	public bool CloseWhenGameExit;

	public bool Vsync = true;

	public bool EnableControllerMode;

	public string LeaderName = string.Empty;

	public VK DisableAllRenderingKey = VK.F9;

	public List<string> SpecialNPCPaths = new List<string>();

	public List<(EntityFilterType filtertype, string filter, Rarity rarity, int group)> PoiMonstersCategories = new List<(EntityFilterType, string, Rarity, int)>();

	public bool ProcessAllRenderableEntities;
}
