using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
public class SerializableDictionaryDrawer : PropertyDrawer
{
    private Dictionary<string, ReorderableList> lists = new Dictionary<string, ReorderableList>();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ReorderableList list = GetList(property);
        return property.isExpanded ? list.GetHeight() + EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            label,
            true
        );

        if (!property.isExpanded) return;

        ReorderableList list = GetList(property);
        position.y += EditorGUIUtility.singleLineHeight + 2;
        list.DoList(position);
    }

    private ReorderableList GetList(SerializedProperty property)
    {
        string key = property.propertyPath;
        if (lists.TryGetValue(key, out ReorderableList existing)) return existing;

        SerializedProperty keysProp = property.FindPropertyRelative("keys");
        SerializedProperty valuesProp = property.FindPropertyRelative("values");

        ReorderableList list = new(property.serializedObject, keysProp, true, true, true, true)
        {
            // --- HEADER
            drawHeaderCallback = rect =>
                {
                    GUIStyle _style = GUI.skin.GetStyle("Label");
                    _style.alignment = TextAnchor.MiddleCenter;
                    float half = rect.width * .5f;
                    float quart = half * .5f;
                    float space = 5f;
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, quart, rect.height), "Key", _style);
                    EditorGUI.LabelField(new Rect(rect.x + quart + space, rect.y, half + quart, rect.height), "Value", _style);
                },

            // --- ELEMENTS
            drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (index >= keysProp.arraySize || index >= valuesProp.arraySize) return;

                    SerializedProperty keyProp = keysProp.GetArrayElementAtIndex(index);
                    SerializedProperty valProp = valuesProp.GetArrayElementAtIndex(index);

                    float half = rect.width * .5f;
                    float quart = half * .5f;
                    float space = 5f;
                    Rect keyRect = new Rect(rect.x, rect.y, quart, EditorGUIUtility.singleLineHeight);
                    Rect valRect = new Rect(rect.x + quart + space, rect.y, half + quart, EditorGUIUtility.singleLineHeight);

                    if (HasDuplicateKey(keysProp, index))
                        EditorGUI.DrawRect(rect, new Color(1f, 0.3f, 0.3f, 0.15f));

                    EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none, true);
                    EditorGUI.PropertyField(valRect, valProp, GUIContent.none, true);
                },

            // --- DYNAMIC HEIGHT
            elementHeightCallback = index =>
                {
                    if (index >= keysProp.arraySize) return EditorGUIUtility.singleLineHeight;
                    SerializedProperty keyProp = keysProp.GetArrayElementAtIndex(index);
                    SerializedProperty valProp = valuesProp.GetArrayElementAtIndex(index);
                    float keyHeight = EditorGUI.GetPropertyHeight(keyProp, true);
                    float valHeight = EditorGUI.GetPropertyHeight(valProp, true);
                    return Mathf.Max(keyHeight, valHeight);
                },

            // --- ADD
            onAddCallback = l =>
                {
                    int i = keysProp.arraySize;
                    keysProp.InsertArrayElementAtIndex(i);
                    valuesProp.InsertArrayElementAtIndex(i);

                    l.index = l.index < 0 || l.index >= i - 1 ? i : Mathf.Clamp(l.index, 0, l.count - 1); //Clamp l'index pour le garder à la fin si dernier selectionné sinon celui actuel

                    SerializedProperty keyProp = keysProp.GetArrayElementAtIndex(i);
                    SerializedProperty valProp = valuesProp.GetArrayElementAtIndex(i);

                    SerializedProperty keyPropCopy = keysProp.GetArrayElementAtIndex(l.index);
                    SerializedProperty valPropCopy = valuesProp.GetArrayElementAtIndex(l.index);

                    ResetSerializedProperty(keyProp, keyPropCopy);
                    ResetSerializedProperty(valProp, valPropCopy);
                    // 🔑 amélioration : si c’est un enum ou un int → prendre une valeur suivante par défaut
                    if (keyProp.propertyType == SerializedPropertyType.Enum)
                        keyProp.enumValueIndex = Mathf.Clamp(i, 0, keyProp.enumDisplayNames.Length - 1);
                    else if (keyProp.propertyType == SerializedPropertyType.Integer)
                        keyProp.intValue = i;

                    property.serializedObject.ApplyModifiedProperties();
                },

            // --- REMOVE
            onRemoveCallback = l =>
                {
                    if (l.index >= 0 && l.index < keysProp.arraySize)
                    {
                        int _index = l.index;
                        keysProp.DeleteArrayElementAtIndex(_index);
                        valuesProp.DeleteArrayElementAtIndex(_index);
                        l.index = Mathf.Clamp(l.index, 0, l.count - 1);
                        property.serializedObject.ApplyModifiedProperties();
                    }
                },

            // --- REORDER (fix déplacement clé/valeur)
            onReorderCallbackWithDetails = (l, oldIndex, newIndex) =>
                {
                    valuesProp.MoveArrayElement(oldIndex, newIndex);
                    property.serializedObject.ApplyModifiedProperties();
                }
        };

        lists[key] = list;
        return list;
    }

    private bool HasDuplicateKey(SerializedProperty keysProp, int index)
    {
        SerializedProperty target = keysProp.GetArrayElementAtIndex(index);
        for (int i = 0; i < keysProp.arraySize; i++)
        {
            if (i == index) continue;
            if (SerializedPropertyEqual(target, keysProp.GetArrayElementAtIndex(i))) return true;
        }
        return false;
    }

    private bool SerializedPropertyEqual(SerializedProperty a, SerializedProperty b)
    {
        if (a.propertyType != b.propertyType) return false;
        switch (a.propertyType)
        {
            case SerializedPropertyType.Integer: return a.intValue == b.intValue;
            case SerializedPropertyType.Boolean: return a.boolValue == b.boolValue;
            case SerializedPropertyType.Float: return Mathf.Approximately(a.floatValue, b.floatValue);
            case SerializedPropertyType.String: return a.stringValue == b.stringValue;
            case SerializedPropertyType.Enum: return a.enumValueIndex == b.enumValueIndex;
            case SerializedPropertyType.ObjectReference: return a.objectReferenceValue == b.objectReferenceValue;

            default: return false;
        }
    }
    private void ResetSerializedProperty(SerializedProperty prop, SerializedProperty propValue = null)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Integer: prop.intValue = propValue != null ? propValue.intValue : 0; break;
            case SerializedPropertyType.Boolean: prop.boolValue = propValue != null && propValue.boolValue; break;
            case SerializedPropertyType.Float: prop.floatValue = propValue != null ? propValue.floatValue : 0f; break;
            case SerializedPropertyType.String: prop.stringValue = propValue != null ? propValue.stringValue : string.Empty; break;
            case SerializedPropertyType.Enum: prop.enumValueIndex = propValue != null ? propValue.enumValueIndex : 0; break;
            case SerializedPropertyType.ObjectReference: prop.objectReferenceValue = propValue != null ? propValue.objectReferenceValue : default; break;

            default:
                {
                    Debug.Log("It's a Default");
                    prop.boxedValue = propValue != null ? propValue.boxedValue : default; break;
                }
        }
    }
}
