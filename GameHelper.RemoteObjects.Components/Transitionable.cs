using GameOffsets.Objects.Components;
using ImGuiNET;

namespace GameHelper.RemoteObjects.Components;

public class Transitionable : ComponentBase
{
	public int CurrentState { get; private set; }

	public Transitionable(nint address)
		: base(address)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text($"Current State: {CurrentState}");
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		TransitionableOffsets data = Core.Process.Handle.ReadMemory<TransitionableOffsets>(base.Address);
		OwnerEntityAddress = data.Header.EntityPtr;
		CurrentState = data.CurrentStateEnum;
	}
}
