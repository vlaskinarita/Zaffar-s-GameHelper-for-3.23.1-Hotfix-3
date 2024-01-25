using System;
using System.Collections.Generic;
using Coroutine;
using GameHelper.CoroutineEvents;
using GameHelper.RemoteEnums;
using GameHelper.RemoteObjects.States;
using GameHelper.Utils;
using GameOffsets.Objects;
using ImGuiNET;

namespace GameHelper.RemoteObjects;

public class GameStates : RemoteObjectBase
{
	private nint currentStateAddress = IntPtr.Zero;

	private GameStateTypes currentStateName = GameStateTypes.GameNotLoaded;

	private GameStateStaticOffset myStaticObj;

	public Dictionary<nint, GameStateTypes> AllStates { get; } = new Dictionary<nint, GameStateTypes>();


	public AreaLoadingState AreaLoading { get; } = new AreaLoadingState(IntPtr.Zero);


	public InGameState InGameStateObject { get; } = new InGameState(IntPtr.Zero);


	public GameStateTypes GameCurrentState
	{
		get
		{
			return currentStateName;
		}
		private set
		{
			if (currentStateName != value)
			{
				currentStateName = value;
				if (value != GameStateTypes.GameNotLoaded)
				{
					CoroutineHandler.RaiseEvent(RemoteEvents.StateChanged);
				}
			}
		}
	}

	internal GameStates(nint address)
		: base(address)
	{
		CoroutineHandler.Start(OnPerFrame(), "", int.MaxValue);
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		if (ImGui.TreeNode("All States Info"))
		{
			foreach (KeyValuePair<nint, GameStateTypes> state in AllStates)
			{
				ImGuiHelper.IntPtrToImGui($"{state.Value}", state.Key);
			}
			ImGui.TreePop();
		}
		ImGui.Text($"Current State: {GameCurrentState}");
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		if (hasAddressChanged)
		{
			myStaticObj = reader.ReadMemory<GameStateStaticOffset>(base.Address);
			GameStateOffset data = reader.ReadMemory<GameStateOffset>(myStaticObj.GameState);
			AllStates[data.State0] = GameStateTypes.AreaLoadingState;
			AllStates[data.State1] = GameStateTypes.ChangePasswordState;
			AllStates[data.State2] = GameStateTypes.CreditsState;
			AllStates[data.State3] = GameStateTypes.EscapeState;
			AllStates[data.State4] = GameStateTypes.InGameState;
			AllStates[data.State5] = GameStateTypes.PreGameState;
			AllStates[data.State6] = GameStateTypes.LoginState;
			AllStates[data.State7] = GameStateTypes.WaitingState;
			AllStates[data.State8] = GameStateTypes.CreateCharacterState;
			AllStates[data.State9] = GameStateTypes.SelectCharacterState;
			AllStates[data.State10] = GameStateTypes.DeleteCharacterState;
			AllStates[data.State11] = GameStateTypes.LoadingState;
			AreaLoading.Address = data.State0;
			InGameStateObject.Address = data.State4;
		}
		else
		{
			nint cStateAddr = reader.ReadMemory<nint>(reader.ReadMemory<GameStateOffset>(myStaticObj.GameState).CurrentStatePtr.Last - 16);
			if (cStateAddr != IntPtr.Zero && cStateAddr != currentStateAddress)
			{
				currentStateAddress = cStateAddr;
				GameCurrentState = AllStates[currentStateAddress];
			}
		}
	}

	protected override void CleanUpData()
	{
		myStaticObj = default(GameStateStaticOffset);
		currentStateAddress = IntPtr.Zero;
		GameCurrentState = GameStateTypes.GameNotLoaded;
		AllStates.Clear();
		AreaLoading.Address = IntPtr.Zero;
		InGameStateObject.Address = IntPtr.Zero;
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
