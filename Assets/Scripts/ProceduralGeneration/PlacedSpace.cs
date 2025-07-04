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

        private List<DoorConnection> connectedDoors = new();
        
        public IReadOnlyList<DoorConnection> ConnectedDoors => connectedDoors;

        public DoorSpawnData GetMatchingDoor(CardinalDirection oppositeDirection) {
            return SourceData.DoorSpawnPoints.FirstOrDefault(d => d.SpawnDirection == oppositeDirection);
        }
    
        /// <summary>
        /// Adds a unique DoorConnection object of the specified Door GameObject
        /// </summary>
        /// <param name="localDoor">Door GameObject of this PlacedSpace to add</param>
        /// <param name="connectedDoor">Door GameObject that localDoor is now connected to</param>
        /// <param name="placedSpace">The PlacedSpace of the newly connectedDoor</param>
        public void AddNewDoorConnection(GameObject localDoor, GameObject connectedDoor, PlacedSpace placedSpace) {
            bool alreadyConnected = connectedDoors.Any(c => c.LocalDoor == localDoor);
            if (alreadyConnected) {
                Debug.LogWarning($"Error: {Instance.name} already has a connection for {localDoor.name}");
                return;
            }

            connectedDoors.Add(new DoorConnection(localDoor, connectedDoor, placedSpace));
        }
        
        /// <summary>
        /// Removes an existing DoorConnection object of the specified Door GameObject
        /// </summary>
        /// <param name="localDoor">Door GameObject of this PlacedSpace to remove</param>
        public void RemoveDoorConnection(GameObject localDoor) {
            var connection = connectedDoors.FirstOrDefault(c => c.LocalDoor == localDoor);
            if (connection.LocalDoor == null) {
                Debug.LogWarning($"No connection found for {localDoor.name} in {Instance.name}");
                return;
            }

            connectedDoors.Remove(connection);
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

            return bounds;
        }

    }

    public struct DoorConnection {
        public GameObject LocalDoor;
        public GameObject ConnectedDoor;
        public PlacedSpace ConnectedSpace;

        public DoorConnection(GameObject localDoor, GameObject connectedDoor, PlacedSpace connectedSpace) {
            LocalDoor = localDoor;
            ConnectedDoor = connectedDoor;
            ConnectedSpace = connectedSpace;
        }
    }
}