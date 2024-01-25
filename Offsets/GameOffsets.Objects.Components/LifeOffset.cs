using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct LifeOffset
{
	[FieldOffset(0)]
	public ComponentHeader Header;

	[FieldOffset(376)]
	public VitalStruct Health;

	[FieldOffset(456)]
	public VitalStruct Mana;

	[FieldOffset(512)]
	public VitalStruct EnergyShield;
}
