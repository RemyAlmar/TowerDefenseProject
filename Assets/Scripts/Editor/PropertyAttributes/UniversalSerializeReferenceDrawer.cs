using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(object), true)]
public class UniversalSerializeReferenceDrawer : PropertyDrawer
{
    static Dictionary<Type, List<Type>> cache = new();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }

        Type declaredType = GetDeclaredFieldType();
        List<Type> validTypes = GetAssignableTypes(declaredType);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        Rect typeRect = new(position.x, position.y, position.width, lineHeight);
        Rect contentRect = new Rect(position.x, position.y + lineHeight + 2, position.width, position.height - lineHeight - 2);

        EditorGUI.BeginProperty(position, label, property);

        string fullTypeName = property.managedReferenceFullTypename;
        string displayName = GetNameWithSpace(GetShortTypeName(fullTypeName)) ?? $"Select {GetNameWithSpace(declaredType.ToString())}";

        // Menu déroulant du type
        if (EditorGUI.DropdownButton(typeRect, new GUIContent(displayName), FocusType.Keyboard))
        {
            GenericMenu menu = new GenericMenu();

            if (validTypes.Count == 0)
                menu.AddDisabledItem(new GUIContent("No Serializable Types Found"));
            else
            {
                foreach (Type type in validTypes)
                {
                    string name = ObjectNames.NicifyVariableName(type.Name);
                    menu.AddItem(new GUIContent(name), fullTypeName == type.FullName, () =>
                    {
                        property.managedReferenceValue = Activator.CreateInstance(type);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
            }

            menu.ShowAsContext();
        }

        // Dessin compact des champs enfants
        if (property.managedReferenceValue != null)
        {
            EditorGUI.indentLevel++;

            SerializedProperty iterator = property.Copy();
            SerializedProperty end = iterator.GetEndProperty();

            iterator.NextVisible(true); // saute le champ racine
            float yOffset = contentRect.y;

            while (!SerializedProperty.EqualContents(iterator, end))
            {
                float h = EditorGUI.GetPropertyHeight(iterator, true);
                Rect r = new Rect(contentRect.x, yOffset, contentRect.width, h);
                EditorGUI.PropertyField(r, iterator, true);
                yOffset += h + 2; // pas d'espace supplémentaire
                iterator.NextVisible(false);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ManagedReference)
            return EditorGUI.GetPropertyHeight(property, label, true);

        Type declaredType = GetDeclaredFieldType();
        List<Type> validTypes = GetAssignableTypes(declaredType);

        float height = EditorGUIUtility.singleLineHeight; // dropdown
        if (property.managedReferenceValue != null)
        {
            SerializedProperty iterator = property.Copy();
            SerializedProperty end = iterator.GetEndProperty();
            iterator.NextVisible(true);

            while (!SerializedProperty.EqualContents(iterator, end))
            {
                height += EditorGUI.GetPropertyHeight(iterator, true) + 2;
                iterator.NextVisible(false);
            }
        }

        return height;
    }

    Type GetDeclaredFieldType()
    {
        Type type = fieldInfo.FieldType;

        if (type.IsArray) return type.GetElementType();
        if (type.IsGenericType) return type.GetGenericArguments()[0];

        return type;
    }

    List<Type> GetAssignableTypes(Type baseType)
    {
        if (cache.TryGetValue(baseType, out List<Type> list))
            return list;

        List<Type> allTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                Attribute.IsDefined(t, typeof(SerializableAttribute)) &&
                baseType.IsAssignableFrom(t) // filtre selon le type du champ
            )
            .ToList();

        cache[baseType] = allTypes;
        return allTypes;
    }

    static string GetShortTypeName(string fullTypeName)
    {
        if (string.IsNullOrEmpty(fullTypeName)) return null;
        string[] parts = fullTypeName.Split(' ');
        return parts.Length > 1 ? parts[1].Split('.').Last() : fullTypeName;
    }

    static string GetNameWithSpace(string _fullName)
    {
        if (string.IsNullOrEmpty(_fullName))
            return _fullName;
        // Utilisation de Regex : insère un espace avant une majuscule qui suit une lettre minuscule ou un chiffre
        return Regex.Replace(_fullName, "(?<!^)([A-Z])", " $1");
    }
}
