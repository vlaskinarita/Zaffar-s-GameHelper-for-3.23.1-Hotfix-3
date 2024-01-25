using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct InventoryItemStruct
{
	[FieldOffset(0)]
	public nint Item;

	[FieldOffset(8)]
	public StdTuple2D<int> SlotStart;

	[FieldOffset(16)]
	public StdTuple2D<int> SlotEnd;

	[FieldOffset(24)]
	public int ServerRequestCounterSnapShot;
}
