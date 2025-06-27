using System;
using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.GameSystems.Crafting
{
    [Serializable]
    public class CraftedPotion
    {
        public string Name;
        public List<PotionEffect> Effects = new();
        public bool Upgraded;

        // Optional fields for better metadata support:
        public ItemPotionType PotionType;
        public DateTime CraftedTime = DateTime.Now;

        // Cache for signature (used in inventory comparison or saving)
        private string _cachedSignature;

        /// <summary>
        /// Returns a unique string signature for the potion based on its effect composition.
        /// Used for comparison and stacking in runtime inventories.
        /// </summary>
        public string GetEffectSignature()
        {
            if (_cachedSignature == null)
            {
                _cachedSignature = string.Join(
                    ";",
                    Effects
                        .Where(e => e != null)
                        .OrderBy(e => e.name)
                        .Select(e => e.name)
                );
            }

            return _cachedSignature;
        }

        public override string ToString()
        {
            var effectList = string.Join(", ", Effects.Select(e => e?.name ?? "null"));
            return $"Potion: {Name}, Effects: [{effectList}], Upgraded: {Upgraded}";
        }
    }
}
