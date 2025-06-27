using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

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
        public bool IsStackableWith(Potion other)
        {
            if (other == null) return false;
            if (ItemPotionType != other.ItemPotionType) return false;
            if (Upgraded != other.Upgraded) return false;
            if (PotionEffects.Count != other.PotionEffects.Count) return false;

            for (int i = 0; i < PotionEffects.Count; i++)
            {
                if (!PotionEffects[i].Equals(other.PotionEffects[i]))
                    return false;
            }

            return true;
        }

        public string GetEffectSignature()
        {
            StringBuilder builder = new();

            builder.Append(ItemPotionType.ToString());
            builder.Append("_");
            builder.Append(Upgraded ? "U" : "N");

            if (PotionEffects != null)
            {
                foreach (var effect in PotionEffects.OrderBy(e => e.name))
                {
                    builder.Append("|");
                    builder.Append(effect.name); // assuming name is unique

                    builder.Append(":");
                    builder.Append(string.Join(",", effect.EffectTypes.Select(t => t.ToString())));
                    builder.Append(":");
                    builder.Append(string.Join(",", effect.EffectAspects.Select(a => a.ToString())));
                    builder.Append(":");
                    builder.Append(string.Join(",", effect.Potencies.Select(p => p.ToString("F2"))));
                    builder.Append(":");
                    builder.Append(string.Join(",", effect.Durations.Select(d => d.ToString("F2"))));
                }
            }

            return ComputeHash(builder.ToString());
        }

        private string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return System.Convert.ToBase64String(hash);
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
