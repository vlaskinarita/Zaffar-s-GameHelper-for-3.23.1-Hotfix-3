using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ChestsStructInternal
{
	[FieldOffset(33)]
	public bool IsLabelVisible;

	[FieldOffset(80)]
	public nint StrongboxDatPtr;
}
