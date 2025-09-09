using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Characters.Components;
using FourFatesStudios.ProjectWarden.Utilities;
using static FourFatesStudios.ProjectWarden.Utilities.PotionEffectApplicator;

namespace FourFatesStudios.ProjectWarden.Examples
{
    /// <summary>
    /// Example script showing how to consume potions using the new Effect system
    /// </summary>
    public class PotionConsumerExample : MonoBehaviour
    {
        [Header("Testing")]
        [SerializeField] private Potion testPotion;
        [SerializeField] private float qualityMultiplier = 1.0f;
        
        private CombatController combatController;

        private void Awake()
        {
            // Get or add a CombatController for testing
            combatController = GetComponent<CombatController>();
            if (combatController == null)
            {
                combatController = gameObject.AddComponent<CombatController>();
            }
        }

        [ContextMenu("Consume Test Potion")]
        public void ConsumeTestPotion()
        {
            if (testPotion == null)
            {
                Debug.LogWarning("No test potion assigned!");
                return;
            }

            ConsumePotion(testPotion);
        }

        [ContextMenu("Validate Test Potion")]
        public void ValidateTestPotion()
        {
            if (testPotion == null)
            {
                Debug.LogWarning("No test potion assigned!");
                return;
            }

            PotionValidator.ValidateAndLog(testPotion);
        }

        [ContextMenu("Show Potion Effect Summary")]
        public void ShowPotionEffectSummary()
        {
            if (testPotion == null)
            {
                Debug.LogWarning("No test potion assigned!");
                return;
            }

            string summary = PotionValidator.GetEffectSummary(testPotion);
            Debug.Log(summary);
        }

        /// <summary>
        /// Consume a potion and apply its effects to this character
        /// </summary>
        public void ConsumePotion(Potion potion)
        {
            if (potion == null)
            {
                Debug.LogWarning("Cannot consume null potion");
                return;
            }

            Debug.Log($"Consuming potion: {potion.name}");

            // Check if potion can be used in current context
            bool canUse = PotionEffectApplicator.CanUsePotionInContext(potion, PotionUsageContext.OutOfCombat);
            if (!canUse)
            {
                Debug.LogWarning($"Potion {potion.name} cannot be used in current context");
                return;
            }

            // Apply the potion effects to self
            PotionEffectApplicator.ConsumePotionSelf(potion, combatController, qualityMultiplier);

            Debug.Log($"Successfully consumed {potion.name}");
        }

        /// <summary>
        /// Use a potion on another target
        /// </summary>
        public void UsePotionOnTarget(Potion potion, CombatController target)
        {
            if (potion == null || target == null)
            {
                Debug.LogWarning("Cannot use potion: null potion or target");
                return;
            }

            Debug.Log($"Using potion {potion.name} on {target.gameObject.name}");

            // Check if it's a throwable potion
            if (potion.IsThrowable)
            {
                Debug.Log($"Throwing potion with range {potion.ThrowRange}");
                
                if (potion.AffectsArea)
                {
                    Debug.Log($"Area of effect: {potion.AreaRadius}");
                    // In a real implementation, you'd find all targets in the area
                }
            }

            // Apply the potion effects
            PotionEffectApplicator.ApplyPotionToTarget(potion, combatController, target, qualityMultiplier);
        }

        /// <summary>
        /// Example of batch applying a potion to multiple targets
        /// </summary>
        public void UsePotionOnGroup(Potion potion, CombatController[] targets)
        {
            if (potion == null || targets == null || targets.Length == 0)
            {
                Debug.LogWarning("Cannot use potion: null potion or no targets");
                return;
            }

            Debug.Log($"Using potion {potion.name} on {targets.Length} targets");

            var targetList = new System.Collections.Generic.List<CombatController>(targets);
            PotionEffectApplicator.ApplyPotionEffects(potion, combatController, targetList, qualityMultiplier);
        }

        /// <summary>
        /// Example of checking potion properties
        /// </summary>
        [ContextMenu("Analyze Test Potion")]
        public void AnalyzeTestPotion()
        {
            if (testPotion == null)
            {
                Debug.LogWarning("No test potion assigned!");
                return;
            }

            Debug.Log("=== Potion Analysis ===");
            Debug.Log($"Name: {testPotion.ItemName}");
            Debug.Log($"Description: {testPotion.ItemDescription}");
            Debug.Log($"Rarity: {testPotion.Rarity}");
            Debug.Log($"Quality: {testPotion.CraftQuality:F2}");
            Debug.Log($"Can use in combat: {testPotion.CanBeUsedInCombat}");
            Debug.Log($"Can use out of combat: {testPotion.CanBeUsedOutOfCombat}");
            Debug.Log($"Can use in exploration: {testPotion.CanBeUsedInExploration}");
            Debug.Log($"Is throwable: {testPotion.IsThrowable}");
            Debug.Log($"Affects area: {testPotion.AffectsArea}");
            Debug.Log($"Visual description: {testPotion.GetVisualDescription()}");
            
            if (PotionValidator.IsValid(testPotion))
            {
                Debug.Log("✅ Potion is properly configured");
            }
            else
            {
                Debug.Log("⚠️ Potion has no effects configured");
            }
        }
    }
}