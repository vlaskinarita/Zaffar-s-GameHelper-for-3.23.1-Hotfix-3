using System;
using System.Collections.Generic;
using Coroutine;
using GameHelper.CoroutineEvents;
using GameOffsets.Objects.UiElement;
using ImGuiNET;

namespace GameHelper.RemoteObjects;

public class GameWindowScale : RemoteObjectBase
{
	public float[] Values { get; } = new float[6];


	internal GameWindowScale()
		: base(IntPtr.Zero)
	{
		CoroutineHandler.Start(OnGameMove(), "", 2147483646);
		CoroutineHandler.Start(OnGameForegroundChange(), "", 2147483646);
	}

	public (float WidthScale, float HeightScale) GetScaleValue(int index, float multiplier)
	{
		float widthScale = multiplier;
		float heightScale = multiplier;
		switch (index)
		{
		case 1:
			widthScale *= Values[0];
			heightScale *= Values[1];
			break;
		case 2:
			widthScale *= Values[2];
			heightScale *= Values[3];
			break;
		case 3:
			widthScale *= Values[4];
			heightScale *= Values[5];
			break;
		}
		return (WidthScale: widthScale, HeightScale: heightScale);
	}

	internal override void ToImGui()
	{
		base.ToImGui();
		ImGui.Text($"Index 1: width, height {GetScaleValue(1, 1f)} ratio");
		ImGui.Text($"Index 2: width, height {GetScaleValue(2, 1f)} ratio");
		ImGui.Text($"Index 3: width, height {GetScaleValue(3, 1f)} ratio");
	}

	protected override void CleanUpData()
	{
		for (int i = 0; i < Values.Length; i++)
		{
			Values[i] = 1f;
		}
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		float v1 = (float)((double)(Core.Process.WindowArea.Width - Core.GameCull.Value - Core.GameCull.Value) / UiElementBaseFuncs.BaseResolution.X);
		float v2 = (float)((double)Core.Process.WindowArea.Height / UiElementBaseFuncs.BaseResolution.Y);
		Values[0] = v1;
		Values[1] = v1;
		Values[2] = v2;
		Values[3] = v2;
		Values[4] = v1;
		Values[5] = v2;
	}

	private IEnumerator<Wait> OnGameMove()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.OnMoved);
			UpdateData(hasAddressChanged: false);
		}
	}

	private IEnumerator<Wait> OnGameForegroundChange()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.OnForegroundChanged);
			UpdateData(hasAddressChanged: false);
		}
	}
}
