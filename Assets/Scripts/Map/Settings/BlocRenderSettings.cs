using UnityEngine;

[System.Serializable]
public struct BlocRenderSettings
{
	public Mesh mesh;
	public Color color;

	public BlocRenderSettings(Mesh _mesh, Color _color = default)
	{
		mesh = _mesh;
		color = (_color == default) ? Color.white : _color;
	}
}
