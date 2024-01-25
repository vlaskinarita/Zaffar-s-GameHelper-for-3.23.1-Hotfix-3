using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DeployedEntityStructure
{
	public int EntityId;

	public ushort ActiveSkillsDatId;

	public ushort PAD_0x0C;

	public ushort DeployedObjectType;

	public ushort PAD_0x014;

	public override string ToString()
	{
		return $"{DeployedObjectType} - {ActiveSkillsDatId} - {EntityId}";
	}
}
