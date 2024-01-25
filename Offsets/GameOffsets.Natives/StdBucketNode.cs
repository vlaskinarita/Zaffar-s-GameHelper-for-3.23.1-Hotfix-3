using System.Runtime.InteropServices;

namespace GameOffsets.Natives;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StdBucketNode<TValue> where TValue : struct
{
	public byte Flag0;

	public byte Flag1;

	public byte Flag2;

	public byte Flag3;

	public byte Flag4;

	public byte Flag5;

	public byte Flag6;

	public byte Flag7;

	public TValue Pointer0;

	public TValue Pointer1;

	public TValue Pointer2;

	public TValue Pointer3;

	public TValue Pointer4;

	public TValue Pointer5;

	public TValue Pointer6;

	public TValue Pointer7;

	public static byte InValidPointerFlagValue = byte.MaxValue;
}
