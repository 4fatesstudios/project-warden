using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;

public class SpaceCreatorWindow : EditorWindow
{
    private string newSpaceName = "NewSpace";
    private Area selectedArea;
    private SpaceType selectedType;
    private SpaceSize selectedSize;

    private string newAreaName = "";
    private string areaEnumPath = "Assets/Scripts/Enums/Area.cs"; // Adjust if needed

    [MenuItem("Tools/Spaces/Create New Space Asset")]
    public static void ShowWindow()
    {
        GetWindow<SpaceCreatorWindow>("Create Space Asset");
    }

    private void OnGUI()
    {
        GUILayout.Label("Create New Space", EditorStyles.boldLabel);

        newSpaceName = EditorGUILayout.TextField("Name", newSpaceName);
        selectedArea = (Area)EditorGUILayout.EnumPopup("Area", selectedArea);
        selectedType = (SpaceType)EditorGUILayout.EnumPopup("Space Type", selectedType);
        selectedSize = (SpaceSize)EditorGUILayout.EnumPopup("Space Size", selectedSize);

        EditorGUILayout.Space();
        GUILayout.Label("Add New Area", EditorStyles.boldLabel);
        newAreaName = EditorGUILayout.TextField("New Area Name", newAreaName);

        if (GUILayout.Button("Add New Area") && !string.IsNullOrEmpty(newAreaName))
        {
            AddNewAreaEnum(newAreaName);
            AssetDatabase.Refresh();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Create Space"))
        {
            CreateSpaceAssets();
        }
    }

    private void CreateSpaceAssets()
    {
        string areaName = selectedArea.ToString().Replace(" ", "");
        string typeName = selectedType.ToString().Replace(" ", "");
        string sizeName = selectedSize.ToString().Replace(" ", "");

        string fileBaseName = $"{areaName}_{typeName}_{sizeName}_{newSpaceName}";

        string templatePath = $"Assets/Prefabs/Spaces/{typeName}{sizeName}Template.prefab";
        GameObject template = AssetDatabase.LoadAssetAtPath<GameObject>(templatePath);

        if (!template)
        {
            EditorUtility.DisplayDialog("Error", $"Template not found at path:\n{templatePath}", "OK");
            return;
        }

        // Prefab paths
        string prefabDir = $"Assets/Prefabs/Spaces/{areaName}/{typeName}s/{sizeName}/";
        string prefabPath = $"{prefabDir}{fileBaseName}.prefab";

        // ScriptableObject paths
        string soDir = $"Assets/Resources/Spaces/{areaName}/{typeName}s/{sizeName}/";
        string soPath = $"{soDir}{fileBaseName}.asset";

        if (File.Exists(prefabPath) || File.Exists(soPath))
        {
            EditorUtility.DisplayDialog("Already Exists", $"A prefab or SpaceData file with the name:\n'{fileBaseName}' already exists.", "OK");
            return;
        }

        Directory.CreateDirectory(prefabDir);
        Directory.CreateDirectory(soDir);

        // Create and save prefab
        GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(template);
        prefabInstance.name = fileBaseName;
        PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
        GameObject.DestroyImmediate(prefabInstance);

        // Create and save ScriptableObject
        SpaceData spaceData = ScriptableObject.CreateInstance<SpaceData>();
        spaceData.SpacePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        spaceData.SpaceType = selectedType;
        spaceData.SpaceSize = selectedSize;

        SpaceDataUtility.AutoPopulateDoorSpawns(spaceData);

        AssetDatabase.CreateAsset(spaceData, soPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", $"Created:\n- Prefab: {prefabPath}\n- SpaceData: {soPath}", "OK");
        
        // automatically update all databases
        AreaSpacesDatabaseTool.UpdateAllDatabases();
    }


    private void AddNewAreaEnum(string newArea)
    {
        string[] enumLines = File.ReadAllLines(areaEnumPath);

        int insertIndex = -1;
        for (int i = 0; i < enumLines.Length; i++)
        {
            if (enumLines[i].Contains("enum Area"))
            {
                insertIndex = i + 1;
                break;
            }
        }

        if (insertIndex == -1)
        {
            Debug.LogError("Could not find Area enum definition.");
            return;
        }

        for (int i = insertIndex; i < enumLines.Length; i++)
        {
            if (enumLines[i].Contains("}"))
            {
                insertIndex = i;
                break;
            }
        }

        // Ensure unique and valid name
        string cleaned = newArea.Trim().Replace(" ", "_");
        if (enumLines.Any(line => line.Contains(cleaned)))
        {
            Debug.LogWarning("Area already exists in enum.");
            return;
        }

        var enumEntry = $"    {cleaned},";
        var linesList = enumLines.ToList();
        linesList.Insert(insertIndex, enumEntry);
        File.WriteAllLines(areaEnumPath, linesList.ToArray());

        Debug.Log($"Added new Area: {cleaned} to Area enum.");
    }
}
