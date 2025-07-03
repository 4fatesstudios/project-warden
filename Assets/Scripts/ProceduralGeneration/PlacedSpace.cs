using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.ProceduralGeneration
{
    /// <summary>
    /// Runtime placed Space prefab data structure
    /// </summary>
    public class PlacedSpace {
        public SpaceData SourceData { get; set; }
        public GameObject Instance { get; set; }

        // Maps the actual GameObjects in the scene to design-time metadata
        public Dictionary<GameObject, DoorSpawnData> DoorLookup { get; set; } = new();

        // Door groups are mapped to the GameObjects at runtime
        public Dictionary<int, List<GameObject>> DoorGroups { get; set; } = new();

        public DoorSpawnData GetMatchingDoor(CardinalDirection oppositeDirection) {
            return SourceData.DoorSpawnPoints.FirstOrDefault(d => d.SpawnDirection == oppositeDirection);
        }

        public Bounds GetBounds() {
            var collider = Instance.GetComponent<BoxCollider>();
            if (!collider) {
                Debug.LogError($"No BoxCollider found on {Instance.name}");
                return new Bounds(Instance.transform.position, Vector3.zero);
            }

            // Calculate the world position of the collider center accounting for local offset
            Vector3 worldCenter = Instance.transform.TransformPoint(collider.center);
            Bounds bounds = new Bounds(worldCenter, collider.size);

            Debug.Log($"[GetBounds] Instance world pos: {Instance.transform.position}");
            Debug.Log($"[GetBounds] Collider local center: {collider.center}");
            Debug.Log($"[GetBounds] Computed world bounds center: {bounds.center}");
            Debug.Log($"[GetBounds] Computed world bounds size: {bounds.size}");

            return bounds;
        }

    }
}