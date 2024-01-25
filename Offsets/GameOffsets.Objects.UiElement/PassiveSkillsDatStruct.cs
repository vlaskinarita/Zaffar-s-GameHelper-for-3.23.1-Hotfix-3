using System.Runtime.InteropServices;

namespace GameOffsets.Objects.UiElement;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct PassiveSkillsDatStruct
{
	[FieldOffset(48)]
	public int PassiveSkillGraphId;
}
