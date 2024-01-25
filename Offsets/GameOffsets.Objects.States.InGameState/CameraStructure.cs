using System.Numerics;
using System.Runtime.InteropServices;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct CameraStructure
{
	[FieldOffset(0)]
	public nint CodePtr;

	[FieldOffset(320)]
	public Matrix4x4 WorldToScreenMatrix;
}
