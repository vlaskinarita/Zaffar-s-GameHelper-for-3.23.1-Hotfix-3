using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ActorOffset
{
	[FieldOffset(0)]
	public ComponentHeader Header;

	[FieldOffset(572)]
	public int AnimationId;

	[FieldOffset(1728)]
	public StdVector ActiveSkillsPtr;

	[FieldOffset(1752)]
	public StdVector CooldownsPtr;

	[FieldOffset(1776)]
	public StdVector VaalSoulsPtr;

	[FieldOffset(1800)]
	public StdVector DeployedEntityArray;
}
