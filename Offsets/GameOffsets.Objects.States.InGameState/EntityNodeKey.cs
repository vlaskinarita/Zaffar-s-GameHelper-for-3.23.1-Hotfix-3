using System.Runtime.InteropServices;

namespace GameOffsets.Objects.States.InGameState;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct EntityNodeKey
{
	public uint id;

	public int pad_0x24;

	public override string ToString()
	{
		return $"id: {id}";
	}

	public override bool Equals(object ob)
	{
		if (ob is EntityNodeKey entityNodeKey)
		{
			return id == entityNodeKey.id;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return id.GetHashCode();
	}

	public static bool operator ==(EntityNodeKey left, EntityNodeKey right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(EntityNodeKey left, EntityNodeKey right)
	{
		return !(left == right);
	}
}
