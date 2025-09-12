using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.Structs;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    [CreateAssetMenu(fileName = "NewSyntheticIngredient", menuName = "Items/Synthetic Ingredient")]
    public class SyntheticIngredient : Ingredient
    {
        [Header("Synthetic Properties")]
        [SerializeField, Tooltip("The ingredients that were combined to create this synthetic.")]
        private List<Ingredient> sourceIngredients = new List<Ingredient>();
        
        [SerializeField, Tooltip("The aspects that were combined, affecting the result.")]
        private List<Aspect> combinedAspects = new List<Aspect>();
        
        [SerializeField, Tooltip("Whether this synthetic has unpredictable effects.")]
        private bool hasUnpredictableEffects = true;
        
        [SerializeField, Tooltip("Stability rating (0-100). Lower = more volatile.")]
        [Range(0, 100)]
        private int stability = 50;
        
        [SerializeField, Tooltip("Can this synthetic be refined further?")]
        private bool canBeRefined = true;

        public IReadOnlyList<Ingredient> SourceIngredients => sourceIngredients;
        public IReadOnlyList<Aspect> CombinedAspects => combinedAspects;
        public bool HasUnpredictableEffects => hasUnpredictableEffects;
        public int Stability => stability;
        public bool CanBeRefined => canBeRefined;

        /// <summary>
        /// Create a synthetic ingredient from a failed crafting attempt
        /// </summary>
        public static SyntheticIngredient CreateFromFailedCrafting(List<Ingredient> failedIngredients, string recipeName = "Unknown")
        {
            var synthetic = CreateInstance<SyntheticIngredient>();
            
            // Set basic properties based on source ingredients
            synthetic.sourceIngredients = new List<Ingredient>(failedIngredients);
            synthetic.combinedAspects = failedIngredients.Select(i => i.IngredientAspect).Distinct().ToList();
            
            // Generate synthetic properties
            synthetic.GenerateProperties(recipeName);
            
            return synthetic;
        }
        
        private void GenerateProperties(string recipeName)
        {
            // Generate a unique name based on source ingredients
            GenerateName(recipeName);
            
            // Determine primary aspect (most common among sources)
            var aspectCounts = sourceIngredients.GroupBy(i => i.IngredientAspect)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key);
            
            var primaryAspect = aspectCounts.First().Key;
            
            // Set synthetic properties
            // Using reflection to set private fields since they're from base class
            var ingredientAspectField = typeof(Ingredient).GetField("ingredientAspect", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ingredientAspectField?.SetValue(this, primaryAspect);
            
            // Determine grid size (average of sources, minimum 1x1)
            int avgWidth = Mathf.Max(1, Mathf.RoundToInt((float)sourceIngredients.Average(i => i.GridWidth)));
            int avgHeight = Mathf.Max(1, Mathf.RoundToInt((float)sourceIngredients.Average(i => i.GridHeight)));
            
            var gridWidthField = typeof(Ingredient).GetField("gridWidth", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var gridHeightField = typeof(Ingredient).GetField("gridHeight", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            gridWidthField?.SetValue(this, avgWidth);
            gridHeightField?.SetValue(this, avgHeight);
            
            // Set potency (reduced from sources due to instability)
            int avgPotency = Mathf.Max(1, Mathf.RoundToInt((float)sourceIngredients.Average(i => i.Potency) * 0.7f));
            var potencyField = typeof(Ingredient).GetField("potency", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            potencyField?.SetValue(this, avgPotency);
            
            // Determine stability based on aspect conflicts
            CalculateStability();
            
            // Generate description
            GenerateDescription();
        }
        
        private void GenerateName(string recipeName)
        {
            var nameField = typeof(Item).GetField("itemName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            string syntheticName;
            
            if (sourceIngredients.Count == 1)
            {
                syntheticName = $"Altered {sourceIngredients[0].ItemName}";
            }
            else if (sourceIngredients.Count == 2)
            {
                syntheticName = $"{sourceIngredients[0].Adjective}-{sourceIngredients[1].Noun} Compound";
            }
            else
            {
                syntheticName = $"Complex Synthetic #{Random.Range(100, 999)}";
            }
            
            nameField?.SetValue(this, syntheticName);
        }
        
        private void CalculateStability()
        {
            stability = 100;
            
            // Reduce stability for conflicting aspects
            var conflictingPairs = new Dictionary<Aspect, Aspect>
            {
                { Aspect.Scorch, Aspect.Frigid },
                { Aspect.Divine, Aspect.Caustic }
            };
            
            foreach (var aspect1 in combinedAspects)
            {
                foreach (var aspect2 in combinedAspects)
                {
                    if (aspect1 != aspect2 && 
                        conflictingPairs.ContainsKey(aspect1) && 
                        conflictingPairs[aspect1] == aspect2)
                    {
                        stability -= 25; // Major instability
                    }
                }
            }
            
            // Reduce stability for too many different aspects
            if (combinedAspects.Count > 3)
            {
                stability -= (combinedAspects.Count - 3) * 10;
            }
            
            // Ensure minimum stability
            stability = Mathf.Max(10, stability);
            
            // Set unpredictable effects based on stability
            hasUnpredictableEffects = stability < 70;
        }
        
        private void GenerateDescription()
        {
            var descriptionField = typeof(Item).GetField("itemDescription", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            string description = "A synthetic compound created from failed alchemical experimentation. ";
            
            if (hasUnpredictableEffects)
            {
                description += "Its effects are unpredictable and may vary with each use. ";
            }
            
            if (stability < 30)
            {
                description += "HIGHLY UNSTABLE - Handle with extreme caution!";
            }
            else if (stability < 60)
            {
                description += "Somewhat unstable - use with care.";
            }
            else
            {
                description += "Relatively stable for a synthetic compound.";
            }
            
            if (canBeRefined)
            {
                description += " May be refined to extract useful components.";
            }
            
            descriptionField?.SetValue(this, description);
        }
        
        /// <summary>
        /// Get the potential effects when using this synthetic ingredient
        /// </summary>
        public List<string> GetPotentialEffects()
        {
            var effects = new List<string>();
            
            // Base effects from source ingredients
            foreach (var source in sourceIngredients)
            {
                if (source.InfusionBundle?.Infusions?.Count > 0)
                {
                    foreach (var infusion in source.InfusionBundle.Infusions)
                    {
                        effects.Add($"Weakened {infusion.InfusionName}");
                    }
                }
            }
            
            // Unpredictable effects
            if (hasUnpredictableEffects)
            {
                effects.Add("Random Minor Effect");
                
                if (stability < 40)
                {
                    effects.Add("Possible Negative Side Effect");
                }
            }
            
            return effects;
        }
        
        /// <summary>
        /// Check if this synthetic can interact with a specific ingredient
        /// </summary>
        public bool CanInteractWith(Ingredient other)
        {
            // Synthetics can interact with ingredients they were derived from
            if (sourceIngredients.Contains(other))
                return true;
            
            // Or with ingredients of similar aspects
            if (combinedAspects.Contains(other.IngredientAspect))
                return true;
            
            return false;
        }

#if UNITY_EDITOR
        [ContextMenu("Randomize Stability")]
        private void RandomizeStability()
        {
            stability = Random.Range(10, 90);
            hasUnpredictableEffects = stability < 70;
            
            Debug.Log($"Stability set to {stability}%. Unpredictable effects: {hasUnpredictableEffects}");
        }
        
        [ContextMenu("Log Potential Effects")]
        private void LogPotentialEffects()
        {
            var effects = GetPotentialEffects();
            Debug.Log($"Potential effects for {ItemName}:\n{string.Join("\n- ", effects)}");
        }
#endif
    }
}