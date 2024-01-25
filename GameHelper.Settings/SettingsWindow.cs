using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using ClickableTransparentOverlay;
using ClickableTransparentOverlay.Win32;
using Coroutine;
using GameHelper.CoroutineEvents;
using GameHelper.Plugin;
using GameHelper.RemoteEnums;
using GameHelper.RemoteEnums.Entity;
using GameHelper.Utils;
using ImGuiNET;

namespace GameHelper.Settings;

internal static class SettingsWindow
{
	private static Vector4 color = new Vector4(1f, 1f, 0f, 1f);

	private static bool isOverlayRunningLocal = true;

	private static bool isSettingsWindowVisible = true;

	private static EntityFilterType efilterType = EntityFilterType.PATH;

	private static string filterText = string.Empty;

	private static Rarity erarity = Rarity.Normal;

	private static int filterGroup = 0;

	private static string specialNpcPath = string.Empty;

	internal static void InitializeCoroutines()
	{
		HideOnStartCheck();
		CoroutineHandler.Start(SaveCoroutine());
		Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(RenderCoroutine(), "[Settings] Draw Core/Plugin settings", int.MaxValue));
	}

	private static void DrawManuBar()
	{
		if (!ImGui.BeginMenuBar())
		{
			return;
		}
		if (ImGui.BeginMenu("Enable Plugins"))
		{
			foreach (PluginContainer container in PManager.Plugins)
			{
				bool isEnabled = container.Metadata.Enable;
				if (ImGui.Checkbox(container.Name ?? "", ref isEnabled))
				{
					container.Metadata.Enable = !container.Metadata.Enable;
					if (container.Metadata.Enable)
					{
						container.Plugin.OnEnable(Core.Process.Address != IntPtr.Zero);
						continue;
					}
					container.Plugin.SaveSettings();
					container.Plugin.OnDisable();
				}
			}
			ImGui.EndMenu();
		}
		if (ImGui.MenuItem("Donate (捐)"))
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = "https://www.paypal.com/paypalme/tehcheat",
				UseShellExecute = true
			});
		}
		ImGui.EndMenuBar();
	}

	private static void DrawTabs()
	{
		if (!ImGui.BeginTabBar("pluginsTabBar", ImGuiTabBarFlags.Reorderable | ImGuiTabBarFlags.AutoSelectNewTabs))
		{
			return;
		}
		if (ImGui.BeginTabItem("Core"))
		{
			if (ImGui.BeginChild("CoreChildSetting"))
			{
				DrawCoreSettings();
				ImGui.EndChild();
			}
			ImGui.EndTabItem();
		}
		foreach (PluginContainer container in PManager.Plugins)
		{
			if (container.Metadata.Enable && ImGui.BeginTabItem(container.Name))
			{
				if (ImGui.BeginChild("PluginChildSetting"))
				{
					container.Plugin.DrawSettings();
					ImGui.EndChild();
				}
				ImGui.EndTabItem();
			}
		}
		ImGui.EndTabBar();
	}

	private static void DrawCoreSettings()
	{
		ImGui.PushTextWrapPos(ImGui.GetContentRegionMax().X);
		ImGui.TextColored(color, "This is a free software, only use https://ownedcore.com to download it. Do not buy from the fake sellers or websites.");
		ImGui.TextColored(color, "请不要花钱购买本软件，否则你就是个傻逼。这是一个免费软件。不要从假卖家那里购买。前往 https://ownedcore.com 免费下载。");
		ImGui.NewLine();
		ImGui.TextColored(Vector4.One, "Developer of this software is not responsible for any loss that may happen due to the usage of this software. Use this software at your own risk.");
		ImGui.NewLine();
		ImGui.TextColored(Vector4.One, "All Settings (including plugins) are saved automatically " + $"when you close the overlay or hide it via {Core.GHSettings.MainMenuHotKey} button.");
		ImGui.NewLine();
		ImGui.Text($"Current Game State: {Core.States.GameCurrentState}");
		ImGui.PopTextWrapPos();
		ImGui.InputText("Party Leader Name", ref Core.GHSettings.LeaderName, 200u);
		DrawPoiWidget();
		DrawNPCWidget();
		DrawNearbyWidget();
		DrawKeyboardConfigWidget();
		DrawToolsConfig();
		DrawMiscConfig();
		ChangeFontWidget();
		DrawReloadPluginWidget();
	}

	private static void DrawNearbyWidget()
	{
		if (ImGui.CollapsingHeader("Nearby Monster Config"))
		{
			ImGui.DragInt("Small Range", ref Core.GHSettings.InnerCircle.Meaning, 1f, 0, Core.GHSettings.OuterCircle.Meaning);
			ImGui.SameLine();
			ImGui.Checkbox("Visible##small", ref Core.GHSettings.InnerCircle.IsVisible);
			ImGui.DragInt("Large Range", ref Core.GHSettings.OuterCircle.Meaning, 1f, Core.GHSettings.InnerCircle.Meaning, 150);
			ImGui.SameLine();
			ImGui.Checkbox("Visible##large", ref Core.GHSettings.OuterCircle.IsVisible);
		}
	}

	private static void ChangeFontWidget()
	{
		if (!ImGui.CollapsingHeader("Change Fonts"))
		{
			return;
		}
		ImGui.InputText("Pathname", ref Core.GHSettings.FontPathName, 300u);
		ImGui.DragInt("Size", ref Core.GHSettings.FontSize, 0.1f, 13, 40);
		bool num = ImGuiHelper.EnumComboBox("Language", ref Core.GHSettings.FontLanguage);
		bool customLanguage = ImGui.InputText("Custom Glyph Ranges", ref Core.GHSettings.FontCustomGlyphRange, 100u);
		ImGuiHelper.ToolTip("This is advance level feature. Do not modify this if you don't know what you are doing. Example usage:- If you have downloaded and pointed to the ArialUnicodeMS.ttf font, you can use 0x0020, 0xFFFF, 0x00 text in this field to load all of the font texture in ImGui. Note the 0x00 as the last item in the range.");
		if (num)
		{
			Core.GHSettings.FontCustomGlyphRange = string.Empty;
		}
		if (customLanguage)
		{
			Core.GHSettings.FontLanguage = FontGlyphRangeType.English;
		}
		if (ImGui.Button("Apply Changes"))
		{
			if (MiscHelper.TryConvertStringToImGuiGlyphRanges(Core.GHSettings.FontCustomGlyphRange, out var glyphranges))
			{
				Core.Overlay.ReplaceFont(Core.GHSettings.FontPathName, Core.GHSettings.FontSize, glyphranges);
			}
			else
			{
				Core.Overlay.ReplaceFont(Core.GHSettings.FontPathName, Core.GHSettings.FontSize, Core.GHSettings.FontLanguage);
			}
		}
	}

	private static void DrawPoiWidget()
	{
		bool num = ImGui.CollapsingHeader("Special Monster Tracker (A.K.A Monster POI)");
		ImGuiHelper.ToolTip("In order to figure out the path/mod to add please open DV -> States -> InGameState -> CurrentAreaInstance -> Awake Entities -> click dump button against the entity you want to add. This will create a new file in entity_dumps folder with all mod names and path of that entity.");
		if (!num)
		{
			return;
		}
		ImGui.TextWrapped("Please restart gamehelper or change area/zone if you make any changes over here.");
		for (int i = 0; i < Core.GHSettings.PoiMonstersCategories.Count; i++)
		{
			(EntityFilterType filtertype, string filter, Rarity rarity, int group) tuple = Core.GHSettings.PoiMonstersCategories[i];
			EntityFilterType filtertype = tuple.filtertype;
			string filter = tuple.filter;
			Rarity rarity = tuple.rarity;
			int group = tuple.group;
			bool isChanged = false;
			ImGui.SetNextItemWidth(ImGui.GetFontSize() * 5f);
			if (ImGuiHelper.EnumComboBox($"Filter type##{i}", ref filtertype))
			{
				isChanged = true;
			}
			ImGui.SameLine();
			ImGui.SetNextItemWidth(ImGui.GetFontSize() * 17f);
			if (ImGui.InputText($"Filter##{i}", ref filter, 200u))
			{
				isChanged = true;
			}
			ImGuiHelper.ToolTip((filtertype == EntityFilterType.PATH) ? "Path is going to be checked from left to right, up till the filter length." : "Mod name is fully checked, it need to be 100% match.");
			ImGui.SameLine();
			if (filtertype == EntityFilterType.PATHANDRARITY || filtertype == EntityFilterType.MODANDRARITY)
			{
				ImGui.SetNextItemWidth(ImGui.GetFontSize() * 5f);
				if (ImGuiHelper.EnumComboBox($"Rarity##{i}", ref rarity))
				{
					isChanged = true;
				}
				ImGui.SameLine();
			}
			ImGui.SetNextItemWidth(ImGui.GetFontSize() * 5f);
			if (ImGui.InputInt($"Group Number##{i}", ref group))
			{
				if (group < 0)
				{
					group = 0;
				}
				isChanged = true;
			}
			if (isChanged)
			{
				Core.GHSettings.PoiMonstersCategories[i] = (filtertype, filter, rarity, group);
			}
			ImGui.SameLine();
			if (ImGui.Button($"delete##{i}"))
			{
				Core.GHSettings.PoiMonstersCategories.RemoveAt(i);
			}
		}
		ImGui.Separator();
		ImGui.SetNextItemWidth(ImGui.GetFontSize() * 5f);
		ImGuiHelper.EnumComboBox("Filter type##add", ref efilterType);
		ImGui.SameLine();
		ImGui.SetNextItemWidth(ImGui.GetFontSize() * 17f);
		ImGui.InputText("Filter##add", ref filterText, 200u);
		ImGuiHelper.ToolTip((efilterType == EntityFilterType.PATH) ? "Path is going to be checked from left to right, up till the filter length." : "Mod name is fully checked, it need to be 100% match.");
		ImGui.SameLine();
		if (efilterType == EntityFilterType.PATHANDRARITY || efilterType == EntityFilterType.MODANDRARITY)
		{
			ImGui.SetNextItemWidth(ImGui.GetFontSize() * 5f);
			ImGuiHelper.EnumComboBox("Rarity##add", ref erarity);
			ImGui.SameLine();
		}
		ImGui.SetNextItemWidth(ImGui.GetFontSize() * 5f);
		if (ImGui.InputInt("Group Number##add", ref filterGroup) && filterGroup < 0)
		{
			filterGroup = 0;
		}
		ImGui.SameLine();
		if (ImGui.Button("add"))
		{
			Core.GHSettings.PoiMonstersCategories.Add((efilterType, filterText, erarity, filterGroup));
			efilterType = EntityFilterType.PATH;
			filterText = string.Empty;
			filterGroup = 0;
		}
	}

	private static void DrawNPCWidget()
	{
		bool num = ImGui.CollapsingHeader("Special NPC Metadata Paths");
		ImGuiHelper.ToolTip("In order to figure out the path, please open DV -> States -> InGameState -> CurrentAreaInstance -> Awake Entities -> Click Path -> see NPC path in the game world");
		if (!num)
		{
			return;
		}
		ImGui.TextWrapped("Please restart gamehelper or change area/zone if you make any changes over here.");
		ImGui.InputText("Special NPC Path", ref specialNpcPath, 200u);
		ImGui.SameLine();
		if (ImGui.Button("Add##specialNPCPath") && !string.IsNullOrEmpty(specialNpcPath))
		{
			Core.GHSettings.SpecialNPCPaths.Add(specialNpcPath);
			specialNpcPath = string.Empty;
		}
		for (int i = 0; i < Core.GHSettings.SpecialNPCPaths.Count; i++)
		{
			ImGui.Text("Path: " + Core.GHSettings.SpecialNPCPaths[i]);
			ImGui.SameLine();
			if (ImGui.Button($"Delete##{i}"))
			{
				Core.GHSettings.SpecialNPCPaths.RemoveAt(i);
			}
		}
	}

	private static void DrawKeyboardConfigWidget()
	{
		if (ImGui.CollapsingHeader("Keyboard Config"))
		{
			ImGui.DragInt("Key Timeout", ref Core.GHSettings.KeyPressTimeout, 0.2f, 60, 300);
			ImGuiHelper.ToolTip("When GameOverlay press a key in the game, the key has to go to the GGG server for it to work. This process takes time equal to your latency x 3. During this time GameOverlay might press that key again. Set the key timeout value to latency x 3 so this doesn't happen. e.g. for 30ms latency, set it to 90ms. Also, do not go below 60 (due to server ticks), no matter how good your latency is.");
			ImGuiHelper.NonContinuousEnumComboBox("Settings Window Key", ref Core.GHSettings.MainMenuHotKey);
			ImGuiHelper.NonContinuousEnumComboBox("Disable Rendering Key", ref Core.GHSettings.DisableAllRenderingKey);
			ImGui.Checkbox("I am using console controller to play the game.", ref Core.GHSettings.EnableControllerMode);
			ImGuiHelper.ToolTip("This will disable AutoHotkeyTrigger plugin by ensuring all keyboard inputs that AHK wants to send to the game doesn't reach the game. Feel free to disable that plugin  from top bar if you want to safe some cpu cycles.");
		}
	}

	private static void DrawToolsConfig()
	{
		if (ImGui.CollapsingHeader("Misc Tools"))
		{
			ImGui.Checkbox("Performance Stats", ref Core.GHSettings.ShowPerfStats);
			if (Core.GHSettings.ShowPerfStats)
			{
				ImGui.Spacing();
				ImGui.SameLine();
				ImGui.Spacing();
				ImGui.SameLine();
				ImGui.Checkbox("Hide when game is in background", ref Core.GHSettings.HidePerfStatsWhenBg);
				ImGui.Spacing();
				ImGui.SameLine();
				ImGui.Spacing();
				ImGui.SameLine();
				ImGui.Checkbox("Show minimum stats", ref Core.GHSettings.MinimumPerfStats);
			}
			ImGui.Checkbox("Game UiExplorer (GE)", ref Core.GHSettings.ShowGameUiExplorer);
			ImGui.Checkbox("Data Visualization (DV)", ref Core.GHSettings.ShowDataVisualization);
		}
	}

	private static void DrawMiscConfig()
	{
		if (ImGui.CollapsingHeader("Miscellaneous Config"))
		{
			ImGui.Checkbox("Disable entity processing when in town or hideout", ref Core.GHSettings.DisableEntityProcessingInTownOrHideout);
			ImGui.Checkbox("Hide overlay settings upon start", ref Core.GHSettings.HideSettingWindowOnStart);
			ImGui.Checkbox("Close GameHelper when Game Exit", ref Core.GHSettings.CloseWhenGameExit);
			if (ImGui.Checkbox("V-Sync", ref Core.Overlay.VSync))
			{
				Core.GHSettings.Vsync = Core.Overlay.VSync;
			}
			ImGuiHelper.ToolTip("WARNING: There is no rate limiter in GameHelper, once V-Sync is off,\nit's your responsibility to use external rate limiter e.g. NVIDIA Control Panel\n-> Manage 3D Settings -> Set Max Framerate to what your monitor support.");
			ImGui.Checkbox("Using Unending Nightmare Delirium Keystone", ref Core.GHSettings.UsingUnendingNightmareDeliriumKeystone);
			ImGui.Checkbox("Disable debug counters (do it on 6 man party + juiced maps only)", ref Core.GHSettings.DisableAllCounters);
			ImGui.InputInt("Entities to read in series before going parallel", ref Core.GHSettings.EntitiesToReadBeforeGoingParallel);
		}
	}

	private static void DrawReloadPluginWidget()
	{
	}

	private static void DrawConfirmationPopup()
	{
		ImGui.SetNextWindowPos(new Vector2((float)Core.Overlay.Size.Width / 3f, (float)Core.Overlay.Size.Height / 3f));
		if (ImGui.BeginPopup("GameHelperCloseConfirmation"))
		{
			ImGui.Text("Do you want to quit the GameHelper overlay?");
			ImGui.Separator();
			if (ImGui.Button("Yes", new Vector2(ImGui.GetContentRegionAvail().X / 2f, ImGui.GetTextLineHeight() * 2f)))
			{
				Core.GHSettings.IsOverlayRunning = false;
				ImGui.CloseCurrentPopup();
				isOverlayRunningLocal = true;
			}
			ImGui.SameLine();
			if (ImGui.Button("No", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetTextLineHeight() * 2f)))
			{
				ImGui.CloseCurrentPopup();
				isOverlayRunningLocal = true;
			}
			ImGui.EndPopup();
		}
	}

	private static void HideOnStartCheck()
	{
		if (Core.GHSettings.HideSettingWindowOnStart)
		{
			isSettingsWindowVisible = false;
		}
	}

	private static IEnumerator<Wait> RenderCoroutine()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.OnRender);
			if (ClickableTransparentOverlay.Win32.Utils.IsKeyPressedAndNotTimeout(Core.GHSettings.MainMenuHotKey))
			{
				isSettingsWindowVisible = !isSettingsWindowVisible;
				ImGui.GetIO().WantCaptureMouse = true;
				if (!isSettingsWindowVisible)
				{
					CoroutineHandler.RaiseEvent(GameHelperEvents.TimeToSaveAllSettings);
				}
			}
			if (isSettingsWindowVisible)
			{
				ImGui.SetNextWindowSizeConstraints(new Vector2(800f, 600f), Vector2.One * float.MaxValue);
				bool num = ImGui.Begin("Game Overlay Settings [ " + Core.GetVersion() + " ]", ref isOverlayRunningLocal, ImGuiWindowFlags.MenuBar);
				if (!isOverlayRunningLocal)
				{
					ImGui.OpenPopup("GameHelperCloseConfirmation");
				}
				DrawConfirmationPopup();
				if (!Core.GHSettings.IsOverlayRunning)
				{
					CoroutineHandler.RaiseEvent(GameHelperEvents.TimeToSaveAllSettings);
				}
				if (!num)
				{
					ImGui.End();
					continue;
				}
				DrawManuBar();
				DrawTabs();
				ImGui.End();
			}
		}
	}

	private static IEnumerator<Wait> SaveCoroutine()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.TimeToSaveAllSettings);
			JsonHelper.SafeToFile(Core.GHSettings, State.CoreSettingFile);
		}
	}
}
