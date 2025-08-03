using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Grinds;

[CustomEditor(typeof(GrindDatabase))]
public class GrindDatabaseEditor : Editor
{
    private const int GridColumns = 3;
    private const float BoxWidth = 200f;
    private const float Padding = 10f;

    public override void OnInspectorGUI()
    {
        GrindDatabase db = (GrindDatabase)target;

        if (GUILayout.Button("Auto-Populate From Resources/Grinds"))
        {
            AutoPopulate(db);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Grinds", EditorStyles.boldLabel);

        if (db.Grinds != null && db.Grinds.Count > 0)
        {
            int count = 0;
            EditorGUILayout.BeginHorizontal();
            foreach (var grind in db.Grinds)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(BoxWidth));
                EditorGUILayout.LabelField("Input: " + (grind.InputIngredient?.ItemName ?? "None"), EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField("Output: " + (grind.OutputIngredient?.ItemName ?? "None"), EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Grind: {grind.MinGrindSpeed}-{grind.MaxGrindSpeed}");
                EditorGUILayout.LabelField($"Pound: {grind.MinPoundSpeed}-{grind.MaxPoundSpeed}");
                EditorGUILayout.EndVertical();

                count++;
                if (count % GridColumns == 0)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("No grinds in database.", MessageType.Info);
        }

        EditorGUILayout.Space(10);
        DrawDefaultInspector(); // Optional to keep serialized view
    }

    private void AutoPopulate(GrindDatabase db)
    {
        string[] guids = AssetDatabase.FindAssets("t:Grind", new[] { "Assets/Resources/Grinds" });
        int addedCount = 0;

        SerializedObject so = new SerializedObject(db);
        SerializedProperty grindsProp = so.FindProperty("grinds");

        HashSet<string> existingNames = new();
        for (int i = 0; i < grindsProp.arraySize; i++)
        {
            var grind = grindsProp.GetArrayElementAtIndex(i).objectReferenceValue as Grind;
            if (grind != null && grind.name != null)
                existingNames.Add(grind.name);
        }

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Grind grind = AssetDatabase.LoadAssetAtPath<Grind>(path);
            if (grind != null && !existingNames.Contains(grind.name))
            {
                grindsProp.InsertArrayElementAtIndex(grindsProp.arraySize);
                grindsProp.GetArrayElementAtIndex(grindsProp.arraySize - 1).objectReferenceValue = grind;
                existingNames.Add(grind.name);
                addedCount++;
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(db);
        Debug.Log($"[GrindDatabaseEditor] Added {addedCount} new grinds.");
    }
}
