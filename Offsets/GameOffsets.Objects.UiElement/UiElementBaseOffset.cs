using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.UiElement;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct UiElementBaseOffset
{
	[FieldOffset(0)]
	public nint Vtable;

	[FieldOffset(40)]
	public nint Self;

	[FieldOffset(48)]
	public StdVector ChildrensPtr;

	[FieldOffset(168)]
	public StdTuple2D<float> PositionModifier;

	[FieldOffset(176)]
	public byte ScaleIndex;

	[FieldOffset(192)]
	public StdTuple2D<float> RelativePosition;

	[FieldOffset(216)]
	public float LocalScaleMultiplier;

	[FieldOffset(320)]
	public nint ParentPtr;

	[FieldOffset(328)]
	public uint Flags;

	[FieldOffset(400)]
	public StdTuple2D<float> UnscaledSize;

	[FieldOffset(460)]
	public uint BackgroundColor;
}
