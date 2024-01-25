using System.Runtime.InteropServices;

namespace GameOffsets.Objects.UiElement;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct SkillInfoStruct
{
	[FieldOffset(8)]
	public nint PassiveSkillsDatRow;
}
