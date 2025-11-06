using FourFatesStudios.ProjectWarden.Camera;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

public class SceneSetupTool : EditorWindow {
    private EditorConfig config;
    
    public GameObject playerPrefab;
    public GameObject mainCameraPrefab;
    public GameObject vCamExplorationPrefab;
    public GameObject vCamCombatPrefab;

    private GameObject playerInstance;
    
    [MenuItem("Tools/Scene Tools/Scene Setup Tool")]
    public static void ShowWindow()
    {
        GetWindow<SceneSetupTool>("Scene Setup");
    }
    
    private void OnEnable()
    {
        // Load the config
        config = EditorConfigUtility.GetConfig();

        // Assign defaults if fields are empty
        if (playerPrefab == null) playerPrefab = config.playerPrefab;
        if (mainCameraPrefab == null) mainCameraPrefab = config.mainCameraPrefab;
        if (vCamExplorationPrefab == null) vCamExplorationPrefab = config.vCamExplorationPrefab;
        if (vCamCombatPrefab == null) vCamCombatPrefab = config.vCamCombatPrefab;
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Scene Setup Tool", EditorStyles.boldLabel);
        GUILayout.Space(8);

        playerPrefab = (GameObject)EditorGUILayout.ObjectField("Player Prefab", playerPrefab, typeof(GameObject), false);
        mainCameraPrefab = (GameObject)EditorGUILayout.ObjectField("Main Camera Prefab", mainCameraPrefab, typeof(GameObject), false);
        vCamExplorationPrefab = (GameObject)EditorGUILayout.ObjectField("Exploration vCam Prefab", vCamExplorationPrefab, typeof(GameObject), false);
        vCamCombatPrefab = (GameObject)EditorGUILayout.ObjectField("Combat vCam Prefab", vCamCombatPrefab, typeof(GameObject), false);

        GUILayout.Space(10);
        if (GUILayout.Button("Place Player in Current View"))
            PlacePlayerInScene();

        GUILayout.Space(5);
        if (GUILayout.Button("Place All Cameras"))
            ConfirmCameraPlacement();
    }
    
    private void ConfirmCameraPlacement()
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            bool proceed = EditorUtility.DisplayDialog(
                "Player Not Found",
                "No Player found in the scene. Do you want to place the cameras anyway?",
                "Yes, Place Cameras",
                "Cancel"
            );
            if (!proceed) return;
        }

        PlaceCameras();
    }
    
    private void PlacePlayerInScene()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("Player prefab not assigned!");
            return;
        }

        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null)
        {
            Debug.LogWarning("No active SceneView found.");
            return;
        }

        Vector3 spawnPos = sceneView.camera.transform.position + sceneView.camera.transform.forward * 2f;
        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        playerInstance = player;
        player.transform.position = spawnPos;
        player.name = "Player";
        Selection.activeGameObject = player;
    }

    private void PlaceCameras()
    {
        if (mainCameraPrefab == null || vCamExplorationPrefab == null || vCamCombatPrefab == null)
        {
            Debug.LogWarning("Camera prefabs not assigned!");
            return;
        }

        // Instantiate
        var mainCam = (GameObject)PrefabUtility.InstantiatePrefab(mainCameraPrefab);
        mainCam.name = "MainCamera";

        var vCamExploration = (GameObject)PrefabUtility.InstantiatePrefab(vCamExplorationPrefab);
        vCamExploration.name = "vCam_Exploration";

        var vCamCombat = (GameObject)PrefabUtility.InstantiatePrefab(vCamCombatPrefab);
        vCamCombat.name = "vCam_Combat";
        
        // Set exploration cam's Tracking Target
        var explorationVCam = vCamExploration.GetComponent<CinemachineCamera>();
        var test = new CameraTarget {
            TrackingTarget = playerInstance.transform
        };
        if (explorationVCam != null)
            explorationVCam.Target = test;

        // Auto-linking setup
        var controller = mainCam.GetComponent<CameraController>();
        if (controller)
        {
            controller.GetType().GetField("explorationCam", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, vCamExploration.GetComponent<CinemachineCamera>());
            controller.GetType().GetField("combatCam", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, vCamCombat.GetComponent<CinemachineCamera>());
            controller.GetType().GetField("mainCam", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, mainCam.GetComponent<UnityEngine.Camera>());
            EditorUtility.SetDirty(controller);
        }

        Selection.objects = new Object[] { mainCam, vCamExploration, vCamCombat };
    }
}

