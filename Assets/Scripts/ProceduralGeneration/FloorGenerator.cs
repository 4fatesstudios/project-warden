using System;
using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FourFatesStudios.ProjectWarden.ProceduralGeneration
{
    public class FloorGenerator : MonoBehaviour {
        [SerializeField] private Area area;
        [SerializeField] private int minimumFreeRooms = 8;
        [SerializeField] private int maximumFreeRooms = 12;
        [SerializeField] private List<GameObject> storyPrefabs;
        [SerializeField] private SpaceData startingRoom;
        [SerializeField, Tooltip("Enter 0 or leave blank for random seed")] private int randomSeed;
        [SerializeField] private GlobalAreasDatabase globalAreasDatabase;

        private System.Random _rng;
        private Queue<SpawnGroupQueueItem> _spawnQueue;
        private List<PlacedSpace> _placedSpaces;
        private int numberOfFreeRooms;

        public void GenerateFloor() {
            if (!startingRoom) {
                Debug.Log("Missing starting room");
                return;
            }
            
            // Set up rng and reset necessary structures
            SetRandomSeed();
            _placedSpaces = new List<PlacedSpace>();
            _spawnQueue = new Queue<SpawnGroupQueueItem>();
            numberOfFreeRooms = _rng.Next(minimumFreeRooms, maximumFreeRooms + 1);
            
            // Obtain all hallways and all rooms from the given Area Database
            var hallways = globalAreasDatabase.GetDatabaseByArea(area).Hallways;
            var rooms = globalAreasDatabase.GetDatabaseByArea(area).Rooms;
            
            // Place the starting room
            var placed = InstantiateSpaceAtOrigin(startingRoom, transform.position);
            if (placed == null) return;
            Debug.Log($"Placed starting room: {placed.Instance.name} at {placed.Instance.transform.position}");

            LogPlacedSpaceDoors(placed);
            
            // Enqueue starting room door spawn groups and add starting room to placed spaces
            EnqueueSpaceDoorSpawnGroups(placed);
            _placedSpaces.Add(placed);

            int attempts = 0;
            // Start propagation loop
            while (_placedSpaces.Count(space => space.SourceData.SpaceType == SpaceType.Room) < numberOfFreeRooms && _spawnQueue.Count > 0 && attempts < 50) {
                var queueItem = _spawnQueue.Dequeue();
                var source = queueItem.Source;

                Debug.Log($"Processing spawn group {queueItem.GroupID} from {source.Instance.name} with hallwayDepth {queueItem.HallwayDepth}");

                if (!source.DoorGroups.TryGetValue(queueItem.GroupID, out var doorGroup)) {
                    Debug.LogWarning($"Group {queueItem.GroupID} not found in {source.Instance.name}");
                    continue;
                }

                Debug.Log($"Trying {doorGroup.Count} doors in group {queueItem.GroupID}");

                ShuffleList(doorGroup); // Randomize door usage order

                bool placedAny = false;

                foreach (var sourceDoorGO in doorGroup) {
                    switch (queueItem.HallwayDepth) {
                        case 0: {
                            // At depth 0, always place a hallway
                            var newHallwayData = hallways[_rng.Next(hallways.Count)];
                            Debug.Log($"Attempting to place hallway {newHallwayData.name} at door {sourceDoorGO.name} from {source.Instance.name}");

                            if (TryPlaceSpaceAtDoor(sourceDoorGO, source, newHallwayData, queueItem.HallwayDepth + 1, out var newHallway)) {
                                Debug.Log($"Placed hallway: {newHallway.Instance.name} at {newHallway.Instance.transform.position}");
                                _placedSpaces.Add(newHallway);
                                EnqueueSpaceDoorSpawnGroups(newHallway, queueItem.HallwayDepth + 1);
                                placedAny = true;
                            } else {
                                Debug.LogWarning($"Failed to place hallway {newHallwayData.name} at door {sourceDoorGO.name}");
                            }

                            break;
                        }
                        case 1: {
                            // At depth 1, try either hallway or room

                            // Try hallway first or room first — you decide your preference
                            // Example: try hallway first

                            var newHallwayData = hallways[_rng.Next(hallways.Count)];
                            Debug.Log($"Attempting to place hallway {newHallwayData.name} at door {sourceDoorGO.name} from {source.Instance.name}");

                            if (TryPlaceSpaceAtDoor(sourceDoorGO, source, newHallwayData, queueItem.HallwayDepth + 1, out var newHallway)) {
                                Debug.Log($"Placed hallway: {newHallway.Instance.name} at {newHallway.Instance.transform.position}");
                                _placedSpaces.Add(newHallway);
                                EnqueueSpaceDoorSpawnGroups(newHallway, queueItem.HallwayDepth + 1);
                                placedAny = true;
                            } else {
                                var newRoomData = rooms[_rng.Next(rooms.Count)];
                                Debug.Log($"Attempting to place room {newRoomData.name} at door {sourceDoorGO.name} from {source.Instance.name}");

                                if (TryPlaceSpaceAtDoor(sourceDoorGO, source, newRoomData, queueItem.HallwayDepth, out var newRoom)) {
                                    Debug.Log($"Placed room: {newRoom.Instance.name} at {newRoom.Instance.transform.position}");
                                    _placedSpaces.Add(newRoom);
                                    EnqueueSpaceDoorSpawnGroups(newRoom);
                                    placedAny = true;
                                } else {
                                    Debug.LogWarning($"Failed to place room {newRoomData.name} at door {sourceDoorGO.name}");
                                }
                            }

                            break;
                        }
                        case 2: {
                            // Always place room at depth 2
                            var newRoomData = rooms[_rng.Next(rooms.Count)];
                            Debug.Log($"Attempting to place room {newRoomData.name} at door {sourceDoorGO.name} from {source.Instance.name}");

                            if (TryPlaceSpaceAtDoor(sourceDoorGO, source, newRoomData, queueItem.HallwayDepth, out var newRoom)) {
                                Debug.Log($"Placed room: {newRoom.Instance.name} at {newRoom.Instance.transform.position}");
                                _placedSpaces.Add(newRoom);
                                EnqueueSpaceDoorSpawnGroups(newRoom);
                                placedAny = true;
                            } else {
                                Debug.LogWarning($"Failed to place room {newRoomData.name} at door {sourceDoorGO.name}");
                            }

                            break;
                        }
                    }

                    if (placedAny)
                        break; // One placement per door group
                }

                if (!placedAny) {
                    Debug.LogWarning($"No valid space placed from group {queueItem.GroupID} of {source.Instance.name}");
                    _spawnQueue.Enqueue(queueItem);
                }
                ++attempts;
            }
            
            // Cleanup pass to remove hallways that go nowhere

            // Connection pass to connect rooms that can be connected together
        }

        private void SetRandomSeed() {
            if (randomSeed == 0)
                randomSeed = Random.Range(int.MinValue, int.MaxValue);
            
            _rng = new System.Random(randomSeed);
        }
        
        /// <summary>
        /// Instantiates SpaceData's prefab and places its root transform at the given target position
        /// </summary>
        /// <param name="data">the Space Data containing the prefab to instantiate</param>
        /// <param name="targetPosition">the target position to place the prefab's root transform</param>
        /// <returns></returns>
        private PlacedSpace InstantiateSpaceAtOrigin(SpaceData data, Vector3 targetPosition) {
            var placed = PlacedSpaceFactory.Create(data, Vector3.zero);
            placed.Instance.transform.position = targetPosition;
            return placed;
        }

        
        private bool TryPlaceSpaceAtDoor(GameObject sourceDoorGO, PlacedSpace sourceSpace, SpaceData newData, int hallwayDepth, out PlacedSpace placed) {
            placed = PlacedSpaceFactory.Create(newData, Vector3.zero);

            // Get the source door's direction from the source space
            if (!sourceSpace.DoorLookup.TryGetValue(sourceDoorGO, out var sourceDoorData)) {
                Debug.LogError("Source door GameObject not found in DoorLookup.");
                Destroy(placed.Instance);
                placed = null;
                return false;
            }

            var targetDoorData = placed.GetMatchingDoor(CardinalDirectionMask.GetOpposite(sourceDoorData.SpawnDirection));
            if (targetDoorData == null) {
                Debug.LogWarning($"Failed: Matching door not found in {newData.name}");
                Destroy(placed.Instance);
                placed = null;
                return false;
            }

            // Find the actual GameObject in the new PlacedSpace corresponding to the target DoorSpawnData
            var targetDoorGO = placed.DoorLookup.FirstOrDefault(kvp => kvp.Value == targetDoorData).Key;
            if (targetDoorGO == null) {
                Debug.LogError("Matching target door GameObject not found.");
                Destroy(placed.Instance);
                placed = null;
                return false;
            }

            // Align using actual Transforms - position root so target door aligns with source door
            var offset = sourceDoorGO.transform.position - targetDoorGO.transform.position;
            placed.Instance.transform.position += offset;

            Debug.Log(placed.Instance.transform.position);

            if (IsOverlapping(placed.GetBounds())) {
                Debug.LogWarning($"Failed: Overlap detected for {newData.name}");
#if UNITY_EDITOR
                DestroyImmediate(placed.Instance);
#else
        Destroy(placed.Instance);
#endif
                placed = null;
                return false;
            }
            return true;
        }
        
        private bool IsOverlapping(Bounds newBounds) {
            foreach (var placed in _placedSpaces) {
                if (placed.GetBounds().Intersects(newBounds)) {
                    Debug.LogWarning($"Overlap detected for {placed.Instance.name}");
                    return true;
                }
            }
            return false;
        }
        
        private void LogPlacedSpaceDoors(PlacedSpace placed) {
            Debug.Log($"Door count: {placed.DoorLookup.Count}");
    
            foreach (var kvp in placed.DoorLookup) {
                Debug.Log($"Door GameObject: {kvp.Key.name} -> {kvp.Value.SpawnDirection} [Group {kvp.Value.DoorSpawnGroup}]");
            }

            foreach (var group in placed.DoorGroups) {
                Debug.Log($"Group {group.Key} has {group.Value.Count} doors");
            }
        }

        private void EnqueueSpaceDoorSpawnGroups(PlacedSpace placed, int hallwayDepth=0) {
            foreach (var group in placed.DoorGroups) {
                _spawnQueue.Enqueue(new SpawnGroupQueueItem(placed, group.Key, hallwayDepth));
            }
        }
        
        private void ShuffleList<T>(List<T> list) {
            for (int i = list.Count - 1; i > 0; i--) {
                int j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private class SpawnGroupQueueItem {
            public PlacedSpace Source;
            public int GroupID;
            public int HallwayDepth;

            public SpawnGroupQueueItem(PlacedSpace source, int groupID, int hallwayDepth) {
                Source = source;
                GroupID = groupID;
                HallwayDepth = hallwayDepth;
            }
        }
    }
}
