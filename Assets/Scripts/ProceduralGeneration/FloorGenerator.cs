using System;
using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using UnityEngine;
using UnityEngine.PlayerLoop;
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
        private Queue<SpawnGroupQueueItem> _spawnQueue = new();
        private List<PlacedSpace> _placedSpaces = new();
        private List<SpaceConnectionItem> _spaceConnections = new();
        private int numberOfFreeRooms;

        public void GenerateFloor() {
            if (!startingRoom) {
                Debug.Log("Missing starting room");
                return;
            }
            
            InitializeGeneration();
            
            // Obtain all hallways and all rooms from the given Area Database
            var hallways = globalAreasDatabase.GetDatabaseByArea(area).Hallways;
            var rooms = globalAreasDatabase.GetDatabaseByArea(area).Rooms;
            
            // Place the starting room
            // Enqueue starting room door spawn groups and add starting room to placed spaces
            var placedStart = PlaceStartingRoom();
            if (placedStart == null) return;
            
            // First pass, generate all rooms and hallways where valid
            ProcessSpawnQueue(rooms, hallways, numberOfFreeRooms*4);
            
            // Delete remaining spawn door groups (TEMP)
            
            
            // Cleanup pass to remove hallways that go nowhere
            // Connection pass to connect rooms that can be connected together
        }
        
        private void ProcessSpawnQueue(IReadOnlyList<SpaceData> rooms, IReadOnlyList<SpaceData> hallways, int maxAttempts=50) {
            int attempts = 0;
            while (_placedSpaces.Count(s => s.SourceData.SpaceType == SpaceType.Room) < numberOfFreeRooms 
                   && _spawnQueue.Count > 0 && attempts < maxAttempts) {
                var queueItem = _spawnQueue.Dequeue();
                TryPlaceFromSpawnGroup(queueItem, rooms, hallways);
                attempts++;
            }
            
            Debug.Log($"\n### Processed Queue ###\n" +
                      $"Placed Rooms: {_placedSpaces.Count(p => p.SourceData.SpaceType == SpaceType.Room)} / {numberOfFreeRooms}\n" +
                      $"Remaining Queue Items: {_spawnQueue.Count}\n" +
                      $"Attempts: {attempts} / {maxAttempts}\n" +
                      $"########################\n");
        }
        
        private void TryPlaceFromSpawnGroup(SpawnGroupQueueItem queueItem, IReadOnlyList<SpaceData> rooms, IReadOnlyList<SpaceData> hallways) {
            var source = queueItem.Source;
            if (!source.DoorGroups.TryGetValue(queueItem.GroupID, out var doorGroup)) {
                Debug.LogWarning($"Group {queueItem.GroupID} not found in {source.Instance.name}");
                return;
            }
            ShuffleList(doorGroup);
            bool placedAny = false;

            foreach (var sourceDoorGO in doorGroup) {
                placedAny = TryPlaceSpaceByDepth(sourceDoorGO, source, queueItem.HallwayDepth, rooms, hallways);
                if (placedAny) break;
            }
            if (!placedAny) {
                LogSpawnQueue("After Failed Placement");
                _spawnQueue.Enqueue(queueItem);
            }
            else {
                LogSpawnQueue("After Successful Placement");
            }

        }
        
        private bool TryPlaceSpaceByDepth(GameObject doorGO, PlacedSpace source, int hallwayDepth,
            IReadOnlyList<SpaceData> rooms, IReadOnlyList<SpaceData> hallways) {
            switch (hallwayDepth) {
                case 0:
                    return TryPlaceHallway(doorGO, source, hallways, hallwayDepth + 1);
                case 1:
                    if (_rng.Next(4) == 1)
                        return TryPlaceHallway(doorGO, source, hallways, hallwayDepth + 1);
                    else
                        return TryPlaceRoom(doorGO, source, rooms, hallwayDepth);
                case 2:
                    return TryPlaceRoom(doorGO, source, rooms, hallwayDepth);
                default:
                    return false;
            }
        }
        
        private bool TryPlaceHallway(GameObject doorGO, PlacedSpace source, IReadOnlyList<SpaceData> hallways, int depth) {
            var hallwayData = hallways[_rng.Next(hallways.Count)];
            if (TryPlaceSpaceAtDoor(doorGO, source, hallwayData, depth, out var placed)) {
                _placedSpaces.Add(placed);
                
                int usedGroupId = placed.DoorLookup.First(d => d.Value.SpawnDirection == 
                                                               CardinalDirectionMask.GetOpposite(source.DoorLookup[doorGO].SpawnDirection)).Value.DoorSpawnGroup;
                EnqueueSpaceDoorSpawnGroups(placed, depth, usedGroupId);
                return true;
            }
            return false;
        }

        private bool TryPlaceRoom(GameObject doorGO, PlacedSpace source, IReadOnlyList<SpaceData> rooms, int depth) {
            var roomData = rooms[_rng.Next(rooms.Count)];
            if (TryPlaceSpaceAtDoor(doorGO, source, roomData, depth, out var placed)) {
                _placedSpaces.Add(placed);
                
                int usedGroupId = placed.DoorLookup.First(d => d.Value.SpawnDirection ==
                                                               CardinalDirectionMask.GetOpposite(source.DoorLookup[doorGO].SpawnDirection)).Value.DoorSpawnGroup;
                EnqueueSpaceDoorSpawnGroups(placed, 0, usedGroupId);
                return true;
            }
            return false;
        }
        
        private PlacedSpace PlaceStartingRoom() {
            var placed = InstantiateSpaceAtOrigin(startingRoom, transform.position);
            if (placed == null) return null;
            EnqueueSpaceDoorSpawnGroups(placed);
            _placedSpaces.Add(placed);
            
            Debug.Log($"\n--- STARTING ROOM ---\n" +
                      $"Prefab: {startingRoom.name}\n" +
                      $"Position: {placed.Instance.transform.position}\n" +
                      $"----------------------\n");
            
            return placed;
        }

        private void InitializeGeneration() {
            // Set up rng and reset necessary structures
            SetRandomSeed();
            _placedSpaces = new List<PlacedSpace>();
            _spawnQueue = new Queue<SpawnGroupQueueItem>();
            numberOfFreeRooms = _rng.Next(minimumFreeRooms, maximumFreeRooms + 1);
            _spaceConnections = new List<SpaceConnectionItem>();
        }

        public void ClearFloor() {
            foreach (var space in _placedSpaces) {
                DestroyImmediate(space.Instance);
            }
            _spaceConnections.Clear();
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
#if UNITY_EDITOR
                DestroyImmediate(placed.Instance);
#else
                Destroy(placed.Instance);
#endif
                placed = null;
                return false;
            }

            var targetDoorData = placed.GetMatchingDoor(CardinalDirectionMask.GetOpposite(sourceDoorData.SpawnDirection));
            if (targetDoorData == null) {
                Debug.LogWarning($"Failed: Matching door not found in {newData.name}");
#if UNITY_EDITOR
                DestroyImmediate(placed.Instance);
#else
                Destroy(placed.Instance);
#endif
                placed = null;
                return false;
            }

            // Find the actual GameObject in the new PlacedSpace corresponding to the target DoorSpawnData
            var targetDoorGO = placed.DoorLookup.FirstOrDefault(kvp => kvp.Value == targetDoorData).Key;
            if (targetDoorGO == null) {
                Debug.LogError("Matching target door GameObject not found.");
#if UNITY_EDITOR
                DestroyImmediate(placed.Instance);
#else
                Destroy(placed.Instance);
#endif
                placed = null;
                return false;
            }

            // Align using actual Transforms - position root so target door aligns with source door
            var offset = sourceDoorGO.transform.position - targetDoorGO.transform.position;
            placed.Instance.transform.position += offset;
            
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
            
            // On successful SpacePlace, add DoorConnection to sourceSpace and new placedSpace
            PlacedSpaceConnectionUtility.ConnectSpaces(sourceSpace, sourceDoorGO, placed, targetDoorGO);
            _spaceConnections.Add(new SpaceConnectionItem(sourceSpace, sourceDoorGO, placed, targetDoorGO));
            
            Debug.Log(
                $"\n+++ SPACE PLACED +++\n" +
                $"Prefab: {newData.name}\n" +
                $"Placed at: {placed.Instance.transform.position}\n" +
                $"From Source: {sourceSpace.SourceData.name} ({sourceDoorGO.name})\n" +
                $"Connected via direction: {sourceSpace.DoorLookup[sourceDoorGO].SpawnDirection} -> {targetDoorData.SpawnDirection}\n" +
                $"Hallway Depth: {hallwayDepth}\n" +
                $"Total Placed Spaces: {_placedSpaces.Count}\n" +
                $"++++++++++++++++++++\n"
            );
            
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
        
        private void LogSpawnQueue(string context) {
            Debug.Log($"\n--- Spawn Queue ({context}) ---\n" +
                      $"Queue Count: {_spawnQueue.Count}");

            int i = 0;
            foreach (var item in _spawnQueue) {
                Debug.Log(
                    $"[{i++}] Source: {item.Source.SourceData.name}, Group ID: {item.GroupID}, Depth: {item.HallwayDepth}"
                );
            }
            Debug.Log("--- End of Queue ---\n");
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

        private void EnqueueSpaceDoorSpawnGroups(PlacedSpace placed, int hallwayDepth=0, int excludeGroupId=-1) {
            foreach (var group in placed.DoorGroups) {
                if (group.Key == 0 || group.Key == excludeGroupId) continue;
                
                _spawnQueue.Enqueue(new SpawnGroupQueueItem(placed, group.Key, hallwayDepth));
            }
        }
        
        private void ShuffleList<T>(List<T> list) {
            for (int i = list.Count - 1; i > 0; i--) {
                int j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private void OnDrawGizmos() {
            if (_spaceConnections.Count == 0) return;
            foreach (var connection in _spaceConnections) {
                if (connection != null) {
                    Vector3 location = connection.DoorA.transform.position;
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireCube(location, new Vector3(4, 4, 4));
                }
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

        private class SpaceConnectionItem {
            public PlacedSpace SpaceA;
            public GameObject DoorA;
            public PlacedSpace SpaceB;
            public GameObject DoorB;

            public SpaceConnectionItem(PlacedSpace spaceA, GameObject doorA, PlacedSpace spaceB, GameObject doorB) {
                SpaceA = spaceA;
                DoorA = doorA;
                SpaceB = spaceB;
                DoorB = doorB;
            }
        }
    }
}
