using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.States;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct AreaLoadingStateOffset
{
	[FieldOffset(328)]
	public int IsLoading;

	[FieldOffset(1024)]
	public uint TotalLoadingScreenTimeMs;

	[FieldOffset(1088)]
	public StdWString CurrentAreaName;
}
