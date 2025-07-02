using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ProceduralGeneration
{
    public class FloorGenerator : MonoBehaviour {
        [SerializeField] private Area area;
        [SerializeField] private int minimumFreeRooms = 8;
        [SerializeField] private int maximumFreeRooms = 12;
        [SerializeField] private List<GameObject> storyPrefabs;
        [SerializeField] private GameObject startingRoom;
        [SerializeField] private GlobalAreasDatabase globalAreasDatabase;

        public void GenerateFloor() {
            if (!startingRoom) {
                Debug.Log("Missing starting room");
                return;
            }
            
            
            
            Debug.Log("Generating floor");
        }

        private DoorSpawnData GetRandomDoorSpawn(List<DoorSpawnData> doorSpawnGroup) {
            return doorSpawnGroup[Random.Range(0, doorSpawnGroup.Count)];
        }
    }
}