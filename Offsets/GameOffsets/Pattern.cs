using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GameOffsets;

public struct Pattern
{
	public readonly string Name;

	public readonly byte[] Data;

	public readonly bool[] Mask;

	public readonly int BytesToSkip;

	private static (byte[], bool[]) ParseArrayOfHexBytes(List<string> arrayOfHexBytes)
	{
		List<bool> list = new List<bool>();
		List<byte> list2 = new List<byte>();
		for (int i = 0; i < arrayOfHexBytes.Count; i++)
		{
			string text = arrayOfHexBytes[i];
			if (text.StartsWith("?"))
			{
				list2.Add(0);
				list.Add(item: false);
			}
			else
			{
				list2.Add(byte.Parse(text, NumberStyles.HexNumber));
				list.Add(item: true);
			}
		}
		return (list2.ToArray(), list.ToArray());
	}

	public Pattern(string name, string arrayOfHexBytes)
	{
		Name = name;
		List<string> list = arrayOfHexBytes.Split(new string[2] { " ", "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
		BytesToSkip = list.FindIndex("^".Equals);
		(Data, Mask) = ParseArrayOfHexBytes(list.Where((string hex) => hex != "^").ToList());
	}

	public Pattern(string name, string arrayOfHexBytes, int bytesToSkip)
	{
		Name = name;
		BytesToSkip = bytesToSkip;
		(Data, Mask) = ParseArrayOfHexBytes(arrayOfHexBytes.Split(new string[2] { " ", "," }, StringSplitOptions.RemoveEmptyEntries).ToList());
	}

	public override string ToString()
	{
		string text = "Name: " + Name + " Pattern: ";
		for (int i = 0; i < Data.Length; i++)
		{
			text = ((!Mask[i]) ? (text + "?? ") : (text + $"0x{Data[i]:X} "));
		}
		return text + $"BytesToSkip: {BytesToSkip}";
	}
}
