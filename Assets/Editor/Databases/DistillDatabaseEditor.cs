using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Distills;

[CustomEditor(typeof(DistillDatabase))]
public class DistillDatabaseEditor : Editor
{
    private const int GridColumns = 3;
    private const float BoxWidth = 200f;

    public override void OnInspectorGUI()
    {
        DistillDatabase db = (DistillDatabase)target;

        if (GUILayout.Button("Auto-Populate From Resources/Distills"))
        {
            AutoPopulate(db);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Distills", EditorStyles.boldLabel);

        if (db.Distills != null && db.Distills.Count > 0)
        {
            int count = 0;
            EditorGUILayout.BeginHorizontal();
            foreach (var distill in db.Distills)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(BoxWidth));
                EditorGUILayout.LabelField("Input: " + (distill.InputIngredient?.ItemName ?? "None"), EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField("Output: " + (distill.OutputIngredient?.ItemName ?? "None"), EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"Required Duration: {distill.RequiredDuration} sec");
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
            EditorGUILayout.HelpBox("No distills in database.", MessageType.Info);
        }

        EditorGUILayout.Space(10);
        DrawDefaultInspector(); // Keep serialized view
    }

    private void AutoPopulate(DistillDatabase db)
    {
        string[] guids = AssetDatabase.FindAssets("t:Distill", new[] { "Assets/Resources/Distills" });
        int addedCount = 0;

        SerializedObject so = new SerializedObject(db);
        SerializedProperty distillsProp = so.FindProperty("distills");

        HashSet<string> existingNames = new();
        for (int i = 0; i < distillsProp.arraySize; i++)
        {
            var distill = distillsProp.GetArrayElementAtIndex(i).objectReferenceValue as Distill;
            if (distill != null && distill.name != null)
                existingNames.Add(distill.name);
        }

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Distill distill = AssetDatabase.LoadAssetAtPath<Distill>(path);
            if (distill != null && !existingNames.Contains(distill.name))
            {
                distillsProp.InsertArrayElementAtIndex(distillsProp.arraySize);
                distillsProp.GetArrayElementAtIndex(distillsProp.arraySize - 1).objectReferenceValue = distill;
                existingNames.Add(distill.name);
                addedCount++;
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(db);
        Debug.Log($"[DistillDatabaseEditor] Added {addedCount} new distills.");
    }
}
