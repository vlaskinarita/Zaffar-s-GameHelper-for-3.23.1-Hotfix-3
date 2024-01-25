using System.Runtime.InteropServices;

namespace GameOffsets.Objects.FilesStructures;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct GrantedEffectsDatOffset
{
	[FieldOffset(99)]
	public nint ActiveSkillDatPtr;
}
