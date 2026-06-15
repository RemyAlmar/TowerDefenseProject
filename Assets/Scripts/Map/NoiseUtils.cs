using System;
using UnityEngine;

namespace NoiseUtils
{
	public class NoiseMap
	{
		private readonly float[,] mapValues;

		public int Width { get; }
		public int Height { get; }

		public NoiseMap(int width, int height)
		{
			Width = width;
			Height = height;
			mapValues = new float[width, height];
		}

		public float this[int x, int y]
		{
			get
			{
				int clampedX = Mathf.Clamp(x, 0, Width - 1);
				int clampedY = Mathf.Clamp(y, 0, Height - 1);
				return mapValues[clampedX, clampedY];
			}
			set
			{
				if (x >= 0 && x < Width && y >= 0 && y < Height)
				{
					mapValues[x, y] = value;
				}
			}
		}
	}
	public class NoiseGenerator
	{
		private readonly NoiseSettings settings;
		private readonly Vector2[] octaveOffsets;

		public NoiseGenerator(NoiseSettings _settings, int _seed = 0)
		{
			settings = _settings;
			octaveOffsets = new Vector2[settings.Octaves];

			System.Random prng = new System.Random(_seed == 0 ? settings.Seed : _seed);
			for (int i = 0; i < settings.Octaves; i++)
			{
				float offsetX = prng.Next(-100000, 100000) + settings.Offset.x;
				float offsetY = prng.Next(-100000, 100000) + settings.Offset.y;
				octaveOffsets[i] = new Vector2(offsetX, offsetY);
			}
		}

		public float GetNoiseValue(Vector2 _position)
		{
			float _noiseValue = 0f;
			float _amplitude = settings.Amplitude > 0 ? settings.Amplitude : 1f;
			float _frequency = settings.Frequency > 0 ? settings.Frequency : 1f;

			for (int i = 0; i < settings.Octaves; i++)
			{
				float _sampleX = (_position.x + octaveOffsets[i].x) * _frequency / settings.Scale;
				float _sampleY = (_position.y + octaveOffsets[i].y) * _frequency / settings.Scale;

				float _perlinValue = Mathf.PerlinNoise(_sampleX, _sampleY) * 2 - 1;
				_noiseValue += _perlinValue * _amplitude;

				_amplitude *= settings.Persistence;
				_frequency *= settings.Lacunarity;
			}

			return _noiseValue;
		}

		public NoiseMap GenerateNoiseMap(int width, int height)
		{
			NoiseMap noiseMap = new NoiseMap(width, height);

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					noiseMap[x, y] = GetNoiseValue(new Vector2(x, y));
				}
			}

			return noiseMap;
		}
	}

	public static class NoiseTextureRenderer
	{
		public static void GenerateNoiseTexture(NoiseGenerator _generator, Renderer _renderer, ref Texture2D _texture, int _width = 256, int _height = 256)
		{
			if (_renderer == null || _generator == null)
			{
				Debug.LogError("Renderer ou Generator manquant. Impossible de générer la texture.");
				return;
			}

			if (_texture == null || _texture.width != _width || _texture.height != _height)
			{
				_texture = new Texture2D(_width, _height)
				{
					wrapMode = TextureWrapMode.Clamp,
					filterMode = FilterMode.Point
				};
				_renderer.sharedMaterial.mainTexture = _texture;
			}

			Color32[] _colorMap = new Color32[_width * _height];

			for (int y = 0; y < _height; y++)
			{
				for (int x = 0; x < _width; x++)
				{
					Vector2 _pos = new Vector2(x, y);

					float _noiseValue = _generator.GetNoiseValue(_pos);
					float _normalizedNoise = Mathf.Clamp01((_noiseValue + 1f) / 2f);
					_colorMap[y * _width + x] = new Color(_normalizedNoise, _normalizedNoise, _normalizedNoise);
				}
			}

			_texture.SetPixels32(_colorMap);
			_texture.Apply();
		}
	}
	[Serializable]
	public struct NoiseSettings
	{
		public int Seed;
		[Min(0.0001f)] public float Scale;
		[Range(1, 20)] public int Octaves;
		[Min(0)] public float Amplitude;
		[Min(0)] public float Frequency;
		[Min(0)] public float Persistence;
		[Min(0)] public float Lacunarity;
		public Vector2 Offset;

		public NoiseSettings(int seed, float scale, int octaves, float amplitude, float frequency, float persistence, float lacunarity, Vector2 offset)
		{
			if (scale <= 0f) scale = 0.0001f;
			if (octaves < 1) octaves = 1;

			Seed = seed;
			Scale = scale;
			Octaves = octaves;
			Amplitude = amplitude;
			Frequency = frequency;
			Persistence = persistence;
			Lacunarity = lacunarity;
			Offset = offset;
		}
	}
}