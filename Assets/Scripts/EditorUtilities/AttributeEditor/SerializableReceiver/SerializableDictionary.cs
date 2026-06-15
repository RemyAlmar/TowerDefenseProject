using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable, Tooltip("Attention, n'utiliser que des type natifs et pas de liste, oui je sais c'est pas ouf mais trop de temps perdu là dessus")]
public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TKey, TValue>>
{
	[SerializeReference] private List<TKey> keys = new List<TKey>();
	[SerializeReference] private List<TValue> values = new List<TValue>();

	private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

	public Dictionary<TKey, TValue> Dictionary => dictionary;

	public void OnBeforeSerialize()
	{
		//keys.Clear();
		//values.Clear();
		//foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
		//{
		//    keys.Add(kvp.Key);
		//    values.Add(kvp.Value);
		//}
		//Debug.Log("Serialize");
	}

	public void OnAfterDeserialize()
	{
		dictionary = new Dictionary<TKey, TValue>();
		int count = Math.Min(keys.Count, values.Count);
		for (int i = 0; i < count; i++)
		{
			if (!dictionary.ContainsKey(keys[i]))
			{
				dictionary.Add(keys[i], values[i]);
			}
		}
	}

	public void Add(TKey key, TValue value)
	{
		if (dictionary.ContainsKey(key))
			dictionary[key] = value;
		else
			dictionary.TryAdd(key, value);

		ReorderKeyValuePair();
	}
	public void Remove(TKey key)
	{
		if (dictionary.Remove(key))
			ReorderKeyValuePair();
	}
	public bool TryGetValue(TKey key, out TValue value) => dictionary.TryGetValue(key, out value);
	public TValue this[TKey key] { get => dictionary[key]; set => dictionary[key] = value; }
	public void Clear() { dictionary.Clear(); ReorderKeyValuePair(); }
	public int Count => dictionary.Count;
	public int FindIndexFromKey(TKey _key) => keys.FindIndex(k => EqualityComparer<TKey>.Default.Equals(k, _key));
	public List<TKey> Keys => keys;
	public List<TValue> Values => values;

	private void ReorderKeyValuePair()
	{
		keys?.Clear();
		values?.Clear();
		if (dictionary != null && dictionary.Count > 0)
			foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
			{
				keys.Add(kvp.Key);
				values.Add(kvp.Value);
			}
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return dictionary.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}

