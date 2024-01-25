using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ModsOffsets
{
	[FieldOffset(0)]
	public ComponentHeader Header;

	[FieldOffset(0)]
	public ModsAndObjectMagicProperties Details0;
}
