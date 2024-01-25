using System;
using System.Collections.Generic;
using System.Numerics;
using Coroutine;
using GameHelper.Cache;
using GameHelper.CoroutineEvents;
using GameHelper.RemoteEnums;
using GameHelper.RemoteObjects.UiElement;
using GameHelper.Utils;
using ImGuiNET;

namespace GameHelper.Ui;

public static class GameUiExplorer
{
	private struct UiElement
	{
		public int CurrentChildIndex;

		public string CurrentChildPreview;

		public UiElementBase Element;

		public List<UiElementBase> Children;
	}

	private static readonly Vector4 VisibleUiElementColor = new Vector4(0f, 255f, 0f, 255f);

	private static readonly List<UiElement> Elements = new List<UiElement>();

	private static readonly UiElementParents Parents = new UiElementParents(null, GameStateTypes.InGameState, GameStateTypes.EscapeState);

	internal static void InitializeCoroutines()
	{
		CoroutineHandler.Start(GameUiExplorerRenderCoRoutine());
		CoroutineHandler.Start(OnGameStateChange());
	}

	internal static void AddUiElement(UiElementBase element)
	{
		Elements.Add(CreateUiElement(new UiElementBase(element.Address, Parents)));
		Core.GHSettings.ShowGameUiExplorer = true;
	}

	private static UiElement CreateUiElement(UiElementBase element)
	{
		UiElement uiElement = default(UiElement);
		uiElement.CurrentChildIndex = -1;
		uiElement.CurrentChildPreview = string.Empty;
		uiElement.Element = element;
		uiElement.Children = new List<UiElementBase>();
		UiElement eleStruct = uiElement;
		for (int i = 0; i < element.TotalChildrens; i++)
		{
			eleStruct.Children.Add(element[i]);
		}
		return eleStruct;
	}

	private static void RemoveUiElement(int i)
	{
		Elements[i].Children.Clear();
		Elements.RemoveAt(i);
		if (Elements.Count == 0)
		{
			Parents.Clear();
		}
	}

	private static void RemoveAllUiElements()
	{
		for (int i = Elements.Count - 1; i >= 0; i--)
		{
			RemoveUiElement(i);
		}
		Parents.Clear();
	}

