using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ClickableTransparentOverlay.Win32;

namespace GameHelper.Utils;

public static class MiscHelper
{
	private enum TcpTableClass
	{
		TcpTableBasicListener,
		TcpTableBasicConnections,
		TcpTableBasicAll,
		TcpTableOwnerPidListener,
		TcpTableOwnerPidConnections,
		TcpTableOwnerPidAll,
		TcpTableOwnerModuleListener,
		TcpTableOwnerModuleConnections,
		TcpTableOwnerModuleAll
	}

	private struct MibTcprowOwnerPid
	{
		public uint State;

		public readonly uint LocalAddr;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public readonly byte[] LocalPort;

		public readonly uint RemoteAddr;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public readonly byte[] RemotePort;

		public readonly uint OwningPid;
	}

	private readonly struct MibTcptableOwnerPid
	{
		public readonly uint DwNumEntries;

		private readonly MibTcprowOwnerPid table;
	}

	private static readonly Random Rand = new Random();

	private static readonly Stopwatch DelayBetweenKeys = Stopwatch.StartNew();

	private static Task<nint> sendingMessage;

	internal static void ActiveSkillGemDataParser(uint unknownIdAndEquipmentInfo, out bool isUserEquipped, out byte Unknown0, out byte socketIndex, out byte linkId, out byte inventoryName, out uint activeSkillGemUnknownId)
	{
		activeSkillGemUnknownId = unknownIdAndEquipmentInfo >> 16;
		unknownIdAndEquipmentInfo &= 0xFFFFu;
		inventoryName = (byte)((unknownIdAndEquipmentInfo & 0x7F) + 1);
		unknownIdAndEquipmentInfo >>= 7;
		linkId = (byte)(unknownIdAndEquipmentInfo & 7u);
		unknownIdAndEquipmentInfo >>= 3;
		socketIndex = (byte)(unknownIdAndEquipmentInfo & 7u);
		unknownIdAndEquipmentInfo >>= 3;
		Unknown0 = (byte)(unknownIdAndEquipmentInfo & 3u);
		unknownIdAndEquipmentInfo >>= 2;
		isUserEquipped = unknownIdAndEquipmentInfo != 0;
	}

	internal static bool TryConvertStringToImGuiGlyphRanges(string data, out ushort[] ranges)
	{
		if (string.IsNullOrEmpty(data))
		{
			ranges = Array.Empty<ushort>();
			return false;
		}
		string[] intsInHex = data.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		ranges = new ushort[intsInHex.Length];
		for (int i = 0; i < intsInHex.Length; i++)
		{
			try
			{
				ranges[i] = (ushort)Convert.ToInt32(intsInHex[i], 16);
			}
			catch (Exception)
			{
				return false;
			}
		}
		return ranges[^1] == 0;
	}

	internal static string GenerateRandomString()
	{
		Random random = new Random();
		return string.Join(' ', from _ in Enumerable.Range(0, random.Next(1, 4))
			select GetWord());
		char GetRandomCharacter()
		{
			return "qwertyuiopasdfghjklzxcvbnmeioadfc"[random.Next(0, "qwertyuiopasdfghjklzxcvbnmeioadfc".Length)];
		}
		string GetWord()
		{
			return char.ToUpperInvariant(GetRandomCharacter()) + new string((from _ in Enumerable.Range(0, random.Next(5, 10))
				select GetRandomCharacter()).ToArray());
		}
	}

	public static bool KeyUp(VK key)
	{
		if (Core.GHSettings.EnableControllerMode)
		{
			return false;
		}
		if (sendingMessage != null && !sendingMessage.IsCompleted)
		{
			return false;
		}
		if (DelayBetweenKeys.ElapsedMilliseconds >= Core.GHSettings.KeyPressTimeout + Rand.Next() % 10)
		{
			DelayBetweenKeys.Restart();
			if (Core.Process.Address != IntPtr.Zero)
			{
				sendingMessage = Task.Run(() => SendMessage(Core.Process.Information.MainWindowHandle, 257, (int)key, 0));
				return true;
			}
			return false;
		}
		return false;
	}

	public static void KillTCPConnectionForProcess(uint processId)
	{
		int afInet = 2;
		int buffSize = 0;
		GetExtendedTcpTable(IntPtr.Zero, ref buffSize, sort: true, afInet, TcpTableClass.TcpTableOwnerPidAll);
		nint buffTable = Marshal.AllocHGlobal(buffSize);
		MibTcprowOwnerPid[] table;
		try
		{
			if (GetExtendedTcpTable(buffTable, ref buffSize, sort: true, afInet, TcpTableClass.TcpTableOwnerPidAll) != 0)
			{
				return;
			}
			MibTcptableOwnerPid tab = (MibTcptableOwnerPid)Marshal.PtrToStructure(buffTable, typeof(MibTcptableOwnerPid));
			nint rowPtr = (nint)((long)buffTable + (long)Marshal.SizeOf(tab.DwNumEntries));
			table = new MibTcprowOwnerPid[tab.DwNumEntries];
			for (int i = 0; i < tab.DwNumEntries; i++)
			{
				rowPtr = (nint)((long)rowPtr + (long)Marshal.SizeOf(table[i] = (MibTcprowOwnerPid)Marshal.PtrToStructure(rowPtr, typeof(MibTcprowOwnerPid))));
			}
		}
		finally
		{
			Marshal.FreeHGlobal(buffTable);
		}
		MibTcprowOwnerPid pathConnection = table.FirstOrDefault((MibTcprowOwnerPid t) => t.OwningPid == processId);
		if (!EqualityComparer<MibTcprowOwnerPid>.Default.Equals(pathConnection, default(MibTcprowOwnerPid)))
		{
			pathConnection.State = 12u;
			nint ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(pathConnection));
			Marshal.StructureToPtr(pathConnection, ptr, fDeleteOld: false);
			SetTcpEntry(ptr);
			Marshal.FreeCoTaskMem(ptr);
		}
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern nint SendMessage(nint hWnd, int msg, int wParam, int lParam);

	[DllImport("iphlpapi.dll", SetLastError = true)]
	private static extern uint GetExtendedTcpTable(nint pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, TcpTableClass tblClass, uint reserved = 0u);

	[DllImport("iphlpapi.dll")]
	private static extern int SetTcpEntry(nint pTcprow);
}
