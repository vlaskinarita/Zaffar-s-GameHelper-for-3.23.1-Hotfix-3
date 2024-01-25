using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ChargesInternalStruct
{
	[FieldOffset(24)]
	public int PerUseCharges;
}
