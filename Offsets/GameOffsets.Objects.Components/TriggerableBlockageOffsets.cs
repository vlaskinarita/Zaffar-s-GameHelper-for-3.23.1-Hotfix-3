using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct TriggerableBlockageOffsets
{
	[FieldOffset(0)]
	public ComponentHeader Header;

	[FieldOffset(48)]
	public bool IsBlocked;
}
