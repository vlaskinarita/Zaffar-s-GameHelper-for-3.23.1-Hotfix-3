using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct InventoryStruct
{
	[FieldOffset(332)]
	public StdTuple2D<int> TotalBoxes;

	[FieldOffset(368)]
	public StdVector ItemList;

	[FieldOffset(488)]
	public int ServerRequestCounter;
}
