using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.GridDemo;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes
{
    [Serializable]
    public class RequiredIngredientPosition
    {
        [SerializeField] public Vector2Int gridPosition;
        [SerializeField] public Ingredient requiredIngredient;
        [SerializeField] public bool mustBeExactPosition = true;
        [SerializeField] public bool allowsAdjacency = false;
        
        public RequiredIngredientPosition(Vector2Int position, Ingredient ingredient, bool exactPosition = true, bool allowAdjacency = false)
        {
            gridPosition = position;
            requiredIngredient = ingredient;
            mustBeExactPosition = exactPosition;
            allowsAdjacency = allowAdjacency;
        }
    }

    [Serializable]
    public class RequiredIngredientPattern
    {
        [SerializeField] public string patternName;
        [SerializeField] public List<Vector2Int> positions = new List<Vector2Int>();
        [SerializeField] public Ingredient requiredIngredient;
        [SerializeField] public bool isOptional = false;
        [SerializeField] public float successBonus = 0.1f;
        
        public RequiredIngredientPattern(string name, List<Vector2Int> patternPositions, Ingredient ingredient)
        {
            patternName = name;
            positions = patternPositions;
            requiredIngredient = ingredient;
        }
    }

    [Serializable]
    public class AspectSynergy
    {
        [SerializeField] public Aspect primaryAspect;
        [SerializeField] public Aspect synergyAspect;
        [SerializeField] public float synergyMultiplier = 1.2f;
        [SerializeField] public bool requiresAdjacency = true;
        [SerializeField] public string synergyDescription;
    }

    [Serializable]
    public class GridRequirement
    {
        [SerializeField] public int minimumIngredientsUsed = 2;
        [SerializeField] public int maximumIngredientsUsed = 10;
        [SerializeField] public bool requiresSymmetry = false;
        [SerializeField] public bool requiresCompactness = false;
        [SerializeField] public float compactnessThreshold = 0.7f;
    }

    [Serializable]
    public class PlannedObstacle
    {
        [SerializeField] public Vector2Int position;
        [SerializeField] public ObstacleType obstacleType;
        [SerializeField, TextArea(1, 2)] public string description = "";
        
        public PlannedObstacle(Vector2Int pos, ObstacleType type, string desc = "")
        {
            position = pos;
            obstacleType = type;
            description = desc;
        }
    }

    [Serializable]
    public class CustomGridCell
    {
        [SerializeField] public Vector2Int position;
        [SerializeField] public Aspect aspect = Aspect.Corporeal;
        [SerializeField] public Rarity rarity = Rarity.Common;
        [SerializeField] public bool isRequired = false;
        [SerializeField] public bool isOccupied = false;
        [SerializeField] public ObstacleType obstacleType = ObstacleType.Corporeal;
        [SerializeField] public bool hasObstacle = false;
        
        public CustomGridCell(Vector2Int pos)
        {
            position = pos;
        }
        
        public CustomGridCell(Vector2Int pos, Aspect cellAspect, Rarity cellRarity, bool required, bool occupied)
        {
            position = pos;
            aspect = cellAspect;
            rarity = cellRarity;
            isRequired = required;
            isOccupied = occupied;
        }
        
        public CustomGridCell(Vector2Int pos, ObstacleType obstacleType)
        {
            position = pos;
            this.obstacleType = obstacleType;
            hasObstacle = true;
        }
    }

    [CreateAssetMenu(fileName = "NewAlchemyRecipe", menuName = "AlchemyRecipes/Grid Recipe")]
    public class AlchemyRecipe : Recipe
    {
        [Header("Core Ingredients")]
        [SerializeField, Tooltip("Primary ingredient (required).")]
        private Ingredient inputIngredient1;
        [SerializeField, Tooltip("Secondary ingredient (required).")]
        private Ingredient inputIngredient2;
        [SerializeField, Tooltip("Tertiary ingredient (optional for advanced recipes).")]
        private Ingredient inputIngredient3;

        [Header("Output")]
        [SerializeField, Tooltip("Item created when recipe succeeds (Potion or Ingredient only).")]
        private Item outputItem;
        
        [SerializeField, Tooltip("Number of items created on success.")]
        [Range(1, 5)]
        private int outputQuantity = 1;

        [Header("Grid-Based Recipe Configuration")]
        [SerializeField, Tooltip("Is this a key/story recipe that prevents failure?")]
        private bool isKeyRecipe = false;
        
        [SerializeField, Tooltip("Recipe difficulty level affecting success thresholds.")]
        private RecipeDifficulty difficulty = RecipeDifficulty.Standard;
        
        [SerializeField, Tooltip("Required grid positions for specific ingredients.")]
        private List<RequiredIngredientPosition> requiredPositions = new List<RequiredIngredientPosition>();
        
        [SerializeField, Tooltip("Spatial patterns that improve success rate.")]
        private List<RequiredIngredientPattern> bonusPatterns = new List<RequiredIngredientPattern>();
        
        [SerializeField, Tooltip("Aspect synergies that provide bonuses.")]
        private List<AspectSynergy> aspectSynergies = new List<AspectSynergy>();
        
        [SerializeField, Tooltip("Pre-planned obstacles that will be placed when this recipe is loaded.")]
        private List<PlannedObstacle> plannedObstacles = new List<PlannedObstacle>();
        
        [Header("Custom Grid Layout")]
        [SerializeField, Tooltip("Has a custom grid layout designed in the Grid Designer.")]
        private bool hasCustomGrid = false;
        
        [SerializeField, Tooltip("Grid width for the custom layout.")]
        private int customGridWidth = 5;
        
        [SerializeField, Tooltip("Grid height for the custom layout.")]
        private int customGridHeight = 5;
        
        [SerializeField, Tooltip("Custom grid cells containing obstacles and ingredient positions.")]
        private List<CustomGridCell> customGridCells = new List<CustomGridCell>();
        
        [Header("Success Criteria")]
        [SerializeField, Tooltip("Minimum space efficiency required for success (0-1).")]
        [Range(0f, 1f)]
        private float minimumEfficiency = 0.6f;
        
        [SerializeField, Tooltip("Grid arrangement requirements.")]
        private GridRequirement gridRequirements = new GridRequirement();
        
        [SerializeField, Tooltip("Does this recipe benefit from ingredient interactions?")]
        private bool allowsIngredientInteractions = true;
        
        [SerializeField, Tooltip("Forbidden ingredient combinations that cause failure.")]
        private List<Ingredient> forbiddenIngredients = new List<Ingredient>();
        
        [SerializeField, Tooltip("Alternative ingredients that can substitute for main ingredients.")]
        private List<Ingredient> alternativeIngredients = new List<Ingredient>();
        
        [Header("Advanced Features")]
        [SerializeField, Tooltip("Custom success condition script for complex recipes.")]
        private MonoBehaviour customSuccessEvaluator;
        
        [SerializeField, Tooltip("Recipe hints for the player.")]
        [TextArea(2, 4)]
        private string recipeHints = "Arrange ingredients to create a stable reaction.";

        // Public Properties for GridMinigameController Integration
        public Ingredient InputIngredient1 => inputIngredient1;
        public Ingredient InputIngredient2 => inputIngredient2;
        public Ingredient InputIngredient3 => inputIngredient3;
        public Item OutputItem => outputItem;
        public Potion OutputPotion => outputItem as Potion;
        public Ingredient OutputIngredient => outputItem as Ingredient;
        public int OutputQuantity => outputQuantity;
        
        public bool IsKeyRecipe => isKeyRecipe;
        public RecipeDifficulty Difficulty => difficulty;
        public float MinimumEfficiency => minimumEfficiency;
        public bool AllowsIngredientInteractions => allowsIngredientInteractions;
        public string RecipeHints => recipeHints;
        
        public IReadOnlyList<RequiredIngredientPosition> RequiredPositions => requiredPositions;
        public IReadOnlyList<RequiredIngredientPattern> BonusPatterns => bonusPatterns;
        public IReadOnlyList<AspectSynergy> AspectSynergies => aspectSynergies;
        public IReadOnlyList<PlannedObstacle> PlannedObstacles => plannedObstacles;
        public GridRequirement GridRequirements => gridRequirements;
        public IReadOnlyList<Ingredient> ForbiddenIngredients => forbiddenIngredients;
        public IReadOnlyList<Ingredient> AlternativeIngredients => alternativeIngredients;
        
        // Custom Grid Properties
        public bool HasCustomGrid => hasCustomGrid;
        public int CustomGridWidth => customGridWidth;
        public int CustomGridHeight => customGridHeight;
        public IReadOnlyList<CustomGridCell> CustomGridCells => customGridCells;

        /// <summary>
        /// Get all required ingredients for this recipe (excluding null entries)
        /// </summary>
        public List<Ingredient> GetRequiredIngredients()
        {
            var ingredients = new List<Ingredient>();
            if (inputIngredient1 != null) ingredients.Add(inputIngredient1);
            if (inputIngredient2 != null) ingredients.Add(inputIngredient2);
            if (inputIngredient3 != null) ingredients.Add(inputIngredient3);
            return ingredients;
        }

        /// <summary>
        /// Check if an ingredient is required for this recipe
        /// </summary>
        public bool RequiresIngredient(Ingredient ingredient)
        {
            return ingredient == inputIngredient1 || 
                   ingredient == inputIngredient2 || 
                   ingredient == inputIngredient3 ||
                   alternativeIngredients.Contains(ingredient);
        }

        /// <summary>
        /// Check if an ingredient is forbidden in this recipe
        /// </summary>
        public bool IsForbiddenIngredient(Ingredient ingredient)
        {
            return forbiddenIngredients.Contains(ingredient);
        }

        /// <summary>
        /// Get the minimum efficiency based on difficulty
        /// </summary>
        public float GetAdjustedMinimumEfficiency()
        {
            return difficulty switch
            {
                RecipeDifficulty.Beginner => minimumEfficiency * 0.8f,
                RecipeDifficulty.Standard => minimumEfficiency,
                RecipeDifficulty.Advanced => minimumEfficiency * 1.2f,
                RecipeDifficulty.Master => minimumEfficiency * 1.5f,
                _ => minimumEfficiency
            };
        }

        /// <summary>
        /// Check if a specific pattern is satisfied by the given ingredient positions
        /// </summary>
        public bool CheckPatternSatisfied(RequiredIngredientPattern pattern, Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            foreach (var position in pattern.positions)
            {
                if (!placedIngredients.ContainsKey(position) || 
                    placedIngredients[position] != pattern.requiredIngredient)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Calculate success bonus from satisfied patterns
        /// </summary>
        public float CalculatePatternBonus(Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            float totalBonus = 0f;
            foreach (var pattern in bonusPatterns)
            {
                if (CheckPatternSatisfied(pattern, placedIngredients))
                {
                    totalBonus += pattern.successBonus;
                }
            }
            return totalBonus;
        }

        /// <summary>
        /// Calculate aspect synergy bonuses
        /// </summary>
        public float CalculateAspectSynergyBonus(Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            float totalBonus = 0f;
            
            foreach (var synergy in aspectSynergies)
            {
                if (HasAspectSynergy(synergy, placedIngredients))
                {
                    totalBonus += (synergy.synergyMultiplier - 1.0f);
                }
            }
            
            return totalBonus;
        }

        private bool HasAspectSynergy(AspectSynergy synergy, Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            var primaryPositions = new List<Vector2Int>();
            var synergyPositions = new List<Vector2Int>();

            // Find positions of each aspect
            foreach (var kvp in placedIngredients)
            {
                if (kvp.Value.IngredientAspect == synergy.primaryAspect)
                    primaryPositions.Add(kvp.Key);
                else if (kvp.Value.IngredientAspect == synergy.synergyAspect)
                    synergyPositions.Add(kvp.Key);
            }

            if (primaryPositions.Count == 0 || synergyPositions.Count == 0)
                return false;

            // Check adjacency if required
            if (synergy.requiresAdjacency)
            {
                foreach (var primaryPos in primaryPositions)
                {
                    foreach (var synergyPos in synergyPositions)
                    {
                        if (Vector2Int.Distance(primaryPos, synergyPos) <= 1.5f) // Adjacent (including diagonal)
                            return true;
                    }
                }
                return false;
            }

            return true; // Synergy exists without adjacency requirement
        }

        /// <summary>
        /// Save custom grid layout from the Grid Designer
        /// </summary>
        public void SaveCustomGridLayout(int gridWidth, int gridHeight)
        {
            customGridCells.Clear();
            customGridWidth = gridWidth;
            customGridHeight = gridHeight;
            hasCustomGrid = true;
            
            Debug.Log($"💾 Initialized custom grid layout ({gridWidth}x{gridHeight}) for recipe '{name}' - use SaveCustomGridCell to add cells");
        }
        
        /// <summary>
        /// Add a custom grid cell (called from Grid Designer)
        /// </summary>
        public void SaveCustomGridCell(Vector2Int position, Aspect aspect, Rarity rarity, bool isRequired, bool isOccupied, ObstacleType obstacleType, bool hasObstacle)
        {
            // Remove existing cell at this position
            customGridCells.RemoveAll(c => c.position == position);
            
            // Only save if there's meaningful data (not default values)
            if (hasObstacle || aspect != Aspect.Corporeal || rarity != Rarity.Common || isRequired)
            {
                var gridCell = new CustomGridCell(position);
                gridCell.aspect = aspect;
                gridCell.rarity = rarity;
                gridCell.isRequired = isRequired;
                gridCell.isOccupied = isOccupied;
                gridCell.obstacleType = obstacleType;
                gridCell.hasObstacle = hasObstacle;
                
                customGridCells.Add(gridCell);
            }
        }
        
        /// <summary>
        /// Get custom grid data for a specific position
        /// </summary>
        public CustomGridCell GetCustomGridCell(Vector2Int position)
        {
            return customGridCells.FirstOrDefault(c => c.position == position);
        }
        
        /// <summary>
        /// Check if this recipe has custom grid data available
        /// </summary>
        public bool HasCustomGridData()
        {
            return hasCustomGrid && customGridCells.Count > 0;
        }
        
        /// <summary>
        /// Get grid information for the crafting mode selector
        /// </summary>
        public (int gridWidth, int gridHeight, int obstacleCount) GetGridInfo()
        {
            int obstacleCount = customGridCells.Count(cell => cell.hasObstacle);
            return (customGridWidth, customGridHeight, obstacleCount);
        }
        
        /// <summary>
        /// Clear the custom grid layout
        /// </summary>
        public void ClearCustomGridLayout()
        {
            hasCustomGrid = false;
            customGridCells.Clear();
            Debug.Log($"🗑️ Cleared custom grid layout for recipe '{name}'");
        }
        
        /// <summary>
        /// Check if player has access to this recipe's custom grid
        /// This should be called when the player tries to use a custom grid layout
        /// </summary>
        public bool CanPlayerUseCustomGrid()
        {
            // TODO: Implement inventory system check
            // For now, return true since inventory system is not implemented
            // When inventory is implemented, check if player has this recipe page in inventory
            
            /* FUTURE IMPLEMENTATION:
             * if (InventoryManager.Instance != null)
             * {
             *     return InventoryManager.Instance.HasRecipePage(this);
             * }
             */
            
            Debug.Log($"🔍 Checking custom grid access for recipe '{name}' - returning true (inventory not implemented)");
            return true; // Placeholder - always allow access until inventory is implemented
        }

#if UNITY_EDITOR
        private new void OnValidate()
        {
            // Validate core ingredients
            if (inputIngredient1 == null)
                Debug.LogWarning($"[{name}] Primary ingredient (Input 1) is required.");
            if (inputIngredient2 == null)
                Debug.LogWarning($"[{name}] Secondary ingredient (Input 2) is required.");

            if (outputItem == null)
            {
                Debug.LogWarning($"[{name}] Output Item is not assigned.");
            }
            else if (outputItem is Potion)
            {
                // Potion output is valid
            }
            else if (outputItem is Ingredient ingredient)
            {
                // Only Synthetic ingredients are allowed as outputs
                if (ingredient.IngredientArchetype != IngredientArchetype.Synthetic)
                {
                    Debug.LogError($"[{name}] Output Ingredient must be of type Synthetic. Current type: {ingredient.IngredientArchetype}");
                    outputItem = null;
                }
            }
            else
            {
                Debug.LogError($"[{name}] Output Item must be either a Potion or Synthetic Ingredient. Current type: {outputItem.GetType().Name}");
                outputItem = null;
            }

            // Validate output quantity
            if (outputQuantity <= 0)
                outputQuantity = 1;

            // Validate efficiency thresholds
            if (minimumEfficiency < 0.1f)
                Debug.LogWarning($"[{name}] Minimum efficiency is very low ({minimumEfficiency:P}). Consider increasing for better gameplay.");
            
            if (minimumEfficiency > 0.95f)
                Debug.LogWarning($"[{name}] Minimum efficiency is very high ({minimumEfficiency:P}). This might make the recipe too difficult.");

            // Validate grid requirements
            if (gridRequirements.minimumIngredientsUsed < 1)
                gridRequirements.minimumIngredientsUsed = 1;
            
            if (gridRequirements.maximumIngredientsUsed < gridRequirements.minimumIngredientsUsed)
                gridRequirements.maximumIngredientsUsed = gridRequirements.minimumIngredientsUsed;

            // Validate key recipe configuration
            if (isKeyRecipe && requiredPositions.Count == 0)
                Debug.LogWarning($"[{name}] Key recipe should have required ingredient positions defined for proper constraints.");

            // Check for conflicting forbidden and required ingredients
            var requiredIngredients = GetRequiredIngredients();
            foreach (var forbidden in forbiddenIngredients)
            {
                if (requiredIngredients.Contains(forbidden))
                    Debug.LogError($"[{name}] Ingredient '{forbidden.name}' cannot be both required and forbidden!");
            }

            // Validate patterns
            foreach (var pattern in bonusPatterns)
            {
                if (pattern.positions.Count == 0)
                    Debug.LogWarning($"[{name}] Pattern '{pattern.patternName}' has no positions defined.");
                
                if (pattern.successBonus < 0f)
                    Debug.LogWarning($"[{name}] Pattern '{pattern.patternName}' has negative bonus. Consider using positive values.");
            }

            // Validate aspect synergies
            foreach (var synergy in aspectSynergies)
            {
                if (synergy.primaryAspect == synergy.synergyAspect)
                    Debug.LogWarning($"[{name}] Aspect synergy has same primary and synergy aspect ({synergy.primaryAspect}). This is redundant.");
                
                if (synergy.synergyMultiplier <= 0f)
                    Debug.LogWarning($"[{name}] Synergy multiplier should be positive (current: {synergy.synergyMultiplier}).");
            }

            // Generate recipe preview for debugging
            if (inputIngredient1 != null && inputIngredient2 != null)
            {
                var ingredients = GetRequiredIngredients();
                ingredients.Sort((a, b) => String.Compare(a.name, b.name, StringComparison.Ordinal));
                
                string preview = $"[{name}] Recipe: {string.Join(", ", ingredients.Select(i => i.name))}";
                preview += $" -> {(outputItem != null ? outputItem.name : "Unknown")}";
                
                if (isKeyRecipe) preview += " (KEY RECIPE)";
                if (difficulty != RecipeDifficulty.Standard) preview += $" [{difficulty}]";
                
                Debug.Log(preview);
            }

            // Validate recipe hints
            if (string.IsNullOrWhiteSpace(recipeHints))
                recipeHints = "Arrange ingredients to create a stable reaction.";
        }
#endif
    }
}