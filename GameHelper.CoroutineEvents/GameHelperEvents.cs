using Coroutine;

namespace GameHelper.CoroutineEvents;

public static class GameHelperEvents
{
	public static readonly Event OnMoved = new Event();

	public static readonly Event OnForegroundChanged = new Event();

	public static readonly Event OnClose = new Event();

	internal static readonly Event PerFrameDataUpdate = new Event();

	internal static readonly Event OnRender = new Event();

	internal static readonly Event OnOpened = new Event();

	internal static readonly Event TimeToSaveAllSettings = new Event();

	internal static Event PostPerFrameDataUpdate = new Event();
}
