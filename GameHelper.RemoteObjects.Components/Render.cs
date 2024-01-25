using System;
using GameOffsets.Natives;
using GameOffsets.Objects.Components;
using GameOffsets.Objects.States.InGameState;
using ImGuiNET;

namespace GameHelper.RemoteObjects.Components;

public class Render : ComponentBase
{
	private static readonly float WorldToGridRatio = TileStructure.TileToWorldConversion / (float)TileStructure.TileToGridConversion;

	private StdTuple3D<float> gridPos2D;

	public StdTuple3D<float> GridPosition
	{
		get
		{
			return gridPos2D;
		}
		private set
		{
			gridPos2D = value;
		}
	}

	public StdTuple3D<float> ModelBounds { get; private set; }

	public StdTuple3D<float> WorldPosition { get; private set; }

	public float TerrainHeight { get; private set; }

	public Render(nint address)
		: base(address)
	{
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text($"Grid Position: {gridPos2D}");
		ImGui.Text($"World Position: {WorldPosition}");
		ImGui.Text($"Terrain Height (Z-Axis): {TerrainHeight}");
		ImGui.Text($"Model Bounds: {ModelBounds}");
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		RenderOffsets data = Core.Process.Handle.ReadMemory<RenderOffsets>(base.Address);
		OwnerEntityAddress = data.Header.EntityPtr;
		WorldPosition = data.CurrentWorldPosition;
		ModelBounds = data.CharactorModelBounds;
		TerrainHeight = (float)Math.Round(data.TerrainHeight, 4);
		gridPos2D.X = data.CurrentWorldPosition.X / WorldToGridRatio;
		gridPos2D.Y = data.CurrentWorldPosition.Y / WorldToGridRatio;
	}
}
