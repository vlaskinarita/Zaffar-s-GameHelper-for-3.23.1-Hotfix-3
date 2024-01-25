using GameHelper.Utils;
using ImGuiNET;

namespace GameHelper.RemoteObjects;

public class TerrainHeightHelper : RemoteObjectBase
{
	public byte[] Values { get; private set; }

	internal TerrainHeightHelper(nint address, int size)
		: base(address)
	{
		Values = new byte[size];
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text(string.Join(' ', Values));
	}

	protected override void CleanUpData()
	{
		for (int i = 0; i < Values.Length; i++)
		{
			Values[i] = 0;
		}
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		Values = reader.ReadMemoryArray<byte>(base.Address, Values.Length);
	}
}
