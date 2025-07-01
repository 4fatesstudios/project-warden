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
            if (data.roomPrefab == null)
            {
                Debug.LogError("Assign roomPrefab first!");
                return;
            }

            // Get all GameObjects with DoorSpawn tag in the prefab
            var spawnObjects = data.roomPrefab.GetComponentsInChildren<Transform>()
                .Where(t => t.CompareTag("DoorSpawn"))
                .Select(t => t.gameObject)
                .ToList();

            // Clear null entries
            data.doorSpawnPoints.RemoveAll(x => x.doorSpawnPoint == null);

            // Add new spawn points
            foreach (var spawnObj in spawnObjects)
            {
                if (!data.doorSpawnPoints.Any(x => x.doorSpawnPoint == spawnObj))
                {
                    data.doorSpawnPoints.Add(new DoorSpawnData()
                    {
                        doorSpawnPoint = spawnObj,
                        isActive = true,
                        doorType = DoorType.Default,
                        spawnWeight = 1f,
                        spawnDirection = spawnObj.transform.forward
                    });
                }
            }

            // Remove entries that no longer exist in prefab
            data.doorSpawnPoints.RemoveAll(x => 
                !spawnObjects.Contains(x.doorSpawnPoint));

            EditorUtility.SetDirty(data);
        }
    }
}
#endif