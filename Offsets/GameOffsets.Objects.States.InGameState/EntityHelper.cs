using System;
using GameOffsets.Natives;

namespace GameOffsets.Objects.States.InGameState;

public static class EntityHelper
{
	public static Func<byte, bool> IsValidEntity = (byte param) => !Util.isBitSetByte(param, 0);

	public static Func<byte, bool> IsFriendly = (byte param) => (param & 0x7F) == 1;
}
