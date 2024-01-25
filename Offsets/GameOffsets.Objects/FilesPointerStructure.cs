using System.Runtime.InteropServices;

namespace GameOffsets.Objects;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FilesPointerStructure
{
	public nint Useless0;

	public nint FilesPointer;

	public nint Useless1;
}
