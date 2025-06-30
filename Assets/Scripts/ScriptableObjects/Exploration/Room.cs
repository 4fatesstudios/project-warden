using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DoorSpawnData
{
    [Header("References")]
    public GameObject doorSpawnPoint;  // Direct reference to the GameObject
    
    [Header("Settings")]
    public bool isActive = true;
    public DoorType doorType;
    public float spawnWeight = 1f;
    public Vector3 spawnDirection = Vector3.forward;
}

[CreateAssetMenu(fileName = "New Room Data", menuName = "Game/Room Data")]
public class RoomData : ScriptableObject
{
    public GameObject roomPrefab;  // Reference to the main prefab
    public List<DoorSpawnData> doorSpawnPoints = new List<DoorSpawnData>();
}

public enum DoorType {
    Default=1
}