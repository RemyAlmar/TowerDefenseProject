using NoiseUtils;
using System;
using UnityEngine;
[CreateAssetMenu(fileName = "NoiseSettings_SO", menuName = "Scriptable Objects/Map/NoiseSettings_SO")]
public class NoiseSettings_SO : ScriptableObject
{
	public NoiseSettings noiseSettings = new();
	public event Action OnSettingsChanged;

	private void OnValidate()
	{
		OnSettingsChanged?.Invoke();
	}
}
