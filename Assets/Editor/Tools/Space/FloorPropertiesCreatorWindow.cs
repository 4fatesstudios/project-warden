using System.IO;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using UnityEditor;
using UnityEngine;

public class FloorPropertiesCreatorWindow : EditorWindow
{
    private int floorLevel = 1;
    private Area selectedArea;

    private string globalDatabasePath = "Assets/Resources/Databases/GlobalAreasDatabase.asset";

    [MenuItem("Tools/Spaces/Create New Floor Properties Asset")]
    public static void ShowWindow() {
        GetWindow<FloorPropertiesCreatorWindow>("Create New Floor Properties Asset");
    }

    private void OnGUI() {
        floorLevel = EditorGUILayout.IntField("Floor Level", floorLevel);
        selectedArea = (Area)EditorGUILayout.EnumPopup("Area", selectedArea);

        if (GUILayout.Button("Create")) {
            CreateNewFloorProperties();
        }
    }

    private void CreateNewFloorProperties() {
        string areaName = selectedArea.ToString().Replace(" ", "");
        
        string soDir = "Assets/Resources/FloorProperties/";
        string soPath = $"{soDir}Floor_{floorLevel}_{areaName}.asset";

        var existingAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(soPath);
        if (existingAsset != null) {
            int option = EditorUtility.DisplayDialogComplex(
                "Already Exists",
                $"A FloorProperties file with the name:\n'{soPath}' already exists.\nDo you want to replace it?",
                "Replace",    // 0
                "Cancel",     // 1
                "Keep Both"   // 2
            );

            switch (option) {
                case 1:
                    return;
                case 2:
                    soPath = AssetDatabase.GenerateUniqueAssetPath(soPath);
                    break;
                default:
                    AssetDatabase.DeleteAsset(soPath);
                    break;
            }
        }

        var newFloor = ScriptableObject.CreateInstance<FloorProperties>();
        newFloor.FloorNumber = floorLevel;
        newFloor.Area = selectedArea;
        newFloor.GlobalAreasDatabase = AssetDatabase.LoadAssetAtPath<GlobalAreasDatabase>(globalDatabasePath);
        
        AssetDatabase.CreateAsset(newFloor, soPath);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        // Selection.activeObject = newFloor;
        
        EditorUtility.DisplayDialog("Success", "Created:\n- FloorProperties: {soPath}", "OK");
    }
}
