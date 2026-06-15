using NoiseUtils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapGenerator : MonoBehaviour
{
	public NoiseSettings_SO noiseSettingsSO;
	public GameObject prefabBloc;
	public Material blocMat;

	[SerializeField]
	private SerializableDictionary<Bloc_SO, float> biomesRange = new();

	[Min(0)] public Vector2Int sizeMap = new(128, 128);
	private Queue<Bloc> poolBlocs = new();
	private List<Bloc> map = new();
	private NoiseMap _noiseMap;
	private Transform activeBlocRoot;
	private Transform poolObjectBlocRoot;

	private void Awake()
	{
		ClearMapImmediate();
		StartCoroutine(RefreshMapRoutine());
	}
	private void OnEnable()
	{
		if (noiseSettingsSO != null)
		{
			noiseSettingsSO.OnSettingsChanged -= RefreshMap;
			noiseSettingsSO.OnSettingsChanged += RefreshMap;
		}
	}

	private void OnDisable()
	{
		if (noiseSettingsSO != null)
		{
			noiseSettingsSO.OnSettingsChanged -= RefreshMap;
		}
	}

	[Button]
	private void RefreshMap()
	{
		if (Application.isPlaying)
		{
			StopAllCoroutines();
			StartCoroutine(RefreshMapRoutine());
		}
		else
		{
			ClearMapImmediate();
			GenerateMap();
		}
	}

	private IEnumerator RefreshMapRoutine()
	{
		yield return new WaitForEndOfFrame();
		ClearMap();
		GenerateMap();
	}

	private void GenerateMap()
	{
		if (noiseSettingsSO == null || prefabBloc == null || blocMat == null) return;

		CheckAndCreateRoot(ref activeBlocRoot, "ActiveBlocs");
		NoiseGenerator noiseGenerator = new NoiseGenerator(noiseSettingsSO.noiseSettings);
		_noiseMap = noiseGenerator.GenerateNoiseMap((int)sizeMap.x, (int)sizeMap.y);

		float _offsetX = (sizeMap.x - 1) / 2f;
		float _offsetY = (sizeMap.y - 1) / 2f;

		for (int x = 0; x < _noiseMap.Width; x++)
		{
			for (int y = 0; y < _noiseMap.Height; y++)
			{
				float _value = _noiseMap[x, y];

				Bloc_SO _sharedBlocSO = GetBiomeSOFromValue01(_value);

				Bloc _bloc;
				if (poolBlocs.Count > 0)
				{
					_bloc = poolBlocs.Dequeue();
					_bloc.gameObject.SetActive(true);
				}
				else
				{
					GameObject _blocGO = Instantiate(prefabBloc, transform);
					_bloc = _blocGO.GetComponent<Bloc>();
				}
				_bloc.transform.SetParent(activeBlocRoot);
				_bloc.gameObject.name = $"Bloc_{x}_{y}";
				_bloc.gameObject.transform.SetLocalPositionAndRotation(new Vector3(x - _offsetX, 0, y - _offsetY), Quaternion.identity);

				_bloc.Initialize((uint)(x * sizeMap.y + y), _sharedBlocSO, blocMat);
				map.Add(_bloc);
			}
		}
	}

	private void CheckAndCreateRoot(ref Transform _transform, string _name)
	{
		if (_transform == null)
		{
			_transform = new GameObject(_name).transform;
			_transform.SetParent(transform);
			_transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		}
	}

	private void ClearMap()
	{
		if (map.Count <= 0) return;
		CheckAndCreateRoot(ref poolObjectBlocRoot, "PoolBlocs");
		map.ForEach(bloc =>
		{
			if (bloc != null)
			{
				bloc.gameObject.SetActive(false);
				poolBlocs.Enqueue(bloc);
				bloc.transform.SetParent(poolObjectBlocRoot);
			}
		});
		map.Clear();
	}

	//Uniquement en editor
	private void ClearMapImmediate()
	{
		if (map.Count > 0)
		{
			map.ForEach(bloc =>
			{
				if (bloc != null)
					DestroyImmediate(bloc.gameObject);
			});
			map.Clear();
		}

		ClearFolder(ref activeBlocRoot, "ActiveBlocs");
		ClearFolder(ref poolObjectBlocRoot, "PoolBlocs");

		poolBlocs.Clear();
	}

	private void ClearFolder(ref Transform _folder, string _name)
	{
		if (_folder != null)
		{
			DestroyImmediate(_folder.gameObject);
			_folder = null;
		}
		else
		{
			Transform oldPool = transform.Find(_name);
			if (oldPool != null) DestroyImmediate(oldPool.gameObject);
		}
	}

	private Bloc_SO GetBiomeSOFromValue01(float _value)
	{
		Bloc_SO _fallbackSO = null;

		foreach (KeyValuePair<Bloc_SO, float> _biomeZone in biomesRange)
		{
			_fallbackSO = _biomeZone.Key;

			if (_value <= _biomeZone.Value)
				return _biomeZone.Key;
		}

		return _fallbackSO;
	}
}