#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;

public static class AreaSpacesDatabaseTool {
    private const string DatabaseFolder = "Assets/Resources/Databases/AreaSpaces";

    [MenuItem("Tools/Update All Area Databases")]
    public static void UpdateAllDatabases() {
        if (!Directory.Exists(DatabaseFolder))
            Directory.CreateDirectory(DatabaseFolder);

        var areaValues = System.Enum.GetValues(typeof(Area)).Cast<Area>().ToHashSet();

        var existingAssets = AssetDatabase.FindAssets("t:AreaSpacesDatabase", new[] { DatabaseFolder })
            .Select(guid => AssetDatabase.LoadAssetAtPath<AreaSpacesDatabase>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(db => db != null)
            .ToList();

        var toDelete = existingAssets.Where(db => !areaValues.Contains(db.Area)).ToList();

        if (toDelete.Count > 0) {
            string msg = "The following databases do not match any Area enum and will be deleted:\n\n";
            msg += string.Join("\n", toDelete.Select(d => d.name));
            msg += "\n\nProceed?";

            if (!EditorUtility.DisplayDialog("Caution: Orphaned Databases Found", msg, "Yes, Delete", "Cancel")) {
                Debug.Log("Update canceled by user.");
                return;
            }

            foreach (var db in toDelete) {
                string path = AssetDatabase.GetAssetPath(db);
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"Deleted: {path}");
            }
        }

        foreach (Area area in areaValues) {
            string dbPath = $"{DatabaseFolder}/{area}Spaces.asset";
            AreaSpacesDatabase db = AssetDatabase.LoadAssetAtPath<AreaSpacesDatabase>(dbPath);

            if (db == null) {
                db = ScriptableObject.CreateInstance<AreaSpacesDatabase>();
                db.name = $"{area}Spaces";

                // Assign area via reflection since Area has a private setter
                var areaField = typeof(AreaSpacesDatabase).GetField("area", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                areaField?.SetValue(db, area);

                AssetDatabase.CreateAsset(db, dbPath);
                Debug.Log($"Created database: {dbPath}");
            }

            AutoPopulate(db);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("All area databases updated.");
    }

    private static void AutoPopulate(AreaSpacesDatabase db) {
        string areaName = db.Area.ToString();
        string searchPath = $"Assets/Resources/Spaces/{areaName}";

        string[] guids = AssetDatabase.FindAssets("t:SpaceData", new[] { searchPath });
        List<SpaceData> rooms = new();
        List<SpaceData> hallways = new();

        foreach (string guid in guids) {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            SpaceData spaceData = AssetDatabase.LoadAssetAtPath<SpaceData>(path);

            if (spaceData == null) continue;

            if (spaceData.SpaceType == SpaceType.Room)
                rooms.Add(spaceData);
            else if (spaceData.SpaceType == SpaceType.Hallway)
                hallways.Add(spaceData);
        }

        db.SetRooms(rooms);
        db.SetHallways(hallways);
        EditorUtility.SetDirty(db);
    }
}
#endif
