using System;

namespace GameOffsets.Natives;

public static class Util
{
	public static Func<byte, int, bool> isBitSetByte = (byte x, int pos) => (x & (1 << pos)) != 0;

	public static Func<uint, int, bool> isBitSetUint = (uint x, int pos) => (x & (1 << pos)) != 0;
}
