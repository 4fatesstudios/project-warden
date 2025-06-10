using System;
using System.Collections.Generic;
using System.Reflection;
using FourFatesStudios.ProjectWarden.Enums;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Stat))]
public class StatDrawer : PropertyDrawer
{
    private string[] _displayNames;
    private Array _values;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (_displayNames == null)
        {
            var type = fieldInfo.FieldType;
            _values = Enum.GetValues(type);
            var names = Enum.GetNames(type);
            var displayNames = new List<string>();

            for (int i = 0; i < names.Length; i++)
            {
                var member = type.GetMember(names[i])[0];
                var attr = member.GetCustomAttribute<EnumDisplayNameAttribute>();
                displayNames.Add(attr != null ? attr.DisplayName : names[i]);
            }

            _displayNames = displayNames.ToArray();
        }

        property.enumValueIndex = EditorGUI.Popup(position, property.displayName, property.enumValueIndex, _displayNames);
    }
}