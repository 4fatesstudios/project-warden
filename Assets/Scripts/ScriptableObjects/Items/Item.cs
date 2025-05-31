using System;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    public enum ItemRarity {
        None,
        Common,
        Rare,
        Epic,
        Mythic,
        Unique
    }
    
    public abstract class Item : ScriptableObject {
        [SerializeField] private string itemName;
        [SerializeField] private string itemDescription;
        private string _itemID;
        [SerializeField] private ItemRarity itemRarity;

        public string ItemName => itemName;
        public string ItemDescription => itemDescription;
        public string ItemID => _itemID;
        public ItemRarity ItemRarity => itemRarity;
        
#if UNITY_EDITOR
        private void OnValidate() {
            if (string.IsNullOrEmpty(_itemID)) {
                _itemID = Guid.NewGuid().ToString();
            }
        }
#endif
    }
}