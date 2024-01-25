using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Coroutine;
using GameHelper.Cache;
using GameHelper.CoroutineEvents;
using GameHelper.RemoteObjects;
using GameHelper.Settings;
using GameHelper.Utils;
using ImGuiNET;

namespace GameHelper;

public static class Core
{
	private static string version;

	public static GameOverlay Overlay { get; internal set; } = null;


	public static List<ActiveCoroutine> CoroutinesRegistrar { get; } = new List<ActiveCoroutine>();


	public static GameStates States { get; } = new GameStates(IntPtr.Zero);


	public static LoadedFiles CurrentAreaLoadedFiles { get; } = new LoadedFiles(IntPtr.Zero);


	public static GameProcess Process { get; } = new GameProcess();


	internal static GgpkAddresses<string> GgpkStringCache { get; } = new GgpkAddresses<string>();


	internal static GgpkAddresses<object> GgpkObjectCache { get; } = new GgpkAddresses<object>();


	internal static AreaChangeCounter AreaChangeCounter { get; } = new AreaChangeCounter(IntPtr.Zero);


	internal static GameWindowScale GameScale { get; } = new GameWindowScale();


	internal static GameWindowCull GameCull { get; } = new GameWindowCull(IntPtr.Zero);


	internal static TerrainHeightHelper RotationSelector { get; } = new TerrainHeightHelper(IntPtr.Zero, 9);


	internal static TerrainHeightHelper RotatorHelper { get; } = new TerrainHeightHelper(IntPtr.Zero, 25);


	internal static State GHSettings { get; } = JsonHelper.CreateOrLoadJsonFile<State>(State.CoreSettingFile);


	public static void Initialize()
	{
		try
		{
			version = File.ReadAllText("VERSION.txt");
		}
		catch (Exception)
		{
			version = "Dev";
		}
	}

	public static string GetVersion()
	{
		return version.Trim();
	}

	internal static void InitializeCororutines()
	{
		CoroutineHandler.Start(GameClosedActions());
		CoroutineHandler.Start(UpdateStatesData(), "", 2147483644);
		CoroutineHandler.Start(UpdateFilesData(), "", 2147483645);
		CoroutineHandler.Start(UpdateAreaChangeData(), "", 2147483646);
		CoroutineHandler.Start(UpdateCullData(), "", int.MaxValue);
		CoroutineHandler.Start(UpdateRotationSelectorData(), "", int.MaxValue);
		CoroutineHandler.Start(UpdateRotatorHelperData(), "", int.MaxValue);
	}

	internal static void Dispose()
	{
		Process.Close(monitorForNewGame: false);
	}

	internal static void RemoteObjectsToImGuiCollapsingHeader()
	{
		foreach (RemoteObjectPropertyDetail property in RemoteObjectBase.GetToImGuiMethods(typeof(Core), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null))
		{
			if (ImGui.CollapsingHeader(property.Name))
			{
				property.ToImGui.Invoke(property.Value, null);
			}
		}
	}

	internal static void CacheImGui()
	{
		if (ImGui.CollapsingHeader("GGPK String Data Cache"))
		{
			GgpkStringCache.ToImGui();
		}
		if (ImGui.CollapsingHeader("GGPK Object Cache"))
		{
			GgpkObjectCache.ToImGui();
		}
	}

	private static IEnumerator<Wait> UpdateCullData()
	{
		while (true)
		{
			yield return new Wait(Process.OnStaticAddressFound);
			GameCull.Address = Process.StaticAddresses["GameCullSize"];
		}
	}

	private static IEnumerator<Wait> UpdateAreaChangeData()
	{
		while (true)
		{
			yield return new Wait(Process.OnStaticAddressFound);
			AreaChangeCounter.Address = Process.StaticAddresses["AreaChangeCounter"];
		}
	}

	private static IEnumerator<Wait> UpdateFilesData()
	{
		while (true)
		{
			yield return new Wait(Process.OnStaticAddressFound);
			CurrentAreaLoadedFiles.Address = Process.StaticAddresses["File Root"];
		}
	}

	private static IEnumerator<Wait> UpdateStatesData()
	{
		while (true)
		{
			yield return new Wait(Process.OnStaticAddressFound);
			States.Address = Process.StaticAddresses["Game States"];
		}
	}

	private static IEnumerator<Wait> UpdateRotationSelectorData()
	{
		while (true)
		{
			yield return new Wait(Process.OnStaticAddressFound);
			RotationSelector.Address = Process.StaticAddresses["Terrain Rotation Selector"];
		}
	}

	private static IEnumerator<Wait> UpdateRotatorHelperData()
	{
		while (true)
		{
			yield return new Wait(Process.OnStaticAddressFound);
			RotatorHelper.Address = Process.StaticAddresses["Terrain Rotator Helper"];
		}
	}

	private static IEnumerator<Wait> GameClosedActions()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.OnClose);
			States.Address = IntPtr.Zero;
			CurrentAreaLoadedFiles.Address = IntPtr.Zero;
			AreaChangeCounter.Address = IntPtr.Zero;
			GameCull.Address = IntPtr.Zero;
			RotationSelector.Address = IntPtr.Zero;
			RotatorHelper.Address = IntPtr.Zero;
			if (GHSettings.CloseWhenGameExit)
			{
				Overlay?.Close();
			}
		}
	}
}
