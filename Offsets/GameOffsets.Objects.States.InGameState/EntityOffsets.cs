using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct EntityOffsets
{
	[FieldOffset(0)]
	public ItemStruct ItemBase;

	[FieldOffset(48)]
	public StdVector UnknownListPtr;

	[FieldOffset(144)]
	public uint Id;

	[FieldOffset(148)]
	public byte IsValid;
}
