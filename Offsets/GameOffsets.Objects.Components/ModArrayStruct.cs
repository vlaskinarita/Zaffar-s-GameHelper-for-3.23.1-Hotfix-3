using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ModArrayStruct
{
	[FieldOffset(0)]
	public StdVector Values;

	[FieldOffset(24)]
	public int Value0;

	[FieldOffset(40)]
	public nint ModsPtr;

	[FieldOffset(48)]
	public nint Unknown0;
}
