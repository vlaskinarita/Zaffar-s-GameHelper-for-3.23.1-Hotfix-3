using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct StatsStructInternal
{
	[FieldOffset(240)]
	public StdVector Stats;
}
