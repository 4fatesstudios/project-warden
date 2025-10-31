using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.GridDemo;
using FourFatesStudios.ProjectWarden.GameSystems.SkillSystem;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu
{
    /// <summary>
    /// Enhanced grid system with dynamic expansion, layered placement, and efficiency tracking
    /// Implements the advanced grid features from the design document
    /// </summary>
    public class EnhancedGridSystem : MonoBehaviour
    {
        [Header("Grid Enhancement")]
        [SerializeField] private bool enableDynamicExpansion = true;
        [SerializeField] private bool enableLayeredPlacement = false;
        [SerializeField] private Vector2Int maxGridSize = new Vector2Int(7, 7);

        [Header("Layered Placement")]
        #pragma warning disable 0414
        [SerializeField] private int maxLayersPerCell = 3;
        #pragma warning restore 0414
        [SerializeField] private bool allowObstacleOverlap = false;

        [Header("Efficiency Tracking")]
        [SerializeField] private bool trackEfficiency = true;
        #pragma warning disable 0414
        [SerializeField] private float targetEfficiency = 0.75f;
        #pragma warning restore 0414

        // Grid state
        private Vector2Int currentGridSize;
        private Dictionary<Vector2Int, List<Ingredient>> layeredIngredients = new Dictionary<Vector2Int, List<Ingredient>>();
        private Dictionary<Vector2Int, bool> expandedCells = new Dictionary<Vector2Int, bool>();
        private Dictionary<Ingredient, Vector2Int> expansionSources = new Dictionary<Ingredient, Vector2Int>();

        // Efficiency tracking
        private float currentEfficiencyScore = 0f;
        private int totalCellsUsed = 0;
        private int totalCellsAvailable = 0;

        // Events
        public System.Action<Vector2Int> OnGridExpanded;
        public System.Action<Vector2Int, List<Ingredient>> OnLayeredPlacementChanged;
        public System.Action<float> OnEfficiencyUpdated;

        private GridGameManager gridManager;

        private void Start()
        {
            gridManager = GridGameManager.Instance;
            if (gridManager != null)
            {
                currentGridSize = new Vector2Int(gridManager.gridWidth, gridManager.gridHeight);
                InitializeEnhancedGrid();
            }
        }

        /// <summary>
        /// Initialize the enhanced grid system
        /// </summary>
        private void InitializeEnhancedGrid()
        {
            layeredIngredients.Clear();
            expandedCells.Clear();
            expansionSources.Clear();

            UpdateEfficiencyMetrics();

            Debug.Log($"🔄 Enhanced Grid System initialized - Size: {currentGridSize.x}x{currentGridSize.y}");
        }

        /// <summary>
        /// Try to expand the grid when an ingredient with expansion properties is placed
        /// </summary>
        public bool TryExpandGrid(Ingredient ingredient, int additionalSpaces)
        {
            if (!enableDynamicExpansion || !ingredient.UnlocksAdditionalSpace)
                return false;

            if (additionalSpaces <= 0)
                return false;

            // Check if we can expand within max limits
            var newSize = CalculateExpandedSize(additionalSpaces);
            if (newSize.x > maxGridSize.x || newSize.y > maxGridSize.y)
            {
                Debug.LogWarning($"🔄 Cannot expand grid beyond maximum size {maxGridSize.x}x{maxGridSize.y}");
                return false;
            }

            // Perform expansion
            var oldSize = currentGridSize;
            currentGridSize = newSize;

            // Update GridGameManager
            if (gridManager != null)
            {
                gridManager.gridWidth = currentGridSize.x;
                gridManager.gridHeight = currentGridSize.y;
            }

            // Track expansion source
            expansionSources[ingredient] = oldSize;

            // Mark new cells as expanded
            MarkExpandedCells(oldSize, newSize);

            OnGridExpanded?.Invoke(newSize);

            Debug.Log($"🔄 Grid expanded from {oldSize.x}x{oldSize.y} to {newSize.x}x{newSize.y} by {ingredient.ItemName}");
            
            UpdateEfficiencyMetrics();
            return true;
        }

        /// <summary>
        /// Calculate new grid size after expansion
        /// </summary>
        private Vector2Int CalculateExpandedSize(int additionalSpaces)
        {
            // Simple expansion: add spaces to both dimensions proportionally
            int spacesPerDimension = Mathf.CeilToInt(additionalSpaces / 2f);
            
            var newSize = new Vector2Int(
                Mathf.Min(currentGridSize.x + spacesPerDimension, maxGridSize.x),
                Mathf.Min(currentGridSize.y + spacesPerDimension, maxGridSize.y)
            );

            return newSize;
        }

        /// <summary>
        /// Mark cells as expanded for visual distinction
        /// </summary>
        private void MarkExpandedCells(Vector2Int oldSize, Vector2Int newSize)
        {
            // Mark cells that are outside the old grid as expanded
            for (int x = 0; x < newSize.x; x++)
            {
                for (int y = 0; y < newSize.y; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (x >= oldSize.x || y >= oldSize.y)
                    {
                        expandedCells[pos] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Check if layered placement is allowed at a position
        /// </summary>
        public bool CanOverlapAt(Vector2Int position, Ingredient ingredient)
        {
            if (!enableLayeredPlacement) return false;

            // Check skill tree for overlap ability
            var skillTree = GetComponent<AlchemySkillTree>();
            if (skillTree == null || skillTree.GetSkillValue("ingredient_overlap_1") <= 0)
                return false;

            // Get current layer count
            int currentLayers = GetLayerCount(position);
            int maxAllowed = GetMaxOverlapForPosition(position);

            return currentLayers < maxAllowed;
        }

        /// <summary>
        /// Get maximum overlap allowed at a position
        /// </summary>
        private int GetMaxOverlapForPosition(Vector2Int position)
        {
            var skillTree = GetComponent<AlchemySkillTree>();
            if (skillTree == null) return 0;

            // Check which overlap skills are unlocked
            if (skillTree.IsSkillUnlocked("ingredient_overlap_3"))
                return 3;
            if (skillTree.IsSkillUnlocked("ingredient_overlap_2"))
                return 2;
            if (skillTree.IsSkillUnlocked("ingredient_overlap_1"))
                return 1;

            return 0;
        }

        /// <summary>
        /// Get current layer count at a position
        /// </summary>
        private int GetLayerCount(Vector2Int position)
        {
            return layeredIngredients.ContainsKey(position) ? layeredIngredients[position].Count : 0;
        }

        /// <summary>
        /// Try to place an ingredient with layered placement
        /// </summary>
        public bool TryPlaceIngredientWithLayers(Ingredient ingredient, Vector2Int position)
        {
            if (!CanOverlapAt(position, ingredient))
                return false;

            // Check for obstacle restrictions
            if (!allowObstacleOverlap && HasObstacleAt(position))
                return false;

            // Add to layered ingredients
            if (!layeredIngredients.ContainsKey(position))
                layeredIngredients[position] = new List<Ingredient>();

            layeredIngredients[position].Add(ingredient);

            OnLayeredPlacementChanged?.Invoke(position, layeredIngredients[position]);

            Debug.Log($"📚 Layered placement: {ingredient.ItemName} added to position {position} (Layer {layeredIngredients[position].Count})");

            UpdateEfficiencyMetrics();
            return true;
        }

        /// <summary>
        /// Remove an ingredient from layered placement
        /// </summary>
        public bool RemoveIngredientFromLayers(Vector2Int position, Ingredient ingredient)
        {
            if (!layeredIngredients.ContainsKey(position))
                return false;

            bool removed = layeredIngredients[position].Remove(ingredient);
            if (removed)
            {
                if (layeredIngredients[position].Count == 0)
                    layeredIngredients.Remove(position);

                OnLayeredPlacementChanged?.Invoke(position, layeredIngredients.ContainsKey(position) ? layeredIngredients[position] : new List<Ingredient>());

                Debug.Log($"📚 Removed {ingredient.ItemName} from layered position {position}");
                
                UpdateEfficiencyMetrics();
            }

            return removed;
        }

        /// <summary>
        /// Check if a position has an obstacle
        /// </summary>
        private bool HasObstacleAt(Vector2Int position)
        {
            if (gridManager == null) return false;
            return gridManager.GetObstacleAt(position) != null;
        }

        /// <summary>
        /// Calculate grid efficiency score
        /// </summary>
        public float GetGridEfficiencyScore()
        {
            return currentEfficiencyScore;
        }

        /// <summary>
        /// Update efficiency metrics
        /// </summary>
        private void UpdateEfficiencyMetrics()
        {
            if (!trackEfficiency) return;

            // Calculate total available cells
            totalCellsAvailable = currentGridSize.x * currentGridSize.y;

            // Count cells that are actually used (including obstacles and ingredients)
            totalCellsUsed = CountUsedCells();

            // Calculate efficiency as usage ratio
            currentEfficiencyScore = totalCellsAvailable > 0 ? (float)totalCellsUsed / totalCellsAvailable : 0f;

            OnEfficiencyUpdated?.Invoke(currentEfficiencyScore);

            Debug.Log($"🎯 Grid Efficiency: {currentEfficiencyScore:P} ({totalCellsUsed}/{totalCellsAvailable} cells used)");
        }

        /// <summary>
        /// Count cells that are actively used
        /// </summary>
        private int CountUsedCells()
        {
            int usedCells = 0;

            // Count obstacle cells
            if (gridManager != null)
            {
                usedCells += gridManager.aspectObstacles.Count;
            }

            // Count ingredient cells (including layered)
            if (gridManager?.GetComponent<IngredientPlacer>() != null)
            {
                var placer = gridManager.GetComponent<IngredientPlacer>();
                var placedIngredients = placer.GetAllPlacedIngredients();
                
                var occupiedPositions = new HashSet<Vector2Int>();
                foreach (var instance in placedIngredients)
                {
                    var cells = gridManager.GetIngredientCells(instance.ingredient, instance.gridPosition);
                    foreach (var cell in cells)
                    {
                        occupiedPositions.Add(cell);
                    }
                }
                usedCells += occupiedPositions.Count;
            }

            // Add layered ingredient positions
            usedCells += layeredIngredients.Count;

            return usedCells;
        }

        /// <summary>
        /// Calculate space utilization for a specific set of ingredients
        /// </summary>
        public SpaceUtilization CalculateSpaceUtilization(Dictionary<Vector2Int, Ingredient> placements)
        {
            var utilization = new SpaceUtilization();

            if (placements.Count == 0)
                return utilization;

            // Calculate bounding box
            var positions = placements.Keys.ToList();
            int minX = positions.Min(p => p.x);
            int maxX = positions.Max(p => p.x);
            int minY = positions.Min(p => p.y);
            int maxY = positions.Max(p => p.y);

            utilization.boundingBoxSize = new Vector2Int(maxX - minX + 1, maxY - minY + 1);
            utilization.boundingBoxArea = utilization.boundingBoxSize.x * utilization.boundingBoxSize.y;

            // Calculate ingredient coverage
            var ingredientCells = new HashSet<Vector2Int>();
            foreach (var kvp in placements)
            {
                var cells = gridManager?.GetIngredientCells(kvp.Value, kvp.Key) ?? new List<Vector2Int> { kvp.Key };
                foreach (var cell in cells)
                {
                    ingredientCells.Add(cell);
                }
            }

            utilization.ingredientCellCount = ingredientCells.Count;
            utilization.compactness = utilization.boundingBoxArea > 0 ? (float)utilization.ingredientCellCount / utilization.boundingBoxArea : 0f;

            // Calculate adjacency score
            utilization.adjacencyScore = CalculateAdjacencyScore(positions);

            // Calculate symmetry score
            utilization.symmetryScore = CalculateSymmetryScore(positions);

            Debug.Log($"📐 Space Utilization: Compactness {utilization.compactness:P}, Adjacency {utilization.adjacencyScore:F2}, Symmetry {utilization.symmetryScore:F2}");

            return utilization;
        }

        /// <summary>
        /// Calculate how well ingredients are connected to each other
        /// </summary>
        private float CalculateAdjacencyScore(List<Vector2Int> positions)
        {
            if (positions.Count <= 1) return 1f;

            int adjacentPairs = 0;
            int totalPairs = 0;

            for (int i = 0; i < positions.Count; i++)
            {
                for (int j = i + 1; j < positions.Count; j++)
                {
                    totalPairs++;
                    float distance = Vector2Int.Distance(positions[i], positions[j]);
                    if (distance <= 1.5f) // Adjacent (including diagonal)
                    {
                        adjacentPairs++;
                    }
                }
            }

            return totalPairs > 0 ? (float)adjacentPairs / totalPairs : 0f;
        }

        /// <summary>
        /// Calculate symmetry score of ingredient placement
        /// </summary>
        private float CalculateSymmetryScore(List<Vector2Int> positions)
        {
            if (positions.Count <= 1) return 1f;

            // Calculate center of mass
            float centerX = (float)positions.Average(p => p.x);
            float centerY = (float)positions.Average(p => p.y);
            var center = new Vector2(centerX, centerY);

            // Check horizontal symmetry
            float horizontalSymmetry = CalculateAxisSymmetry(positions, center, Vector2.right);
            
            // Check vertical symmetry
            float verticalSymmetry = CalculateAxisSymmetry(positions, center, Vector2.up);

            // Return best symmetry score
            return Mathf.Max(horizontalSymmetry, verticalSymmetry);
        }

        /// <summary>
        /// Calculate symmetry along a specific axis
        /// </summary>
        private float CalculateAxisSymmetry(List<Vector2Int> positions, Vector2 center, Vector2 axis)
        {
            int symmetricPairs = 0;
            int totalPositions = positions.Count;

            foreach (var pos in positions)
            {
                // Find the mirrored position across the axis
                Vector2 relativePos = new Vector2(pos.x, pos.y) - center;
                Vector2 mirroredPos = Vector2.Reflect(relativePos, axis) + center;
                Vector2Int mirroredGridPos = new Vector2Int(Mathf.RoundToInt(mirroredPos.x), Mathf.RoundToInt(mirroredPos.y));

                // Check if the mirrored position exists in the list
                if (positions.Contains(mirroredGridPos))
                {
                    symmetricPairs++;
                }
            }

            return totalPositions > 0 ? (float)symmetricPairs / totalPositions : 0f;
        }

        /// <summary>
        /// Get all ingredients at a layered position
        /// </summary>
        public List<Ingredient> GetLayeredIngredientsAt(Vector2Int position)
        {
            return layeredIngredients.ContainsKey(position) ? 
                   new List<Ingredient>(layeredIngredients[position]) : 
                   new List<Ingredient>();
        }

        /// <summary>
        /// Check if a cell is an expanded cell
        /// </summary>
        public bool IsExpandedCell(Vector2Int position)
        {
            return expandedCells.ContainsKey(position) && expandedCells[position];
        }

        /// <summary>
        /// Get grid expansion statistics
        /// </summary>
        public GridExpansionStats GetExpansionStats()
        {
            return new GridExpansionStats
            {
                originalSize = new Vector2Int(3, 3), // Base size from design document
                currentSize = currentGridSize,
                maxSize = maxGridSize,
                expandedCellCount = expandedCells.Count(kvp => kvp.Value),
                expansionSources = expansionSources.Count,
                expansionEfficiency = currentEfficiencyScore
            };
        }

        #region Public API

        /// <summary>
        /// Test the enhanced grid system
        /// </summary>
        [ContextMenu("Test Enhanced Grid System")]
        public void TestEnhancedGridSystem()
        {
            Debug.Log("🔄 === TESTING ENHANCED GRID SYSTEM ===");
            Debug.Log($"🔄 Dynamic Expansion: {enableDynamicExpansion}");
            Debug.Log($"🔄 Layered Placement: {enableLayeredPlacement}");
            Debug.Log($"🔄 Current Grid Size: {currentGridSize.x}x{currentGridSize.y}");
            Debug.Log($"🔄 Max Grid Size: {maxGridSize.x}x{maxGridSize.y}");
            Debug.Log($"🔄 Efficiency Score: {currentEfficiencyScore:P}");
            Debug.Log($"🔄 Expanded Cells: {expandedCells.Count(kvp => kvp.Value)}");
            Debug.Log($"🔄 Layered Positions: {layeredIngredients.Count}");

            var stats = GetExpansionStats();
            Debug.Log($"🔄 Expansion Stats: {stats.currentSize.x}x{stats.currentSize.y} from {stats.originalSize.x}x{stats.originalSize.y}");

            Debug.Log("🔄 === ENHANCED GRID SYSTEM TEST COMPLETE ===");
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class SpaceUtilization
    {
        public Vector2Int boundingBoxSize;
        public int boundingBoxArea;
        public int ingredientCellCount;
        public float compactness;
        public float adjacencyScore;
        public float symmetryScore;
    }

    [System.Serializable]
    public class GridExpansionStats
    {
        public Vector2Int originalSize;
        public Vector2Int currentSize;
        public Vector2Int maxSize;
        public int expandedCellCount;
        public int expansionSources;
        public float expansionEfficiency;
    }

    #endregion
}