using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct GameStateOffset
{
	[FieldOffset(8)]
	public StdVector CurrentStatePtr;

	[FieldOffset(72)]
	public nint State0;

	[FieldOffset(88)]
	public nint State1;

	[FieldOffset(104)]
	public nint State2;

	[FieldOffset(120)]
	public nint State3;

	[FieldOffset(136)]
	public nint State4;

	[FieldOffset(152)]
	public nint State5;

	[FieldOffset(168)]
	public nint State6;

	[FieldOffset(184)]
	public nint State7;

	[FieldOffset(200)]
	public nint State8;

	[FieldOffset(216)]
	public nint State9;

	[FieldOffset(232)]
	public nint State10;

	[FieldOffset(248)]
	public nint State11;
}
