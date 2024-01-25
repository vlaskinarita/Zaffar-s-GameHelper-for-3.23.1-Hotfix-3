using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.UiElement;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct MapUiElementOffset
{
	[FieldOffset(0)]
	public UiElementBaseOffset UiElementBase;

	[FieldOffset(640)]
	public StdTuple2D<float> Shift;

	[FieldOffset(648)]
	public StdTuple2D<float> DefaultShift;

	[FieldOffset(708)]
	public float Zoom;
}
