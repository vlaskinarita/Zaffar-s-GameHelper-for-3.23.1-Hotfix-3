using System;
using System.Collections.Generic;
using Coroutine;
using GameHelper.CoroutineEvents;
using GameHelper.RemoteObjects.States.InGameStateObjects;
using GameHelper.Utils;
using GameOffsets.Objects.States;

namespace GameHelper.RemoteObjects.States;

public class InGameState : RemoteObjectBase
{
	private nint uiRootAddress;

	public WorldData CurrentWorldInstance { get; } = new WorldData(IntPtr.Zero);


	public AreaInstance CurrentAreaInstance { get; } = new AreaInstance(IntPtr.Zero);


	public ImportantUiElements GameUi { get; } = new ImportantUiElements(IntPtr.Zero);


	internal InGameState(nint address)
		: base(address)
	{
		Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(OnPerFrame(), "[InGameState] Update Game State", 2147483645));
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGuiHelper.IntPtrToImGui("UiRoot", uiRootAddress);
	}

	protected override void CleanUpData()
	{
		CurrentAreaInstance.Address = IntPtr.Zero;
		uiRootAddress = IntPtr.Zero;
		GameUi.Address = IntPtr.Zero;
		CurrentWorldInstance.Address = IntPtr.Zero;
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		InGameStateOffset data = Core.Process.Handle.ReadMemory<InGameStateOffset>(base.Address);
		uiRootAddress = data.UiRootPtr;
		GameUi.Address = data.IngameUi;
		CurrentAreaInstance.Address = data.AreaInstanceData;
		CurrentWorldInstance.Address = data.WorldData;
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
