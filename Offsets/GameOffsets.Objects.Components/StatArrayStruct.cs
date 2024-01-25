using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StatArrayStruct
{
	public int key;

	public int value;
}
