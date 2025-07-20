using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases.Items;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GlobalItemsDatabase))]
public class GlobalItemsDatabaseEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        GlobalItemsDatabase globalDatabase = (GlobalItemsDatabase)target;

        if (GUILayout.Button("Auto-Populate From Project")) {
            AutoPopulate(globalDatabase);
        }
    }

    private void AutoPopulate(GlobalItemsDatabase globalDatabase) {
        string[] guids = AssetDatabase.FindAssets("t:ItemDatabase");
        List<ItemDatabase> foundDatabases = new();

        foreach (string guid in guids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemDatabase db = AssetDatabase.LoadAssetAtPath<ItemDatabase>(path);
            if (db != null) {
                foundDatabases.Add(db);
            }
        }
        
        foundDatabases = foundDatabases
            .OrderBy(db => db.ItemType.ToString())
            .ToList();
        
        globalDatabase.SetDatabases(foundDatabases);
        EditorUtility.SetDirty(globalDatabase);
        AssetDatabase.SaveAssets();
        Debug.Log($"Populate Global Item Database with {foundDatabases.Count} item databases.");
    }
}
