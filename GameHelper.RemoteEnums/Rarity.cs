using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GameHelper.RemoteEnums;

[JsonConverter(typeof(StringEnumConverter))]
public enum Rarity
{
	Normal,
	Magic,
	Rare,
	Unique
}
