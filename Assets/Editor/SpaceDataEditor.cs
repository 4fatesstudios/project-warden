#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using UnityEngine;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using System.Collections.Generic;

[CustomEditor(typeof(SpaceData))]
public class SpaceDataEditor : Editor
{
    public static CardinalDirection GetDirectionFromZ(float zRot)
    {
        int z = Mathf.RoundToInt(zRot) % 360;
        return z switch
        {
            0 => CardinalDirection.North,
            90 => CardinalDirection.East,
            180 => CardinalDirection.South,
            270 => CardinalDirection.West,
            _ => CardinalDirection.None
        };
    }

    public static Color GetColorFromGroup(int group)
    {
        return group switch
        {
            0 => Color.black,
            1 => Color.red,
            2 => Color.blue,
            3 => Color.green,
            4 => Color.yellow,
            5 => Color.magenta,
            6 => Color.cyan,
            _ => Color.white
        };
    }

    public static string GetRelativePath(Transform root, Transform target)
    {
        if (target == root) return "";
        var path = target.name;
        var current = target.parent;
        while (current != null && current != root)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        return path;
    }

    public override void OnInspectorGUI()
    {
        // Compact SpacePrefab field
        SerializedProperty spacePrefabProp = serializedObject.FindProperty("spacePrefab");
        EditorGUILayout.PropertyField(spacePrefabProp);
        
        // Compact SpaceType field
        SerializedProperty spaceTypeProp = serializedObject.FindProperty("spaceType");
        EditorGUILayout.PropertyField(spaceTypeProp);
        
        // Compact SpaceSize field
        SerializedProperty spaceSizeProp = serializedObject.FindProperty("spaceSize");
        EditorGUILayout.PropertyField(spaceSizeProp);

        // Compact DoorSpawnPoints list with custom drawer
        SerializedProperty spawnListProp = serializedObject.FindProperty("doorSpawnPoints");
        EditorGUILayout.PropertyField(spawnListProp, true);

        serializedObject.ApplyModifiedProperties();

        SpaceData data = (SpaceData)target;

        if (GUILayout.Button("Auto-Populate Door Spawns")) {
            SpaceDataUtility.AutoPopulateDoorSpawns(data);
        }

        if (GUILayout.Button("Update Groups"))
        {
            if (data.SpacePrefab == null)
            {
                Debug.LogError("Assign SpacePrefab first!");
                return;
            }

            // Step 1: Collect all non-zero groups
            var originalGroups = new Dictionary<int, List<DoorSpawnData>>();

            foreach (var door in data.DoorSpawnPoints)
            {
                if (door.DoorSpawnGroup == 0) continue;

                if (!originalGroups.ContainsKey(door.DoorSpawnGroup))
                    originalGroups[door.DoorSpawnGroup] = new List<DoorSpawnData>();

                originalGroups[door.DoorSpawnGroup].Add(door);
            }

            // Step 2: Create dense remap starting from group 1
            var sortedKeys = originalGroups.Keys.OrderBy(k => k).ToList();
            var groupRemap = new Dictionary<int, int>(); // old -> new
            for (int i = 0; i < sortedKeys.Count; i++)
                groupRemap[sortedKeys[i]] = i + 1; // start at group 1

            // Step 3: Update doorSpawnGroup values using remap
            foreach (var door in data.DoorSpawnPoints)
            {
                if (door.DoorSpawnGroup == 0) continue;

                if (groupRemap.TryGetValue(door.DoorSpawnGroup, out int newGroup))
                    door.DoorSpawnGroup = newGroup;
            }

            // Step 4: Rebuild doorSpawnGroups (index 0 = group 1, index 1 = group 2, etc.)
            var newGroups = new List<List<DoorSpawnData>>();
            int groupCount = groupRemap.Count;
            for (int i = 0; i < groupCount; i++)
                newGroups.Add(new List<DoorSpawnData>());

            foreach (var door in data.DoorSpawnPoints)
            {
                if (door.DoorSpawnGroup == 0) continue;
                int index = door.DoorSpawnGroup - 1;
                newGroups[index].Add(door);
            }

            data.DoorSpawnGroup = newGroups;

            // Step 5: Debug log full group structure
            Debug.Log("Updated and densified door spawn groups (excluding group 0):");
            for (int i = 0; i < data.DoorSpawnGroup.Count; i++)
            {
                int groupId = i + 1;
                string groupStr = $"Group {groupId} ({data.DoorSpawnGroup[i].Count} spawns): ";
                groupStr += string.Join(", ", data.DoorSpawnGroup[i].Select(d => d.DoorSpawnPoint?.name ?? "null"));
                Debug.Log(groupStr);
            }

            // Step 6: Update prefab sprite colors
            string prefabPath = AssetDatabase.GetAssetPath(data.SpacePrefab);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            foreach (var doorData in data.DoorSpawnPoints)
            {
                if (doorData.DoorSpawnPoint == null) continue;

                string relPath = GetRelativePath(data.SpacePrefab.transform, doorData.DoorSpawnPoint.transform);
                var prefabTransform = prefabRoot.transform.Find(relPath);
                if (prefabTransform != null)
                {
                    var sr = prefabTransform.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.color = GetColorFromGroup(doorData.DoorSpawnGroup);
                        EditorUtility.SetDirty(sr);
                    }
                }
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            EditorUtility.SetDirty(data);
            Debug.Log("Door groups densified (starting from 1), and prefab visuals updated.");
        }

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Delete SpaceData & Prefab"))
        {
            if (EditorUtility.DisplayDialog("Delete Asset", 
                    "Are you sure you want to delete this SpaceData asset and its linked prefab? This cannot be undone.",
                    "Yes, Delete", "Cancel"))
            {
                string prefabPath = AssetDatabase.GetAssetPath(data.SpacePrefab);
                string dataPath = AssetDatabase.GetAssetPath(data);

                if (!string.IsNullOrEmpty(prefabPath))
                {
                    AssetDatabase.DeleteAsset(prefabPath);
                    Debug.Log($"Deleted prefab at: {prefabPath}");
                }

                if (!string.IsNullOrEmpty(dataPath))
                {
                    AssetDatabase.DeleteAsset(dataPath);
                    Debug.Log($"Deleted SpaceData at: {dataPath}");
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            
            // automatically update all databases
            AreaSpacesDatabaseTool.UpdateAllDatabases();
        }
        GUI.backgroundColor = Color.white;

    }
}

public static class SpaceDataUtility
{
    public static void AutoPopulateDoorSpawns(SpaceData data)
    {
        if (data.SpacePrefab == null)
        {
            Debug.LogError("Assign SpacePrefab first!");
            return;
        }

        string prefabPath = AssetDatabase.GetAssetPath(data.SpacePrefab);
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

        var spawnObjects = prefabRoot.GetComponentsInChildren<Transform>()
            .Where(t => t.CompareTag("DoorSpawn"))
            .ToList();

        var tempSpawnDataList = new List<(string path, DoorSpawnData data)>();

        foreach (var spawn in spawnObjects)
        {
            float zRot = spawn.localEulerAngles.y;
            CardinalDirection dir = SpaceDataEditor.GetDirectionFromZ(zRot); // make this public
            int group = 0;

            var spriteRenderer = spawn.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = SpaceDataEditor.GetColorFromGroup(group); // make this public
                EditorUtility.SetDirty(spriteRenderer);
            }

            string relPath = SpaceDataEditor.GetRelativePath(prefabRoot.transform, spawn); // make this public

            tempSpawnDataList.Add((relPath, new DoorSpawnData()
            {
                DoorSpawnPoint = null,
                DoorType = DoorType.Default,
                SpawnWeight = 1f,
                SpawnDirection = dir,
                DoorSpawnGroup = group
            }));
        }

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        GameObject resolvedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var realTransforms = resolvedPrefab.GetComponentsInChildren<Transform>();

        data.DoorSpawnPoints.Clear();

        foreach (var (path, doorData) in tempSpawnDataList)
        {
            var found = realTransforms.FirstOrDefault(t => SpaceDataEditor.GetRelativePath(resolvedPrefab.transform, t) == path);
            if (found != null)
                doorData.DoorSpawnPoint = found.gameObject;

            data.DoorSpawnPoints.Add(doorData);
        }

        EditorUtility.SetDirty(data);
    }
}

#endif
