using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Coroutine;
using GameHelper.CoroutineEvents;
using GameHelper.RemoteObjects.States.InGameStateObjects;
using ImGuiNET;

namespace GameHelper.Ui;

public static class PerformanceStats
{
	private class MovingAverage
	{
		private readonly Queue<double> samples = new Queue<double>();

		private readonly int windowSize = 1440;

		private int lastIterationNumber;

		private double sampleAccumulator;

		public double Average { get; private set; }

		public void ComputeAverage(double newSample, int iterationNumber)
		{
			if (iterationNumber > lastIterationNumber)
			{
				lastIterationNumber = iterationNumber;
				sampleAccumulator += newSample;
				samples.Enqueue(newSample);
				if (samples.Count > windowSize)
				{
					sampleAccumulator -= samples.Dequeue();
				}
				Average = sampleAccumulator / (double)samples.Count;
			}
		}
	}

	private static readonly Dictionary<string, MovingAverage> MovingAverageValue = new Dictionary<string, MovingAverage>();

	private static bool isPerformanceWindowHovered;

	internal static void InitializeCoroutines()
	{
		CoroutineHandler.Start(PerformanceStatRenderCoRoutine());
	}

	private static IEnumerator<Wait> PerformanceStatRenderCoRoutine()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.OnRender);
			if (!Core.GHSettings.ShowPerfStats || (Core.GHSettings.HidePerfStatsWhenBg && !Core.Process.Foreground))
			{
				continue;
			}
			ImGui.SetNextWindowPos(Vector2.Zero);
			if (isPerformanceWindowHovered)
			{
				ImGui.PushStyleColor(ImGuiCol.WindowBg, Vector4.Zero);
				ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
			}
			ImGui.Begin("Perf Stats Window", ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBringToFrontOnFocus);
			if (isPerformanceWindowHovered)
			{
				ImGui.PopStyleVar();
				ImGui.PopStyleColor();
			}
			isPerformanceWindowHovered = ImGui.IsMouseHoveringRect(Vector2.Zero, ImGui.GetWindowSize());
			if (isPerformanceWindowHovered)
			{
				ImGui.PushStyleColor(ImGuiCol.Text, Vector4.Zero);
			}
			if (!Core.GHSettings.MinimumPerfStats)
			{
				ImGui.Text("Performance Related Stats");
				using (Process proc = Process.GetCurrentProcess())
				{
					ImGui.Text($"Total Used Memory: {proc.PrivateMemorySize64 / 1048576} (MB)");
				}
				ImGui.Text($"Total Event Coroutines: {CoroutineHandler.EventCount}");
				ImGui.Text($"Total Tick Coroutines: {CoroutineHandler.TickingCount}");
			}
			AreaInstance cAI = Core.States.InGameStateObject.CurrentAreaInstance;
			ImGui.Text($"Total Entities: {cAI.AwakeEntities.Count}");
			if (!Core.GHSettings.DisableAllCounters)
			{
				ImGui.Text($"Awake Entities: {cAI.NetworkBubbleEntityCount}");
				ImGui.Text($"Useless Awake:  {cAI.UselessAwakeEntities}");
			}
			float fps = ImGui.GetIO().Framerate;
			ImGui.Text($"FPS: {fps}");
			if (!Core.GHSettings.MinimumPerfStats)
			{
				ImGui.NewLine();
				ImGui.Text($"==Average of last {(int)(1440f / fps)} seconds==");
				for (int i = 0; i < Core.CoroutinesRegistrar.Count; i++)
				{
					ActiveCoroutine coroutine = Core.CoroutinesRegistrar[i];
					if (coroutine.IsFinished)
					{
						Core.CoroutinesRegistrar.Remove(coroutine);
					}
					if (MovingAverageValue.TryGetValue(coroutine.Name, out var value))
					{
						value.ComputeAverage(coroutine.LastMoveNextTime.TotalMilliseconds, coroutine.MoveNextCount);
						ImGui.Text($"{coroutine.Name}: {value.Average:0.00}(ms)");
					}
					else
					{
						MovingAverageValue[coroutine.Name] = new MovingAverage();
					}
				}
			}
			if (isPerformanceWindowHovered)
			{
				ImGui.PopStyleColor();
			}
			ImGui.End();
		}
	}
}
