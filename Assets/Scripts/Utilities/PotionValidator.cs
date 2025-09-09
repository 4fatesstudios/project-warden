using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace FourFatesStudios.ProjectWarden.Utilities
{
    /// <summary>
    /// Simple utility for validating potions in the new effect system
    /// </summary>
    public static class PotionValidator
    {
        /// <summary>
        /// Check if a potion is properly configured
        /// </summary>
        public static bool IsValid(Potion potion)
        {
            if (potion == null)
                return false;

            // Potion is valid if it has an effect bundle with at least one effect
            return potion.EffectBundle?.Effects?.Count > 0;
        }

        /// <summary>
        /// Get a list of all invalid potions from a collection
        /// </summary>
        public static List<Potion> GetInvalidPotions(IEnumerable<Potion> potions)
        {
            var invalidPotions = new List<Potion>();
            
            foreach (var potion in potions)
            {
                if (!IsValid(potion))
                {
                    invalidPotions.Add(potion);
                }
            }
            
            return invalidPotions;
        }

        /// <summary>
        /// Get a debug summary of a potion's effects
        /// </summary>
        public static string GetEffectSummary(Potion potion)
        {
            if (potion == null)
                return "Invalid potion";

            var summary = $"Potion: {potion.name}\n";
            
            if (potion.EffectBundle?.Effects?.Count > 0)
            {
                summary += $"Effects ({potion.EffectBundle.Effects.Count}):\n";
                foreach (var effect in potion.EffectBundle.Effects)
                {
                    summary += $"  - {effect.GetType().Name}\n";
                }
            }
            else
            {
                summary += "No effects defined\n";
            }

            return summary;
        }

        /// <summary>
        /// Validate and log the status of a potion
        /// </summary>
        public static void ValidateAndLog(Potion potion)
        {
            if (IsValid(potion))
            {
                Debug.Log($"✅ Potion '{potion.name}' is valid with {potion.EffectBundle.Effects.Count} effects");
            }
            else
            {
                Debug.LogWarning($"⚠️ Potion '{potion?.name ?? "null"}' is invalid or has no effects");
            }
        }
    }
}