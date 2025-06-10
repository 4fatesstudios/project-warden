using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects;

[CustomPropertyDrawer(typeof(StatModifier))]
public class StatModifierDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        SerializedProperty statProp = property.FindPropertyRelative("stat");
        SerializedProperty typeProp = property.FindPropertyRelative("type");
        SerializedProperty valProp = property.FindPropertyRelative("modifier");
        SerializedProperty sourceProp = property.FindPropertyRelative("source");

        // default source if empty
        if (string.IsNullOrWhiteSpace(sourceProp.stringValue))
            sourceProp.stringValue = "Unknown Source";

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float verticalSpacing = 2f;

        Rect statRect = new Rect(position.x, position.y, position.width, lineHeight);
        Rect typeRect = new Rect(position.x, statRect.y + lineHeight + verticalSpacing, position.width, lineHeight);
        Rect valRect = new Rect(position.x, typeRect.y + lineHeight + verticalSpacing, position.width, lineHeight);
        Rect sourceRect = new Rect(position.x, valRect.y + lineHeight + verticalSpacing, position.width, lineHeight);

        EditorGUI.PropertyField(statRect, statProp);
        EditorGUI.PropertyField(typeRect, typeProp);

        StatModifierType modifierType = (StatModifierType)typeProp.enumValueIndex;

        int min, max;
        string tooltip;

        if (modifierType == StatModifierType.Additive)
        {
            min = -9999;
            max = 9999;
            tooltip = "Flat bonus or penalty";
        }
        else
        {
            min = 0;
            max = 200;
            tooltip = "Percent modifier (e.g., 100 = 100%)";
        }

        valProp.intValue = Mathf.Clamp(valProp.intValue, min, max);
        GUIContent valLabel = new GUIContent("Value", tooltip);
        EditorGUI.IntSlider(valRect, valProp, min, max, valLabel);

        EditorGUI.PropertyField(sourceRect, sourceProp, new GUIContent("Source"));
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        // 4 lines total (stat, type, value, source) + spacing
        return EditorGUIUtility.singleLineHeight * 4 + 6;
    }
}
