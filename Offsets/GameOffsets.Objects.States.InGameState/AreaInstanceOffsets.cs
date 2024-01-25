using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct AreaInstanceOffsets
{
	[FieldOffset(212)]
	public byte CurrentAreaLevel;

	[FieldOffset(276)]
	public uint CurrentAreaHash;

	[FieldOffset(2112)]
	public StdVector Environments;

	[FieldOffset(2160)]
	public LocalPlayerStruct PlayerInfo;

	[FieldOffset(2344)]
	public EntityListStruct Entities;

	[FieldOffset(2768)]
	public TerrainStruct TerrainMetadata;
}
