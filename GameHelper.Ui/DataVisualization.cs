using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Coroutine;
using GameHelper.CoroutineEvents;
using GameHelper.Utils;
using ImGuiNET;

namespace GameHelper.Ui;

public static class DataVisualization
{
	internal static void InitializeCoroutines()
	{
		CoroutineHandler.Start(DataVisualizationRenderCoRoutine());
	}

	private static IEnumerator<Wait> DataVisualizationRenderCoRoutine()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.OnRender);
			if (!Core.GHSettings.ShowDataVisualization)
			{
				continue;
			}
			if (ImGui.Begin("Data Visualization", ref Core.GHSettings.ShowDataVisualization))
			{
				if (ImGui.CollapsingHeader("Settings"))
				{
					List<FieldInfo> fields = Core.GHSettings.GetType().GetFields().ToList();
					for (int i = 0; i < fields.Count; i++)
					{
						FieldInfo field = fields[i];
						ImGui.Text($"{field.Name}: {field.GetValue(Core.GHSettings)}");
					}
					ImGui.Text($"Current Window Size:{Core.Overlay.Size}");
					ImGui.Text($"Current Window Pos: {Core.Overlay.Position}");
				}
				Core.CacheImGui();
				if (ImGui.CollapsingHeader("Game Process"))
				{
					if (Core.Process.Address != IntPtr.Zero)
					{
						ImGuiHelper.IntPtrToImGui("Base Address", Core.Process.Address);
						ImGui.Text($"Process: {Core.Process.Information}");
						ImGui.Text($"WindowArea: {Core.Process.WindowArea}");
						ImGui.Text($"Foreground: {Core.Process.Foreground}");
						if (ImGui.TreeNode("Static Addresses"))
						{
							foreach (KeyValuePair<string, nint> saddr in Core.Process.StaticAddresses)
							{
								ImGuiHelper.IntPtrToImGui(saddr.Key, saddr.Value);
							}
							ImGui.TreePop();
						}
					}
					else
					{
						ImGui.Text("Game not found.");
					}
				}
				Core.RemoteObjectsToImGuiCollapsingHeader();
			}
			ImGui.End();
		}
	}
}
