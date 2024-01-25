using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct TargetableOffsets
{
	[FieldOffset(0)]
	public ComponentHeader Header;

	[FieldOffset(72)]
	public uint TargetableFlag;

	[FieldOffset(80)]
	public uint HiddenFlag;
}
