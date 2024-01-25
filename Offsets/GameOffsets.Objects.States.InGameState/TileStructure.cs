using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 56)]
public struct TileStructure
{
	public nint SubTileDetailsPtr;

	public nint TgtFilePtr;

	public StdVector EntitiesList;

	public nint PAD_0x28;

	public short TileHeight;

	public byte PAD_0x32;

	public byte PAD_0x33;

	public byte tileIdX;

	public byte tileIdY;

	public byte RotationSelector;

	public byte PAD_0x37;

	public static int TileToGridConversion = 23;

	public static float TileToWorldConversion = 250f;
}
