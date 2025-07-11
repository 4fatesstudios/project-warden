using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Exploration;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Databases
{
    [CreateAssetMenu(fileName = "NewDatabase", menuName = "Databases/Global Areas Database")]
    public class GlobalAreasDatabase : ScriptableObject {
        [SerializeField] private List<AreaSpacesDatabase> areaSpacesDatabases;

        public List<AreaSpacesDatabase> AreaSpacesDatabases => areaSpacesDatabases;

        public AreaSpacesDatabase GetDatabaseByArea(Area area) {
            return areaSpacesDatabases.FirstOrDefault(db => db.Area == area);
        }

        public void SetDatabases(List<AreaSpacesDatabase> newDatabases) {
            areaSpacesDatabases = newDatabases;
        }
    }
}