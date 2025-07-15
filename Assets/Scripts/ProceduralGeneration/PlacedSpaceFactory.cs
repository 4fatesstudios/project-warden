using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ProceduralGeneration
{
    /// <summary>
    /// Translates the Prefab SpaceData to runtime placed Space data
    /// </summary>
    public static class PlacedSpaceFactory
    {
        public static PlacedSpace Create(SpaceData data, Vector3 position) {
            return Create(data, position, Quaternion.identity);
        }
        
        public static PlacedSpace Create(SpaceData data, Vector3 position, Quaternion rotation)
        {
            GameObject instance = GameObject.Instantiate(data.SpacePrefab, position, rotation);
            Dictionary<GameObject, DoorSpawnData> doorLookup = new();
            Dictionary<int, List<GameObject>> doorGroups = new();

            // Collect all children once for efficient lookup
            var allChildren = instance.GetComponentsInChildren<Transform>(true);

            foreach (DoorSpawnData doorData in data.DoorSpawnPoints)
            {
                Transform matched = allChildren.FirstOrDefault(t => t.name == doorData.DoorSpawnPoint.name);
                if (matched != null)
                {
                    GameObject doorObj = matched.gameObject;
                    doorLookup[doorObj] = doorData;

                    if (!doorGroups.ContainsKey(doorData.DoorSpawnGroup))
                        doorGroups[doorData.DoorSpawnGroup] = new List<GameObject>();

                    doorGroups[doorData.DoorSpawnGroup].Add(doorObj);
                }
                else
                {
                    Debug.LogWarning($"[PlacedSpaceFactory] Couldn't find door: {doorData.DoorSpawnPoint.name} in prefab {data.name}");
                }
            }

            return new PlacedSpace
            {
                SourceData = data,
                Instance = instance,
                DoorLookup = doorLookup,
                DoorGroups = doorGroups
            };
        }
    }
}