using System;

namespace GameHelper.RemoteEnums.Entity;

[Flags]
public enum NearbyZones
{
	None = 0,
	InnerCircle = 1,
	OuterCircle = 2
}
