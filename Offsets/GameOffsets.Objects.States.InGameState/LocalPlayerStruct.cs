using System.Runtime.InteropServices;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LocalPlayerStruct
{
	public nint ServerDataPtr;

	public nint LocalPlayerPtr;
}
