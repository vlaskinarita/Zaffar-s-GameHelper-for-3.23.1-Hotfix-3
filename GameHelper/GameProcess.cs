using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Coroutine;
using GameHelper.CoroutineEvents;
using GameHelper.Utils;
using GameOffsets;
using ImGuiNET;

namespace GameHelper;

public class GameProcess
{
	private struct RECT
	{
		private readonly int left;

		private readonly int top;

		private readonly int right;

		private readonly int bottom;

		internal Rectangle ToRectangle(Point point)
		{
			return new Rectangle(point.X, point.Y, right - left, bottom - top);
		}
	}

	private readonly List<Process> processesInfo = new List<Process>();

	private int clientSelected = -1;

	private bool showSelectGameMenu;

	private bool closeForcefully;

	public uint Pid
	{
		get
		{
			try
			{
				return (uint)Information.Id;
			}
			catch
			{
				return 0u;
			}
		}
	}

	public bool Foreground { get; private set; }

	public Rectangle WindowArea { get; private set; } = Rectangle.Empty;


	internal nint Address
	{
		get
		{
			try
			{
				SafeMemoryHandle reader = Handle;
				if (reader != null && !reader.IsClosed && !reader.IsInvalid)
				{
					return Information.MainModule.BaseAddress;
				}
				return IntPtr.Zero;
			}
			catch (Exception)
			{
				return IntPtr.Zero;
			}
		}
		private set
		{
		}
	}

	internal Event OnStaticAddressFound { get; } = new Event();


	internal Dictionary<string, nint> StaticAddresses { get; } = new Dictionary<string, nint>();


	internal Process Information { get; private set; }

	internal SafeMemoryHandle Handle { get; private set; }

	internal GameProcess()
	{
		CoroutineHandler.Start(FindAndOpen());
		CoroutineHandler.Start(FindStaticAddresses());
		CoroutineHandler.Start(AskUserToSelectClient());
	}

	internal void Close(bool monitorForNewGame = true)
	{
		CoroutineHandler.RaiseEvent(GameHelperEvents.OnClose);
		WindowArea = Rectangle.Empty;
		Foreground = false;
		Handle?.Dispose();
		Information?.Close();
		if (monitorForNewGame)
		{
			CoroutineHandler.Start(FindAndOpen());
		}
	}

	private IEnumerator<Wait> FindAndOpen()
	{
		while (true)
		{
			yield return new Wait(2.0);
			processesInfo.Clear();
			Process[] processes = Process.GetProcesses();
			foreach (Process process in processes)
			{
				if (GameProcessDetails.ProcessName.TryGetValue(process.ProcessName, out var windowTitle) && process.MainWindowTitle.ToLower() == windowTitle)
				{
					processesInfo.Add(process);
				}
			}
			if (processesInfo.Count == 1)
			{
				Information = processesInfo[0];
				if (Open())
				{
					yield break;
				}
			}
			else
			{
				if (processesInfo.Count <= 1)
				{
					continue;
				}
				ShowSelectGameMenu();
				if (clientSelected > -1 && clientSelected < processesInfo.Count)
				{
					Information = processesInfo[clientSelected];
					if (Open())
					{
						break;
					}
				}
			}
		}
		processesInfo.Clear();
	}

	private IEnumerator<Wait> AskUserToSelectClient()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.OnRender);
			if (showSelectGameMenu)
			{
				ImGui.OpenPopup("SelectGameMenu");
			}
			if (!ImGui.BeginPopup("SelectGameMenu"))
			{
				continue;
			}
			for (int i = 0; i < processesInfo.Count; i++)
			{
				bool foreground = GetForegroundWindow() == processesInfo[i].MainWindowHandle;
				if (ImGui.RadioButton($"{i} - PathOfExile - Focused: {foreground}", i == clientSelected))
				{
					clientSelected = i;
				}
			}
			ImGui.BeginDisabled(Address == IntPtr.Zero);
			if (ImGui.Button("Done"))
			{
				HideSelectGameMenu();
				ImGui.CloseCurrentPopup();
			}
			ImGui.EndDisabled();
			ImGui.SameLine();
			if (ImGui.Button("Retry or Delay Selection"))
			{
				HideSelectGameMenu();
				ImGui.CloseCurrentPopup();
				closeForcefully = true;
			}
			ImGui.EndPopup();
		}
	}

	private void HideSelectGameMenu()
	{
		clientSelected = -1;
		processesInfo.Clear();
		showSelectGameMenu = false;
	}

	private void ShowSelectGameMenu()
	{
		showSelectGameMenu = true;
	}

	private IEnumerator<Wait> Monitor()
	{
		while (!Information.HasExited && ((IntPtr)Information.MainWindowHandle).ToInt64() > 0 && !closeForcefully)
		{
			UpdateIsForeground();
			UpdateWindowRectangle();
			yield return new Wait(1.0);
		}
		closeForcefully = false;
		Close();
	}

	private IEnumerator<Wait> FindStaticAddresses()
	{
		while (true)
		{
			yield return new Wait(GameHelperEvents.OnOpened);
			nint baseAddress = Address;
			if (baseAddress == IntPtr.Zero)
			{
				continue;
			}
			int procSize = Information.MainModule.ModuleMemorySize;
			foreach (KeyValuePair<string, int> patternInfo in PatternFinder.Find(Handle, baseAddress, procSize))
			{
				int offsetDataValue = Handle.ReadMemory<int>(baseAddress + patternInfo.Value);
				nint address = baseAddress + patternInfo.Value + offsetDataValue + 4;
				StaticAddresses[patternInfo.Key] = address;
			}
			CoroutineHandler.RaiseEvent(OnStaticAddressFound);
		}
	}

	private bool Open()
	{
		Handle = new SafeMemoryHandle(Information.Id);
		if (Handle.IsInvalid)
		{
			return false;
		}
		Core.CoroutinesRegistrar.Add(CoroutineHandler.Start(Monitor(), "[GameProcess] Monitoring Game Process"));
		CoroutineHandler.RaiseEvent(GameHelperEvents.OnOpened);
		return true;
	}

	private void UpdateIsForeground()
	{
		bool foreground = GetForegroundWindow() == Information.MainWindowHandle;
		if (foreground != Foreground)
		{
			Foreground = foreground;
			CoroutineHandler.RaiseEvent(GameHelperEvents.OnForegroundChanged);
		}
	}

	private void UpdateWindowRectangle()
	{
		GetClientRect(Information.MainWindowHandle, out var size);
		ClientToScreen(Information.MainWindowHandle, out var pos);
		Rectangle sizePos = size.ToRectangle(pos);
		if (sizePos != WindowArea && sizePos.Size != Size.Empty)
		{
			WindowArea = sizePos;
			CoroutineHandler.RaiseEvent(GameHelperEvents.OnMoved);
		}
	}

	[DllImport("user32.dll")]
	private static extern nint GetForegroundWindow();

	[DllImport("user32.dll")]
	private static extern bool GetClientRect(nint hWnd, out RECT lpRect);

	[DllImport("user32.dll")]
	private static extern bool ClientToScreen(nint hWnd, out Point lpPoint);
}
