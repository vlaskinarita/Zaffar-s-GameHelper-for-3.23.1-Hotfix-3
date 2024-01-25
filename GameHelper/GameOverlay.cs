using System.Collections.Generic;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using Coroutine;
using GameHelper.CoroutineEvents;
using GameHelper.Plugin;
using GameHelper.Settings;
using GameHelper.Ui;
using GameHelper.Utils;
using ImGuiNET;

namespace GameHelper;

public sealed class GameOverlay : Overlay
{
	public ImFontPtr[] Fonts { get; private set; }

	internal GameOverlay(string windowTitle)
		: base(windowTitle, DPIAware: true)
	{
		CoroutineHandler.Start(UpdateOverlayBounds(), "", int.MaxValue);
		SettingsWindow.InitializeCoroutines();
		PerformanceStats.InitializeCoroutines();
		DataVisualization.InitializeCoroutines();
		GameUiExplorer.InitializeCoroutines();
		OverlayKiller.InitializeCoroutines();
		NearbyVisualization.InitializeCoroutines();
		KrangledPassiveDetector.InitializeCoroutines();
	}

	public override async Task Run()
	{
		Core.Initialize();
		Core.InitializeCororutines();
		VSync = Core.GHSettings.Vsync;
		await base.Run();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Core.Dispose();
		}
		base.Dispose(disposing);
	}

	protected override Task PostInitialized()
	{
		if (MiscHelper.TryConvertStringToImGuiGlyphRanges(Core.GHSettings.FontCustomGlyphRange, out var glyphRanges))
		{
			Core.Overlay.ReplaceFont(Core.GHSettings.FontPathName, Core.GHSettings.FontSize, glyphRanges);
		}
		else
		{
			Core.Overlay.ReplaceFont(Core.GHSettings.FontPathName, Core.GHSettings.FontSize, Core.GHSettings.FontLanguage);
		}
		PManager.InitializePlugins();
		return Task.CompletedTask;
	}

	protected override void Render()
	{
		CoroutineHandler.Tick(ImGui.GetIO().DeltaTime);
		CoroutineHandler.RaiseEvent(GameHelperEvents.PerFrameDataUpdate);
		CoroutineHandler.RaiseEvent(GameHelperEvents.PostPerFrameDataUpdate);
		CoroutineHandler.RaiseEvent(GameHelperEvents.OnRender);
		if (!Core.GHSettings.IsOverlayRunning)
		{
			Close();
		}
	}

	private IEnumerator<Wait> UpdateOverlayBounds()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.OnMoved);
			base.Position = Core.Process.WindowArea.Location;
			base.Size = Core.Process.WindowArea.Size;
		}
	}
}
