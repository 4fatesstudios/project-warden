using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Databases
{
    [CreateAssetMenu(fileName = "NewDatabase", menuName = "Databases/Item Database")]
    public class ItemDatabase : ScriptableObject {
        private static ItemDatabase _instance;

        public static ItemDatabase Instance {
            get {
                if (_instance == null)
                    _instance = Resources.Load<ItemDatabase>("Databases/ItemDatabase");
                if (_instance == null)
                    Debug.LogError("ItemDatabase asset not found in Resources/Databases/ItemDatabase");
                return _instance;
            }
        }
        
        [SerializeField] private List<Item> items = new();
        private Dictionary<string, Item> itemLookup;
        
        public IReadOnlyList<Item> Items => items;

        private void OnEnable() {
            BuildLookup();
        }

        private void BuildLookup() {
            itemLookup = new Dictionary<string, Item>();
            foreach (var item in items) {
                if (itemLookup.ContainsKey(item.ItemID)) {
                    Debug.LogWarning($"Duplicate ItemID detected: {item.ItemID} in {item.name}");
                    continue;
                }
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

        public Ingredient GetIngredientByName(string name)
        {
            return items.OfType<Ingredient>().FirstOrDefault(i => i.name == name);
        }

    }
}