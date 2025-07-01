#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using UnityEngine;

[CustomEditor(typeof(RoomData))]
public class RoomDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoomData data = (RoomData)target;
        
        if (GUILayout.Button("Auto-Populate Door Spawns"))
        {
            if (data.RoomPrefab == null)
            {
                Debug.LogError("Assign roomPrefab first!");
                return;
            }

            // Get all GameObjects with DoorSpawn tag in the prefab
            var spawnObjects = data.RoomPrefab.GetComponentsInChildren<Transform>()
                .Where(t => t.CompareTag("DoorSpawn"))
                .Select(t => t.gameObject)
                .ToList();

            // Clear null entries
            data.DoorSpawnPoints.RemoveAll(x => x.DoorSpawnPoint == null);

            // Add new spawn points
            foreach (var spawnObj in spawnObjects)
            {
                if (data.DoorSpawnPoints.All(x => x.DoorSpawnPoint != spawnObj)) {
                    data.DoorSpawnPoints.Add(new DoorSpawnData()
                    {
                        DoorSpawnPoint = spawnObj,
                        DoorType = DoorType.Default,
                        SpawnWeight = 1f,
                        SpawnDirection = CardinalDirection.None,
                    });
                }
            }

            // Remove entries that no longer exist in prefab
            data.DoorSpawnPoints.RemoveAll(x => 
                !spawnObjects.Contains(x.DoorSpawnPoint));

            EditorUtility.SetDirty(data);
        }
    }
}
#endif