using System.IO;
using Newtonsoft.Json;

namespace GameHelper.Utils;

internal static class JsonHelper
{
	public static T CreateOrLoadJsonFile<T>(FileInfo file) where T : new()
	{
		file.Refresh();
		file.Directory.Create();
		if (file.Exists)
		{
			return JsonConvert.DeserializeObject<T>(File.ReadAllText(file.FullName));
		}
		T val = new T();
		SafeToFile(val, file);
		return val;
	}

	public static void SafeToFile(object classObject, FileInfo file)
	{
		string content = JsonConvert.SerializeObject(classObject, Formatting.Indented);
		File.WriteAllText(file.FullName, content);
	}
}
