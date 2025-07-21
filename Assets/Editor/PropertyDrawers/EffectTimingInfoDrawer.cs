using FourFatesStudios.ProjectWarden.Enums;
using UnityEditor;
using UnityEngine;
using FourFatesStudios.ProjectWarden.Structs;

[CustomPropertyDrawer(typeof(EffectTimingInfo))]
public class EffectTimingInfoDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        var effectTimingProp = property.FindPropertyRelative("effectTiming");
        var showTurnDuration = ShouldShowTurnDuration(effectTimingProp);
        return EditorGUIUtility.singleLineHeight * (showTurnDuration ? 2 : 1) + 4;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var effectTimingProp = property.FindPropertyRelative("effectTiming");
        var turnDurationProp = property.FindPropertyRelative("turnDuration");

        position.height = EditorGUIUtility.singleLineHeight;

        EditorGUI.PropertyField(position, effectTimingProp, new GUIContent("Effect Timing"));

        if (ShouldShowTurnDuration(effectTimingProp)) {
            position.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.PropertyField(position, turnDurationProp, new GUIContent("Turn Duration"));
        }
    }

    private bool ShouldShowTurnDuration(SerializedProperty effectTimingProp) {
        var timing = (EffectTiming)effectTimingProp.enumValueIndex;
        return timing == EffectTiming.Delay || timing == EffectTiming.OverTime || timing == EffectTiming.OnTurn;
    }
}