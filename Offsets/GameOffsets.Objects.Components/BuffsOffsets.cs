using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct BuffsOffsets
{
	[FieldOffset(0)]
	public ComponentHeader Header;

	[FieldOffset(352)]
	public StdVector StatusEffectPtr;
}
