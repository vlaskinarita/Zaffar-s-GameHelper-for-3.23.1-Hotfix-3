using System;
using System.Collections.Generic;
using System.Numerics;
using GameOffsets.Natives;
using ImGuiNET;

namespace GameHelper.Utils;

public static class ImGuiHelper
{
	public const ImGuiWindowFlags TransparentWindowFlags = ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoBringToFrontOnFocus;

	public static void DisplayFloatWithInfinitySupport(string text, float data)
	{
		ImGui.Text(text);
		ImGui.SameLine();
		if (float.IsInfinity(data))
		{
			ImGui.Text("Inf");
			return;
		}
		ImGui.Text($"{data}");
	}

	public static uint Color(uint r, uint g, uint b, uint a)
	{
		return (a << 24) | (b << 16) | (g << 8) | r;
	}

	public static uint Color(Vector4 color)
	{
		color *= 255f;
		return ((uint)color.W << 24) | ((uint)color.Z << 16) | ((uint)color.Y << 8) | (uint)color.X;
	}

	public static Vector4 Color(uint color)
	{
		Vector4 ret = Vector4.Zero;
		ret.Z = (float)(color & 0xFFu) / 255f;
		color >>= 8;
		ret.Y = (float)(color & 0xFFu) / 255f;
		color >>= 8;
		ret.X = (float)(color & 0xFFu) / 255f;
		color >>= 8;
		ret.W = (float)(color & 0xFFu) / 255f;
		return ret;
	}

	public static void DrawRect(Vector2 pos, Vector2 size, byte r, byte g, byte b)
	{
		ImGui.GetForegroundDrawList().AddRect(pos, pos + size, Color(r, g, b, 255u), 0f, ImDrawFlags.RoundCornersNone, 4f);
	}

	public static void DrawText(StdTuple3D<float> pos, string text)
	{
		uint colBg = Color(0u, 0u, 0u, 255u);
		uint colFg = Color(255u, 255u, 255u, 255u);
		Vector2 textSizeHalf = ImGui.CalcTextSize(text) / 2f;
		Vector2 location = Core.States.InGameStateObject.CurrentWorldInstance.WorldToScreen(pos);
		Vector2 max = location + textSizeHalf;
		location -= textSizeHalf;
		ImGui.GetBackgroundDrawList().AddRectFilled(location, max, colBg);
		ImGui.GetForegroundDrawList().AddText(location, colFg, text);
	}

	public static void DrawDisabledButton(string buttonLabel)
	{
		uint col = Color(204u, 204u, 204u, 128u);
		ImGui.PushStyleColor(ImGuiCol.Button, col);
		ImGui.PushStyleColor(ImGuiCol.ButtonActive, col);
		ImGui.PushStyleColor(ImGuiCol.ButtonHovered, col);
		ImGui.Button(buttonLabel);
		ImGui.PopStyleColor(3);
	}

	public static void IntPtrToImGui(string name, nint address)
	{
		string addr = ((IntPtr)address).ToInt64().ToString("X");
		ImGui.Text(name);
		ImGui.SameLine();
		ImGui.PushStyleColor(ImGuiCol.Button, Color(0u, 0u, 0u, 0u));
		if (ImGui.SmallButton(addr))
		{
			ImGui.SetClipboardText(addr);
		}
		ImGui.PopStyleColor();
	}

	public static void DisplayTextAndCopyOnClick(string displayText, string copyText)
	{
		ImGui.PushStyleColor(ImGuiCol.Button, Color(0u, 0u, 0u, 0u));
		if (ImGui.SmallButton(displayText))
		{
			ImGui.SetClipboardText(copyText);
		}
		ImGui.PopStyleColor();
	}

	public static bool EnumComboBox<T>(string displayText, ref T current) where T : struct, Enum
	{
		return IEnumerableComboBox(displayText, Enum.GetValues<T>(), ref current);
	}

	public static bool NonContinuousEnumComboBox<T>(string displayText, ref T current) where T : struct, Enum
	{
		bool ret = false;
		T[] enumValues = Enum.GetValues<T>();
		if (ImGui.BeginCombo(displayText, $"{current}"))
		{
			T[] array = enumValues;
			for (int i = 0; i < array.Length; i++)
			{
				T item = array[i];
				bool selected = item.Equals(current);
				if (ImGui.IsWindowAppearing() && selected)
				{
					ImGui.SetScrollHereY();
				}
				if (ImGui.Selectable($"{Convert.ToInt32(item)}:{item}", selected))
				{
					current = item;
					ret = true;
				}
			}
			ImGui.EndCombo();
		}
		return ret;
	}

	public static bool IEnumerableComboBox<T>(string displayText, IEnumerable<T> items, ref T current)
	{
		bool ret = false;
		if (ImGui.BeginCombo(displayText, $"{current}"))
		{
			int counter = 0;
			foreach (T item in items)
			{
				bool selected = item.Equals(current);
				if (ImGui.IsWindowAppearing() && selected)
				{
					ImGui.SetScrollHereY();
				}
				if (ImGui.Selectable($"{counter}:{item}", selected))
				{
					current = item;
					ret = true;
				}
				counter++;
			}
			ImGui.EndCombo();
		}
		return ret;
	}

	public static void ToolTip(string text)
	{
		if (ImGui.IsItemHovered() && ImGui.BeginTooltip())
		{
			ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
			ImGui.TextUnformatted(text);
			ImGui.PopTextWrapPos();
			ImGui.EndTooltip();
		}
	}

	public static bool Vector2SliderInt(string text, float itemWidth, ref Vector2 data, int min0, int max0, int min1, int max1, ImGuiSliderFlags flags)
	{
		bool dataChanged = false;
		int dataX = (int)data.X;
		int dataY = (int)data.Y;
		ImGui.PushItemWidth(itemWidth / 3.1f);
		if (ImGui.SliderInt("##" + text + "111", ref dataX, min0, max0, "%d", flags))
		{
			dataChanged = true;
			data.X = dataX;
		}
		ImGui.SameLine(0f, 5f);
		if (ImGui.SliderInt(text + "##" + text + "222", ref dataY, min1, max1, "%d", flags))
		{
			dataChanged = true;
			data.Y = dataY;
		}
		ImGui.PopItemWidth();
		return dataChanged;
	}
}
