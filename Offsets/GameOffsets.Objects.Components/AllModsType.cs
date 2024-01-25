using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct AllModsType
{
	public StdVector ImplicitMods;

	public StdVector ExplicitMods;

	public StdVector EnchantMods;

	public StdVector HellscapeMods;

	public StdVector CrucibleMods;
}
