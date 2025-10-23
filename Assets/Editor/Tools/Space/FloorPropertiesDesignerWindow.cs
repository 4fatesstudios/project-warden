using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ProceduralGeneration;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using UnityEditor;
using UnityEngine;

public class FloorPropertiesDesignerWindow : EditorWindow 
{
    private FloorProperties selectedFloorProperties;
    private Editor floorPropertiesEditor;
    private Vector2 scrollPosition;
    private static readonly string previewRootName = "FloorPreviewRoot";
    private static EditorConfig config;
    private GameObject previewRoot;
    
    [MenuItem("Tools/Floors/Floor Properties Designer")]
    public static void ShowWindow() {
        ShowWindow(null);
    }

    public static void ShowWindow(FloorProperties selectedFloorProperties) {
        var window = GetWindow<FloorPropertiesDesignerWindow>("Floor Properties Design Tool");
        window.selectedFloorProperties = selectedFloorProperties;
        window.Focus();
    }

    private void OnEnable() {
        string[] guids = AssetDatabase.FindAssets("t:EditorConfig");
        if (guids.Length > 0) {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            config = AssetDatabase.LoadAssetAtPath<EditorConfig>(path);
        }
    }

    private void OnGUI() {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.LabelField("Floor Properties Designer", EditorStyles.boldLabel);
        selectedFloorProperties = EditorGUILayout.ObjectField("Floor Properties", selectedFloorProperties, typeof(FloorProperties), false) as FloorProperties;

        if (selectedFloorProperties == null) {
            EditorGUILayout.HelpBox("No Floor Properties selected.", MessageType.Info);

            if (GUILayout.Button("Create New Floor Properties", GUILayout.Height(30))) {
                FloorPropertiesCreatorWindow.ShowWindow();
            }
            
            EditorGUILayout.EndScrollView();
            return;
        }
        
        // FloorProperties inspector
        if (floorPropertiesEditor == null || floorPropertiesEditor.target != selectedFloorProperties)
            floorPropertiesEditor = Editor.CreateEditor(selectedFloorProperties);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Floor Properties", EditorStyles.boldLabel);
        floorPropertiesEditor.OnInspectorGUI();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Floor Previews", EditorStyles.boldLabel);

        if (GUILayout.Button("Preview Floor in Current Scene")) {
            GeneratePreview();
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void GeneratePreview() {
        ClearPreview();
    
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null) {
            Debug.LogWarning("No active SceneView found for preview generation.");
            return;
        }

        Camera cam = sceneView.camera;
        if (cam == null) {
            Debug.LogWarning("No active camera found in SceneView.");
            return;
        }

        Vector3 spawnPosition = cam.transform.position + cam.transform.forward * 5f;

        // Instantiate prefab as the root
        var root = (GameObject)PrefabUtility.InstantiatePrefab(config.floorGeneratorPrefab);
        root.name = previewRootName;
        root.transform.position = spawnPosition;

        // Get the FloorGenerator component
        var generator = root.GetComponent<FloorGenerator>();
        if (generator == null) {
            Debug.LogError("FloorGenerator component missing on prefab.");
            return;
        }

        // Generate the preview floor
        generator.GenerateFloor();
        generator.PruneDeadEndHallways();

        // Optionally store reference for later cleanup
        previewRoot = root;
    }


    private void ClearPreview() {
        GameObject root = GameObject.Find(previewRootName);
        if (root != null) {
            GameObject.DestroyImmediate(root);
        }
    }
}
