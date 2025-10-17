using FourFatesStudios.ProjectWarden.Camera;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

public class SceneSetupTool : EditorWindow {
    public GameObject playerPrefab;
    public GameObject mainCameraPrefab;
    public GameObject vCamIsoPrefab;
    public GameObject vCamTwoPointFiveDPrefab;
    
    [MenuItem("Tools/Scene Tools/Scene Setup Tool")]
    public static void ShowWindow()
    {
        GetWindow<SceneSetupTool>("Scene Setup");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Scene Setup Tool", EditorStyles.boldLabel);
        GUILayout.Space(8);

        playerPrefab = (GameObject)EditorGUILayout.ObjectField("Player Prefab", playerPrefab, typeof(GameObject), false);
        mainCameraPrefab = (GameObject)EditorGUILayout.ObjectField("Main Camera Prefab", mainCameraPrefab, typeof(GameObject), false);
        vCamIsoPrefab = (GameObject)EditorGUILayout.ObjectField("Isometric vCam Prefab", vCamIsoPrefab, typeof(GameObject), false);
        vCamTwoPointFiveDPrefab = (GameObject)EditorGUILayout.ObjectField("2.5D vCam Prefab", vCamTwoPointFiveDPrefab, typeof(GameObject), false);

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
        player.transform.position = spawnPos;
        player.name = "Player";
        Selection.activeGameObject = player;
    }

    private void PlaceCameras()
    {
        if (mainCameraPrefab == null || vCamIsoPrefab == null || vCamTwoPointFiveDPrefab == null)
        {
            Debug.LogWarning("Camera prefabs not assigned!");
            return;
        }

        // Instantiate
        var mainCam = (GameObject)PrefabUtility.InstantiatePrefab(mainCameraPrefab);
        mainCam.name = "MainCamera";

        var vCamIso = (GameObject)PrefabUtility.InstantiatePrefab(vCamIsoPrefab);
        vCamIso.name = "vCam_Isometric";

        var vCam25D = (GameObject)PrefabUtility.InstantiatePrefab(vCamTwoPointFiveDPrefab);
        vCam25D.name = "vCam_2.5D";

        // Auto-linking setup
        var controller = mainCam.GetComponent<CameraController>();
        if (controller)
        {
            controller.GetType().GetField("isoCam", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, vCamIso.GetComponent<CinemachineCamera>());
            controller.GetType().GetField("twoPointFiveDCam", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, vCam25D.GetComponent<CinemachineCamera>());
            controller.GetType().GetField("mainCam", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(controller, mainCam.GetComponent<UnityEngine.Camera>());
            EditorUtility.SetDirty(controller);
        }

        Selection.objects = new Object[] { mainCam, vCamIso, vCam25D };
    }
}

