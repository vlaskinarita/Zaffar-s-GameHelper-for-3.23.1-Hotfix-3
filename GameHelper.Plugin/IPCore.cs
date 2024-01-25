namespace GameHelper.Plugin;

internal interface IPCore
{
	void SetPluginDllLocation(string dllLocation);

	void OnEnable(bool isGameOpened);

	void OnDisable();

	void DrawSettings();

	void DrawUI();

	void SaveSettings();
}
