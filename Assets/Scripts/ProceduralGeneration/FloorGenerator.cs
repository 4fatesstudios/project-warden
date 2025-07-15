using System;
using System.Collections;
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
        [SerializeField] private int maxAttemptsPerRoom = 4;
        [SerializeField] private List<GameObject> storyPrefabs;
        [SerializeField] private SpaceData startingRoom;
        [SerializeField] private GlobalAreasDatabase globalAreasDatabase;
        [SerializeField] private SeedRNG seedRNG;

        private Queue<SpawnGroupQueueItem> _spawnQueue = new();
        private List<PlacedSpace> _placedSpaces = new();
        private List<SpaceConnectionItem> _spaceConnections = new();
        private Dictionary<SpaceType, IReadOnlyList<SpaceData>> _spaces = new();
        private int numberOfFreeRooms;

        public void GenerateFloor() {
            if (!startingRoom) {
                Debug.Log("Missing starting room");
                return;
            }
            
            InitializeGeneration();
            
            // Obtain all hallways and all rooms from the given Area Database
            _spaces.Add(SpaceType.Room, globalAreasDatabase.GetDatabaseByArea(area).Rooms);
            _spaces.Add(SpaceType.Hallway, globalAreasDatabase.GetDatabaseByArea(area).Hallways);
            
            // Place the starting room
            // Enqueue starting room door spawn groups and add starting room to placed spaces
            var placedStart = PlaceStartingRoom();
            if (placedStart == null) return;
            
            // First pass, generate all rooms and hallways where valid
            ProcessSpawnQueue();
            
            // Second pass, cleanup pass to remove hallways that go nowhere
            // PruneDeadEndHallways();

            // Third pass, connection pass to connect rooms that can be connected together
        }
        
        private void ProcessSpawnQueue() {
            while (_placedSpaces.Count(s => s.SourceData.SpaceType == SpaceType.Room) < numberOfFreeRooms 
                   && _spawnQueue.Count > 0) {
                var queueItem = _spawnQueue.Dequeue();
                TryPlaceFromSpawnGroup(queueItem);
            }
            
            Debug.Log($"\n### Processed Queue ###\n" +
                      $"Placed Rooms: {_placedSpaces.Count(p => p.SourceData.SpaceType == SpaceType.Room)} / {numberOfFreeRooms}\n" +
                      $"Remaining Queue Items: {_spawnQueue.Count}\n" +
                      // $"Attempts: {attempts} / {maxAttempts}\n" +
                      $"########################\n");
        }

        public void PruneDeadEndHallways() {
            var placedHallways = new Queue<PlacedSpace>(_placedSpaces
                .Where(s => s.SourceData.SpaceType == SpaceType.Hallway).ToList());

            Debug.Log($"[Prune] Starting pruning. Initial hallway count: {placedHallways.Count}");

            while (placedHallways.Count > 0) {
                var hallway = placedHallways.Dequeue();

                Debug.Log($"[Prune] Evaluating hallway: {hallway.SourceData.name} | Connections: {hallway.NumberOfConnections}");

                if (hallway.NumberOfConnections > 1) continue;
                var connectedHallways = hallway.GetAllConnections()
                    .Where(s => s.ConnectedSpace.SourceData.SpaceType == SpaceType.Hallway)
                    .Select(s => s.ConnectedSpace)
                    .Distinct();

                foreach (var connectedHallway in connectedHallways) {
                    if (placedHallways.Contains(connectedHallway)) continue;
                    Debug.Log($"[Prune] Queueing connected hallway: {connectedHallway.SourceData.name}");
                    placedHallways.Enqueue(connectedHallway);
                }

                Debug.Log($"[Prune] Pruning dead-end hallway: {hallway.SourceData.name}");
                DeletePlacedSpaceAndConnections(hallway);
            }

            Debug.Log($"[Prune] Finished pruning. Remaining placed spaces: {_placedSpaces.Count}");
        }

        private void DeletePlacedSpaceAndConnections(PlacedSpace placedSpace) {
            if (!_placedSpaces.Contains(placedSpace)) {
                Debug.LogWarning($"[Delete] Placed space not found: {placedSpace.SourceData.name}");
                return;
            }

            Debug.Log($"[Delete] Removing placed space: {placedSpace.SourceData.name}");

            _placedSpaces.Remove(placedSpace);
            _spaceConnections.RemoveAll(item => item.SpaceA == placedSpace || item.SpaceB == placedSpace);
            PlacedSpaceConnectionUtility.RemoveAllConnections(placedSpace);
            
#if UNITY_EDITOR
            DestroyImmediate(placedSpace.Instance);
#else
            Destroy(placed.Instance);
#endif
        }

        
        private void TryPlaceFromSpawnGroup(SpawnGroupQueueItem queueItem) {
            var source = queueItem.Source;
            if (!source.DoorGroups.TryGetValue(queueItem.GroupID, out var doorGroup)) {
                Debug.LogWarning($"Group {queueItem.GroupID} not found in {source.Instance.name}");
                return;
            }
            ShuffleList(doorGroup);
            queueItem.AttemptCount++;
            bool placedAny = false;
            
            Debug.Log($"[Attempt {queueItem.AttemptCount}/{maxAttemptsPerRoom}] Trying group {queueItem.GroupID} from {source.SourceData.name}");
            
            foreach (var sourceDoorGO in doorGroup) {
                placedAny = TryPlaceSpaceByDepth(queueItem, sourceDoorGO, source, queueItem.HallwayDepth);
                if (placedAny) break;
            }
            
            if (placedAny) {
                Debug.Log($"✅ Success: Placed space from group {queueItem.GroupID} (Attempt {queueItem.AttemptCount})");
                LogSpawnQueue("After Successful Placement");
            } else if (queueItem.AttemptCount >= maxAttemptsPerRoom) {
                Debug.LogWarning($"❌ Max attempts reached for group {queueItem.GroupID} from {source.SourceData.name}");
                LogSpawnQueue("After Failed Placement & Max Attempts");
            } else {
                Debug.Log($"🔁 Re-enqueueing group {queueItem.GroupID} (Attempt {queueItem.AttemptCount})");
                _spawnQueue.Enqueue(queueItem);
                LogSpawnQueue("After Failed Placement & Re-enqueue");
            }
        }
        
        private bool TryPlaceSpaceByDepth(SpawnGroupQueueItem queueItem, GameObject doorGO, PlacedSpace source, int hallwayDepth) {
            switch (hallwayDepth) {
                case 0:
                    return TryPlace(queueItem, doorGO, source, SpaceType.Hallway, hallwayDepth + 1);
                case 1:
                    return seedRNG.Rng.Next(4) == 1 
                        ? TryPlace(queueItem, doorGO, source, SpaceType.Hallway, hallwayDepth + 1) 
                        : TryPlace(queueItem, doorGO, source, SpaceType.Room, 0);
                case 2:
                    return TryPlace(queueItem, doorGO, source, SpaceType.Room, 0);
                default:
                    return false;
            }
        }

        private bool TryPlace(SpawnGroupQueueItem queueItem, GameObject doorGO, PlacedSpace source, SpaceType spaceTypeToTry, 
            int hallwayDepth) {
            var list = _spaces[spaceTypeToTry];
            var filtered = list
                .Where(space => !queueItem.FailedRoomSizes.Contains(space.RoomSize))
                .ToList();
            // If nothing valid left to try, set attempts to max (probably cleaner way to do this later)
            if (filtered.Count == 0) {
                queueItem.AttemptCount = maxAttemptsPerRoom;
                return false;
            }
            var spaceData = filtered[seedRNG.Rng.Next(filtered.Count)];

            Debug.Log($"→ Attempting to place {spaceTypeToTry}: {spaceData.name} | Size: {spaceData.RoomSize} | Remaining options: {filtered.Count}");
            
            if (!TryPlaceSpaceAtDoor(queueItem, doorGO, source, spaceData, hallwayDepth, out var placed)) return false;
            _placedSpaces.Add(placed);

            var userGroupId = placed.DoorLookup.First(
                d 
                    => d.Value.SpawnDirection 
                       == CardinalDirectionMask.GetOpposite(source.DoorLookup[doorGO].SpawnDirection)).Value.DoorSpawnGroup;
                
            EnqueueSpaceDoorSpawnGroups(placed, hallwayDepth, userGroupId);
            return true;
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
            seedRNG.SetRandomSeed();
            _placedSpaces = new List<PlacedSpace>();
            _spawnQueue = new Queue<SpawnGroupQueueItem>();
            numberOfFreeRooms = seedRNG.Rng.Next(minimumFreeRooms, maximumFreeRooms + 1);
            _spaceConnections = new List<SpaceConnectionItem>();
        }

        public void ClearFloor() {
            foreach (var space in _placedSpaces) {
                DestroyImmediate(space.Instance);
            }
            _spaceConnections.Clear();
            _placedSpaces.Clear();
            _spawnQueue.Clear();
            _spaces.Clear();
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
        
        private bool TryPlaceSpaceAtDoor(SpawnGroupQueueItem queueItem, GameObject sourceDoorGO, PlacedSpace sourceSpace, 
            SpaceData newData, int hallwayDepth, out PlacedSpace placed) {
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
                // Adds failed size and larger sizes to queueItem hashset
                queueItem.FailedRoomSizes.UnionWith(GetLargerSizes(placed.SourceData.RoomSize));
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

        private List<RoomSize> GetLargerSizes(RoomSize targetSizes) {
            var largerSizes = new List<RoomSize>();
            
            switch (targetSizes) {
                case RoomSize.Small:
                    largerSizes.AddRange(new[] { RoomSize.Medium, RoomSize.Large });
                    break;
                case RoomSize.Medium:
                    largerSizes.Add(RoomSize.Large);
                    break;
                case RoomSize.Large:
                    break;
            }
            
            return largerSizes;
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
                int j = seedRNG.Rng.Next(i + 1);
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
            public int AttemptCount;
            public HashSet<RoomSize> FailedRoomSizes;

            public SpawnGroupQueueItem(PlacedSpace source, int groupID, int hallwayDepth, int attemptCount = 0, 
                HashSet<RoomSize> failedRoomSizes = null) {
                Source = source;
                GroupID = groupID;
                HallwayDepth = hallwayDepth;
                AttemptCount = attemptCount;
                FailedRoomSizes = failedRoomSizes ?? new HashSet<RoomSize>();
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
