using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ClickableTransparentOverlay.Win32;
using Coroutine;
using GameHelper.CoroutineEvents;
using GameHelper.Settings;
using GameHelper.Utils;

namespace GameHelper.Plugin;

internal static class PManager
{
	private static bool disableRendering = false;

	internal static readonly List<PluginContainer> Plugins = new List<PluginContainer>();

	internal static void InitializePlugins()
	{
		State.PluginsDirectory.Create();
		LoadPluginMetadata(LoadPlugins());
		Parallel.ForEach(Plugins, EnablePluginIfRequired);
		CoroutineHandler.Start(SavePluginSettingsCoroutine());
		CoroutineHandler.Start(SavePluginMetadataCoroutine());
		Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(DrawPluginUiRenderCoroutine(), "[PManager] Draw Plugins UI"));
	}

	private static List<PluginWithName> LoadPlugins()
	{
		//return (from x in GetPluginsDirectories().AsParallel().Select(LoadPlugin)
		//	where x != null
		//	orderby x.Name
		//	select x).ToList();
        return (from x in GetPluginsDirectories().Select(LoadPlugin)
                where x != null
                orderby x.Name
                select x).ToList();
    }

	private static List<DirectoryInfo> GetPluginsDirectories()
	{
		return (from x in State.PluginsDirectory.GetDirectories()
			where (x.Attributes & FileAttributes.Hidden) == 0
			select x).ToList();
	}

	private static Assembly ReadPluginFiles(DirectoryInfo pluginDirectory)
	{
		try
		{
			FileInfo dllFile = pluginDirectory.GetFiles(pluginDirectory.Name + "*.dll", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (dllFile == null)
			{
				Console.WriteLine($"Couldn't find plugin dll with name {pluginDirectory.Name} in directory {pluginDirectory.FullName}." + " Please make sure DLL & the plugin got same name.");
				return null;
			}
			return new PluginAssemblyLoadContext(dllFile.FullName).LoadFromAssemblyPath(dllFile.FullName);
		}
		catch (Exception e)
		{
			Console.WriteLine($"Failed to load plugin {pluginDirectory.FullName} due to {e}");
			return null;
		}
	}

	private static PluginWithName LoadPlugin(DirectoryInfo pluginDirectory)
	{
		Assembly assembly = ReadPluginFiles(pluginDirectory);
		if (assembly != null)
		{
			string relativePluginDir = pluginDirectory.FullName.Replace(State.PluginsDirectory.FullName, State.PluginsDirectory.Name);
			return LoadPlugin(assembly, relativePluginDir);
		}
		return null;
	}

	private static PluginWithName LoadPlugin(Assembly assembly, string pluginRootDirectory)
	{
		try
		{
			Type[] types = assembly.GetTypes();
			if (types.Length == 0)
			{
				Console.WriteLine($"Plugin (in {pluginRootDirectory}) {assembly} doesn't " + "contain any types (i.e. classes/stuctures).");
				return null;
			}
			List<Type> iPluginClasses = types.Where((Type type) => typeof(IPCore).IsAssignableFrom(type) && type.IsSealed).ToList();
			if (iPluginClasses.Count != 1)
			{
				Console.WriteLine($"Plugin (in {pluginRootDirectory}) {assembly} contains {iPluginClasses.Count} sealed classes derived from CoreBase<TSettings>." + " It should have one sealed class derived from IPlugin.");
				return null;
			}
			IPCore pluginCore = Activator.CreateInstance(iPluginClasses[0]) as IPCore;
			pluginCore.SetPluginDllLocation(pluginRootDirectory);
			return new PluginWithName(assembly.GetName().Name, pluginCore);
		}
		catch (Exception e)
		{
			Console.WriteLine($"Error loading plugin {assembly.FullName} due to {e}");
			return null;
		}
	}

	private static void LoadPluginMetadata(IEnumerable<PluginWithName> plugins)
	{
		Dictionary<string, PluginMetadata> metadata = JsonHelper.CreateOrLoadJsonFile<Dictionary<string, PluginMetadata>>(State.PluginsMetadataFile);
		Plugins.AddRange(plugins.Select((PluginWithName x) => new PluginContainer(x.Name, x.Plugin, metadata.GetValueOrDefault(x.Name, new PluginMetadata()))));
		SavePluginMetadata();
	}

	private static void EnablePluginIfRequired(PluginContainer container)
	{
		if (container.Metadata.Enable)
		{
			container.Plugin.OnEnable(Core.Process.Address != IntPtr.Zero);
		}
	}

	private static void SavePluginMetadata()
	{
		JsonHelper.SafeToFile(Plugins.ToDictionary((PluginContainer x) => x.Name, (PluginContainer x) => x.Metadata), State.PluginsMetadataFile);
	}

	private static IEnumerator<Wait> SavePluginMetadataCoroutine()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.TimeToSaveAllSettings);
			SavePluginMetadata();
		}
	}

	private static IEnumerator<Wait> SavePluginSettingsCoroutine()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.TimeToSaveAllSettings);
			foreach (PluginContainer plugin in Plugins)
			{
				plugin.Plugin.SaveSettings();
			}
		}
	}

	private static IEnumerator<Wait> DrawPluginUiRenderCoroutine()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.OnRender);
			if (ClickableTransparentOverlay.Win32.Utils.IsKeyPressedAndNotTimeout(Core.GHSettings.DisableAllRenderingKey))
			{
				disableRendering = !disableRendering;
			}
			if (disableRendering)
			{
				continue;
			}
			foreach (PluginContainer container in Plugins)
			{
				if (container.Metadata.Enable)
				{
					container.Plugin.DrawUI();
				}
			}
		}
	}
}
