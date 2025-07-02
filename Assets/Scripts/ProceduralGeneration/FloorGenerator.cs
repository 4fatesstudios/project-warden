using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ProceduralGeneration
{
    public class FloorGenerator : MonoBehaviour {
        [SerializeField] private int minimumFreeRooms = 8;
        [SerializeField] private int maximumFreeRooms = 12;
        [SerializeField] private List<GameObject> storyPrefabs;
        [SerializeField] private GameObject startingRoom;

        public void GenerateFloor() {
            Debug.Log("Generating floor");
        }
        
        // private List<DoorSpawnData> 
    }
}