	private static IEnumerator<Wait> GameUiExplorerRenderCoRoutine()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.OnRender);
			if (!Core.GHSettings.ShowGameUiExplorer)
			{
				continue;
			}
			Parents.UpdateAllParentsParallel();
			if (ImGui.Begin("Game UiExplorer", ref Core.GHSettings.ShowGameUiExplorer))
			{
				if (ImGui.TreeNode("NOTES"))
				{
					ImGui.BulletText("Closing the game will remove all objects.");
					ImGui.BulletText("To add element in this window go to any UiElement in Data Visualization window and click Explore button.");
					ImGui.BulletText("To check currently loaded element bounds, hover over the element header in blue.");
					ImGui.BulletText("To check bounds of all the children hover over the Children box.");
					ImGui.BulletText("Feel free to add same element more than once.");
					ImGui.BulletText("When children combo box is opened feel free to use the up/down arrow key.");
					ImGui.BulletText("Children bounds are drawn with RED color.");
					ImGui.BulletText("Current element bounds are drawn with Yellow Color.");
					ImGui.BulletText("Green color child means it's visible, white means it isn't.");
					ImGui.TreePop();
				}
				Parents.ToImGui();
				if (ImGui.Button("Clear all Ui Elements (Mischief managed)") || Core.Process.Address == IntPtr.Zero)
				{
					RemoveAllUiElements();
				}
				ImGui.Separator();
				for (int i = 0; i < Elements.Count; i++)
				{
					UiElement current = Elements[i];
					bool isRequired = true;
					bool isCurrentModified = false;
					bool isEnterPressed = false;
					if (ImGui.CollapsingHeader($"{i}", ref isRequired, ImGuiTreeNodeFlags.DefaultOpen))
					{
						if (ImGui.IsItemHovered())
						{
							ImGuiHelper.DrawRect(current.Element.Postion, current.Element.Size, byte.MaxValue, byte.MaxValue, 0);
						}
						ImGuiHelper.IntPtrToImGui("Address:", current.Element.Address);
						current.Element.Address = current.Element.Address;
						if (ImGui.BeginCombo($"Children##{i}", current.CurrentChildPreview))
						{
							if (current.CurrentChildIndex > -1 && ImGui.IsItemHovered())
							{
								UiElementBase cChild = current.Children[current.CurrentChildIndex];
								ImGuiHelper.DrawRect(cChild.Postion, cChild.Size, byte.MaxValue, 64, 64);
							}
							if (ImGui.IsKeyPressed(ImGuiKey.UpArrow))
							{
								if (current.CurrentChildIndex > 0)
								{
									current.CurrentChildIndex--;
								}
								else
								{
									current.CurrentChildIndex = current.Children.Count - 1;
								}
								isCurrentModified = true;
								Elements[i] = current;
							}
							else if (ImGui.IsKeyPressed(ImGuiKey.DownArrow))
							{
								if (current.CurrentChildIndex < current.Children.Count - 1)
								{
									current.CurrentChildIndex++;
								}
								else
								{
									current.CurrentChildIndex = 0;
								}
								isCurrentModified = true;
								Elements[i] = current;
							}
							else if (ImGui.IsKeyPressed(ImGuiKey.Enter))
							{
								isEnterPressed = true;
							}
							for (int k = 0; k < current.Children.Count; k++)
							{
								bool selected = k == current.CurrentChildIndex;
								UiElementBase child2 = current.Children[k];
								child2.Address = child2.Address;
								if (child2.IsVisible)
								{
									ImGui.PushStyleColor(ImGuiCol.Text, VisibleUiElementColor);
								}
								if (ImGui.Selectable($"{k}-{((IntPtr)child2.Address).ToInt64():X}", selected))
								{
									current.CurrentChildIndex = k;
									current.CurrentChildPreview = $"{((IntPtr)child2.Address).ToInt64():X}";
									Elements[i] = current;
								}
								if (child2.IsVisible)
								{
									ImGui.PopStyleColor();
								}
								if (isEnterPressed && selected)
								{
									current.CurrentChildIndex = k;
									current.CurrentChildPreview = $"{((IntPtr)child2.Address).ToInt64():X}";
									Elements[i] = current;
									ImGui.CloseCurrentPopup();
									isEnterPressed = false;
								}
								if ((ImGui.IsWindowAppearing() || isCurrentModified) && selected)
								{
									isCurrentModified = false;
									ImGui.SetScrollHereY();
								}
								if (ImGui.IsItemHovered())
								{
									ImGuiHelper.DrawRect(child2.Postion, child2.Size, byte.MaxValue, byte.MaxValue, 0);
								}
							}
							ImGui.EndCombo();
						}
						if (ImGui.IsItemHovered())
						{
							for (int j = 0; j < current.Children.Count; j++)
							{
								UiElementBase child = current.Children[j];
								child.Address = child.Address;
								ImGuiHelper.DrawRect(child.Postion, child.Size, byte.MaxValue, byte.MaxValue, 0);
								ImGui.GetForegroundDrawList().AddText(child.Postion, 4278190335u, $"{j}");
							}
						}
						if (current.Children.Count > 0 && current.CurrentChildIndex > -1)
						{
							if (ImGui.Button($"Go to child##{i}"))
							{
								Elements[i] = CreateUiElement(current.Children[current.CurrentChildIndex]);
							}
						}
						else
						{
							ImGuiHelper.DrawDisabledButton($"Go to child##{i}");
						}
						ImGui.SameLine();
						if (current.Element.TryGetParent(out var parent))
						{
							if (ImGui.Button($"Go to parent##{i}"))
							{
								Elements[i] = CreateUiElement(parent);
							}
						}
						else
						{
							ImGuiHelper.DrawDisabledButton($"Go to parent##{i}");
						}
					}
					if (!isRequired)
					{
						RemoveUiElement(i);
					}
				}
			}
			ImGui.End();
		}
	}

	private static IEnumerator<Wait> OnGameStateChange()
	{
		while (true)
		{
			yield return new Wait(RemoteEvents.StateChanged);
			if (Core.States.GameCurrentState != GameStateTypes.InGameState && Core.States.GameCurrentState != GameStateTypes.EscapeState && Core.States.GameCurrentState != 0)
			{
				RemoveAllUiElements();
			}
		}
	}
}
