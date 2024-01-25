using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameOffsets;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GameProcessDetails
{
	public static readonly Dictionary<string, string> ProcessName = new Dictionary<string, string>
	{
		{
			"PathOfExile",
			"Path of Exile".ToLower()
		},
		{
			"PathOfExile_KG",
			"Path of Exile".ToLower()
		},
		{
			"PathOfExileSteam",
			"Path of Exile".ToLower()
		},
		{
			"PathOfExile_x64",
			"Path of Exile".ToLower()
		}
	};

	public static readonly List<string> Contributors = new List<string> { "Dax***", "Scrippydoo", "Riyu", "Noneyatemp", "hienngocloveyou" };
}
