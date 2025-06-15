using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases;

[CustomEditor(typeof(SkillDatabase))]
public class SkillDatabaseEditor : Editor
{
    private const int GridColumns = 4;
    private const float IconSize = 64f;
    private const float Padding = 10f;

    public override void OnInspectorGUI()
    {
        SkillDatabase db = (SkillDatabase)target;

        if (GUILayout.Button("Auto-Populate From Resources/Skills"))
        {
            AutoPopulate(db);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Skills", EditorStyles.boldLabel);

        if (db.Skills != null && db.Skills.Count > 0)
        {
            int count = 0;
            EditorGUILayout.BeginHorizontal();
            foreach (var skill in db.Skills)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(IconSize + Padding));
                GUIContent content = new GUIContent(skill.SkillName ?? skill.name);
                GUILayout.Label(content, EditorStyles.helpBox, GUILayout.Height(IconSize));

                EditorGUILayout.LabelField(skill.SkillName ?? skill.name, EditorStyles.miniLabel, GUILayout.Width(IconSize + Padding));
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
            EditorGUILayout.HelpBox("No skills in database.", MessageType.Info);
        }

        EditorGUILayout.Space(10);
        DrawDefaultInspector(); // Optional: keeps showing the serialized list
    }

    private void AutoPopulate(SkillDatabase db)
    {
        string[] guids = AssetDatabase.FindAssets("t:Skill", new[] { "Assets/Resources/Skills" });
        int addedCount = 0;

        SerializedObject so = new SerializedObject(db);
        SerializedProperty skillsProp = so.FindProperty("skills");

        HashSet<string> existingIDs = new();
        for (int i = 0; i < skillsProp.arraySize; i++)
        {
            var skill = skillsProp.GetArrayElementAtIndex(i).objectReferenceValue as Skill;
            if (skill != null && !string.IsNullOrEmpty(skill.SkillID))
                existingIDs.Add(skill.SkillID);
        }

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Skill skill = AssetDatabase.LoadAssetAtPath<Skill>(path);
            if (skill != null && !existingIDs.Contains(skill.SkillID))
            {
                skillsProp.InsertArrayElementAtIndex(skillsProp.arraySize);
                skillsProp.GetArrayElementAtIndex(skillsProp.arraySize - 1).objectReferenceValue = skill;
                existingIDs.Add(skill.SkillID);
                addedCount++;
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(db);
        Debug.Log($"[SkillDatabaseEditor] Added {addedCount} new skills.");
    }
}
