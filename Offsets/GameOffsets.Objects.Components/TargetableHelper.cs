using System;
using GameOffsets.Natives;

namespace GameOffsets.Objects.Components;

public static class TargetableHelper
{
	public static Func<uint, bool> IsTargetable = (uint param) => Util.isBitSetUint(param, 0);
}
