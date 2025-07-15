using System.Collections.Generic;
using System.IO;
using System.Linq;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AreaSpacesDatabase))]
public class AreaSpacesDatabaseEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        AreaSpacesDatabase areaSpacesDatabase = (AreaSpacesDatabase)target;

        if (GUILayout.Button($"Populate Database From Resources/Spaces/{areaSpacesDatabase.Area}")) {
            AutoPopulate(areaSpacesDatabase);
        }
    }

    private void AutoPopulate(AreaSpacesDatabase areaSpacesDatabase) {
        string areaName = areaSpacesDatabase.Area.ToString();
        string searchPath = $"Assets/Resources/Spaces/{areaName}";

        // Find all SpaceData assets in the specified folder
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

        areaSpacesDatabase.SetRooms(rooms);
        areaSpacesDatabase.SetHallways(hallways);

        EditorUtility.SetDirty(areaSpacesDatabase);
        AssetDatabase.SaveAssets();
        Debug.Log($"Auto-populated {areaSpacesDatabase.name} with {rooms.Count} rooms and {hallways.Count} hallways.");
    }
}