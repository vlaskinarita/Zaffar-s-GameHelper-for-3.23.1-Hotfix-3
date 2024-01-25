using System;
using System.Collections.Generic;
using System.Numerics;
using Coroutine;
using GameHelper.CoroutineEvents;
using GameHelper.RemoteEnums;
using GameHelper.RemoteObjects.Components;
using GameHelper.Utils;
using GameOffsets.Objects.States.InGameState;
using ImGuiNET;

namespace GameHelper.Ui;

public static class NearbyVisualization
{
	internal static void InitializeCoroutines()
	{
		CoroutineHandler.Start(NearbyVisualizationRenderCoRoutine());
	}

	private static IEnumerator<Wait> NearbyVisualizationRenderCoRoutine()
	{
		int totalLines = 40;
		uint bigColor = ImGuiHelper.Color(255u, 0u, 0u, 255u);
		uint smallColor = ImGuiHelper.Color(255u, 255u, 0u, 255u);
		while (true)
		{
			yield return new Wait(GameHelperEvents.OnRender);
			if (Core.States.GameCurrentState == GameStateTypes.InGameState && Core.States.InGameStateObject.CurrentAreaInstance.Player.TryGetComponent<Render>(out var r))
			{
				if (Core.GHSettings.OuterCircle.IsVisible)
				{
					DrawNearbyRange(totalLines, Core.GHSettings.OuterCircle.Meaning, r.GridPosition.X, r.GridPosition.Y, r.TerrainHeight, bigColor);
				}
				if (Core.GHSettings.InnerCircle.IsVisible)
				{
					DrawNearbyRange(totalLines, Core.GHSettings.InnerCircle.Meaning, r.GridPosition.X, r.GridPosition.Y, r.TerrainHeight, smallColor);
				}
			}
		}
	}

	private static void DrawNearbyRange(int totalLines, int nearbyMeaning, float gX, float gY, float height, uint color)
	{
		float gridToWorld = TileStructure.TileToWorldConversion / (float)TileStructure.TileToGridConversion;
		Span<Vector2> points = new Vector2[totalLines];
		float gap = 360f / (float)totalLines;
		for (int i = 0; i < totalLines; i++)
		{
			points[i].X = gX + (float)(Math.Cos(Math.PI / 180.0 * (double)i * (double)gap) * (double)nearbyMeaning);
			points[i].Y = gY + (float)(Math.Sin(Math.PI / 180.0 * (double)i * (double)gap) * (double)nearbyMeaning);
			try
			{
				height = Core.States.InGameStateObject.CurrentAreaInstance.GridHeightData[(int)points[i].Y][(int)points[i].X];
			}
			catch
			{
			}
			points[i] = Core.States.InGameStateObject.CurrentWorldInstance.WorldToScreen(points[i] * gridToWorld, height);
		}
		ImGui.GetBackgroundDrawList().AddPolyline(ref points[0], totalLines, color, ImDrawFlags.Closed, 5f);
	}
}
