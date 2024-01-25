using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ModsAndObjectMagicProperties
{
	[FieldOffset(180)]
	public int Rarity;

	[FieldOffset(192)]
	public AllModsType Mods;
}
