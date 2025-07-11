using System.Collections.Generic;
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
    }
}