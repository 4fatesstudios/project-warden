using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpaceDesignerWindow : EditorWindow
{
    private GameObject selectedSpacePrefab;
    private SpaceData selectedSpaceData;
    private Editor spaceDataEditor;
    private Vector2 scrollPosition;

    [MenuItem("Tools/Spaces/Space Designer")]
    public static void ShowWindow()
    {
        GetWindow<SpaceDesignerWindow>("Space Designer Window");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.LabelField("Space Designer", EditorStyles.boldLabel);
        selectedSpaceData = EditorGUILayout.ObjectField("Selected Space", selectedSpaceData, typeof(SpaceData), false) as SpaceData;

        if (selectedSpaceData == null)
            return;

        selectedSpacePrefab = selectedSpaceData.SpacePrefab;

        // SpaceData inspector
        if (spaceDataEditor == null || spaceDataEditor.target != selectedSpaceData)
            spaceDataEditor = Editor.CreateEditor(selectedSpaceData);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("SpaceData Editor", EditorStyles.boldLabel);
        spaceDataEditor.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Prefab Editing", EditorStyles.boldLabel);
        
        GUILayout.Label("***Reminder to always nest ProBuilder assets under Terrain GameObject***");

        if (selectedSpacePrefab != null) {
            if (GUILayout.Button("Open Prefab in Prefab Stage")) {
                string prefabPath = AssetDatabase.GetAssetPath(selectedSpacePrefab);
                PrefabStage stage = PrefabStageUtility.OpenPrefab(prefabPath);
                if (stage != null)
                {
                    SceneView.lastActiveSceneView.FrameSelected();
                }
            }

            if (GUILayout.Button("Spawn Prefab in Current Scene")) {
                SpawnGameObjectInCurrentScene(selectedSpacePrefab);
            }

            if (GUILayout.Button("Spawn Loot Gen (Loot Orb) in Current Scene")) {
                HandlePrefabLootGenSpawn();
            }
        }
        else {
            EditorGUILayout.HelpBox("No prefab assigned to this SpaceData.", MessageType.Warning);
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void HandlePrefabLootGenSpawn() {
        string path = "Assets/Prefabs/LootGenerator.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
        if (prefab == null) return;
        
        SpawnGameObjectInCurrentScene(prefab);
    }

    private void SpawnGameObjectInCurrentScene(GameObject go) {
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null) return;
        
        Camera cam = sceneView.camera;
        Vector3 spawnPosition = cam.transform.position + cam.transform.forward * 5f;
        Quaternion spawnRotation = Quaternion.identity;
        
        GameObject instance = PrefabUtility.InstantiatePrefab(go) as GameObject;
        instance.transform.position = spawnPosition;
        instance.transform.rotation = spawnRotation;
        
        Undo.RegisterCreatedObjectUndo(instance, $"Spawn {go.name} Prefab");
        Selection.activeGameObject = instance;
        
        sceneView.FrameSelected();
    }
}
