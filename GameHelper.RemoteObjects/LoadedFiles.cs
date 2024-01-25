using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Coroutine;
using GameHelper.CoroutineEvents;
using GameHelper.Utils;
using GameOffsets.Objects;
using ImGuiNET;

namespace GameHelper.RemoteObjects;

public class LoadedFiles : RemoteObjectBase
{
	private bool areaAlreadyDone;

	private string areaHashCache = string.Empty;

	private string filename = string.Empty;

	private string searchText = string.Empty;

	private string[] searchTextSplit = Array.Empty<string>();

	public ConcurrentDictionary<string, int> PathNames { get; } = new ConcurrentDictionary<string, int>();


	internal LoadedFiles(nint address)
		: base(address)
	{
		Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(OnAreaChange(), "[LoadedFiles] Gather Preload Data", 2147483646));
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text($"Total Loaded Files in current area: {PathNames.Count}");
		ImGui.TextWrapped("NOTE: The Overlay caches the preloads when you enter a new map. This cache is only cleared & updated when you enter a new Map. Going to town or hideout isn't considered a new Map. So basically you can find important preloads even after you have completed the whole map/gone to town/hideouts and entered the same Map again.");
		ImGui.Text("File Name: ");
		ImGui.SameLine();
		ImGui.InputText("##filename", ref filename, 100u);
		ImGui.SameLine();
		if (!areaAlreadyDone)
		{
			if (ImGui.Button("Save"))
			{
				Directory.CreateDirectory("preload_dumps");
				List<string> dataToWrite = PathNames.Keys.ToList();
				dataToWrite.Sort();
				File.WriteAllText(Path.Join("preload_dumps", filename), string.Join("\n", dataToWrite));
				areaAlreadyDone = true;
			}
		}
		else
		{
			ImGuiHelper.DrawDisabledButton("Save");
		}
		ImGui.Text("Search:    ");
		ImGui.SameLine();
		if (ImGui.InputText("##LoadedFiles", ref searchText, 50u))
		{
			searchTextSplit = searchText.ToLower().Split(",", StringSplitOptions.RemoveEmptyEntries);
		}
		ImGui.Text("NOTE: Search is Case-Insensitive. Use commas (,) to narrow down the resulting files.");
		if (string.IsNullOrEmpty(searchText) || !ImGui.BeginChild("Result##loadedfiles", Vector2.Zero, ImGuiChildFlags.Border))
		{
			return;
		}
		ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0f, 0f, 0f, 0f));
		foreach (KeyValuePair<string, int> kv in PathNames)
		{
			bool containsAll = true;
			for (int i = 0; i < searchTextSplit.Length; i++)
			{
				if (!kv.Key.ToLower().Contains(searchTextSplit[i]))
				{
					containsAll = false;
				}
			}
			if (containsAll && ImGui.SmallButton($"AreaId: {kv.Value} Path: {kv.Key}"))
			{
				ImGui.SetClipboardText(kv.Key);
			}
		}
		ImGui.PopStyleColor();
		ImGui.EndChild();
	}

	protected override void CleanUpData()
	{
		PathNames.Clear();
		areaHashCache = string.Empty;
		areaAlreadyDone = false;
		filename = string.Empty;
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
	}

	private LoadedFilesRootObject[] GetAllPointers()
	{
		int totalFiles = LoadedFilesRootObject.TotalCount;
		return Core.Process.Handle.ReadMemoryArray<LoadedFilesRootObject>(base.Address, totalFiles);
	}

	private void ScanForFilesParallel(SafeMemoryHandle reader, LoadedFilesRootObject filesRootObj)
	{
		Parallel.ForEach(reader.ReadStdBucket<FilesPointerStructure>(filesRootObj.LoadedFiles), delegate(FilesPointerStructure fileNode)
		{
			AddFileIfLoadedInCurrentArea(reader, fileNode.FilesPointer);
		});
	}

	private void AddFileIfLoadedInCurrentArea(SafeMemoryHandle reader, nint address)
	{
		FileInfoValueStruct information = reader.ReadMemory<FileInfoValueStruct>(address);
		if (information.AreaChangeCount > FileInfoValueStruct.IGNORE_FIRST_X_AREAS && information.AreaChangeCount == Core.AreaChangeCounter.Value)
		{
			string name = reader.ReadStdWString(information.Name).Split('@')[0];
			PathNames.AddOrUpdate(name, information.AreaChangeCount, (string key, int oldValue) => Math.Max(oldValue, information.AreaChangeCount));
		}
	}

	private IEnumerator<Wait> OnAreaChange()
	{
		while (true)
		{
			yield return new Wait(RemoteEvents.AreaChanged);
			if (base.Address == IntPtr.Zero)
			{
				continue;
			}
			string areaHash = Core.States.InGameStateObject.CurrentAreaInstance.AreaHash;
			bool isHideout = Core.States.InGameStateObject.CurrentWorldInstance.AreaDetails.IsHideout;
			bool iT = Core.States.InGameStateObject.CurrentWorldInstance.AreaDetails.IsTown;
			string name = Core.States.AreaLoading.CurrentAreaName;
			if (!((isHideout && Core.GHSettings.SkipPreloadedFilesInHideout) || iT) && !(areaHash == areaHashCache))
			{
				CleanUpData();
				filename = name + "_" + areaHash + ".txt";
				areaAlreadyDone = false;
				areaHashCache = areaHash;
				LoadedFilesRootObject[] filesRootObjs = GetAllPointers();
				SafeMemoryHandle reader = Core.Process.Handle;
				for (int i = 0; i < filesRootObjs.Length; i++)
				{
					ScanForFilesParallel(reader, filesRootObjs[i]);
					yield return new Wait(0.0);
				}
				CoroutineHandler.RaiseEvent(HybridEvents.PreloadsUpdated);
			}
		}
	}
}
