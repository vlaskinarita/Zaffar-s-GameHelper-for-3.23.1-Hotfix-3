using System;
using GameOffsets.Natives;

namespace GameOffsets.Objects.UiElement;

public static class UiElementBaseFuncs
{
	private const int SHOULD_MODIFY_BINARY_POS = 10;

	private const int IS_VISIBLE_BINARY_POS = 11;

	public static readonly StdTuple2D<double> BaseResolution = new StdTuple2D<double>(2560.0, 1600.0);

	public static Func<uint, bool> IsVisibleChecker = (uint param) => Util.isBitSetUint(param, 11);

	public static Func<uint, bool> ShouldModifyPos = (uint param) => Util.isBitSetUint(param, 10);
}
