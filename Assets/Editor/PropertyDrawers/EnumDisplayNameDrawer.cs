using System;
using System.Collections.Generic;
using System.Reflection;
using FourFatesStudios.ProjectWarden.Attributes;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Enum), true)]
public class EnumDisplayNameDrawer : PropertyDrawer
{
    private Dictionary<string, string[]> _displayNameCache = new();
    private Dictionary<string, Array> _enumValueCache = new();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Enum)
        {
            EditorGUI.LabelField(position, label.text, "Use EnumDisplayName with enum.");
            return;
        }

        var enumType = fieldInfo.FieldType;
        var key = enumType.FullName;

        if (!_displayNameCache.TryGetValue(key, out var displayNames))
        {
            var names = Enum.GetNames(enumType);
            var displayNameList = new List<string>();

            foreach (var name in names)
            {
                var member = enumType.GetMember(name)[0];
                var attr = member.GetCustomAttribute<EnumDisplayNameAttribute>();
                displayNameList.Add(attr?.DisplayName ?? name);
            }

            displayNames = displayNameList.ToArray();
            _displayNameCache[key] = displayNames;
        }

        property.enumValueIndex = EditorGUI.Popup(position, label.text, property.enumValueIndex, displayNames);
    }
}