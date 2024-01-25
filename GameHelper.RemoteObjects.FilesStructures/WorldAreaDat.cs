using GameHelper.Utils;
using GameOffsets.Objects.FilesStructures;
using ImGuiNET;

namespace GameHelper.RemoteObjects.FilesStructures;

public class WorldAreaDat : RemoteObjectBase
{
	public string Id { get; private set; } = string.Empty;


	public string Name { get; private set; } = string.Empty;


	public int Act { get; private set; }

	public bool IsTown { get; private set; }

	public bool IsHideout { get; private set; }

	public bool IsBattleRoyale { get; private set; }

	public bool HasWaypoint { get; private set; }

	internal WorldAreaDat(nint address)
		: base(address)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text("Id: " + Id);
		ImGui.Text("Name: " + Name);
		ImGui.Text($"Is Town: {IsTown}");
		ImGui.Text($"Is Hideout: {IsHideout}");
		ImGui.Text($"Is BattleRoyale: {IsBattleRoyale}");
		ImGui.Text($"Has Waypoint: {HasWaypoint}");
	}

	protected override void CleanUpData()
	{
		Id = string.Empty;
		Name = string.Empty;
		Act = 0;
		IsTown = false;
		IsHideout = false;
		IsBattleRoyale = false;
		HasWaypoint = false;
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		WorldAreaDatOffsets data = reader.ReadMemory<WorldAreaDatOffsets>(base.Address);
		Id = reader.ReadUnicodeString(data.IdPtr);
		Name = reader.ReadUnicodeString(data.NamePtr);
		IsTown = data.IsTown || Id == "HeistHub";
		HasWaypoint = data.HasWaypoint || Id == "HeistHub";
		IsHideout = Id.ToLower().Contains("hideout");
		IsBattleRoyale = Id.ToLower().Contains("exileroyale");
	}
}
