using System;
using System.Runtime.InteropServices;

namespace GameOffsets.Natives;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StdWString
{
	public nint Buffer;

	public nint ReservedBytes;

	public int Length;

	public int PAD_14;

	public int Capacity;

	public int PAD_1C;

	public override string ToString()
	{
		return $"Buffer: {((IntPtr)Buffer).ToInt64():X}, ReservedBytes: {((IntPtr)ReservedBytes).ToInt64():X}, Length: {Length}, Capacity: {Capacity}";
	}
}
