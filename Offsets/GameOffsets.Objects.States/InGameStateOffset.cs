using System.Runtime.InteropServices;

namespace GameOffsets.Objects.States;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct InGameStateOffset
{
	[FieldOffset(24)]
	public nint AreaInstanceData;

	[FieldOffset(120)]
	public nint WorldData;

	[FieldOffset(800)]
	public nint UiRootPtr;

	[FieldOffset(1512)]
	public nint IngameUi;
}
