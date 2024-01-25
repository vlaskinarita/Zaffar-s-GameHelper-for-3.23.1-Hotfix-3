using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct FileInfoValueStruct
{
	[FieldOffset(8)]
	public StdWString Name;

	[FieldOffset(64)]
	public int AreaChangeCount;

	public static readonly int IGNORE_FIRST_X_AREAS = 2;
}
