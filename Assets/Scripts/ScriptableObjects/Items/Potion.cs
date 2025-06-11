using System;
using System.Collections.Generic;
using UnityEngine;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    [CreateAssetMenu(fileName = "NewPotion", menuName = "Items/Potion")]
    public class Potion : Item
    {
        [SerializeField, Tooltip("Potion item type.")]
        private ItemPotionType itemPotionType;

        [SerializeField, Tooltip("List of effects this potion applies.")]
        private List<PotionEffect> potionEffects;

        private bool upgraded = false;

        public event EventHandler OnUpgradeStatusChanged;

        public ItemPotionType ItemPotionType => itemPotionType;
        public IReadOnlyList<PotionEffect> PotionEffects => potionEffects;

        public bool Upgraded
        {
            get => upgraded;
            set
            {
                if (value == upgraded) return;
                upgraded = value;
                OnUpgradeStatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void AddPotionEffect(PotionEffect effect)
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));

            if (potionEffects == null)
                potionEffects = new List<PotionEffect>();

            potionEffects.Add(effect);
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (potionEffects.Contains(null))
            {
                Debug.LogWarning($"[{name}] contains null entries in its PotionEffects list.");
            }
        }
#endif
    }
}
