using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Databases.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Databases
{
    [CreateAssetMenu(fileName = "GlobalItemsDatabase", menuName = "Databases/Items/Global Items Database")]
    public class GlobalItemsDatabase : ScriptableObject {
        [SerializeField] private List<ItemDatabase> itemDatabases;

        public IReadOnlyList<ItemDatabase> ItemDatabases => itemDatabases;
        
        // for parameter use typeof(<Item>) for ex typeof(Ingredient)
        public ItemDatabase GetDatabaseByItemType(System.Type itemType) {
            return itemDatabases.FirstOrDefault(db => db.ItemType == itemType);
        }

        public ItemDatabase<T> GetDatabase<T>() where T : Item {
            return itemDatabases.OfType<ItemDatabase<T>>().FirstOrDefault();
        }

        public void SetDatabases(List<ItemDatabase> databaseList) {
            itemDatabases = databaseList;
        }
    }
}