using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ObjectMagicPropertiesOffsets
{
	[FieldOffset(0)]
	public ComponentHeader Header;

	[FieldOffset(144)]
	public ModsAndObjectMagicProperties Details1;
}
