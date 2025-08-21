using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class PrefabReparenter : EditorWindow
{
    private GameObject prefabA;
    private GameObject prefabZ;

    [MenuItem("Tools/Prefabs/Reparent Prefab A under Prefab Z")]
    public static void ShowWindow() => GetWindow<PrefabReparenter>("Reparent Prefab A under Prefab Z");

    private void OnGUI()
    {
        prefabA = (GameObject)EditorGUILayout.ObjectField("Prefab A", prefabA, typeof(GameObject), false);
        prefabZ = (GameObject)EditorGUILayout.ObjectField("Prefab Z", prefabZ, typeof(GameObject), false);

        EditorGUILayout.Space();

        GUI.enabled = prefabA != null && prefabZ != null;
        if (GUILayout.Button("Reparent Now"))
            ReparentPrefabs();
        GUI.enabled = true;
    }

    private void ReparentPrefabs()
    {
        string prefabAPath = AssetDatabase.GetAssetPath(prefabA);
        string prefabZPath = AssetDatabase.GetAssetPath(prefabZ);

        if (string.IsNullOrEmpty(prefabAPath) || string.IsNullOrEmpty(prefabZPath))
        {
            Debug.LogError("Invalid prefab references.");
            return;
        }

        // Create wrapped prefab
        string wrappedPath = Path.Combine(Path.GetDirectoryName(prefabAPath), "PrefabA_Wrapped.prefab");
        GameObject zInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabZ);
        PrefabUtility.InstantiatePrefab(prefabA, zInstance.transform);
        PrefabUtility.SaveAsPrefabAsset(zInstance, wrappedPath);
        Object.DestroyImmediate(zInstance);

        Debug.Log("Created wrapped prefab: " + wrappedPath);

        // Find all Prefab B paths first
        string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");
        List<string> prefabBPaths = new List<string>();

        foreach (string guid in allPrefabs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(prefab) == prefabA)
                prefabBPaths.Add(path);
        }

        // Update variants using SaveAsPrefabAssetAndConnect
        GameObject wrappedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(wrappedPath);

        foreach (string path in prefabBPaths)
        {
            GameObject prefabB = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            // Instantiate wrapped prefab in scene
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(wrappedPrefab);

            // Apply all overrides from original variant
            PrefabUtility.RevertObjectOverride(instance, InteractionMode.AutomatedAction);
    
            // Replace the prefab asset with the new instance
            PrefabUtility.SaveAsPrefabAsset(instance, path);

            Object.DestroyImmediate(instance);
            Debug.Log("Updated variant: " + path);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Prefab reparenting complete.");
    }
}
