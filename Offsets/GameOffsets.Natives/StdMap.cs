using System.Runtime.InteropServices;

namespace GameOffsets.Natives;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StdMap
{
	public nint Head;

	public int Size;

	public int PAD_C;
}
