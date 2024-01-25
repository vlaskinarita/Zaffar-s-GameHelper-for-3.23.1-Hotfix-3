using System.Runtime.InteropServices;

namespace GameOffsets.Objects.Components;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ActiveSkillStructure
{
	public nint UselessPtr0;

	public nint ActiveSkillPtr;
}
