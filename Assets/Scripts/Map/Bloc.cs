using UnityEngine;

public class Bloc : MonoBehaviour
{
	private uint id;
	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;

	public BlocSettings settings;
	public void Initialize(uint _uniqueId, BlocSettings _settings)
	{
		id = _uniqueId;
		settings = _settings;

		ApplyVisuals();
	}
	private void ApplyVisuals()
	{
		if (meshFilter && meshFilter.sharedMesh != settings.renderSettings.mesh) meshFilter.sharedMesh = settings.renderSettings.mesh;
		if (meshRenderer && meshRenderer.sharedMaterial != settings.renderSettings.material) meshRenderer.sharedMaterial = settings.renderSettings.material;

		if (meshRenderer != null && settings.renderSettings.color != meshRenderer.material.color)
			meshRenderer.material.color = settings.renderSettings.color;
	}

	public void SetDataSettings(BlocDataSettings _dataSettings)
	{
		if (settings.dataSettings.Equals(_dataSettings)) return;

		SetSettings(new BlocSettings(_dataSettings, settings.renderSettings));
	}

	public void SetRenderSettings(BlocRenderSettings _renderSettings)
	{
		if (settings.renderSettings.Equals(_renderSettings)) return;
		SetSettings(new BlocSettings(settings.dataSettings, _renderSettings));
	}

	public void SetSettings(BlocSettings _newSettings)
	{
		settings = _newSettings;
	}
}
