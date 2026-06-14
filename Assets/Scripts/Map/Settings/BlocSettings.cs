[System.Serializable]

public struct BlocSettings
{
	public BlocDataSettings dataSettings;
	public BlocRenderSettings renderSettings;
	public BlocSettings(BlocDataSettings _dataSettings, BlocRenderSettings _renderSettings)
	{
		dataSettings = _dataSettings;
		renderSettings = _renderSettings;
	}
}
