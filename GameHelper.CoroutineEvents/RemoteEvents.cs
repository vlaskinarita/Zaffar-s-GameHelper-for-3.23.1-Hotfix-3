using Coroutine;

namespace GameHelper.CoroutineEvents;

public static class RemoteEvents
{
	public static readonly Event AreaChanged = new Event();

	internal static readonly Event StateChanged = new Event();
}
