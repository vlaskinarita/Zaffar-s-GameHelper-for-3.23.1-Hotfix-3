using System.Runtime.InteropServices;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct InventoryArrayStruct
{
	[FieldOffset(0)]
	public int InventoryId;

	[FieldOffset(4)]
	public int PAD_0;

	[FieldOffset(8)]
	public nint InventoryPtr0;

	[FieldOffset(16)]
	public nint InventoryPtr1;

	[FieldOffset(24)]
	public long PAD_1;
}
