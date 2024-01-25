using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ChestOffsets
{
	[FieldOffset(0)]
	public ComponentHeader Header;

	[FieldOffset(352)]
	public nint ChestsDataPtr;

	[FieldOffset(360)]
	public bool IsOpened;
}
