using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LoadedFilesRootObject
{
	public StdBucket LoadedFiles;

	public static int TotalCount = 16;
}
