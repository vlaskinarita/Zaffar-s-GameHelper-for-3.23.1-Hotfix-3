using System;
using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct VitalStruct
{
	[FieldOffset(0)]
	public nint VtablePtr;

	[FieldOffset(8)]
	public nint PtrToLifeComponent;

	[FieldOffset(16)]
	public int ReservedFlat;

	[FieldOffset(20)]
	public int ReservedPercent;

	[FieldOffset(40)]
	public float Regeneration;

	[FieldOffset(44)]
	public int Total;

	[FieldOffset(48)]
	public int Current;

	public int ReservedTotal => (int)Math.Ceiling((float)ReservedPercent / 10000f * (float)Total) + ReservedFlat;

	public int Unreserved => Total - ReservedTotal;

	public int CurrentInPercent()
	{
		if (Total == 0)
		{
			return 0;
		}
		return (int)Math.Round(100.0 * (double)Current / (double)Unreserved);
	}

	public int ReservedInPercent()
	{
		if (Total == 0)
		{
			return 0;
		}
		return (int)Math.Round(100.0 * (double)ReservedTotal / (double)Total);
	}
}
