using System.Runtime.InteropServices;

namespace GameOffsets.Objects.FilesStructures;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct BuffDefinitionsOffset
{
	[FieldOffset(0)]
	public nint Name;

	[FieldOffset(103)]
	public byte BuffType;
}
