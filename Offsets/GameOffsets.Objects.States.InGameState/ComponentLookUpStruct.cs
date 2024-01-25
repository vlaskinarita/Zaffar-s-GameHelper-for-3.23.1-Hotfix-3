using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ComponentLookUpStruct
{
	[FieldOffset(0)]
	public nint Unknown0;

	[FieldOffset(8)]
	public nint Unknown1;

	[FieldOffset(16)]
	public StdVector Unknown2;

	[FieldOffset(40)]
	public StdBucket ComponentsNameAndIndex;
}
