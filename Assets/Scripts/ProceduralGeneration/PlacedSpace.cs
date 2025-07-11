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

        private List<DoorConnection> ConnectedDoors { get; set; } = new();
        
        public DoorSpawnData GetMatchingDoor(CardinalDirection oppositeDirection) {
            return SourceData.DoorSpawnPoints.FirstOrDefault(d => d.SpawnDirection == oppositeDirection);
        }
        
        public List<DoorConnection> GetAllConnections() {
            return ConnectedDoors.ToList(); // Return a copy to prevent external mutation
        }
    
        /// <summary>
        /// Adds a unique DoorConnection object of the specified Door GameObject
        /// </summary>
        /// <param name="localDoor">Door GameObject of this PlacedSpace to add</param>
        /// <param name="connectedDoor">Door GameObject that localDoor is now connected to</param>
        /// <param name="otherSpace">The PlacedSpace of the newly connectedDoor</param>
        public void AddNewDoorConnection(GameObject localDoor, GameObject connectedDoor, PlacedSpace otherSpace) {
            if (ConnectedDoors.Any(c => c.LocalDoor == localDoor)) {
                Debug.LogWarning($"Error: {Instance.name} already has a connection for {localDoor.name}");
                return;
            }

            ConnectedDoors.Add(new DoorConnection(localDoor, connectedDoor, otherSpace));

            // Add reverse connection to the other space
            if (!otherSpace.ConnectedDoors.Any(c => c.LocalDoor == connectedDoor)) {
                otherSpace.ConnectedDoors.Add(new DoorConnection(connectedDoor, localDoor, this));
            }
        }
        
        /// <summary>
        /// Removes an existing DoorConnection object of the specified Door GameObject
        /// </summary>
        /// <param name="localDoor">Door GameObject of this PlacedSpace to remove</param>
        public void RemoveDoorConnection(GameObject localDoor) {
            var connection = ConnectedDoors.FirstOrDefault(c => c.LocalDoor == localDoor);
            if (connection.LocalDoor == null) {
                Debug.LogWarning($"No connection found for {localDoor.name} in {Instance.name}");
                return;
            }

            ConnectedDoors.Remove(connection);

            // Remove reverse connection from the other space
            var otherSpace = connection.ConnectedSpace;
            if (otherSpace != null) {
                var reverse = otherSpace.ConnectedDoors.FirstOrDefault(c => c.LocalDoor == connection.ConnectedDoor);
                if (reverse.LocalDoor != null) {
                    otherSpace.ConnectedDoors.Remove(reverse);
                }
            }
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
    
    public static class PlacedSpaceConnectionUtility
    {
        /// <summary>
        /// Connects two spaces via the specified doors. Ensures doors exist on their respective spaces.
        /// </summary>
        public static void ConnectSpaces(PlacedSpace spaceA, GameObject doorA, PlacedSpace spaceB, GameObject doorB) {
            if (!spaceA.DoorLookup.ContainsKey(doorA)) {
                Debug.LogError($"Door {doorA.name} not found in DoorLookup of {spaceA.Instance.name}");
                return;
            }

            if (!spaceB.DoorLookup.ContainsKey(doorB)) {
                Debug.LogError($"Door {doorB.name} not found in DoorLookup of {spaceB.Instance.name}");
                return;
            }

            spaceA.AddNewDoorConnection(doorA, doorB, spaceB);
        }

        /// <summary>
        /// Disconnects the door from its connected counterpart bidirectionally.
        /// </summary>
        public static void DisconnectDoor(PlacedSpace space, GameObject door) {
            space.RemoveDoorConnection(door);
        }

        /// <summary>
        /// Removes all connections from the given space and clears them bidirectionally.
        /// </summary>
        public static void RemoveAllConnections(PlacedSpace space) {
            // Use ToList to avoid modifying collection while iterating
            var connections = space.GetAllConnections().ToList();

            foreach (var connection in connections) {
                space.RemoveDoorConnection(connection.LocalDoor);
            }
        }
    }
}