using System.Runtime.InteropServices;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EnvironmentStruct
{
	public ushort Key;

	public ushort Value0;

	public float Value1;
}
