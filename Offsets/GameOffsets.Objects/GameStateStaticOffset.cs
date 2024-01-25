using System.Runtime.InteropServices;

namespace GameOffsets.Objects;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct GameStateStaticOffset
{
	[FieldOffset(0)]
	public nint GameState;
}
