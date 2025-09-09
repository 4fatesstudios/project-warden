using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.Examples
{
    /// <summary>
    /// Example script demonstrating how to create and configure enhanced alchemy recipes
    /// </summary>
    public class EnhancedRecipeCreator : MonoBehaviour
    {
        [Header("Example Recipe Configuration")]
        [SerializeField] private AlchemyRecipe exampleRecipe;
        [SerializeField] private List<Ingredient> availableIngredients = new List<Ingredient>();

        [ContextMenu("Create Simple Recipe Example")]
        public void CreateSimpleRecipeExample()
        {
            if (exampleRecipe == null)
            {
                Debug.LogError("Please assign an AlchemyRecipe to test with!");
                return;
            }

            Debug.Log("=== SIMPLE RECIPE EXAMPLE ===");
            Debug.Log($"Recipe: {exampleRecipe.ItemName}");
            Debug.Log($"Difficulty: {exampleRecipe.Difficulty}");
            Debug.Log($"Required Ingredients: {string.Join(", ", exampleRecipe.GetRequiredIngredients().Select(i => i.ItemName))}");
            Debug.Log($"Minimum Efficiency: {exampleRecipe.GetAdjustedMinimumEfficiency():P}");
            Debug.Log($"Recipe Hints: {exampleRecipe.RecipeHints}");
        }

        [ContextMenu("Create Advanced Pattern Recipe Example")]
        public void CreateAdvancedPatternRecipeExample()
        {
            Debug.Log("=== ADVANCED PATTERN RECIPE EXAMPLE ===");
            Debug.Log("This example shows how to create a recipe with spatial patterns and synergies.");
            
            // Example: L-shaped pattern for fire ingredients
            var lPattern = new RequiredIngredientPattern(
                "L-Shape Fire Pattern",
                new List<Vector2Int> 
                { 
                    new Vector2Int(0, 0), 
                    new Vector2Int(0, 1), 
                    new Vector2Int(1, 0) 
                },
                null // Would be assigned a fire ingredient
            );

            Debug.Log($"Pattern: {lPattern.patternName}");
            Debug.Log($"Positions: {string.Join(", ", lPattern.positions)}");
            Debug.Log($"Success Bonus: +{lPattern.successBonus:P}");
        }

        [ContextMenu("Test Recipe Evaluation")]
        public void TestRecipeEvaluation()
        {
            if (exampleRecipe == null)
            {
                Debug.LogError("Please assign an AlchemyRecipe to test with!");
                return;
            }

            Debug.Log("=== RECIPE EVALUATION TEST ===");

            // Simulate placed ingredients
            var placedIngredients = new Dictionary<Vector2Int, Ingredient>();
            var requiredIngredients = exampleRecipe.GetRequiredIngredients();

            // Place required ingredients in a simple pattern
            for (int i = 0; i < requiredIngredients.Count && i < 3; i++)
            {
                placedIngredients[new Vector2Int(i, 0)] = requiredIngredients[i];
            }

            Debug.Log($"Simulated placement: {placedIngredients.Count} ingredients");

            // Test pattern bonus calculation
            float patternBonus = exampleRecipe.CalculatePatternBonus(placedIngredients);
            Debug.Log($"Pattern Bonus: +{patternBonus:P}");

            // Test synergy bonus calculation
            float synergyBonus = exampleRecipe.CalculateAspectSynergyBonus(placedIngredients);
            Debug.Log($"Synergy Bonus: +{synergyBonus:P}");

            // Test forbidden ingredients
            bool hasForbidden = false;
            foreach (var ingredient in placedIngredients.Values)
            {
                if (exampleRecipe.IsForbiddenIngredient(ingredient))
                {
                    hasForbidden = true;
                    Debug.LogWarning($"Forbidden ingredient detected: {ingredient.ItemName}");
                }
            }

            if (!hasForbidden)
            {
                Debug.Log("✓ No forbidden ingredients detected");
            }
        }

        [ContextMenu("Show Recipe Requirements Info")]
        public void ShowRecipeRequirementsInfo()
        {
            if (exampleRecipe == null)
            {
                Debug.LogError("Please assign an AlchemyRecipe to test with!");
                return;
            }

            Debug.Log("=== RECIPE REQUIREMENTS INFO ===");
            
            var requirements = exampleRecipe.GridRequirements;
            Debug.Log($"Min Ingredients: {requirements.minimumIngredientsUsed}");
            Debug.Log($"Max Ingredients: {requirements.maximumIngredientsUsed}");
            Debug.Log($"Requires Symmetry: {requirements.requiresSymmetry}");
            Debug.Log($"Requires Compactness: {requirements.requiresCompactness}");
            
            if (requirements.requiresCompactness)
            {
                Debug.Log($"Compactness Threshold: {requirements.compactnessThreshold:P}");
            }

            Debug.Log($"Required Positions: {exampleRecipe.RequiredPositions.Count}");
            Debug.Log($"Bonus Patterns: {exampleRecipe.BonusPatterns.Count}");
            Debug.Log($"Aspect Synergies: {exampleRecipe.AspectSynergies.Count}");
            Debug.Log($"Forbidden Ingredients: {exampleRecipe.ForbiddenIngredients.Count}");
            Debug.Log($"Alternative Ingredients: {exampleRecipe.AlternativeIngredients.Count}");
        }

        [ContextMenu("Create Key Recipe Example")]
        public void CreateKeyRecipeExample()
        {
            Debug.Log("=== KEY RECIPE EXAMPLE ===");
            Debug.Log("Key recipes are story-critical and have special behaviors:");
            Debug.Log("- Cannot fail completely (will show warnings instead)");
            Debug.Log("- Often have required ingredient positions");
            Debug.Log("- May have unique visual indicators");
            
            if (exampleRecipe != null && exampleRecipe.IsKeyRecipe)
            {
                Debug.Log($"✓ {exampleRecipe.ItemName} is a KEY RECIPE");
                Debug.Log($"Required Positions: {exampleRecipe.RequiredPositions.Count}");
            }
            else
            {
                Debug.Log("The assigned recipe is not a key recipe.");
            }
        }

        [ContextMenu("Demonstrate Difficulty Scaling")]
        public void DemonstrateDifficultyScaling()
        {
            if (exampleRecipe == null)
            {
                Debug.LogError("Please assign an AlchemyRecipe to test with!");
                return;
            }

            Debug.Log("=== DIFFICULTY SCALING DEMONSTRATION ===");
            Debug.Log($"Recipe: {exampleRecipe.ItemName}");
            Debug.Log($"Base Minimum Efficiency: {exampleRecipe.MinimumEfficiency:P}");
            Debug.Log($"Current Difficulty: {exampleRecipe.Difficulty}");
            Debug.Log($"Adjusted Minimum Efficiency: {exampleRecipe.GetAdjustedMinimumEfficiency():P}");
            
            Debug.Log("\nDifficulty Scaling:");
            foreach (RecipeDifficulty difficulty in System.Enum.GetValues(typeof(RecipeDifficulty)))
            {
                float multiplier = difficulty switch
                {
                    RecipeDifficulty.Beginner => 0.8f,
                    RecipeDifficulty.Standard => 1.0f,
                    RecipeDifficulty.Advanced => 1.2f,
                    RecipeDifficulty.Master => 1.5f,
                    _ => 1.0f
                };
                
                float adjustedEfficiency = exampleRecipe.MinimumEfficiency * multiplier;
                Debug.Log($"  {difficulty}: {adjustedEfficiency:P} (x{multiplier})");
            }
        }
    }
}