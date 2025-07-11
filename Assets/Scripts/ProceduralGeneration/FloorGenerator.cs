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
        [SerializeField] private GlobalAreasDatabase globalAreasDatabase;

        public void GenerateFloor() {
            if (!startingRoom) {
                Debug.Log("Missing starting room");
                return;
            }

            Debug.Log("Generating floor...");

            var hallways = globalAreasDatabase.GetDatabaseByArea(area).Hallways;
            var rooms = globalAreasDatabase.GetDatabaseByArea(area).Rooms;

            var placed = InstantiateSpaceAtOrigin(startingRoom, transform.position);
            if (placed == null) return;

            Debug.Log($"Placed starting room: {placed.Instance.name} at {placed.Instance.transform.position}");

            LogPlacedSpaceDoors(placed);
        }
        
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
    }
}