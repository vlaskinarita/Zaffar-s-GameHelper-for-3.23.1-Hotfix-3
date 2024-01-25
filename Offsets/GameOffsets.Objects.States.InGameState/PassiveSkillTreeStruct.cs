using System.Runtime.InteropServices;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct PassiveSkillTreeStruct
{
	[FieldOffset(80)]
	public nint SkillTreeNodeUiElements;
}
