using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ShrineOffsets
{
	[FieldOffset(0)]
	public ComponentHeader Header;

	[FieldOffset(36)]
	public bool IsUsed;
}
