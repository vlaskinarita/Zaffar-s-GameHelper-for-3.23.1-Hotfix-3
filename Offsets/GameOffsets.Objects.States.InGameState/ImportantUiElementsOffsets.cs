using System.Runtime.InteropServices;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ImportantUiElementsOffsets
{
	[FieldOffset(1144)]
	public nint ChatParentPtr;

	[FieldOffset(1416)]
	public nint PassiveSkillTreePanel;

	[FieldOffset(1560)]
	public nint MapParentPtr;

	[FieldOffset(1936)]
	public nint ControllerModeMapParentPtr;
}
