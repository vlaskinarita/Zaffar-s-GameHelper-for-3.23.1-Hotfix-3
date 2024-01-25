using System;
using System.Runtime.InteropServices;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EntityNodeValue
{
	public nint EntityPtr;

	public override string ToString()
	{
		return $"EntityPtr: {((IntPtr)EntityPtr).ToInt64():X}";
	}
}
