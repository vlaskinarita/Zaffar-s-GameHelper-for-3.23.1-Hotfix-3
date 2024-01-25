using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameOffsets.Natives;
using Microsoft.Win32.SafeHandles;
using ProcessMemoryUtilities.Managed;
using ProcessMemoryUtilities.Native;

namespace GameHelper.Utils;

internal class SafeMemoryHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	internal SafeMemoryHandle()
		: base(ownsHandle: true)
	{
		Console.WriteLine("Opening a new handle.");
	}

	internal SafeMemoryHandle(int processId)
		: base(ownsHandle: true)
	{
		nint handle = NativeWrapper.OpenProcess(ProcessAccessFlags.Read, processId);
		if (NativeWrapper.HasError)
		{
			Console.WriteLine($"Failed to open a new handle 0x{handle:X} due to ErrorNo: {NativeWrapper.LastError}");
		}
		else
		{
			Console.WriteLine($"Opened a new handle using IntPtr 0x{handle:X}");
		}
		SetHandle(handle);
	}

	internal T ReadMemory<T>(nint address) where T : unmanaged
	{
		T result = default(T);
		if (IsInvalid || ((IntPtr)address).ToInt64() <= 0)
		{
			return result;
		}
		try
		{
			if (!NativeWrapper.ReadProcessMemory(handle, address, ref result))
			{
				throw new Exception("Failed To Read the Memory (T)" + $" due to Error Number: 0x{NativeWrapper.LastError:X} on " + $"adress 0x{((IntPtr)address).ToInt64():X}");
			}
			return result;
		}
		catch (Exception e)
		{
			Console.WriteLine("ERROR: " + e.Message);
			return default(T);
		}
	}

	internal T[] ReadStdVector<T>(StdVector nativeContainer) where T : unmanaged
	{
		int typeSize = Marshal.SizeOf<T>();
		long length = ((IntPtr)nativeContainer.Last).ToInt64() - ((IntPtr)nativeContainer.First).ToInt64();
		if (length <= 0 || length % typeSize != 0L)
		{
			return Array.Empty<T>();
		}
		return ReadMemoryArray<T>(nativeContainer.First, (int)length / typeSize);
	}

	internal T[] ReadMemoryArray<T>(nint address, int nsize) where T : unmanaged
	{
		if (IsInvalid || ((IntPtr)address).ToInt64() <= 0 || nsize <= 0)
		{
			return Array.Empty<T>();
		}
		T[] buffer = new T[nsize];
		try
		{
			if (!NativeWrapper.ReadProcessMemoryArray(handle, address, buffer, out var numBytesRead))
			{
				throw new Exception("Failed To Read the Memory (array)" + $" due to Error Number: 0x{NativeWrapper.LastError:X}" + $" on address 0x{((IntPtr)address).ToInt64():X} with size {nsize}");
			}
			if (((IntPtr)numBytesRead).ToInt32() < nsize)
			{
				throw new Exception($"Number of bytes read {((IntPtr)numBytesRead).ToInt32()} is less than the passed nsize {nsize} on address 0x{((IntPtr)address).ToInt64():X}.");
			}
			return buffer;
		}
		catch (Exception e)
		{
			Console.WriteLine("ERROR: " + e.Message);
			return Array.Empty<T>();
		}
	}

	internal string ReadStdWString(StdWString nativecontainer)
	{
		if (nativecontainer.Length <= 0 || nativecontainer.Length > 1000 || nativecontainer.Capacity <= 0 || nativecontainer.Capacity > 1000)
		{
			return string.Empty;
		}
		if (nativecontainer.Capacity <= 8)
		{
			byte[] buffer = BitConverter.GetBytes(((IntPtr)nativecontainer.Buffer).ToInt64());
			string @string = Encoding.Unicode.GetString(buffer);
			buffer = BitConverter.GetBytes(((IntPtr)nativecontainer.ReservedBytes).ToInt64());
			return (@string + Encoding.Unicode.GetString(buffer)).Substring(0, nativecontainer.Length);
		}
		byte[] buffer2 = ReadMemoryArray<byte>(nativecontainer.Buffer, nativecontainer.Length * 2);
		return Encoding.Unicode.GetString(buffer2);
	}

	internal string ReadString(nint address)
	{
		byte[] buffer = ReadMemoryArray<byte>(address, 128);
		int count = Array.IndexOf(buffer, (byte)0, 0);
		if (count > 0)
		{
			return Encoding.ASCII.GetString(buffer, 0, count);
		}
		return string.Empty;
	}

	internal string ReadUnicodeString(nint address)
	{
		byte[] buffer = ReadMemoryArray<byte>(address, 256);
		int count = 0;
		for (int i = 0; i < buffer.Length - 2; i++)
		{
			if (buffer[i] == 0 && buffer[i + 1] == 0 && buffer[i + 2] == 0)
			{
				count = ((i % 2 == 0) ? i : (i + 1));
				break;
			}
		}
		if (count == 0)
		{
			return string.Empty;
		}
		return Encoding.Unicode.GetString(buffer, 0, count);
	}

	internal int ReadStdMap<TKey, TValue>(StdMap nativeContainer, int maxSizeAllowed, bool enableCounting, Func<TKey, TValue, bool> onEachNotNullNode) where TKey : unmanaged where TValue : unmanaged
	{
		int maxPartitions = Core.GHSettings.EntitiesToReadBeforeGoingParallel;
		if (nativeContainer.Size <= 0 || nativeContainer.Size > maxSizeAllowed)
		{
			return 0;
		}
		StdMapNode<TKey, TValue> parent = ReadMemory<StdMapNode<TKey, TValue>>(ReadMemory<StdMapNode<TKey, TValue>>(nativeContainer.Head).Parent);
		Queue<StdMapNode<TKey, TValue>> first32Childrens = new Queue<StdMapNode<TKey, TValue>>();
		int totalChildrenProcessed = processSubTree(first32Childrens, parent, maxPartitions);
		Parallel.ForEach(first32Childrens, () => (new Queue<StdMapNode<TKey, TValue>>(2000), 0), delegate(StdMapNode<TKey, TValue> first32Child, ParallelLoopState _, long _, (Queue<StdMapNode<TKey, TValue>>, int) localState)
		{
			localState.Item2 += processSubTree(localState.Item1, first32Child, maxSizeAllowed / first32Childrens.Count);
			return localState;
		}, delegate((Queue<StdMapNode<TKey, TValue>>, int) localFinal)
		{
			if (enableCounting)
			{
				Interlocked.Add(ref totalChildrenProcessed, localFinal.Item2);
			}
		});
		return totalChildrenProcessed;
		void processNode(Queue<StdMapNode<TKey, TValue>> childrens, StdMapNode<TKey, TValue> current)
		{
			if (!current.IsNil)
			{
				onEachNotNullNode(current.Data.Key, current.Data.Value);
			}
			StdMapNode<TKey, TValue> leftChild = ReadMemory<StdMapNode<TKey, TValue>>(current.Left);
			if (!leftChild.IsNil)
			{
				childrens.Enqueue(leftChild);
			}
			StdMapNode<TKey, TValue> rightChild = ReadMemory<StdMapNode<TKey, TValue>>(current.Right);
			if (!rightChild.IsNil)
			{
				childrens.Enqueue(rightChild);
			}
		}
		int processSubTree(Queue<StdMapNode<TKey, TValue>> childrens, StdMapNode<TKey, TValue> subTreeRoot, int forceBreakOnIteration)
		{
			childrens.Enqueue(subTreeRoot);
			int counter = 0;
			StdMapNode<TKey, TValue> current2;
			while (++counter < forceBreakOnIteration && childrens.TryDequeue(out current2))
			{
				processNode(childrens, current2);
			}
			return counter;
		}
	}

	internal List<TValue> ReadStdList<TValue>(StdList nativeContainer) where TValue : unmanaged
	{
		List<TValue> retList = new List<TValue>();
		nint currNodeAddress = ReadMemory<StdListNode>(nativeContainer.Head).Next;
		while (currNodeAddress != nativeContainer.Head)
		{
			StdListNode<TValue> currNode = ReadMemory<StdListNode<TValue>>(currNodeAddress);
			if (currNodeAddress == IntPtr.Zero)
			{
				Console.WriteLine("Terminating reading of list next nodes because ofunexpected 0x00 found. This is normal if it happens after closing the game, otherwise report it.");
				break;
			}
			retList.Add(currNode.Data);
			currNodeAddress = currNode.Next;
		}
		return retList;
	}

	internal List<TValue> ReadStdBucket<TValue>(StdBucket nativeContainer) where TValue : unmanaged
	{
		if (nativeContainer.Data == IntPtr.Zero || nativeContainer.Capacity <= 0)
		{
			return new List<TValue>();
		}
		int size = ((int)nativeContainer.Capacity + 1) / 8;
		List<TValue> ret = new List<TValue>();
		StdBucketNode<TValue>[] dataArray = ReadMemoryArray<StdBucketNode<TValue>>(nativeContainer.Data, size);
		for (int i = 0; i < dataArray.Length; i++)
		{
			StdBucketNode<TValue> data = dataArray[i];
			if (data.Flag0 != StdBucketNode<TValue>.InValidPointerFlagValue)
			{
				ret.Add(data.Pointer0);
			}
			if (data.Flag1 != StdBucketNode<TValue>.InValidPointerFlagValue)
			{
				ret.Add(data.Pointer1);
			}
			if (data.Flag2 != StdBucketNode<TValue>.InValidPointerFlagValue)
			{
				ret.Add(data.Pointer2);
			}
			if (data.Flag3 != StdBucketNode<TValue>.InValidPointerFlagValue)
			{
				ret.Add(data.Pointer3);
			}
			if (data.Flag4 != StdBucketNode<TValue>.InValidPointerFlagValue)
			{
				ret.Add(data.Pointer4);
			}
			if (data.Flag5 != StdBucketNode<TValue>.InValidPointerFlagValue)
			{
				ret.Add(data.Pointer5);
			}
			if (data.Flag6 != StdBucketNode<TValue>.InValidPointerFlagValue)
			{
				ret.Add(data.Pointer6);
			}
			if (data.Flag7 != StdBucketNode<TValue>.InValidPointerFlagValue)
			{
				ret.Add(data.Pointer7);
			}
		}
		return ret;
	}

	protected override bool ReleaseHandle()
	{
		Console.WriteLine($"Releasing handle on 0x{handle:X}\n");
		return NativeWrapper.CloseHandle(handle);
	}
}
