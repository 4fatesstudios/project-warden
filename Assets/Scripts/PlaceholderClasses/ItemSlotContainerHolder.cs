using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden
{
    /// <summary>
    /// PLACEHOLDER: Mock inventory system for demo purposes
    /// TODO: Replace with actual inventory system implementation
    /// </summary>
    public class ItemSlotContainerHolder : MonoBehaviour
    {
        [Header("Demo Inventory")]
        [SerializeField] private Item[] demoItems;
        
        public ItemSlotContainer Container { get; private set; }
        
        private void Awake()
        {
            Container = new ItemSlotContainer();
            
            // Add demo items to inventory
            if (demoItems != null)
            {
                foreach (var item in demoItems)
                {
                    if (item != null)
                    {
                        Container.Add(item, 10); // Add 10 of each item for testing
                    }
                }
            }
        }
        
        public void AddItem(Item item, int count)
        {
            Container?.Add(item, count);
        }
        
        public bool RemoveItem(Item item, int count)
        {
            return Container?.Remove(item, count) ?? false;
        }
    }
    
    /// <summary>
    /// PLACEHOLDER: Mock container for holding items
    /// TODO: Replace with actual container system implementation
    /// </summary>
    [System.Serializable]
    public class ItemSlotContainer
    {
        private System.Collections.Generic.Dictionary<Item, int> items = new System.Collections.Generic.Dictionary<Item, int>();
        
        public void Add(Item item, int count)
        {
            if (item == null) return;
            
            if (items.ContainsKey(item))
                items[item] += count;
            else
                items[item] = count;
                
            Debug.Log($"Added {count}x {item.ItemName} to inventory");
        }
        
        public bool Remove(Item item, int count)
        {
            if (item == null || !items.ContainsKey(item)) return false;
            
            if (items[item] >= count)
            {
                items[item] -= count;
                if (items[item] <= 0)
                    items.Remove(item);
                    
                Debug.Log($"Removed {count}x {item.ItemName} from inventory");
                return true;
            }
            
            return false;
        }
        
        public int GetItemCount(Item item)
        {
            return items.ContainsKey(item) ? items[item] : 0;
        }
        
        public System.Collections.Generic.IEnumerable<Item> GetAllItems()
        {
            return items.Keys;
        }
        
        public System.Collections.Generic.Dictionary<Item, int> GetAllItemsWithCounts()
        {
            return new System.Collections.Generic.Dictionary<Item, int>(items);
        }
    }
}