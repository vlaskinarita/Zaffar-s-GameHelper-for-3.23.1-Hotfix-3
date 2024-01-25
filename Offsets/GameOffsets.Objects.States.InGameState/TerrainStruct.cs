using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct TerrainStruct
{
	[FieldOffset(24)]
	public StdTuple2D<long> TotalTiles;

	[FieldOffset(40)]
	public StdVector TileDetailsPtr;

	[FieldOffset(208)]
	public StdVector GridWalkableData;

	[FieldOffset(232)]
	public StdVector GridLandscapeData;

	[FieldOffset(256)]
	public int BytesPerRow;

	[FieldOffset(260)]
	public short TileHeightMultiplier;

	public static float TileHeightFinalMultiplier = 7.8125f;
}
