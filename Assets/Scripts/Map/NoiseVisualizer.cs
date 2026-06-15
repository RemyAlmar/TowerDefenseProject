using NoiseUtils;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class NoiseVisualizer : MonoBehaviour
{
	private new Renderer renderer;
	private Texture2D noiseTexture;
	[SerializeField] private NoiseSettings_SO noiseSettingsSO;
	[SerializeField] private bool updateIRL;

	void Awake()
	{
		renderer = GetComponent<Renderer>();
		GenerateMap();
	}

	void Update()
	{
		if (updateIRL)
			GenerateMap();
	}

	private void GenerateMap()
	{
		if (renderer == null) renderer = GetComponent<Renderer>();
		if (noiseSettingsSO == null || renderer == null) return;

		NoiseGenerator generator = new(noiseSettingsSO.noiseSettings);
		NoiseTextureRenderer.GenerateNoiseTexture(generator, renderer, ref noiseTexture);
	}

	private void OnEnable()
	{
		if (noiseSettingsSO != null)
		{
			noiseSettingsSO.OnSettingsChanged -= GenerateMap;
			noiseSettingsSO.OnSettingsChanged += GenerateMap;
		}
	}

	private void OnDisable()
	{
		if (noiseSettingsSO != null)
		{
			noiseSettingsSO.OnSettingsChanged -= GenerateMap;
		}
	}

	public void OnValidate()
	{
		if (noiseSettingsSO == null)
		{
			Debug.LogWarning("Insérer un NoiseSettings_SO dans le script NoiseVisualizer pour générer le bruit.");
			return;
		}

		OnEnable();
		GenerateMap();
	}
}