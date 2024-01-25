using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct PlayerOffsets
{
	[FieldOffset(0)]
	public ComponentHeader Header;

	[FieldOffset(360)]
	public StdWString Name;
}
