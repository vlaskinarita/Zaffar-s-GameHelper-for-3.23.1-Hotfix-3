using System.Runtime.InteropServices;

namespace GameOffsets.Objects.UiElement;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct SkillTreeNodeUiElementOffset
{
	[FieldOffset(0)]
	public UiElementBaseOffset UiElementBase;

	[FieldOffset(504)]
	public nint SkillInfo;
}
