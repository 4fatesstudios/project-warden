using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration {
    [CreateAssetMenu(fileName = "New Room Data", menuName = "Exploration/Room Data")]
    public class RoomData : ScriptableObject {
        [SerializeField] private GameObject roomPrefab; // Reference to the main prefab
        [SerializeField] private RoomSize roomSize = RoomSize.Small;
        [SerializeField] private List<DoorSpawnData> doorSpawnPoints = new();
        
        public GameObject RoomPrefab { get => roomPrefab; set => roomPrefab = value; }
        public RoomSize RoomSize { get => roomSize; set => roomSize = value; }
        public List<DoorSpawnData> DoorSpawnPoints { get => doorSpawnPoints; set => doorSpawnPoints = value; }
    }

    [System.Serializable]
    public class DoorSpawnData {
        [SerializeField] private GameObject doorSpawnPoint; // Direct reference to the GameObject

        [SerializeField] private DoorType doorType;
        [SerializeField] private float spawnWeight = 1f;
        [SerializeField] private CardinalDirection spawnDirection = CardinalDirection.None;
        [SerializeField] private int doorSpawnGroup = 0;
        
        public GameObject DoorSpawnPoint { get => doorSpawnPoint; set => doorSpawnPoint = value; }
        
        public DoorType DoorType { get => doorType; set => doorType = value; }
        public float SpawnWeight { get => spawnWeight; set => spawnWeight = value; }
        public CardinalDirection SpawnDirection { get => spawnDirection; set => spawnDirection = value; }
        public int DoorSpawnGroup { get => doorSpawnGroup; set => doorSpawnGroup = value; }
    }
}