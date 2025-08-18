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

    [MenuItem("Tools/Spaces/Space Designer")]
    public static void ShowWindow()
    {
        GetWindow<SpaceDesignerWindow>("Space Designer Window");
    }

    private void OnGUI()
    {
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
                if (selectedSpacePrefab != null)
                {
                    // Get the last active SceneView camera
                    SceneView sceneView = SceneView.lastActiveSceneView;
                    if (sceneView != null)
                    {
                        Camera cam = sceneView.camera;
                        Vector3 spawnPosition = cam.transform.position + cam.transform.forward * 5f; // 5 units in front
                        Quaternion spawnRotation = Quaternion.identity;

                        GameObject instance = PrefabUtility.InstantiatePrefab(selectedSpacePrefab, SceneManager.GetActiveScene()) as GameObject;
                        instance.transform.position = spawnPosition;
                        instance.transform.rotation = spawnRotation;

                        Undo.RegisterCreatedObjectUndo(instance, "Spawn Space Prefab");
                        Selection.activeGameObject = instance;

                        // Frame it in the SceneView
                        sceneView.FrameSelected();
                    }
                }
            }
        }
        else {
            EditorGUILayout.HelpBox("No prefab assigned to this SpaceData.", MessageType.Warning);
        }
    }
}
