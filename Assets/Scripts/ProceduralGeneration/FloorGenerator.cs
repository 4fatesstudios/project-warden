using System;
using System.Collections.Generic;
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
            // Enqueue starting room door spawn groups and add starting room to placed spaces
            EnqueueSpaceDoorSpawnGroups(placed);
            _placedSpaces.Add(placed);
            
            // Start propagation loop
            

            LogPlacedSpaceDoors(placed);
        }

        private void SetRandomSeed() {
            if (randomSeed == 0)
                randomSeed = Random.Range(int.MinValue, int.MaxValue);
            
            _rng = new System.Random(randomSeed);
        }
        
        /// <summary>
        /// Instantiates SpaceData's prefab and places their Origin at given target position
        /// </summary>
        /// <param name="data">the Space Data containing the prefab to instantiate</param>
        /// <param name="targetPosition">the target position to place the Origin point of the Space Data prefab</param>
        /// <returns></returns>
        private PlacedSpace InstantiateSpaceAtOrigin(SpaceData data, Vector3 targetPosition) {
            var placed = PlacedSpaceFactory.Create(data, Vector3.zero);
            var origin = placed.Instance.transform.Find("Origin");

            if (!origin) {
                Debug.LogError($"Prefab {data.name} missing an Origin child.");
                return null;
            }

            var offset = targetPosition - origin.position;
            placed.Instance.transform.position += offset;
            return placed;
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