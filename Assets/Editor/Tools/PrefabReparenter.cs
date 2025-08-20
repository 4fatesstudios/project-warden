using UnityEditor;
using UnityEngine;
using System.IO;

public class PrefabReparenter : EditorWindow
{
    private GameObject prefabA;
    private GameObject prefabZ;

    [MenuItem("Tools/Prefabs/Reparent Prefab A under Prefab Z")]
    public static void ShowWindow()
    {
        GetWindow<PrefabReparenter>("Reparent Prefab A under Prefab Z");
    }

    private void OnGUI()
    {
        prefabA = (GameObject)EditorGUILayout.ObjectField("Prefab A", prefabA, typeof(GameObject), false);
        prefabZ = (GameObject)EditorGUILayout.ObjectField("Prefab Z", prefabZ, typeof(GameObject), false);

        EditorGUILayout.Space();

        GUI.enabled = (prefabA != null && prefabZ != null);
        if (GUILayout.Button("Reparent Now"))
        {
            ReparentPrefabs();
        }
        GUI.enabled = true;
    }

    private void ReparentPrefabs()
    {
        string prefabAPath = AssetDatabase.GetAssetPath(prefabA);
        string prefabZPath = AssetDatabase.GetAssetPath(prefabZ);

        if (string.IsNullOrEmpty(prefabAPath) || string.IsNullOrEmpty(prefabZPath))
        {
            Debug.LogError("Invalid prefab references. Make sure both are prefab assets.");
            return;
        }

        // Create wrapped prefab = Z with A inside
        string wrappedPath = Path.Combine(Path.GetDirectoryName(prefabAPath), "PrefabA_Wrapped.prefab");

        GameObject zInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabZ);
        GameObject aInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabA, zInstance.transform);

        PrefabUtility.SaveAsPrefabAsset(zInstance, wrappedPath);
        Object.DestroyImmediate(zInstance);

        Debug.Log("Created wrapped prefab: " + wrappedPath);

        // Update all Prefab Bs (variants of A) to be variants of wrapped prefab
        string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");
        GameObject wrappedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(wrappedPath);

        foreach (string guid in allPrefabs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(prefab) == prefabA)
            {
                Debug.Log("Updating variant: " + path);

                GameObject variantInstance = (GameObject)PrefabUtility.InstantiatePrefab(wrappedPrefab);

                // Save variant over the same file path
                PrefabUtility.SaveAsPrefabAsset(variantInstance, path);
                Object.DestroyImmediate(variantInstance);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Prefab reparenting complete.");
    }
}
