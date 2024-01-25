using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct StatsOffsets
{
	[FieldOffset(0)]
	public ComponentHeader Header;

	[FieldOffset(32)]
	public nint StatsDataPtr;
}
