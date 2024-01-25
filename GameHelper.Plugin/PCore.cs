namespace GameHelper.Plugin;

public abstract class PCore<TSettings> : IPCore where TSettings : IPSettings, new()
{
	public string DllDirectory;

	public TSettings Settings = new TSettings();

	public abstract void OnDisable();

	public abstract void OnEnable(bool isGameOpened);

	public abstract void DrawSettings();

	public abstract void DrawUI();

	public abstract void SaveSettings();

	public void SetPluginDllLocation(string dllLocation)
	{
		DllDirectory = dllLocation;
	}
}
