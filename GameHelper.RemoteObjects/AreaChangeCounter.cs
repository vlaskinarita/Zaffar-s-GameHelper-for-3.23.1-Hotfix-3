using System;
using System.Collections.Generic;
using Coroutine;
using GameHelper.CoroutineEvents;
using GameHelper.Utils;
using GameOffsets.Objects;
using ImGuiNET;

namespace GameHelper.RemoteObjects;

public class AreaChangeCounter : RemoteObjectBase
{
	public int Value { get; private set; } = int.MaxValue;


	internal AreaChangeCounter(nint address)
		: base(address)
	{
		CoroutineHandler.Start(OnAreaChange(), "", int.MaxValue);
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text($"Area Change Counter: {Value}");
	}

	protected override void CleanUpData()
	{
		Value = int.MaxValue;
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		Value = reader.ReadMemory<AreaChangeOffset>(base.Address).counter;
	}

	private IEnumerator<Wait> OnAreaChange()
	{
		while (true)
		{
			yield return new Wait(RemoteEvents.AreaChanged);
			if (base.Address != IntPtr.Zero)
			{
				UpdateData(hasAddressChanged: false);
			}
		}
	}
}
