using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases;

[CustomEditor(typeof(GlobalAreasDatabase))]
public class GlobalSpacesDatabaseEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        GlobalAreasDatabase globalDatabase = (GlobalAreasDatabase)target;

        if (GUILayout.Button("Auto-Populate From Project")) {
            AutoPopulate(globalDatabase);
        }
    }

    private void AutoPopulate(GlobalAreasDatabase globalDatabase) {
        string[] guids = AssetDatabase.FindAssets("t:AreaSpacesDatabase");
        List<AreaSpacesDatabase> foundDatabases = new();

        foreach (string guid in guids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AreaSpacesDatabase db = AssetDatabase.LoadAssetAtPath<AreaSpacesDatabase>(path);
            if (db != null) {
                foundDatabases.Add(db);
            }
        }

        foundDatabases = foundDatabases
            .OrderBy(db => db.Area.ToString()) // optional: keep consistent order
            .ToList();

        globalDatabase.SetDatabases(foundDatabases);
        EditorUtility.SetDirty(globalDatabase);
        AssetDatabase.SaveAssets();
        Debug.Log($"Populated Global Database with {foundDatabases.Count} area databases.");
    }
}