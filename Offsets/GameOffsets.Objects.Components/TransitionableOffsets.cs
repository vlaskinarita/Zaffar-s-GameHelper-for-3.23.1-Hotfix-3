using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct TransitionableOffsets
{
	[FieldOffset(0)]
	public ComponentHeader Header;

	[FieldOffset(288)]
	public short CurrentStateEnum;
}
