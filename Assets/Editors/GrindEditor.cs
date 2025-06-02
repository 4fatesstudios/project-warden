using UnityEditor;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Grinds;

[CustomEditor(typeof(Grind))]
public class GrindEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Grind grind = (Grind)target;

        EditorGUI.BeginDisabledGroup(true);  // Disable editing, not sure if needed later

        EditorGUI.EndDisabledGroup();
    }
}