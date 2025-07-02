using System.Collections.Generic;
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
    }
}