using System;
using System.Collections.Generic;
using Coroutine;
using GameHelper.CoroutineEvents;
using GameHelper.Utils;
using GameOffsets.Objects.States;
using ImGuiNET;

namespace GameHelper.RemoteObjects.States;

public sealed class AreaLoadingState : RemoteObjectBase
{
	private AreaLoadingStateOffset lastCache;

	public string CurrentAreaName { get; private set; } = string.Empty;


	internal bool IsLoading { get; private set; }

	internal AreaLoadingState(nint address)
		: base(address)
	{
		CoroutineHandler.Start(OnPerFrame(), "", 2147483646);
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text("Current Area Name: " + CurrentAreaName);
		ImGui.Text($"Is Loading Screen: {IsLoading}");
		ImGui.Text($"Total Loading Time(ms): {lastCache.TotalLoadingScreenTimeMs}");
	}

	protected override void CleanUpData()
	{
		lastCache = default(AreaLoadingStateOffset);
		CurrentAreaName = string.Empty;
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		AreaLoadingStateOffset data = reader.ReadMemory<AreaLoadingStateOffset>(base.Address);
		IsLoading = data.IsLoading == 1;
		bool hasAreaChanged = false;
		if (data.CurrentAreaName.Buffer != IntPtr.Zero && !IsLoading && data.TotalLoadingScreenTimeMs > lastCache.TotalLoadingScreenTimeMs)
		{
			string areaName = reader.ReadStdWString(data.CurrentAreaName);
			CurrentAreaName = areaName;
			lastCache = data;
			hasAreaChanged = true;
		}
		if (hasAreaChanged)
		{
			CoroutineHandler.InvokeLater(new Wait(0.1), delegate
			{
				CoroutineHandler.RaiseEvent(RemoteEvents.AreaChanged);
			});
		}
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
