using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Roasts;

[CustomEditor(typeof(RoastDatabase))]
public class RoastDatabaseEditor : Editor
{
    private const int GridColumns = 3;
    private const float BoxWidth = 220f;

    public override void OnInspectorGUI()
    {
        RoastDatabase db = (RoastDatabase)target;

        if (GUILayout.Button("Auto-Populate From Resources/Roasts"))
        {
            AutoPopulate(db);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Roasts", EditorStyles.boldLabel);

        if (db.Roasts != null && db.Roasts.Count > 0)
        {
            int count = 0;
            EditorGUILayout.BeginHorizontal();
            foreach (var roast in db.Roasts)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(BoxWidth));
                EditorGUILayout.LabelField("Input: " + (roast.InputIngredient?.ItemName ?? "None"), EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField("Output: " + (roast.OutputIngredient?.ItemName ?? "None"), EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Cook Time: {roast.CookedTime}/{roast.TotalCookTime}");
                EditorGUILayout.LabelField($"Thresholds:");
                EditorGUILayout.LabelField($" - Uncooked: {roast.UncookedThreshold}");
                EditorGUILayout.LabelField($" - Cooked: {roast.CookedThreshold}");
                EditorGUILayout.LabelField($" - Burnt: {roast.BurntThreshold}");

                if (roast.RequiresCorruptedCooking)
                {
                    EditorGUILayout.LabelField($"Corrupted Count: {roast.RequiredCookCount}", EditorStyles.miniBoldLabel);
                }

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
            EditorGUILayout.HelpBox("No roasts in database.", MessageType.Info);
        }

        EditorGUILayout.Space(10);
        DrawDefaultInspector();
    }

    private void AutoPopulate(RoastDatabase db)
    {
        string[] guids = AssetDatabase.FindAssets("t:Roast", new[] { "Assets/Resources/Roasts" });
        int addedCount = 0;

        SerializedObject so = new SerializedObject(db);
        SerializedProperty roastsProp = so.FindProperty("roasts");

        HashSet<string> existingNames = new();
        for (int i = 0; i < roastsProp.arraySize; i++)
        {
            var roast = roastsProp.GetArrayElementAtIndex(i).objectReferenceValue as Roast;
            if (roast != null && roast.name != null)
                existingNames.Add(roast.name);
        }

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Roast roast = AssetDatabase.LoadAssetAtPath<Roast>(path);
            if (roast != null && !existingNames.Contains(roast.name))
            {
                roastsProp.InsertArrayElementAtIndex(roastsProp.arraySize);
                roastsProp.GetArrayElementAtIndex(roastsProp.arraySize - 1).objectReferenceValue = roast;
                existingNames.Add(roast.name);
                addedCount++;
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(db);
        Debug.Log($"[RoastDatabaseEditor] Added {addedCount} new roasts.");
    }
}
