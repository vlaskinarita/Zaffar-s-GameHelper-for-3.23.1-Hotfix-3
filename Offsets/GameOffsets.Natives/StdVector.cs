using System;
using System.Runtime.InteropServices;

namespace GameOffsets.Natives;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StdVector
{
	public nint First;

	public nint Last;

	public nint End;

	public long TotalElements(int elementSize)
	{
		return (((IntPtr)Last).ToInt64() - ((IntPtr)First).ToInt64()) / elementSize;
	}

	public override string ToString()
	{
		return $"First: {((IntPtr)First).ToInt64():X} - Last: {((IntPtr)Last).ToInt64():X} - Size (bytes): {TotalElements(1)}";
	}
}
