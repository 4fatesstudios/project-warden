using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.GridDemo;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu
{
    /// <summary>
    /// Handles proficiency grading calculations for the alchemy system
    /// Implements the grading system from the design document
    /// </summary>
    public static class ProficiencyGrading
    {
        public static ProficiencyGrade CalculateProficiency(
            GridCraftingManager gridManager,
            Dictionary<Vector2Int, Ingredient> placedIngredients,
            List<AspectObstacle> obstacles,
            ProficiencyWeights weights,
            bool isFreeCraftingMode = false)
        {
            var grade = new ProficiencyGrade();

            if (placedIngredients.Count == 0)
            {
                grade.gradeLevel = GradeLevel.F;
                grade.feedback = "No ingredients placed";
                return grade;
            }

            grade.coverageRatio = CalculateCoverageRatio(gridManager, placedIngredients, obstacles);
            grade.adjacencySynergy = CalculateAdjacencySynergy(placedIngredients);
            grade.expansionUtilization = CalculateExpansionUtilization(gridManager, placedIngredients);
            
            grade.shapeDifficulty = 0f;
            grade.orientationEfficiency = 0f;
            
            if (isFreeCraftingMode)
            {
                grade.obstaclesCompleted = 0f;
            }
            else
            {
                grade.obstaclesCompleted = CalculateObstacleCompletion(obstacles);
            }

            grade.overallScore = CalculateWeightedScore(grade, weights, isFreeCraftingMode);
            grade.proficiencyPercentage = grade.overallScore * 100f;
            grade.gradeLevel = DetermineGradeLevel(grade.overallScore);
            grade.feedback = GenerateFeedback(grade, isFreeCraftingMode);

            return grade;
        }
        
        /// <summary>
        /// Calculate proficiency grade for current crafting attempt
        /// </summary>
        public static ProficiencyGrade CalculateProficiency(
            GridGameManager gridManager,
            Dictionary<Vector2Int, Ingredient> placedIngredients,
            List<AspectObstacle> obstacles,
            ProficiencyWeights weights,
            bool isFreeCraftingMode = false)
        {
            var grade = new ProficiencyGrade();

            if (placedIngredients.Count == 0)
            {
                grade.gradeLevel = GradeLevel.F;
                grade.feedback = "No ingredients placed";
                return grade;
            }

            // Calculate core metrics for both modes
            grade.coverageRatio = CalculateCoverageRatio(gridManager, placedIngredients);
            grade.adjacencySynergy = CalculateAdjacencySynergy(placedIngredients);
            grade.expansionUtilization = CalculateExpansionUtilization(gridManager, placedIngredients);
            
            // Shape difficulty and orientation efficiency are removed from the system entirely
            grade.shapeDifficulty = 0f;
            grade.orientationEfficiency = 0f;
            
            // Obstacles completed only calculated for recipe crafting mode
            if (isFreeCraftingMode)
            {
                grade.obstaclesCompleted = 0f; // Free crafting ignores obstacles
            }
            else
            {
                grade.obstaclesCompleted = CalculateObstacleCompletion(obstacles); // Recipe crafting includes obstacles
            }

            // Calculate weighted overall score using appropriate equation
            grade.overallScore = CalculateWeightedScore(grade, weights, isFreeCraftingMode);
            
            // Convert to percentage for display (0-100)
            grade.proficiencyPercentage = grade.overallScore * 100f;

            // Determine grade level
            grade.gradeLevel = DetermineGradeLevel(grade.overallScore);

            // Generate feedback (context-aware for free crafting)
            grade.feedback = GenerateFeedback(grade, isFreeCraftingMode);

            return grade;
        }

        /// <summary>
        /// Calculate coverage ratio (how much of the grid is efficiently used)
        /// Only Corporeal and Void obstacles subtract from available grid space
        /// Other obstacles (Frigid, Scorch, Caustic, Arc, Divine) are counted as available space
        /// </summary>
        private static float CalculateCoverageRatio(GridCraftingManager gridManager, Dictionary<Vector2Int, Ingredient> placedIngredients, List<AspectObstacle> obstacles)
        {
            if (placedIngredients.Count == 0) return 0f;

            var occupiedCells = new HashSet<Vector2Int>();
            foreach (var kvp in placedIngredients)
            {
                var cells = gridManager.GetIngredientCells(kvp.Value, kvp.Key);
                foreach (var cell in cells)
                {
                    occupiedCells.Add(cell);
                }
            }

            int totalCells = gridManager.gridWidth * gridManager.gridHeight;
            int blockedCells = obstacles.Count(o => o.SubtractsFromAvailableSpace());
            int availableCells = totalCells - blockedCells;

            if (availableCells <= 0) return 1f;

            float coverage = (float)occupiedCells.Count / availableCells;
            return Mathf.Clamp01(coverage);
        }
        
        private static float CalculateCoverageRatio(GridGameManager gridManager, Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            if (placedIngredients.Count == 0) return 0f;

            // Count total cells occupied by ingredients
            var occupiedCells = new HashSet<Vector2Int>();
            foreach (var kvp in placedIngredients)
            {
                var cells = gridManager.GetIngredientCells(kvp.Value, kvp.Key);
                foreach (var cell in cells)
                {
                    occupiedCells.Add(cell);
                }
            }

            // Calculate against available space (minus only Corporeal and Void obstacles)
            int totalCells = gridManager.gridWidth * gridManager.gridHeight;
            int blockedCells = gridManager.aspectObstacles.Count(o => o.SubtractsFromAvailableSpace()); // Only Corporeal and Void obstacles
            int availableCells = totalCells - blockedCells;

            if (availableCells <= 0) return 1f;

            float coverage = (float)occupiedCells.Count / availableCells;
            return Mathf.Clamp01(coverage);
        }

        /// <summary>
        /// Calculate adjacency synergy score (how well ingredients are connected)
        /// </summary>
        private static float CalculateAdjacencySynergy(Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            if (placedIngredients.Count <= 1) return 1f;

            var positions = placedIngredients.Keys.ToList();
            int adjacentPairs = 0;
            int totalPairs = 0;
            int similarAspectAdjacencies = 0;

            for (int i = 0; i < positions.Count; i++)
            {
                for (int j = i + 1; j < positions.Count; j++)
                {
                    totalPairs++;
                    float distance = Vector2Int.Distance(positions[i], positions[j]);
                    
                    if (distance <= 1.5f) // Adjacent (including diagonal)
                    {
                        adjacentPairs++;

                        // Bonus for similar aspects being adjacent
                        var ingredient1 = placedIngredients[positions[i]];
                        var ingredient2 = placedIngredients[positions[j]];
                        if (ingredient1.IngredientAspect == ingredient2.IngredientAspect)
                        {
                            similarAspectAdjacencies++;
                        }
                    }
                }
            }

            float baseAdjacency = totalPairs > 0 ? (float)adjacentPairs / totalPairs : 0f;
            float aspectBonus = totalPairs > 0 ? (float)similarAspectAdjacencies / totalPairs * 0.5f : 0f;

            return Mathf.Clamp01(baseAdjacency + aspectBonus);
        }

        /// <summary>
        /// Calculate expansion utilization (how well expanded grid space is used)
        /// </summary>
        private static float CalculateExpansionUtilization(GridCraftingManager gridManager, Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            bool hasExpansionIngredients = placedIngredients.Values.Any(i => i.UnlocksAdditionalSpace);
            if (!hasExpansionIngredients) return 1f;

            var enhancedGrid = gridManager.GetComponent<EnhancedGridSystem>();
            if (enhancedGrid != null)
            {
                return enhancedGrid.GetGridEfficiencyScore();
            }

            var baseGridSize = new Vector2Int(3, 3);
            var currentGridSize = new Vector2Int(gridManager.gridWidth, gridManager.gridHeight);
            
            if (currentGridSize.x <= baseGridSize.x && currentGridSize.y <= baseGridSize.y)
                return 1f;

            int baseArea = baseGridSize.x * baseGridSize.y;
            int expandedArea = currentGridSize.x * currentGridSize.y - baseArea;
            
            var occupiedCells = new HashSet<Vector2Int>();
            foreach (var kvp in placedIngredients)
            {
                var cells = gridManager.GetIngredientCells(kvp.Value, kvp.Key);
                foreach (var cell in cells)
                {
                    if (cell.x >= baseGridSize.x || cell.y >= baseGridSize.y)
                    {
                        occupiedCells.Add(cell);
                    }
                }
            }

            if (expandedArea <= 0) return 1f;
            
            float utilization = (float)occupiedCells.Count / expandedArea;
            return Mathf.Clamp01(utilization);
        }
        
        private static float CalculateExpansionUtilization(GridGameManager gridManager, Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            // Check if any ingredients unlock additional space
            bool hasExpansionIngredients = placedIngredients.Values.Any(i => i.UnlocksAdditionalSpace);
            if (!hasExpansionIngredients) return 1f; // Perfect score if no expansion needed

            // Get enhanced grid system if available
            var enhancedGrid = gridManager.GetComponent<EnhancedGridSystem>();
            if (enhancedGrid != null)
            {
                return enhancedGrid.GetGridEfficiencyScore();
            }

            // Fallback calculation
            var baseGridSize = new Vector2Int(3, 3); // Starting size from design doc
            var currentGridSize = new Vector2Int(gridManager.gridWidth, gridManager.gridHeight);
            
            if (currentGridSize.x <= baseGridSize.x && currentGridSize.y <= baseGridSize.y)
                return 1f; // No expansion used

            int baseArea = baseGridSize.x * baseGridSize.y;
            int expandedArea = currentGridSize.x * currentGridSize.y - baseArea;
            
            // Count how many ingredient cells are in the expanded area
            var occupiedCells = new HashSet<Vector2Int>();
            foreach (var kvp in placedIngredients)
            {
                var cells = gridManager.GetIngredientCells(kvp.Value, kvp.Key);
                foreach (var cell in cells)
                {
                    if (cell.x >= baseGridSize.x || cell.y >= baseGridSize.y)
                    {
                        occupiedCells.Add(cell);
                    }
                }
            }

            return expandedArea > 0 ? (float)occupiedCells.Count / expandedArea : 1f;
        }

        /// <summary>
        /// Calculate shape difficulty score (complexity of ingredient shapes used)
        /// </summary>
        private static float CalculateShapeDifficulty(Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            if (placedIngredients.Count == 0) return 0f;

            float totalDifficulty = 0f;
            int ingredientCount = 0;

            foreach (var ingredient in placedIngredients.Values)
            {
                float shapeDifficulty = CalculateIngredientShapeDifficulty(ingredient);
                totalDifficulty += shapeDifficulty;
                ingredientCount++;
            }

            return ingredientCount > 0 ? totalDifficulty / ingredientCount : 0f;
        }

        /// <summary>
        /// Calculate difficulty score for a single ingredient's shape
        /// </summary>
        private static float CalculateIngredientShapeDifficulty(Ingredient ingredient)
        {
            var shape = ingredient.GetShape();
            int width = shape.GetLength(0);
            int height = shape.GetLength(1);
            int totalCells = width * height;
            
            if (totalCells == 1) return 0.2f; // Single cell is easiest

            // Count active cells
            int activeCells = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (shape[x, y]) activeCells++;
                }
            }

            // Shape complexity based on fill ratio and size
            float fillRatio = (float)activeCells / totalCells;
            float sizeComplexity = Mathf.Sqrt(totalCells) / 4f; // Normalize to 0-1 range roughly
            float shapeComplexity = 1f - fillRatio; // More complex if less filled (more holes)

            return Mathf.Clamp01((sizeComplexity + shapeComplexity) / 2f);
        }

        /// <summary>
        /// Calculate orientation efficiency (how well ingredients are rotated for space usage)
        /// </summary>
        private static float CalculateOrientationEfficiency(Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            if (placedIngredients.Count == 0) return 0f;

            // For now, assume optimal orientation (this would be complex to calculate properly)
            // In a full implementation, this would check if ingredients could be rotated to save space
            return 0.8f; // Default good efficiency
        }

        /// <summary>
        /// Calculate obstacle completion percentage
        /// Excludes void obstacles from completion calculations (they are purely cosmetic)
        /// </summary>
        private static float CalculateObstacleCompletion(List<AspectObstacle> obstacles)
        {
            // Filter out void obstacles - they don't count for completion
            var countableObstacles = obstacles.Where(o => o.CountsForCompletion()).ToList();
            
            if (countableObstacles.Count == 0) return 1f; // Perfect if no countable obstacles

            int completedObstacles = countableObstacles.Count(o => o.IsCompleted);
            return (float)completedObstacles / countableObstacles.Count;
        }

        /// <summary>
        /// Calculate weighted overall score using two different equations based on crafting mode
        /// </summary>
        private static float CalculateWeightedScore(ProficiencyGrade grade, ProficiencyWeights weights, bool isFreeCraftingMode = false)
        {
            float score = 0f;
            
            if (isFreeCraftingMode)
            {
                // FREE CRAFTING EQUATION: Coverage + Adjacency + Expansion
                // Redistribute weights among the three active metrics (excluding obstacles, shape, orientation)
                float totalActiveWeight = weights.coverageWeight + weights.adjacencyWeight + weights.expansionWeight;
                
                if (totalActiveWeight > 0f)
                {
                    // Normalize the active weights to sum to 1.0
                    float coverageNormalized = weights.coverageWeight / totalActiveWeight;
                    float adjacencyNormalized = weights.adjacencyWeight / totalActiveWeight;
                    float expansionNormalized = weights.expansionWeight / totalActiveWeight;
                    
                    score += grade.coverageRatio * coverageNormalized;
                    score += grade.adjacencySynergy * adjacencyNormalized;
                    score += grade.expansionUtilization * expansionNormalized;
                }
            }
            else
            {
                // RECIPE CRAFTING EQUATION: Coverage + Adjacency + Expansion + Obstacles
                // Redistribute weights among the four active metrics (excluding shape and orientation)
                float totalActiveWeight = weights.coverageWeight + weights.adjacencyWeight + weights.expansionWeight + weights.obstacleWeight;
                
                if (totalActiveWeight > 0f)
                {
                    // Normalize the active weights to sum to 1.0
                    float coverageNormalized = weights.coverageWeight / totalActiveWeight;
                    float adjacencyNormalized = weights.adjacencyWeight / totalActiveWeight;
                    float expansionNormalized = weights.expansionWeight / totalActiveWeight;
                    float obstacleNormalized = weights.obstacleWeight / totalActiveWeight;
                    
                    score += grade.coverageRatio * coverageNormalized;
                    score += grade.adjacencySynergy * adjacencyNormalized;
                    score += grade.expansionUtilization * expansionNormalized;
                    score += grade.obstaclesCompleted * obstacleNormalized;
                }
            }

            return Mathf.Clamp01(score);
        }

        /// <summary>
        /// Determine grade level from overall score
        /// </summary>
        private static GradeLevel DetermineGradeLevel(float score)
        {
            if (score >= 0.95f) return GradeLevel.S;
            if (score >= 0.85f) return GradeLevel.A;
            if (score >= 0.75f) return GradeLevel.B;
            if (score >= 0.65f) return GradeLevel.C;
            if (score >= 0.50f) return GradeLevel.D;
            return GradeLevel.F;
        }

        /// <summary>
        /// Generate helpful feedback for the player
        /// </summary>
        private static string GenerateFeedback(ProficiencyGrade grade, bool isFreeCraftingMode = false)
        {
            var feedback = new List<string>();

            // Coverage feedback
            if (grade.coverageRatio < 0.5f)
                feedback.Add("Try using more of the available grid space");
            else if (grade.coverageRatio > 0.9f)
                feedback.Add("Excellent space utilization!");

            // Adjacency feedback
            if (grade.adjacencySynergy < 0.4f)
                feedback.Add("Consider placing ingredients closer together for synergy effects");
            else if (grade.adjacencySynergy > 0.8f)
                feedback.Add("Great ingredient connectivity!");

            // Expansion feedback
            if (grade.expansionUtilization > 0.8f)
                feedback.Add("Excellent use of grid expansion!");

            // Mode-specific feedback
            if (!isFreeCraftingMode)
            {
                // Only include obstacle feedback in recipe crafting mode
                if (grade.obstaclesCompleted < 0.5f)
                    feedback.Add("Focus on completing obstacle challenges for bonus points");
                else if (grade.obstaclesCompleted >= 1.0f)
                    feedback.Add("Perfect obstacle mastery!");
            }
            else
            {
                // Free crafting mode specific feedback - focus on core placement strategies
                if (grade.coverageRatio > 0.7f && grade.adjacencySynergy > 0.7f)
                    feedback.Add("Perfect ingredient placement strategy!");
            }

            // Overall feedback
            switch (grade.gradeLevel)
            {
                case GradeLevel.S:
                    feedback.Insert(0, isFreeCraftingMode ? "Masterful free crafting technique!" : "Masterful recipe crafting technique!");
                    break;
                case GradeLevel.A:
                    feedback.Insert(0, "Excellent crafting skills!");
                    break;
                case GradeLevel.B:
                    feedback.Insert(0, "Good technique with room for improvement");
                    break;
                case GradeLevel.C:
                    feedback.Insert(0, "Average performance - keep practicing!");
                    break;
                case GradeLevel.D:
                    feedback.Insert(0, "Basic technique - consider studying placement strategies");
                    break;
                case GradeLevel.F:
                    feedback.Insert(0, "Poor technique - review the fundamentals");
                    break;
            }

            return string.Join(". ", feedback);
        }

        /// <summary>
        /// Get color for grade level display
        /// </summary>
        public static Color GetGradeColor(GradeLevel grade)
        {
            return grade switch
            {
                GradeLevel.S => new Color(1f, 0.84f, 0f), // Gold
                GradeLevel.A => new Color(0f, 0.8f, 0f),  // Green
                GradeLevel.B => new Color(0.2f, 0.6f, 1f), // Light Blue
                GradeLevel.C => new Color(1f, 0.6f, 0f),  // Orange
                GradeLevel.D => new Color(1f, 0.4f, 0.4f), // Light Red
                GradeLevel.F => new Color(0.8f, 0.2f, 0.2f), // Red
                _ => Color.white
            };
        }
    }

    #region Data Structures

    [System.Serializable]
    public class ProficiencyGrade
    {
        [Header("Individual Scores")]
        public float coverageRatio;
        public float adjacencySynergy;
        public float expansionUtilization;
        public float shapeDifficulty;
        public float orientationEfficiency;
        public float obstaclesCompleted;

        [Header("Overall Results")]
        public float overallScore;
        public float proficiencyPercentage; // 0-100 percentage for display
        public GradeLevel gradeLevel;
        public string feedback;
    }

    [System.Serializable]
    public class ProficiencyWeights
    {
        [Header("Grading Weights (sum = 1.0)")]
        [Range(0f, 1f)] public float coverageWeight = 0.33f;
        [Range(0f, 1f)] public float adjacencyWeight = 0.27f;
        [Range(0f, 1f)] public float expansionWeight = 0.20f;
        [Range(0f, 1f)] public float obstacleWeight = 0.20f;
        
        public void NormalizeWeights()
        {
        }
    }

    public enum GradeLevel
    {
        F = 0,
        D = 1,
        C = 2,
        B = 3,
        A = 4,
        S = 5
    }

    public enum RecipeDifficulty
    {
        Beginner,
        Standard,
        Advanced,
        Master
    }

    #endregion
}