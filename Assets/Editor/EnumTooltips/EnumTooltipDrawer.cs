using System;
using System.Collections;
using System.Linq;
using FourFatesStudios.ProjectWarden.Enums.Tooltips.Attributes;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EnumTooltipAttribute))]
public class EnumTooltipDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var enumAttr = (EnumTooltipAttribute)attribute;
        var enumType = enumAttr.EnumType;
        var tooltipProviderType = enumAttr.TooltipProviderType;

        if (!enumType.IsEnum || property.propertyType != SerializedPropertyType.Enum)
        {
            EditorGUI.LabelField(position, label.text, "EnumTooltip only works with enum fields.");
            return;
        }

        var tooltipsField = tooltipProviderType.GetField("Tooltips");

        if (tooltipsField == null || !typeof(IDictionary).IsAssignableFrom(tooltipsField.FieldType))
        {
            EditorGUI.LabelField(position, label.text, "Tooltip provider must expose a static Tooltips dictionary.");
            return;
        }

        var tooltips = (IDictionary)tooltipsField.GetValue(null);
        int index = property.enumValueIndex;
        string[] enumNames = property.enumDisplayNames;
        Array enumValues = Enum.GetValues(enumType);
        object enumValue = enumValues.GetValue(index);

        string tooltip = tooltips.Contains(enumValue) ? (string)tooltips[enumValue] : "No description.";
        GUIContent content = new GUIContent(label.text, tooltip);

        EditorGUI.BeginProperty(position, label, property);
        // Convert enumNames (string[]) to GUIContent[]
        var guiContents = enumNames.Select(name => new GUIContent(name)).ToArray();
        property.enumValueIndex = EditorGUI.Popup(position, content, index, guiContents);
        EditorGUI.EndProperty();
    }
}