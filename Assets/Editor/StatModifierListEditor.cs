using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects;

[CustomEditor(typeof(StatModifierListSO))]
public class StatModifierListEditor : Editor {
    public override void OnInspectorGUI() {
        StatModifierListSO statModifierListSO = (StatModifierListSO)target;
        
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextField("StatModifierList ID", statModifierListSO.StatModifierListID);
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Regenerate StatModifierList ID")) {
            Undo.RecordObject(statModifierListSO, "StatModifierList ID");
            statModifierListSO.RegenerateStatModifierListID();
            EditorUtility.SetDirty(statModifierListSO);
        }
        
        DrawDefaultInspector();
    }
}
