using System.Runtime.InteropServices;

namespace GameOffsets.Natives;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct StdMapNodeData<TKey, TValue> where TKey : struct where TValue : struct
{
	public TKey Key;

	public TValue Value;
}
