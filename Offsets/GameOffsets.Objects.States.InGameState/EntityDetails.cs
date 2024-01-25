using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct EntityDetails
{
	[FieldOffset(8)]
	public StdWString name;

	[FieldOffset(48)]
	public nint ComponentLookUpPtr;
}
