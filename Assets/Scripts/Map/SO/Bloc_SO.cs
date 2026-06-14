using UnityEngine;

[CreateAssetMenu(fileName = "Bloc_SO", menuName = "Scriptable Objects/Map/Bloc_SO")]
public class Bloc_SO : ScriptableObject
{
	public BlocSettings settings = new(
		new(BiomeType.Plain, CrossingType.Walk, true, true),
		new(null, null, Color.white)
	);
}