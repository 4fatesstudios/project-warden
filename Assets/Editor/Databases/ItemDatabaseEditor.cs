using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

[CustomEditor(typeof(ItemDatabase))]
public class ItemDatabaseEditor : Editor
{
    private const int GridColumns = 4;
    private const float IconSize = 64f;
    private const float Padding = 10f;

    public override void OnInspectorGUI()
    {
        ItemDatabase db = (ItemDatabase)target;

        if (GUILayout.Button("Auto-Populate From Resources/Items"))
        {
            AutoPopulate(db);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Items", EditorStyles.boldLabel);

        if (db.Items != null && db.Items.Count > 0)
        {
            int count = 0;
            EditorGUILayout.BeginHorizontal();
            foreach (var item in db.Items)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(IconSize + Padding));
                GUIContent content = new GUIContent(item.ItemName ?? item.name);
                GUILayout.Label(content, EditorStyles.helpBox, GUILayout.Height(IconSize));

                EditorGUILayout.LabelField(item.ItemName ?? item.name, EditorStyles.miniLabel, GUILayout.Width(IconSize + Padding));
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
            EditorGUILayout.HelpBox("No items in database.", MessageType.Info);
        }

        EditorGUILayout.Space(10);
        DrawDefaultInspector(); // Optional: keeps showing the serialized list
    }

    private void AutoPopulate(ItemDatabase db)
    {
        string[] guids = AssetDatabase.FindAssets("t:Item", new[] { "Assets/Resources/Items" });
        int addedCount = 0;

        SerializedObject so = new SerializedObject(db);
        SerializedProperty itemsProp = so.FindProperty("items");

        HashSet<string> existingIDs = new();
        for (int i = 0; i < itemsProp.arraySize; i++)
        {
            var item = itemsProp.GetArrayElementAtIndex(i).objectReferenceValue as Item;
            if (item != null && !string.IsNullOrEmpty(item.ItemID))
                existingIDs.Add(item.ItemID);
        }

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Item item = AssetDatabase.LoadAssetAtPath<Item>(path);
            if (item != null && !existingIDs.Contains(item.ItemID))
            {
                itemsProp.InsertArrayElementAtIndex(itemsProp.arraySize);
                itemsProp.GetArrayElementAtIndex(itemsProp.arraySize - 1).objectReferenceValue = item;
                existingIDs.Add(item.ItemID);
                addedCount++;
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(db);
        Debug.Log($"[ItemDatabaseEditor] Added {addedCount} new items.");
    }
}
