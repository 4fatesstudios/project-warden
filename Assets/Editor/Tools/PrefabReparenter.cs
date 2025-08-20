using UnityEditor;
using UnityEngine;
using System.IO;

public static class PrefabReparenter
{
    [MenuItem("Tools/Prefabs/Reparent Prefab A under Prefab Z")]
    static void ReparentPrefabA()
    {
        // paths need to be updated for your project
        string prefabAPath = "Assets/Prefabs/PrefabA.prefab";
        string prefabZPath = "Assets/Prefabs/PrefabZ.prefab";

        GameObject prefabA = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAPath);
        GameObject prefabZ = AssetDatabase.LoadAssetAtPath<GameObject>(prefabZPath);

        if (prefabA == null || prefabZ == null)
        {
            Debug.LogError("Missing prefab A or Z, check paths!");
            return;
        }

        // Create a new wrapped prefab = Z with A inside
        string wrappedPath = Path.GetDirectoryName(prefabAPath) + "/PrefabA_Wrapped.prefab";

        GameObject zInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabZ);
        GameObject aInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabA, zInstance.transform);

        PrefabUtility.SaveAsPrefabAsset(zInstance, wrappedPath);
        Object.DestroyImmediate(zInstance);

        Debug.Log("Created wrapped prefab: " + wrappedPath);

        // Update all Prefab Bs (variants of A) to now be variants of the wrapped prefab
        string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in allPrefabs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(prefab) == prefabA)
            {
                // This is a prefab B (variant of A)
                Debug.Log("Updating variant: " + path);

                GameObject wrapped = AssetDatabase.LoadAssetAtPath<GameObject>(wrappedPath);
                GameObject variantInstance = (GameObject)PrefabUtility.InstantiatePrefab(wrapped);

                // copy overrides
                PrefabUtility.SaveAsPrefabAsset(variantInstance, path);
                Object.DestroyImmediate(variantInstance);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
