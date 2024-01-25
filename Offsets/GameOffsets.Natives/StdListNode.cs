using System.Runtime.InteropServices;

namespace GameOffsets.Natives;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StdListNode<TValue> where TValue : struct
{
	public nint Next;

	public nint Previous;

	public TValue Data;
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StdListNode
{
	public nint Next;

	public nint Previous;
}
