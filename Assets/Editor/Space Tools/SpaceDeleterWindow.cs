using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;

public class SpaceDeleterWindow : EditorWindow {
    private SpaceData selectedSpaceData;

    [MenuItem("Tools/Spaces/Delete Space Asset")]
    public static void ShowWindow() {
        GetWindow<SpaceDeleterWindow>("Delete Space Asset");
    }

    private void OnGUI() {
        GUILayout.Label("Delete SpaceData & Prefab", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        selectedSpaceData = (SpaceData)EditorGUILayout.ObjectField("SpaceData Asset", selectedSpaceData, typeof(SpaceData), false);

        if (selectedSpaceData != null) {
            GameObject linkedPrefab = selectedSpaceData.SpacePrefab;
            string spaceDataPath = AssetDatabase.GetAssetPath(selectedSpaceData);
            string prefabPath = linkedPrefab != null ? AssetDatabase.GetAssetPath(linkedPrefab) : "None";

            EditorGUILayout.LabelField("Prefab Path:", prefabPath);

            EditorGUILayout.Space();
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Delete SpaceData & Prefab")) {
                if (EditorUtility.DisplayDialog("Confirm Deletion",
                        $"Are you sure you want to delete:\n\n- {spaceDataPath}\n- {prefabPath}\n\nThis cannot be undone.",
                        "Yes, Delete", "Cancel")) {
                    
                    if (!string.IsNullOrEmpty(prefabPath))
                        AssetDatabase.DeleteAsset(prefabPath);

                    if (!string.IsNullOrEmpty(spaceDataPath))
                        AssetDatabase.DeleteAsset(spaceDataPath);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    Debug.Log($"Deleted SpaceData and prefab:\n{spaceDataPath}\n{prefabPath}");

                    selectedSpaceData = null;
                }
                
                // automatically update all databases
                AreaSpacesDatabaseTool.UpdateAllDatabases();
            }
            GUI.backgroundColor = Color.white;
        }
    }
}