using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using FourFatesStudios.ProjectWarden.Effects;

[CustomPropertyDrawer(typeof(EffectBundle))]
public class EffectBundleDrawer : PropertyDrawer
{
    private List<Type> _effectTypes;
    private string[] _effectTypeNames;
    private int _selectedIndex = -1;

    private void Init()
    {
        if (_effectTypes != null) return;

        _effectTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IEffect).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
            .OrderBy(t => t.Name)
            .ToList();

        _effectTypeNames = _effectTypes.Select(t => t.Name).ToArray();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Init();

        SerializedProperty effectsProp = property.FindPropertyRelative("Effects");
        float lineHeight = EditorGUIUtility.singleLineHeight + 4;
        float y = position.y;

        property.isExpanded = EditorGUI.Foldout(new Rect(position.x, y, position.width, lineHeight), property.isExpanded, label, true);
        y += lineHeight;

        if (!property.isExpanded) return;

        // Draw existing effects
        for (int i = 0; i < effectsProp.arraySize; i++)
        {
            SerializedProperty effectProp = effectsProp.GetArrayElementAtIndex(i);
            Rect boxRect = new Rect(position.x, y, position.width - 60, lineHeight);
            EditorGUI.PropertyField(boxRect, effectProp, GUIContent.none, true);

            if (GUI.Button(new Rect(position.x + position.width - 80, y, 75, lineHeight), "Remove"))
            {
                effectsProp.DeleteArrayElementAtIndex(i);
                break;
            }

            y += EditorGUI.GetPropertyHeight(effectProp, true) + 4;
        }

        // Dropdown
        EditorGUI.LabelField(new Rect(position.x, y, 80, lineHeight), "Add Effect:");
        _selectedIndex = EditorGUI.Popup(new Rect(position.x + 80, y, position.width - 160, lineHeight), _selectedIndex, _effectTypeNames);

        if (GUI.Button(new Rect(position.x + position.width - 70, y, 60, lineHeight), "Add") && _selectedIndex >= 0)
        {
            Type selectedType = _effectTypes[_selectedIndex];
            object instance = Activator.CreateInstance(selectedType);

            effectsProp.arraySize++;
            effectsProp.GetArrayElementAtIndex(effectsProp.arraySize - 1).managedReferenceValue = instance;

            _selectedIndex = -1;

            property.serializedObject.ApplyModifiedProperties();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;

        SerializedProperty effectsProp = property.FindPropertyRelative("Effects");
        float height = EditorGUIUtility.singleLineHeight + 4; // foldout
        for (int i = 0; i < effectsProp.arraySize; i++)
        {
            SerializedProperty effectProp = effectsProp.GetArrayElementAtIndex(i);
            height += EditorGUI.GetPropertyHeight(effectProp, true) + 4;
        }

        // Dropdown
        height += EditorGUIUtility.singleLineHeight + 6;
        return height;
    }
}
