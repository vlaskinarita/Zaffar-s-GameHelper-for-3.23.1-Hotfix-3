using System;
using System.Collections.Generic;
using System.Numerics;
using Coroutine;
using GameHelper.CoroutineEvents;
using GameHelper.RemoteObjects.FilesStructures;
using GameHelper.Utils;
using GameOffsets.Natives;
using GameOffsets.Objects.States.InGameState;
using ImGuiNET;

namespace GameHelper.RemoteObjects.States.InGameStateObjects;

public class WorldData : RemoteObjectBase
{
	private nint areaDetailsPtrCache = IntPtr.Zero;

	private Matrix4x4 worldToScreenMatrix = Matrix4x4.Identity;

	public WorldAreaDat AreaDetails { get; } = new WorldAreaDat(IntPtr.Zero);


	public Vector2 WorldToScreen(StdTuple3D<float> worldPosition)
	{
		return WorldToScreen(worldPosition, worldPosition.Z);
	}

	public Vector2 WorldToScreen(StdTuple3D<float> worldPosition, float height)
	{
		Vector2 result = Vector2.Zero;
		if (base.Address == IntPtr.Zero)
		{
			return result;
		}
		Vector4 temp0 = new Vector4(worldPosition.X, worldPosition.Y, height, 1f);
		temp0 = Vector4.Transform(temp0, worldToScreenMatrix);
		temp0 /= temp0.W;
		result.X = (temp0.X + 1f) * ((float)Core.Process.WindowArea.Width / 2f);
		result.Y = (1f - temp0.Y) * ((float)Core.Process.WindowArea.Height / 2f);
		return result;
	}

	public Vector2 WorldToScreen(Vector2 worldPosition, float height)
	{
		Vector2 result = Vector2.Zero;
		if (base.Address == IntPtr.Zero)
		{
			return result;
		}
		Vector4 temp0 = new Vector4(worldPosition.X, worldPosition.Y, height, 1f);
		temp0 = Vector4.Transform(temp0, worldToScreenMatrix);
		temp0 /= temp0.W;
		result.X = (temp0.X + 1f) * ((float)Core.Process.WindowArea.Width / 2f);
		result.Y = (1f - temp0.Y) * ((float)Core.Process.WindowArea.Height / 2f);
		return result;
	}

	internal WorldData(nint address)
		: base(address)
	{
		Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(OnPerFrame(), "[AreaInstance] Update World Data"));
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		if (ImGui.TreeNode("WindowToScreenMatrix"))
		{
			Matrix4x4 d = worldToScreenMatrix;
			ImGui.Text($"{d.M11:0.00}\t{d.M12:0.00}\t{d.M13:0.00}\t{d.M14:0.00}");
			ImGui.Text($"{d.M21:0.00}\t{d.M22:0.00}\t{d.M23:0.00}\t{d.M24:0.00}");
			ImGui.Text($"{d.M31:0.00}\t{d.M32:0.00}\t{d.M33:0.00}\t{d.M34:0.00}");
			ImGui.Text($"{d.M41:0.00}\t{d.M42:0.00}\t{d.M43:0.00}\t{d.M44:0.00}");
			ImGui.TreePop();
		}
	}

	protected override void CleanUpData()
	{
		areaDetailsPtrCache = IntPtr.Zero;
		AreaDetails.Address = IntPtr.Zero;
		worldToScreenMatrix = Matrix4x4.Identity;
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		SafeMemoryHandle reader = Core.Process.Handle;
		WorldDataOffset data = reader.ReadMemory<WorldDataOffset>(base.Address);
		if (areaDetailsPtrCache != data.WorldAreaDetailsPtr)
		{
			WorldAreaDetailsStruct areaInfo = reader.ReadMemory<WorldAreaDetailsStruct>(data.WorldAreaDetailsPtr);
			AreaDetails.Address = areaInfo.WorldAreaDetailsRowPtr;
			areaDetailsPtrCache = data.WorldAreaDetailsPtr;
		}
		worldToScreenMatrix = data.CameraStructurePtr.WorldToScreenMatrix;
	}

	private IEnumerator<Wait> OnPerFrame()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.PostPerFrameDataUpdate);
			if (base.Address != IntPtr.Zero)
			{
				UpdateData(hasAddressChanged: false);
			}
		}
	}
}
