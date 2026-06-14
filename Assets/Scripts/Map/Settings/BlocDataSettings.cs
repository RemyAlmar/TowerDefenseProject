using UnityEngine;

[System.Serializable]
public struct BlocDataSettings
{
	[SerializeField] private CrossingType crossType;

	public BiomeType biomeType;
	public bool isEmpty;
	public bool isBuildable;

	public CrossingType CrossType
	{
		readonly get => crossType;
		set => crossType = HandleCrossTypeLogic(value);
	}

	private readonly CrossingType HandleCrossTypeLogic(CrossingType _value)
	{
		if (_value == CrossingType.None) return CrossingType.None;

		if ((_value & CrossingType.Any) == CrossingType.Any) return CrossingType.Any;

		return _value;
	}

	public BlocDataSettings(BiomeType _biomeType = BiomeType.Plain, CrossingType _crossType = CrossingType.Any, bool _isEmpty = true, bool _isBuildable = true)
	{
		biomeType = _biomeType;
		crossType = _crossType;
		isEmpty = _isEmpty;
		isBuildable = _isBuildable;
	}

}
