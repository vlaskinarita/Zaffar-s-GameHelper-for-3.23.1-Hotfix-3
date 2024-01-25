using System;

namespace GameOffsets.Objects.States.InGameState;

public static class EntityFilter
{
	public static Func<EntityNodeKey, bool> IgnoreVisualsAndDecorations = (EntityNodeKey param) => param.id < 1073741824;
}
