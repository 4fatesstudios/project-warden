#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using UnityEngine;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using System.Collections.Generic;

[CustomEditor(typeof(SpaceData))]
public class RoomDataEditor : Editor
{
    private static CardinalDirection GetDirectionFromZ(float zRot)
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

    private static Color GetColorFromGroup(int group)
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

    private static string GetRelativePath(Transform root, Transform target)
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
        SerializedProperty roomPrefabProp = serializedObject.FindProperty("roomPrefab");
        EditorGUILayout.PropertyField(roomPrefabProp);
        
        // Compact SpaceType field
        SerializedProperty spaceTypeProp = serializedObject.FindProperty("spaceType");
        EditorGUILayout.PropertyField(spaceTypeProp);
        
        // Only show RoomSize if spaceType == SpaceType.Room
        if ((SpaceType)spaceTypeProp.enumValueIndex == SpaceType.Room) {
            SerializedProperty roomSizeProp = serializedObject.FindProperty("roomSize");
            EditorGUILayout.PropertyField(roomSizeProp);
        }

        // Compact DoorSpawnPoints list with custom drawer
        SerializedProperty spawnListProp = serializedObject.FindProperty("doorSpawnPoints");
        EditorGUILayout.PropertyField(spawnListProp, true);

        serializedObject.ApplyModifiedProperties();

        SpaceData data = (SpaceData)target;

        if (GUILayout.Button("Auto-Populate Door Spawns"))
        {
            if (data.SpacePrefab == null)
            {
                Debug.LogError("Assign roomPrefab first!");
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(data.SpacePrefab);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            var spawnObjects = prefabRoot.GetComponentsInChildren<Transform>()
                .Where(t => t.CompareTag("DoorSpawn"))
                .ToList();

            // Temporarily hold data with relative paths
            var tempSpawnDataList = new List<(string path, DoorSpawnData data)>();

            foreach (var spawn in spawnObjects)
            {
                float zRot = spawn.localEulerAngles.y;
                Debug.Log(spawn.localEulerAngles.y);
                CardinalDirection dir = GetDirectionFromZ(zRot);
                int group = 0;

                var spriteRenderer = spawn.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = GetColorFromGroup(group);
                    EditorUtility.SetDirty(spriteRenderer);
                }

                string relPath = GetRelativePath(prefabRoot.transform, spawn);

                tempSpawnDataList.Add((relPath, new DoorSpawnData()
                {
                    DoorSpawnPoint = null, // will be resolved later
                    DoorType = DoorType.Default,
                    SpawnWeight = 1f,
                    SpawnDirection = dir,
                    DoorSpawnGroup = group
                }));
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            // Now resolve prefab GameObjects using saved paths
            GameObject resolvedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var realTransforms = resolvedPrefab.GetComponentsInChildren<Transform>();

            data.DoorSpawnPoints.Clear();

            foreach (var (path, doorData) in tempSpawnDataList)
            {
                var found = realTransforms.FirstOrDefault(t => GetRelativePath(resolvedPrefab.transform, t) == path);
                if (found != null)
                    doorData.DoorSpawnPoint = found.gameObject;

                data.DoorSpawnPoints.Add(doorData);
            }

            EditorUtility.SetDirty(data);
        }

        if (GUILayout.Button("Update Sprite Colors From Groups"))
        {
            if (data.SpacePrefab == null)
            {
                Debug.LogError("Assign RoomPrefab first!");
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(data.SpacePrefab);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

            foreach (var doorData in data.DoorSpawnPoints)
            {
                if (doorData.DoorSpawnPoint == null) continue;

                string relPath = GetRelativePath(data.SpacePrefab.transform, doorData.DoorSpawnPoint.transform);
                var prefabTransform = prefabRoot.transform.Find(relPath);
                if (prefabTransform != null)
                {
                    var sr = prefabTransform.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.color = GetColorFromGroup(doorData.DoorSpawnGroup);
                        EditorUtility.SetDirty(sr);
                    }
                }
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            Debug.Log("Sprite colors updated and saved to prefab.");
        }

    }
}
#endif
