using System;
using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Items/Generic Item")]
    public class Item : ScriptableObject
    {
        [SerializeField, Tooltip("Display name shown to the player.")]
        private string itemName = "Unnamed Item";

        [SerializeField, Tooltip("Description shown to the player.")]
        private string itemDescription = "Empty Description";

        [FormerlySerializedAs("itemRarity")] [SerializeField] private Rarity itemRarity;

        [SerializeField, HideInInspector] private string _itemID;

        public string ItemName => itemName;
        public string ItemDescription => itemDescription;
        public Rarity ItemRarity => itemRarity;
        public string ItemID => _itemID;

#if UNITY_EDITOR
        [ContextMenu("Regenerate Item ID")]
        public void RegenerateID() => _itemID = Guid.NewGuid().ToString();

        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(itemName)) itemName = "Unnamed Item";
            if (string.IsNullOrEmpty(itemDescription)) itemDescription = "Empty Description";
            if (!string.IsNullOrEmpty(_itemID)) return;
            _itemID = Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
#endif
    }
}