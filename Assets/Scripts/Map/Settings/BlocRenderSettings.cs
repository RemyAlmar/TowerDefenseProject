using UnityEngine;

[System.Serializable]
public struct BlocRenderSettings
{
	public Mesh mesh;
	public Material material;
	public Color color;

	public BlocRenderSettings(Mesh _mesh, Material _material, Color _color = default)
	{
		mesh = _mesh;
		material = _material;
		color = (_color == default) ? Color.white : _color;
	}
}
