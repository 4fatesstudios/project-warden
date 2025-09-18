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
        [SerializeField] private GlobalAreasDatabase globalAreasDatabase;

        
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
        public class SpacePlacementProperties {
            public SpaceType spaceType;
            public SpaceData spaceData;
            public bool enabled = true;

            [SerializeField, Tooltip("0 = no max instances")]
            private int maxInstances = 0;

            [SerializeField, Tooltip("0 = no minimum depth")]
            private int minimumDepthFromStartingRoom = 0;

            [NonSerialized] public int currentInstances = 0;

            public int MaxInstances {
                get {
                    // Story rooms always 1, hallways unlimited (0), free rooms editable
                    return spaceType switch {
                        SpaceType.StoryRoom => 1,
                        SpaceType.Hallway   => 0,
                        _                   => maxInstances
                    };
                }
                set {
                    if (spaceType == SpaceType.Room) maxInstances = Mathf.Max(0, value);
                }
            }

            public int MinimumDepthFromStartingRoom {
                get => Mathf.Max(0, minimumDepthFromStartingRoom);
                set {
                    if (spaceType != SpaceType.Hallway) 
                        minimumDepthFromStartingRoom = Mathf.Max(0, value);
                }
            }
        }
        
        [System.Serializable]
        public class FreeRoomProperties : SpacePlacementProperties {
            public FreeRoomProperties() {
                spaceType = SpaceType.Room;
            }
        }

        [System.Serializable]
        public class StoryRoomProperties : SpacePlacementProperties {
            public StoryRoomProperties() {
                spaceType = SpaceType.StoryRoom;
            }
        }

        [System.Serializable]
        public class HallwayProperties : SpacePlacementProperties {
            public HallwayProperties() {
                spaceType = SpaceType.Hallway;
            }
        }
    }
}