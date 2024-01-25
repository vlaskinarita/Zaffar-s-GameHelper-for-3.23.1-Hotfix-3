using System.Runtime.InteropServices;

namespace GameOffsets.Objects.FilesStructures;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct GrantedEffectsPerLevelDatOffset
{
	[FieldOffset(0)]
	public nint GrantedEffectDatPtr;
}
