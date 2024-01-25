using System.Runtime.InteropServices;
using GameOffsets.Natives;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
public struct ActiveSkillCooldown
{
	[FieldOffset(0)]
	public int Unknown0;

	[FieldOffset(4)]
	public int Unknown1;

	[FieldOffset(8)]
	public int ActiveSkillsDatId;

	[FieldOffset(12)]
	public int Unknown2;

	[FieldOffset(16)]
	public StdVector CooldownsList;

	[FieldOffset(40)]
	public nint PtrToActorComponentPlusOffset;

	[FieldOffset(48)]
	public int MaxUses;

	[FieldOffset(52)]
	public int TotalCooldownTimeInMs;

	[FieldOffset(60)]
	public uint UnknownIdAndEquipmentInf0;

	[FieldOffset(68)]
	public int PAD_0x44;

	public int TotalActiveCooldowns()
	{
		return (int)CooldownsList.TotalElements(16);
	}

	public bool CannotBeUsed()
	{
		return TotalActiveCooldowns() >= MaxUses;
	}
}
