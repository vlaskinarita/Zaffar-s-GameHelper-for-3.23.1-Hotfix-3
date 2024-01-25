using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GameHelper.RemoteEnums.Entity;

[JsonConverter(typeof(StringEnumConverter))]
public enum EntityFilterType
{
	PATH,
	PATHANDRARITY,
	MOD,
	MODANDRARITY
}
