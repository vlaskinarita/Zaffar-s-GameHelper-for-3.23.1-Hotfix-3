using System.Runtime.InteropServices;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ComponentNameAndIndexStruct
{
	public nint NamePtr;

	public int Index;

	public int PAD_0xC;
}
