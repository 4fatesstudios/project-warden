using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Databases
{
    [CreateAssetMenu(fileName = "NewDatabase", menuName = "Databases/Item Database")]
    public class ItemDatabase : ScriptableObject {
        [SerializeField] private List<Item> items = new();

        private Dictionary<string, Item> itemLookup;
        
        public IReadOnlyList<Item> Items => items;

        private void OnEnable() {
            BuildLookup();
        }

        private void BuildLookup() {
            itemLookup = new Dictionary<string, Item>();
            foreach (var item in items) {
                if (item != null && !string.IsNullOrEmpty(item.ItemID)) 
                    itemLookup[item.ItemID] = item;
            }
        }

        public Item GetItemById(string itemID) {
            if (itemLookup == null)
                BuildLookup();
            
            itemLookup.TryGetValue(itemID, out var item);
            return item;
        }
    }
}