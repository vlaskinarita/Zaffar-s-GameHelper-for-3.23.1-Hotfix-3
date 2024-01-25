using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ChargesOffsets
{
	[FieldOffset(0)]
	public ComponentHeader Header;

	[FieldOffset(16)]
	public nint ChargesInternalPtr;

	[FieldOffset(24)]
	public int current;
}
