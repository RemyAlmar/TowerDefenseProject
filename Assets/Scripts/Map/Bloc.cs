using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class Bloc : MonoBehaviour
{
	public Bloc_SO settingsSO;
	private uint id;
	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;

	private BlocSettings settings;
	private MaterialPropertyBlock propBlock;

	public void OnEnable()
	{
		if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
		if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();

		if (settingsSO != null)
		{
			settings = settingsSO.settings;
			settingsSO.OnSettingsChanged -= UpdateSettings;
			settingsSO.OnSettingsChanged += UpdateSettings;
		}
	}

	public void OnDisable()
	{
		if (settingsSO != null)
			settingsSO.OnSettingsChanged -= UpdateSettings;
	}

	public void Initialize(uint _uniqueId, Bloc_SO _settingsSO, Material _material = null)
	{
		id = _uniqueId;

		if (settingsSO != null && settingsSO != _settingsSO)
			settingsSO.OnSettingsChanged -= UpdateSettings;

		settingsSO = _settingsSO;

		if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
		if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();

		if (_material && meshRenderer)
			meshRenderer.sharedMaterial = _material;

		if (settingsSO != null)
		{
			settingsSO.OnSettingsChanged -= UpdateSettings;
			settingsSO.OnSettingsChanged += UpdateSettings;
			SetSettings(settingsSO.settings);
		}
	}

	private void ApplyVisuals()
	{
		if (meshRenderer == null || meshFilter == null) return;

		if (meshFilter.sharedMesh != settings.renderSettings.mesh && settings.renderSettings.mesh)
			meshFilter.sharedMesh = settings.renderSettings.mesh;

		propBlock ??= new MaterialPropertyBlock();
		meshRenderer.GetPropertyBlock(propBlock);

		propBlock.SetColor("_BaseColor", settings.renderSettings.color);
		meshRenderer.SetPropertyBlock(propBlock);
	}

	public void SetSettings(BlocSettings newSettings)
	{
		settings = newSettings;
		ApplyVisuals();
	}

	private void UpdateSettings()
	{
		if (settingsSO != null)
		{
			SetSettings(settingsSO.settings);
		}
	}
#if UNITY_EDITOR
	private void OnValidate()
	{
		if (settingsSO != null)
		{
			if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
			Initialize(id, settingsSO, meshRenderer ? meshRenderer.sharedMaterial : null);
		}
	}
#endif
}