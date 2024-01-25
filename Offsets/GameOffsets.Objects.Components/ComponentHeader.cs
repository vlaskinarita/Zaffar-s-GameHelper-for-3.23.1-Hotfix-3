using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ComponentHeader
{
	[FieldOffset(0)]
	public nint StaticPtr;

	[FieldOffset(8)]
	public nint EntityPtr;
}
