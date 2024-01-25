using System.Runtime.InteropServices;

namespace GameOffsets.Objects.FilesStructures;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct WorldAreaDatOffsets
{
	[FieldOffset(0)]
	public nint IdPtr;

	[FieldOffset(8)]
	public nint NamePtr;

	[FieldOffset(16)]
	public int Act;

	[FieldOffset(20)]
	public bool IsTown;

	[FieldOffset(21)]
	public bool HasWaypoint;
}
