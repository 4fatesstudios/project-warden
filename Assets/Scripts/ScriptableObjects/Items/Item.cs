using System;
using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Items/Generic Item")]
    public class Item : ScriptableObject
    {
        [SerializeField, Tooltip("Display name shown to the player.")]
        private string itemName;

        [SerializeField, Tooltip("Description shown to the player.")]
        private string itemDescription;

        [SerializeField] private ItemRarity itemRarity;

        [SerializeField, HideInInspector] private string _itemID;

        public string ItemName => itemName;
        public string ItemDescription => itemDescription;
        public ItemRarity ItemRarity => itemRarity;
        public string ItemID => _itemID;

#if UNITY_EDITOR
        [ContextMenu("Regenerate Item ID")]
        public void RegenerateID() => _itemID = Guid.NewGuid().ToString();

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_itemID))
            {
                _itemID = Guid.NewGuid().ToString();
                EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}