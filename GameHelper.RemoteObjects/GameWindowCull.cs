using System.Collections.Generic;
using Coroutine;
using GameHelper.CoroutineEvents;
using GameHelper.Utils;
using ImGuiNET;

namespace GameHelper.RemoteObjects;

public class GameWindowCull : RemoteObjectBase
{
	public int Value { get; private set; }

	internal GameWindowCull(nint address)
		: base(address)
	{
		CoroutineHandler.Start(OnGameMove(), "", int.MaxValue);
		CoroutineHandler.Start(OnGameForegroundChange(), "", int.MaxValue);
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text($"Game Window Cull Size: {Value}");
	}

	protected override void CleanUpData()
	{
		Value = 0;
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		Value = reader.ReadMemory<int>(base.Address);
	}

	private IEnumerator<Wait> OnGameMove()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.OnMoved);
			UpdateData(hasAddressChanged: false);
		}
	}

	private IEnumerator<Wait> OnGameForegroundChange()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.OnForegroundChanged);
			UpdateData(hasAddressChanged: false);
		}
	}
}
