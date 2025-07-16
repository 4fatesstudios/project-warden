using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Databases
{
    [CreateAssetMenu(fileName = "NewDatabase", menuName = "Databases/Area Spaces Database")]
    public class AreaSpacesDatabase : ScriptableObject {
        [SerializeField] private Area area;
        [SerializeField] private List<SpaceData> rooms;
        [SerializeField] private List<SpaceData> hallways;
        
        public Area Area => area;
        public IReadOnlyList<SpaceData> Rooms => rooms;
        public IReadOnlyList<SpaceData> Hallways => hallways;
        
        public void SetRooms(List<SpaceData> newRooms) => rooms = newRooms;
        public void SetHallways(List<SpaceData> newHallways) => hallways = newHallways;
    }
}