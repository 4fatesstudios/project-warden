using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Databases
{
    [CreateAssetMenu(fileName = "NewDatabase", menuName = "Databases/Item Database")]
    public class ItemDatabase<T> : ScriptableObject where T : Item {
        [SerializeField] private List<T> items;
        
        private Dictionary<string, Item> itemLookup;
        
        public IReadOnlyList<Item> Items => items;
        
        public System.Type ItemType => typeof(T);

        private void OnEnable() {
            BuildLookup();
        }

        private void BuildLookup() {
            itemLookup = new Dictionary<string, Item>();
            foreach (var item in items) {
                if (itemLookup.ContainsKey(item.ID)) {
                    Debug.LogWarning($"Duplicate ItemID detected: {item.ID} in {item.name}");
                    continue;
                }
                if (item != null && !string.IsNullOrEmpty(item.ID)) 
                    itemLookup[item.ID] = item;
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