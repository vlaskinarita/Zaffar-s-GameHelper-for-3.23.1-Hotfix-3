using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct RenderOffsets
{
	[FieldOffset(0)]
	public ComponentHeader Header;

	[FieldOffset(168)]
	public StdTuple3D<float> CurrentWorldPosition;

	[FieldOffset(180)]
	public StdTuple3D<float> CharactorModelBounds;

	[FieldOffset(268)]
	public float TerrainHeight;
}
