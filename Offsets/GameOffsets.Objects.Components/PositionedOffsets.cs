using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct PositionedOffsets
{
	[FieldOffset(0)]
	public ComponentHeader Header;

	[FieldOffset(480)]
	public byte Reaction;
}
