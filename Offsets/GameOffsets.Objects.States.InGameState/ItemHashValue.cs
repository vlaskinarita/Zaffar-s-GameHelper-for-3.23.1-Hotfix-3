using System.Runtime.InteropServices;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ItemHashValue
{
	public nint InventoryItemStructPtr;

	public nint UselessItemPtr;
}
