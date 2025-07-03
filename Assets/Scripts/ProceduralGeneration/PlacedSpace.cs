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
            var origin = Instance.transform.Find("Origin");
            if (!origin) {
                Debug.LogError($"Missing 'Origin' under {Instance.name}");
                return new Bounds(Instance.transform.position, Vector3.zero);
            }

            var collider = origin.GetComponent<Collider>();
            if (!collider) {
                Debug.LogError($"No Collider found under 'Origin' of {Instance.name}");
                return new Bounds(origin.position, Vector3.zero);
            }

            return collider.bounds;
        }

    }
}