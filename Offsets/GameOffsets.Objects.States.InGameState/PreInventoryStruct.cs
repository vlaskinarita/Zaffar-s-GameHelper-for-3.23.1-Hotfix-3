using System.Runtime.InteropServices;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct PreInventoryStruct
{
	[FieldOffset(16)]
	public nint ptr;
}
