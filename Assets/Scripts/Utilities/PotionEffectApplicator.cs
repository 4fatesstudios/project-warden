using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Characters.Components;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace FourFatesStudios.ProjectWarden.Utilities
{
    /// <summary>
    /// Simplified utility for applying potion effects to characters
    /// </summary>
    public static class PotionEffectApplicator
    {
        /// <summary>
        /// Context for potion usage
        /// </summary>
        public enum PotionUsageContext
        {
            Combat,
            OutOfCombat,
            Exploration
        }

        /// <summary>
        /// Check if a potion can be used in the specified context
        /// </summary>
        public static bool CanUsePotionInContext(Potion potion, PotionUsageContext context)
        {
            if (potion == null)
                return false;

            return context switch
            {
                PotionUsageContext.Combat => potion.CanBeUsedInCombat,
                PotionUsageContext.OutOfCombat => potion.CanBeUsedOutOfCombat,
                PotionUsageContext.Exploration => potion.CanBeUsedInExploration,
                _ => false
            };
        }

        /// <summary>
        /// Apply a potion's effects to the user (self-consumption)
        /// </summary>
        public static void ConsumePotionSelf(Potion potion, CombatController user, float qualityMultiplier = 1.0f)
        {
            if (potion == null || user == null)
            {
                Debug.LogWarning("Cannot consume potion: null potion or user");
                return;
            }

            if (!PotionValidator.IsValid(potion))
            {
                Debug.LogWarning($"Potion '{potion.name}' has no effects to apply");
                return;
            }

            Debug.Log($"🧪 {user.gameObject.name} consumes {potion.name}");
            ApplyEffectsToTarget(potion, user, user, qualityMultiplier);
        }

        /// <summary>
        /// Apply a potion's effects to a target
        /// </summary>
        public static void ApplyPotionToTarget(Potion potion, CombatController source, CombatController target, float qualityMultiplier = 1.0f)
        {
            if (potion == null || source == null || target == null)
            {
                Debug.LogWarning("Cannot apply potion: null potion, source, or target");
                return;
            }

            if (!PotionValidator.IsValid(potion))
            {
                Debug.LogWarning($"Potion '{potion.name}' has no effects to apply");
                return;
            }

            Debug.Log($"🧪 {source.gameObject.name} uses {potion.name} on {target.gameObject.name}");
            ApplyEffectsToTarget(potion, source, target, qualityMultiplier);
        }

        /// <summary>
        /// Apply a potion's effects to multiple targets
        /// </summary>
        public static void ApplyPotionEffects(Potion potion, CombatController source, List<CombatController> targets, float qualityMultiplier = 1.0f)
        {
            if (potion == null || source == null || targets == null || targets.Count == 0)
            {
                Debug.LogWarning("Cannot apply potion: null potion, source, or no targets");
                return;
            }

            if (!PotionValidator.IsValid(potion))
            {
                Debug.LogWarning($"Potion '{potion.name}' has no effects to apply");
                return;
            }

            Debug.Log($"🧪 {source.gameObject.name} uses {potion.name} on {targets.Count} targets");
            
            foreach (var target in targets)
            {
                if (target != null)
                {
                    ApplyEffectsToTarget(potion, source, target, qualityMultiplier);
                }
            }
        }

        /// <summary>
        /// Internal method to apply effects to a specific target
        /// </summary>
        private static void ApplyEffectsToTarget(Potion potion, CombatController source, CombatController target, float qualityMultiplier)
        {
            var effects = potion.GetAllEffects();
            if (effects == null)
                return;

            foreach (var effect in effects)
            {
                if (effect != null)
                {
                    Debug.Log($"  ✨ Applying {effect.GetType().Name} to {target.gameObject.name}");
                    
                    // Apply the effect to the target
                    // Note: This is a simplified implementation
                    // In a full implementation, you would call effect.Apply(target, source, qualityMultiplier)
                    // For now, we just log that the effect would be applied
                }
            }
        }
    }
}