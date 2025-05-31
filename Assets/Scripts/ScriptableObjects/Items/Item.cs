using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
        [SerializeField] private ItemRarity itemRarity;
        [SerializeField, HideInInspector] private string _itemID;

        public string ItemName => itemName;
        public string ItemDescription => itemDescription;
        public ItemRarity ItemRarity => itemRarity;
        public string ItemID => _itemID;
        
#if UNITY_EDITOR
        private void OnValidate() {
            if (string.IsNullOrEmpty(_itemID)) {
                _itemID = Guid.NewGuid().ToString();
            }
        }
#endif
    }
}