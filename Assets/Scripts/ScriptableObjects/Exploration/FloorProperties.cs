using System;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ProceduralGeneration;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration
{
    public class FloorProperties : BaseDataSO {
        public static readonly int MaxFreeRooms = 50;
        
        [SerializeField] private int floorNumber = 1;
        [SerializeField] private Area area;
        [SerializeField] private int minimumFreeRooms = 8;
        [SerializeField] private int maximumFreeRooms = 12;
        [SerializeField] private int maxAttemptsPerRoom = 4;
        [SerializeField] private List<StoryRoomProperties> storyRooms;
        [SerializeField] private List<FreeRoomProperties> freeRooms;
        [SerializeField] private List<HallwayProperties> hallways;
        [SerializeField] private SpaceData startingRoom;
        [SerializeField] private SeedRNG seedRNG;
        private GlobalAreasDatabase globalAreasDatabase;

        
        public int FloorNumber { get => floorNumber; set => floorNumber = value; }
        public Area Area { get => area; set => area = value; }
        public int MinimumFreeRooms { get => minimumFreeRooms; set => minimumFreeRooms = value; }
        public int MaximumFreeRooms { get => maximumFreeRooms; set => maximumFreeRooms = value; }
        public int MaxAttemptsPerRoom { get => maxAttemptsPerRoom; set => maxAttemptsPerRoom = value; }
        public List<StoryRoomProperties> StoryPrefabs { get => storyRooms; set => storyRooms = value; }
        public List<FreeRoomProperties> FreeRooms { get => freeRooms; set => freeRooms = value; }
        public List<HallwayProperties> Hallways { get => hallways; set => hallways = value; }
        public SpaceData StartingRoom { get => startingRoom; set => startingRoom = value; }
        public SeedRNG SeedRNG { get => seedRNG; set => seedRNG = value; }
        public GlobalAreasDatabase GlobalAreasDatabase { get => globalAreasDatabase; set => globalAreasDatabase = value; }
        
        [System.Serializable]
        public class FreeRoomProperties {
            public GameObject freeRoomPrefab;
            public bool enabled;
            [SerializeField] public int maxInstances = 0;
            [SerializeField] public int minimumDepthFromStartingRoom = 0;
        }
        
        [System.Serializable]
        public class HallwayProperties {
            public GameObject hallwayPrefab;
            public bool enabled;
            // private int maxInstances = 0;
            // private int minimumDepthFromStartingRoom = 0;
        }

        [System.Serializable]
        public class StoryRoomProperties {
            public GameObject storyPrefab;
            public int minimumDepthFromStartingRoom = 0;
        }
    }
}