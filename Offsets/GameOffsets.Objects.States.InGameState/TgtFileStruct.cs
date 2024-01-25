using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TgtFileStruct
{
	public nint Vtable;

	public StdWString TgtPath;
}
