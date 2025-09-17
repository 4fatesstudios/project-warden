using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    [System.Serializable]
    public struct ProficiencyGrade
    {
        public float overallScore;
        public float coverageRatio;
        public float adjacencySynergy;
        public float expansionUtilization;
        public float shapeDifficulty;
        public float orientationEfficiency;
        public float obstaclesCompleted;
        public GradeLevel gradeLevel;
        public string feedback;

        public ProficiencyGrade(float overall, float coverage, float adjacency, float expansion, 
                               float shape, float orientation, float obstacles, GradeLevel level, string feedbackText)
        {
            overallScore = overall;
            coverageRatio = coverage;
            adjacencySynergy = adjacency;
            expansionUtilization = expansion;
            shapeDifficulty = shape;
            orientationEfficiency = orientation;
            obstaclesCompleted = obstacles;
            gradeLevel = level;
            feedback = feedbackText;
        }
    }

    public enum GradeLevel
    {
        F = 0,  // 0-59%
        D = 1,  // 60-69%
        C = 2,  // 70-79%
        B = 3,  // 80-89%
        A = 4,  // 90-94%
        S = 5   // 95-100%
    }

    [System.Serializable]
    public class ProficiencyWeights
    {
        [Range(0f, 1f)] public float coverageWeight = 0.25f;
        [Range(0f, 1f)] public float adjacencyWeight = 0.2f;
        [Range(0f, 1f)] public float expansionWeight = 0.15f;
        [Range(0f, 1f)] public float shapeWeight = 0.15f;
        [Range(0f, 1f)] public float orientationWeight = 0.1f;
        [Range(0f, 1f)] public float obstacleWeight = 0.15f;

        public void NormalizeWeights()
        {
            float total = coverageWeight + adjacencyWeight + expansionWeight + 
                         shapeWeight + orientationWeight + obstacleWeight;
            if (total > 0f)
            {
                coverageWeight /= total;
                adjacencyWeight /= total;
                expansionWeight /= total;
                shapeWeight /= total;
                orientationWeight /= total;
                obstacleWeight /= total;
            }
        }
    }

    public static class ProficiencyGrading
    {
        private static readonly ProficiencyWeights defaultWeights = new ProficiencyWeights();

        /// <summary>
        /// Calculate the overall proficiency grade for a crafting attempt
        /// </summary>
        public static ProficiencyGrade CalculateProficiency(
            GridGameManager gridManager,
            Dictionary<Vector2Int, Ingredient> placedIngredients,
            List<AspectObstacle> obstacles,
            ProficiencyWeights weights = null)
        {
            if (weights == null)
            {
                weights = defaultWeights;
                weights.NormalizeWeights();
            }

            // Calculate individual factors
            float coverage = CalculateCoverageRatio(gridManager, placedIngredients);
            float adjacency = CalculateAdjacencySynergy(gridManager, placedIngredients);
            float expansion = CalculateExpansionUtilization(gridManager, placedIngredients);
            float shape = CalculateShapeDifficulty(placedIngredients);
            float orientation = CalculateOrientationEfficiency(placedIngredients);
            float obstacleScore = CalculateObstacleCompletion(obstacles);

            // Calculate weighted overall score
            float overallScore = (coverage * weights.coverageWeight) +
                               (adjacency * weights.adjacencyWeight) +
                               (expansion * weights.expansionWeight) +
                               (shape * weights.shapeWeight) +
                               (orientation * weights.orientationWeight) +
                               (obstacleScore * weights.obstacleWeight);

            // Clamp to 0-100 range
            overallScore = Mathf.Clamp01(overallScore) * 100f;

            GradeLevel grade = GetGradeLevel(overallScore);
            string feedback = GenerateFeedback(coverage, adjacency, expansion, shape, orientation, obstacleScore, grade);

            return new ProficiencyGrade(
                overallScore, coverage * 100f, adjacency * 100f, expansion * 100f,
                shape * 100f, orientation * 100f, obstacleScore * 100f, grade, feedback);
        }

        /// <summary>
        /// Calculate coverage ratio: more filled space = better
        /// </summary>
        private static float CalculateCoverageRatio(GridGameManager gridManager, Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            if (placedIngredients.Count == 0) return 0f;

            int totalAvailableCells = 0;
            int occupiedCells = 0;

            // Count available cells in the current grid
            for (int x = 0; x < gridManager.gridWidth; x++)
            {
                for (int y = 0; y < gridManager.gridHeight; y++)
                {
                    var cell = gridManager.GetCell(x, y);
                    if (cell != null)
                    {
                        totalAvailableCells++;
                        if (cell.IsOccupied)
                        {
                            occupiedCells++;
                        }
                    }
                }
            }

            if (totalAvailableCells == 0) return 0f;

            float ratio = (float)occupiedCells / totalAvailableCells;
            
            // Apply diminishing returns for very high coverage to encourage efficiency
            if (ratio > 0.8f)
            {
                float excess = ratio - 0.8f;
                ratio = 0.8f + (excess * 0.5f); // Reduce benefit of excessive coverage
            }

            return Mathf.Clamp01(ratio);
        }

        /// <summary>
        /// Calculate adjacency synergy: compatible aspects next to each other
        /// </summary>
        private static float CalculateAdjacencySynergy(GridGameManager gridManager, Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            if (placedIngredients.Count < 2) return 0f;

            int totalAdjacencies = 0;
            int synergeticAdjacencies = 0;

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var kvp in placedIngredients)
            {
                Vector2Int pos = kvp.Key;
                Ingredient ingredient = kvp.Value;

                foreach (var direction in directions)
                {
                    Vector2Int adjacentPos = pos + direction;
                    
                    if (placedIngredients.TryGetValue(adjacentPos, out Ingredient adjacentIngredient))
                    {
                        totalAdjacencies++;
                        
                        if (AreAspectsCompatible(ingredient.IngredientAspect, adjacentIngredient.IngredientAspect))
                        {
                            synergeticAdjacencies++;
                        }
                    }
                }
            }

            if (totalAdjacencies == 0) return 0f;

            return (float)synergeticAdjacencies / totalAdjacencies;
        }

        /// <summary>
        /// Calculate expansion utilization: efficiency of using expanded cells
        /// </summary>
        private static float CalculateExpansionUtilization(GridGameManager gridManager, Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            // Check how efficiently expanded grid space is being used
            int baseGridSize = 3 * 3; // Base 3x3 grid
            int currentGridSize = gridManager.gridWidth * gridManager.gridHeight;
            
            if (currentGridSize <= baseGridSize)
            {
                // No expansion occurred, give neutral score
                return 0.7f;
            }

            int expandedCells = currentGridSize - baseGridSize;
            int occupiedExpandedCells = 0;

            // Count occupied cells outside the base 3x3 area
            for (int x = 0; x < gridManager.gridWidth; x++)
            {
                for (int y = 0; y < gridManager.gridHeight; y++)
                {
                    // Check if this cell is outside the base 3x3 area (centered)
                    bool isOutsideBase = x < 1 || x > 3 || y < 1 || y > 3;
                    
                    if (isOutsideBase && placedIngredients.ContainsKey(new Vector2Int(x, y)))
                    {
                        occupiedExpandedCells++;
                    }
                }
            }

            if (expandedCells == 0) return 0.7f; // No expanded cells to utilize

            float utilizationRatio = (float)occupiedExpandedCells / expandedCells;
            
            // Bonus for using expansion-triggering ingredients efficiently
            float expansionBonusScore = 0f;
            foreach (var ingredient in placedIngredients.Values)
            {
                if (ingredient.UnlocksAdditionalSpace)
                {
                    expansionBonusScore += 0.2f; // Bonus for using expansion ingredients
                }
            }

            return Mathf.Clamp01(utilizationRatio + expansionBonusScore);
        }

        /// <summary>
        /// Calculate shape difficulty: bonus for successfully placing harder shapes
        /// </summary>
        private static float CalculateShapeDifficulty(Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            if (placedIngredients.Count == 0) return 0f;

            float totalDifficulty = 0f;
            int ingredientCount = 0;

            foreach (var ingredient in placedIngredients.Values.Distinct())
            {
                float shapeDifficulty = CalculateIngredientShapeDifficulty(ingredient);
                totalDifficulty += shapeDifficulty;
                ingredientCount++;
            }

            if (ingredientCount == 0) return 0f;

            float averageDifficulty = totalDifficulty / ingredientCount;
            
            // Scale difficulty score (0.3 to 1.0 range, where 0.3 is for simple rectangles)
            return Mathf.Clamp(averageDifficulty, 0.3f, 1.0f);
        }

        /// <summary>
        /// Calculate orientation efficiency: reward preferred rotations
        /// </summary>
        private static float CalculateOrientationEfficiency(Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            if (placedIngredients.Count == 0) return 0f;

            float totalEfficiency = 0f;
            int ingredientCount = 0;

            foreach (var kvp in placedIngredients)
            {
                Vector2Int position = kvp.Key;
                Ingredient ingredient = kvp.Value;

                // Calculate orientation efficiency based on grid position and ingredient shape
                float positionEfficiency = CalculatePositionEfficiency(position, ingredient);
                totalEfficiency += positionEfficiency;
                ingredientCount++;
            }

            if (ingredientCount == 0) return 0f;

            return totalEfficiency / ingredientCount;
        }

        /// <summary>
        /// Calculate obstacles completed score
        /// </summary>
        private static float CalculateObstacleCompletion(List<AspectObstacle> obstacles)
        {
            if (obstacles == null || obstacles.Count == 0) return 1f; // No obstacles = perfect score

            int completedObstacles = obstacles.Count(o => o.IsCompleted);
            
            float completionRatio = (float)completedObstacles / obstacles.Count;
            
            // Bonus for completing difficult obstacle types
            float difficultyBonus = 0f;
            foreach (var obstacle in obstacles.Where(o => o.IsCompleted))
            {
                difficultyBonus += GetObstacleDifficultyBonus(obstacle.ObstacleType);
            }

            difficultyBonus /= obstacles.Count; // Average the bonus

            return Mathf.Clamp01(completionRatio + difficultyBonus);
        }

        /// <summary>
        /// Determine if two aspects are compatible for adjacency synergy
        /// </summary>
        private static bool AreAspectsCompatible(Aspect aspect1, Aspect aspect2)
        {
            // Define aspect compatibility rules
            var compatibilityMap = new Dictionary<Aspect, List<Aspect>>
            {
                { Aspect.Scorch, new List<Aspect> { Aspect.Caustic, Aspect.Arc } },
                { Aspect.Frigid, new List<Aspect> { Aspect.Corporeal, Aspect.Divine } },
                { Aspect.Corporeal, new List<Aspect> { Aspect.Frigid, Aspect.Divine } },
                { Aspect.Caustic, new List<Aspect> { Aspect.Scorch, Aspect.Arc } },
                { Aspect.Arc, new List<Aspect> { Aspect.Scorch, Aspect.Caustic } },
                { Aspect.Divine, new List<Aspect> { Aspect.Corporeal, Aspect.Frigid } }
            };

            return compatibilityMap.ContainsKey(aspect1) && compatibilityMap[aspect1].Contains(aspect2);
        }

        /// <summary>
        /// Calculate shape difficulty for a specific ingredient
        /// </summary>
        private static float CalculateIngredientShapeDifficulty(Ingredient ingredient)
        {
            if (ingredient.ShapeData == null)
            {
                // Simple rectangle
                int area = ingredient.GridWidth * ingredient.GridHeight;
                return 0.3f + (area * 0.05f); // Base difficulty + area bonus
            }

            var shape = ingredient.GetShape();
            int shapeWidth = shape.GetLength(0);
            int shapeHeight = shape.GetLength(1);
            int totalCells = shapeWidth * shapeHeight;
            int activeCells = 0;

            // Count active cells and calculate complexity
            for (int x = 0; x < shapeWidth; x++)
            {
                for (int y = 0; y < shapeHeight; y++)
                {
                    if (shape[x, y])
                    {
                        activeCells++;
                    }
                }
            }

            // Calculate density (active cells / total bounding box)
            float density = (float)activeCells / totalCells;
            
            // Calculate perimeter complexity
            float perimeterComplexity = CalculateShapePerimeterComplexity(shape);
            
            // More complex shapes (lower density, higher perimeter complexity) get higher difficulty
            float difficulty = 0.3f + (1f - density) * 0.4f + perimeterComplexity * 0.3f;
            
            return Mathf.Clamp01(difficulty);
        }

        /// <summary>
        /// Calculate how complex the perimeter of a shape is
        /// </summary>
        private static float CalculateShapePerimeterComplexity(bool[,] shape)
        {
            int width = shape.GetLength(0);
            int height = shape.GetLength(1);
            int perimeter = 0;
            int corners = 0;

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (shape[x, y])
                    {
                        int exposedSides = 0;
                        foreach (var dir in directions)
                        {
                            int checkX = x + dir.x;
                            int checkY = y + dir.y;
                            
                            if (checkX < 0 || checkX >= width || checkY < 0 || checkY >= height || !shape[checkX, checkY])
                            {
                                exposedSides++;
                            }
                        }

                        perimeter += exposedSides;
                        
                        // Count corners (cells with 2+ exposed adjacent sides)
                        if (exposedSides >= 2)
                        {
                            corners++;
                        }
                    }
                }
            }

            // Normalize complexity based on perimeter and corner count
            float complexity = (float)(corners + perimeter * 0.5f) / (width * height);
            return Mathf.Clamp01(complexity);
        }

        /// <summary>
        /// Calculate position efficiency for an ingredient
        /// </summary>
        private static float CalculatePositionEfficiency(Vector2Int position, Ingredient ingredient)
        {
            // Reward central positioning and efficient grid usage
            float centerDistance = Vector2.Distance(position, new Vector2(2f, 2f)); // Distance from center of base 3x3
            float centralityScore = Mathf.Clamp01(1f - (centerDistance / 3f)); // Closer to center = better

            // Bonus for larger ingredients being placed efficiently
            int ingredientSize = ingredient.GridWidth * ingredient.GridHeight;
            float sizeEfficiency = ingredientSize > 1 ? 1.2f : 1f;

            return centralityScore * sizeEfficiency;
        }

        /// <summary>
        /// Get difficulty bonus for completing specific obstacle types
        /// </summary>
        private static float GetObstacleDifficultyBonus(ObstacleType obstacleType)
        {
            return obstacleType switch
            {
                ObstacleType.Corporeal => 0.05f, // Cannot be completed directly
                ObstacleType.Frigid => 0.1f,     // Requires adjacency planning
                ObstacleType.Scorch => 0.15f,    // Requires specific aspects
                ObstacleType.Caustic => 0.05f,   // Easy to complete but with penalty
                ObstacleType.Arc => 0.2f,        // High risk/reward
                ObstacleType.Divine => 0.25f,    // Most restrictive requirements
                _ => 0f
            };
        }

        /// <summary>
        /// Convert numerical score to grade level
        /// </summary>
        private static GradeLevel GetGradeLevel(float score)
        {
            return score switch
            {
                >= 95f => GradeLevel.S,
                >= 90f => GradeLevel.A,
                >= 80f => GradeLevel.B,
                >= 70f => GradeLevel.C,
                >= 60f => GradeLevel.D,
                _ => GradeLevel.F
            };
        }

        /// <summary>
        /// Generate contextual feedback for the grade
        /// </summary>
        private static string GenerateFeedback(float coverage, float adjacency, float expansion, 
                                             float shape, float orientation, float obstacles, GradeLevel grade)
        {
            var feedback = new List<string>();

            // Add grade-specific feedback
            feedback.Add(grade switch
            {
                GradeLevel.S => "Exceptional alchemical mastery! Your spatial reasoning and aspect synergy are unparalleled.",
                GradeLevel.A => "Excellent work! You demonstrate strong understanding of alchemical principles.",
                GradeLevel.B => "Good crafting technique. Your approach shows solid fundamentals.",
                GradeLevel.C => "Adequate results. There's room for improvement in your spatial arrangements.",
                GradeLevel.D => "Below average performance. Consider studying aspect interactions more carefully.",
                GradeLevel.F => "Poor results. Fundamental understanding of grid alchemy appears lacking.",
                _ => "Unknown performance level."
            });

            // Add specific improvement suggestions
            if (coverage < 0.5f)
                feedback.Add("Try to fill more of the available grid space efficiently.");
            if (adjacency < 0.5f)
                feedback.Add("Focus on placing compatible aspects adjacent to each other.");
            if (expansion < 0.5f)
                feedback.Add("Make better use of expansion ingredients and unlocked space.");
            if (shape < 0.5f)
                feedback.Add("Experiment with more complex ingredient shapes for higher difficulty bonuses.");
            if (orientation < 0.5f)
                feedback.Add("Consider ingredient placement and rotation more carefully.");
            if (obstacles < 0.5f)
                feedback.Add("Pay attention to aspect obstacles and their completion requirements.");

            return string.Join(" ", feedback);
        }

        /// <summary>
        /// Get a color representation for the grade level
        /// </summary>
        public static Color GetGradeColor(GradeLevel grade)
        {
            return grade switch
            {
                GradeLevel.S => new Color(1f, 0.9f, 0.2f),    // Gold
                GradeLevel.A => new Color(0.2f, 1f, 0.3f),    // Green
                GradeLevel.B => new Color(0.3f, 0.8f, 1f),    // Blue
                GradeLevel.C => new Color(1f, 0.8f, 0.2f),    // Yellow
                GradeLevel.D => new Color(1f, 0.5f, 0.2f),    // Orange
                GradeLevel.F => new Color(1f, 0.2f, 0.2f),    // Red
                _ => Color.gray
            };
        }
    }
}