using System.Runtime.InteropServices;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct MapParentStruct
{
	[FieldOffset(72)]
	public nint LargeMapPtr;

	[FieldOffset(80)]
	public nint MiniMapPtr;
}
