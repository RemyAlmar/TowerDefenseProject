using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
[CanEditMultipleObjects]
public class ButtonEditor : Editor
{
	private Dictionary<string, object> methodValues = new Dictionary<string, object>();

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		foreach (Object targetObject in targets)
		{
			MonoBehaviour mono = targetObject as MonoBehaviour;
			if (mono == null) continue;

			MethodInfo[] methods = mono.GetType()
									   .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

			foreach (MethodInfo method in methods)
			{
				ButtonAttribute attr = method.GetCustomAttribute<ButtonAttribute>();
				if (attr == null) continue;

				string buttonName = string.IsNullOrEmpty(attr.Name) ? method.Name : attr.Name;
				ParameterInfo[] parameters = method.GetParameters();

				object paramValue = null;

				if (parameters.Length == 1)
				{
					string key = mono.GetEntityId() + method.Name;
					if (!methodValues.ContainsKey(key))
						methodValues[key] = parameters[0].ParameterType == typeof(string) ? "" : 0f;

					System.Type paramType = parameters[0].ParameterType;

					if (paramType == typeof(float))
						methodValues[key] = EditorGUILayout.FloatField(buttonName, (float)methodValues[key]);
					else if (paramType == typeof(int))
						methodValues[key] = EditorGUILayout.IntField(buttonName, (int)methodValues[key]);
					else if (paramType == typeof(string))
						methodValues[key] = EditorGUILayout.TextField(buttonName, (string)methodValues[key]);
					else
						EditorGUILayout.LabelField(buttonName + " (unsupported type)");

					paramValue = methodValues[key];
				}
				else if (parameters.Length > 1)
				{
					EditorGUILayout.LabelField(buttonName + " (more than 1 parameter not supported)");
					continue;
				}
				else
				{
					GUILayout.Label(buttonName);
				}

				if (GUILayout.Button("▶ " + buttonName))
				{
					if (parameters.Length == 1)
						method.Invoke(mono, new object[] { paramValue });
					else
						method.Invoke(mono, null);
				}
			}
		}
	}
}
