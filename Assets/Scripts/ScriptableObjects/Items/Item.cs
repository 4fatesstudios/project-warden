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
    public abstract class Item : BaseDataSO
    {
        [SerializeField, Tooltip("Display name shown to the player.")]
        private string itemName = "Unnamed Item";

        [SerializeField, Tooltip("Description shown to the player.")]
        private string itemDescription = "Empty Description";

        [SerializeField] private Rarity itemRarity;

        public string ItemName => itemName;
        public string ItemDescription => itemDescription;
        public Rarity ItemRarity => itemRarity;

#if UNITY_EDITOR
        private void OnValidate()
        {
            base.OnValidate();
            if (string.IsNullOrEmpty(itemName)) itemName = "Unnamed Item";
            if (string.IsNullOrEmpty(itemDescription)) itemDescription = "Empty Description";
            EditorUtility.SetDirty(this);
        }
#endif
    }
}