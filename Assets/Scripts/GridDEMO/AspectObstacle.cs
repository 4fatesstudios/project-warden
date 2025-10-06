using UnityEngine;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    public enum ObstacleType
    {
        Corporeal,  // Blocked cell (stone/residue), not counted for completion
        FrigidFrozen,     // Frozen cell, needs adjacency to unlock
        FrigidMelted, // Frozen cell, needs adjacency to unlock and melt
        Scorch,     // Volatile cell, needs compatible aspects
        Caustic,    // Degrade cell, reduces potency by 20%
        Arc,        // Chaotic RNG effects
        Divine,     // Sanctified cell, only unrefined divine aspects
        Void        // Cosmetic void - disables visuals, blocks placement, not counted for completion
    }

    [System.Serializable]
    public class AspectObstacle
    {
        [SerializeField] private ObstacleType obstacleType;
        [SerializeField] private Vector2Int position;
        [SerializeField] private bool isCompleted = false;
        [SerializeField] private Ingredient placedIngredient;

        public ObstacleType ObstacleType => obstacleType;
        public Vector2Int Position => position;
        public bool IsCompleted => isCompleted;
        public Ingredient PlacedIngredient => placedIngredient;

        public AspectObstacle(ObstacleType type, Vector2Int pos)
        {
            obstacleType = type;
            position = pos;
            isCompleted = false;
            placedIngredient = null;
        }

        /// <summary>
        /// Check if an ingredient can be placed on this obstacle
        /// </summary>
        public bool CanPlaceIngredient(Ingredient ingredient)
        {
            if (ingredient == null) return false;

            switch (obstacleType)
            {
                case ObstacleType.Corporeal:
                    return false; // Blocked cells cannot have ingredients placed

                case ObstacleType.FrigidFrozen:
                    return false; // Frozen cells need to be melted first by adjacency

                case ObstacleType.FrigidMelted:
                    // Melted cells can accept any ingredient, but with special effects
                    return true;

                case ObstacleType.Scorch:
                    // Volatile cells accept scorch, caustic, or arc aspects
                    return ingredient.IngredientAspect == Aspect.Scorch ||
                           ingredient.IngredientAspect == Aspect.Caustic ||
                           ingredient.IngredientAspect == Aspect.Arc;

                case ObstacleType.Caustic:
                    return true; // Any ingredient can be placed but will have reduced potency

                case ObstacleType.Arc:
                    return true; // Any ingredient can be placed but triggers RNG effects

                case ObstacleType.Divine:
                    // Only unrefined divine aspects
                    return ingredient.IngredientAspect == Aspect.Divine && ingredient.IsUnrefined;

                case ObstacleType.Void:
                    return false; // Void cells cannot have ingredients placed

                default:
                    return false;
            }
        }

        /// <summary>
        /// Attempt to place an ingredient on this obstacle
        /// </summary>
        public bool TryPlaceIngredient(Ingredient ingredient, GridGameManager gridManager)
        {
            if (!CanPlaceIngredient(ingredient))
                return false;

            placedIngredient = ingredient;

            switch (obstacleType)
            {
                case ObstacleType.Corporeal:
                    // Should not reach here as CanPlaceIngredient returns false
                    return false;

                case ObstacleType.FrigidFrozen:
                    // Only completed through melting, not direct placement
                    return false;

                case ObstacleType.FrigidMelted:
                    HandleFrigidMeltedObstacle(ingredient, gridManager);
                    break;

                case ObstacleType.Scorch:
                    HandleScorchObstacle(ingredient, gridManager);
                    break;

                case ObstacleType.Caustic:
                    HandleCausticObstacle(ingredient, gridManager);
                    break;

                case ObstacleType.Arc:
                    HandleArcObstacle(ingredient, gridManager);
                    break;

                case ObstacleType.Divine:
                    HandleDivineObstacle(ingredient, gridManager);
                    break;

                case ObstacleType.Void:
                    // Void obstacles should never have ingredients placed on them
                    return false;
            }

            return true;
        }
        
        /// <summary>
        /// Handle Scorch (Fire) obstacle interactions
        /// </summary>
        private void HandleScorchObstacle(Ingredient ingredient, GridGameManager gridManager)
        {
            bool isCompatible = (ingredient.IngredientAspect == Aspect.Scorch ||
                               ingredient.IngredientAspect == Aspect.Caustic ||
                               ingredient.IngredientAspect == Aspect.Arc);

            if (isCompatible)
            {
                // Compatible placement enhances potency
                Debug.Log($"🔥 Scorch obstacle: {ingredient.ItemName} potency enhanced by 50%");
                isCompleted = true;
            }
            else
            {
                // Incompatible placement burns ingredient
                float sizeReduction = 1.1f / GetIngredientSize(ingredient);
                Debug.Log($"🔥 Scorch obstacle: {ingredient.ItemName} burned, potency reduced by {sizeReduction:P0}%");
                
                // Note: In a full implementation, you'd modify the ingredient's actual potency
                // For now, we'll just track the penalty
            }
        }
        
        /// <summary>
        /// Handle Caustic (Poison/Acid) obstacle interactions  
        /// </summary>
        private void HandleCausticObstacle(Ingredient ingredient, GridGameManager gridManager)
        {
            if (ingredient.IngredientArchetype == IngredientArchetype.Herb)
            {
                // Herb ingredients trigger "Controlled" state
                Debug.Log($"🧪 Caustic obstacle: Herb {ingredient.ItemName} reduces negative effects by 60%");
                // Set state to Controlled
            }
            else if (ingredient.IngredientAspect == Aspect.Caustic)
            {
                // Caustic ingredients trigger "Hyperactive" state
                Debug.Log($"🧪 Caustic obstacle: Caustic {ingredient.ItemName} potency increased by 20%");
                // Set state to Hyperactive
            }
            else
            {
                // Default "Deteriorated" state
                Debug.Log($"🧪 Caustic obstacle: {ingredient.ItemName} potency reduced by 20%");
                // Set state to Deteriorated
            }
            
            isCompleted = true;
        }
        
        /// <summary>
        /// Handle Arc (Lightning) obstacle interactions
        /// </summary>
        private void HandleArcObstacle(Ingredient ingredient, GridGameManager gridManager)
        {
            // Arc obstacles create volatile/static states
            Debug.Log($"⚡ Arc obstacle: {ingredient.ItemName} creates static field");
            
            // Switch from Volatile to Static
            Debug.Log($"⚡ Volatile cell becomes Static, {ingredient.ItemName} potency increased by 20%");
            
            // Spawn up to 2 new Fulminating cells randomly
            SpawnRandomFulminatingCells(gridManager, 2);
            
            isCompleted = true;
        }
        
        /// <summary>
        /// Handle Divine (Holy) obstacle interactions
        /// </summary>
        private void HandleDivineObstacle(Ingredient ingredient, GridGameManager gridManager)
        {
            if (ingredient.IngredientAspect == Aspect.Divine && ingredient.IsUnrefined)
            {
                Debug.Log($"✨ Divine obstacle: Unrefined Divine {ingredient.ItemName} potency increased by 10%");
                isCompleted = true;
            }
            else
            {
                Debug.LogWarning($"⚠️ Divine obstacle: {ingredient.ItemName} cannot be placed (requires unrefined Divine aspect)");
            }
        }
        
        /// <summary>
        /// Handle FrigidMelted obstacle interactions
        /// </summary>
        private void HandleFrigidMeltedObstacle(Ingredient ingredient, GridGameManager gridManager)
        {
            if (ingredient.IngredientAspect == Aspect.Scorch)
            {
                // Scorch ingredients trigger "Evaporated" state - enhanced potency but ingredient is consumed
                Debug.Log($"❄️ FrigidMelted obstacle: Scorch {ingredient.ItemName} evaporates, potency increased by 30%");
                // Set state to Evaporated
            }
            else if (ingredient.IngredientAspect == Aspect.Frigid)
            {
                // Frigid ingredients trigger "Refrozen" state - revert back to FrigidFrozen
                Debug.Log($"❄️ FrigidMelted obstacle: Frigid {ingredient.ItemName} refreezes the cell");
                obstacleType = ObstacleType.FrigidFrozen;
                isCompleted = false;
                placedIngredient = null;
                return; // Don't mark as completed since it reverted
            }
            else if (ingredient.IngredientAspect == Aspect.Corporeal)
            {
                // Corporeal ingredients trigger "Stabilized" state - normal effect with slight bonus
                Debug.Log($"❄️ FrigidMelted obstacle: Corporeal {ingredient.ItemName} stabilizes the melted cell, potency increased by 10%");
                // Set state to Stabilized
            }
            else
            {
                // Default "Chilled" state for other ingredients
                Debug.Log($"❄️ FrigidMelted obstacle: {ingredient.ItemName} gets chilled, potency reduced by 10%");
                // Set state to Chilled
            }
            
            isCompleted = true;
        }
        
        /// <summary>
        /// Spawn random Fulminating cells for Arc obstacles
        /// </summary>
        private void SpawnRandomFulminatingCells(GridGameManager gridManager, int maxCount)
        {
            var emptyCells = new List<Vector2Int>();
            
            // Find available cells
            for (int x = 0; x < gridManager.gridWidth; x++)
            {
                for (int y = 0; y < gridManager.gridHeight; y++)
                {
                    var cell = gridManager.GetCell(x, y);
                    var pos = new Vector2Int(x, y);
                    
                    if (cell != null && !cell.IsOccupied && gridManager.GetObstacleAt(pos) == null)
                    {
                        emptyCells.Add(pos);
                    }
                }
            }
            
            // Spawn up to maxCount new Arc obstacles
            int spawnCount = Mathf.Min(maxCount, emptyCells.Count);
            for (int i = 0; i < spawnCount; i++)
            {
                if (emptyCells.Count > 0)
                {
                    int randomIndex = Random.Range(0, emptyCells.Count);
                    Vector2Int spawnPos = emptyCells[randomIndex];
                    emptyCells.RemoveAt(randomIndex);
                    
                    var newObstacle = new AspectObstacle(ObstacleType.Arc, spawnPos);
                    // Note: In a full implementation, you'd add this to the grid manager's obstacle list
                    Debug.Log($"⚡ New Arc obstacle spawned at ({spawnPos.x}, {spawnPos.y})");
                }
            }
        }
        
        /// <summary>
        /// Get ingredient size for burn calculations
        /// </summary>
        private int GetIngredientSize(Ingredient ingredient)
        {
            if (ingredient.ShapeData != null)
            {
                return ingredient.ShapeData.ActiveCells.Count;
            }
            return ingredient.GridWidth * ingredient.GridHeight;
        }

        /// <summary>
        /// Attempt to melt a frozen obstacle through adjacency
        /// </summary>
        public bool TryMeltFrozen(GridGameManager gridManager)
        {
            if (obstacleType != ObstacleType.FrigidFrozen || isCompleted)
                return false;

            // Check adjacent cells for scorch or corporeal aspects
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            
            foreach (var direction in directions)
            {
                Vector2Int adjacentPos = position + direction;
                var adjacentCell = gridManager.GetCell(adjacentPos.x, adjacentPos.y);
                
                if (adjacentCell != null && adjacentCell.IsOccupied)
                {
                    var adjacentIngredient = adjacentCell.OccupiedByIngredient;
                    if (adjacentIngredient.IngredientAspect == Aspect.Scorch ||
                        adjacentIngredient.IngredientAspect == Aspect.Corporeal)
                    {
                        // Transition from FrigidFrozen to FrigidMelted
                        obstacleType = ObstacleType.FrigidMelted;
                        isCompleted = false; // Reset completion status for the new state
                        Debug.Log($"❄️ Frigid obstacle melted by adjacent {adjacentIngredient.IngredientAspect} aspect - now FrigidMelted");
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check if this obstacle can be melted by a specific adjacent ingredient
        /// Used for validation and UI feedback
        /// </summary>
        public bool CanBeMeltedBy(Ingredient adjacentIngredient)
        {
            if (obstacleType != ObstacleType.FrigidFrozen || isCompleted)
                return false;

            return adjacentIngredient.IngredientAspect == Aspect.Scorch ||
                   adjacentIngredient.IngredientAspect == Aspect.Corporeal;
        }

        /// <summary>
        /// Get the current state description for the frigid obstacle
        /// </summary>
        public string GetFrigidStateDescription()
        {
            return obstacleType switch
            {
                ObstacleType.FrigidFrozen => "Frozen - Requires adjacent Scorch/Corporeal to melt",
                ObstacleType.FrigidMelted => "Melted - Can place ingredients with special effects",
                _ => "Not a frigid obstacle"
            };
        }

        /// <summary>
        /// Get the effective potency of an ingredient placed on this obstacle
        /// </summary>
        public int GetEffectivePotency(Ingredient ingredient)
        {
            if (ingredient == null) return 0;

            switch (obstacleType)
            {
                case ObstacleType.FrigidMelted:
                    // Varies based on ingredient aspect
                    if (ingredient.IngredientAspect == Aspect.Scorch)
                        return Mathf.RoundToInt(ingredient.Potency * 1.3f); // 30% increase for evaporation
                    else if (ingredient.IngredientAspect == Aspect.Corporeal)
                        return Mathf.RoundToInt(ingredient.Potency * 1.1f); // 10% increase for stabilization
                    else
                        return Mathf.RoundToInt(ingredient.Potency * 0.9f); // 10% decrease for chilled state
                
                case ObstacleType.Caustic:
                    // 20% reduction in potency
                    return Mathf.RoundToInt(ingredient.Potency * 0.8f);

                case ObstacleType.Divine:
                    // 10% increase in potency for unrefined divine aspects
                    if (ingredient.IngredientAspect == Aspect.Divine && ingredient.IsUnrefined)
                        return Mathf.RoundToInt(ingredient.Potency * 1.1f);
                    break;
            }

            return ingredient.Potency;
        }

        /// <summary>
        /// Trigger chaotic RNG effect for Arc obstacles
        /// </summary>
        private void TriggerArcEffect(GridGameManager gridManager)
        {
            float roll = Random.Range(0f, 100f);

            if (roll < 40f) // 40%: new moving obstacle spawns
            {
                Debug.Log("⚡ Arc Effect: New moving obstacle spawned!");
                // This would spawn a new obstacle in a random location
                SpawnNewMovingObstacle(gridManager);
            }
            else if (roll < 65f) // 25%: shuffle all obstacle locations
            {
                Debug.Log("⚡ Arc Effect: All obstacle locations shuffled!");
                // This would shuffle obstacle positions
                ShuffleObstacleLocations(gridManager);
            }
            else if (roll < 85f) // 20%: nudge placed ingredient 1 tile
            {
                Debug.Log("⚡ Arc Effect: Ingredient nudged 1 tile!");
                NudgeIngredient(gridManager);
            }
            else if (roll < 95f) // 10%: teleport ingredient with rotation
            {
                Debug.Log("⚡ Arc Effect: Ingredient teleported with random rotation!");
                TeleportIngredient(gridManager);
            }
            else // 5%: shuffle all ingredient placements
            {
                Debug.Log("⚡ Arc Effect: All ingredient placements shuffled!");
                ShuffleAllIngredients(gridManager);
            }
        }

        private void SpawnNewMovingObstacle(GridGameManager gridManager)
        {
            // Find a random empty cell to spawn a new obstacle
            var emptyCells = new List<Vector2Int>();
            for (int x = 0; x < gridManager.gridWidth; x++)
            {
                for (int y = 0; y < gridManager.gridHeight; y++)
                {
                    var cell = gridManager.GetCell(x, y);
                    if (cell != null && !cell.IsOccupied)
                    {
                        emptyCells.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (emptyCells.Count > 0)
            {
                var randomPos = emptyCells[Random.Range(0, emptyCells.Count)];
                var newObstacleType = (ObstacleType)Random.Range(0, System.Enum.GetValues(typeof(ObstacleType)).Length);
                // GridManager would need to handle spawning new obstacles
                Debug.Log($"New {newObstacleType} obstacle would spawn at {randomPos}");
            }
        }

        private void ShuffleObstacleLocations(GridGameManager gridManager)
        {
            // This would be implemented by the GridGameManager to shuffle all obstacles
            Debug.Log("Shuffling obstacle locations...");
        }

        private void NudgeIngredient(GridGameManager gridManager)
        {
            if (placedIngredient == null) return;

            // Find current ingredient position and try to move it 1 tile in a random direction
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            var randomDirection = directions[Random.Range(0, directions.Length)];
            var newPosition = position + randomDirection;

            // GridManager would handle moving the ingredient
            Debug.Log($"Ingredient would nudge from {position} to {newPosition}");
        }

        private void TeleportIngredient(GridGameManager gridManager)
        {
            if (placedIngredient == null) return;

            // Find a random empty location to teleport the ingredient
            var emptyCells = new List<Vector2Int>();
            for (int x = 0; x < gridManager.gridWidth; x++)
            {
                for (int y = 0; y < gridManager.gridHeight; y++)
                {
                    var cell = gridManager.GetCell(x, y);
                    if (cell != null && !cell.IsOccupied)
                    {
                        emptyCells.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (emptyCells.Count > 0)
            {
                var randomPos = emptyCells[Random.Range(0, emptyCells.Count)];
                Debug.Log($"Ingredient would teleport from {position} to {randomPos} with random rotation");
            }
        }

        private void ShuffleAllIngredients(GridGameManager gridManager)
        {
            // GridManager would handle shuffling all placed ingredients
            Debug.Log("All ingredient placements would be shuffled with randomized rotations");
        }

        /// <summary>
        /// Get visual representation color for the obstacle (matches aspect colors for consistency)
        /// </summary>
        public Color GetObstacleColor()
        {
            return obstacleType switch
            {
                ObstacleType.Corporeal => new Color(0.8f, 0.6f, 0.4f, 0.9f), // Brown/Earth (matches Corporeal aspect)
                ObstacleType.Scorch => new Color(1.0f, 0.4f, 0.2f, 0.9f),    // Fire Red (matches Scorch aspect)  
                ObstacleType.FrigidFrozen => new Color(0.2f, 0.4f, 1.0f, 0.9f),    // Dark Ice Blue (matches Frigid aspect)
                ObstacleType.FrigidMelted => new Color(0.4f, 0.8f, 1.0f, 0.9f), // Ice Blue (matches Frigid aspect)
                ObstacleType.Arc => new Color(1.0f, 1.0f, 0.4f, 0.9f),       // Lightning Yellow (matches Arc aspect)
                ObstacleType.Caustic => new Color(0.6f, 1.0f, 0.2f, 0.9f),   // Acid Green (matches Caustic aspect)
                ObstacleType.Divine => new Color(1.0f, 0.8f, 1.0f, 0.9f),    // Holy Purple (matches Divine aspect)
                ObstacleType.Void => new Color(0.1f, 0.1f, 0.1f, 0.95f),     // Dark void (mostly transparent)
                _ => new Color(0.7f, 0.7f, 0.7f, 0.9f)                      // Default grey
            };
        }

        /// <summary>
        /// Check if this obstacle subtracts from available grid space
        /// Only Corporeal and Void obstacles block space entirely
        /// </summary>
        public bool SubtractsFromAvailableSpace()
        {
            return obstacleType == ObstacleType.Corporeal || obstacleType == ObstacleType.Void;
        }

        /// <summary>
        /// Check if this obstacle should be counted for completion metrics
        /// Void and Corporeal obstacles are not counted for completion
        /// </summary>
        public bool CountsForCompletion()
        {
            return obstacleType != ObstacleType.Void && obstacleType != ObstacleType.Corporeal;
        }

        /// <summary>
        /// Get description of the obstacle for UI
        /// </summary>
        public string GetObstacleDescription()
        {
            return obstacleType switch
            {
                ObstacleType.Corporeal => "Blocked cell - cannot place ingredients",
                ObstacleType.FrigidFrozen => "Frozen cell - unlock with adjacent Scorch/Corporeal",
                ObstacleType.FrigidMelted => "Melted cell - unlocked with adjacent Scorch/Corporeal",
                ObstacleType.Scorch => "Volatile cell - requires Scorch, Caustic, or Arc aspects",
                ObstacleType.Caustic => "Degrade cell - reduces ingredient potency by 20%",
                ObstacleType.Arc => "Chaotic cell - triggers random effects",
                ObstacleType.Divine => "Sanctified cell - only unrefined Divine aspects (+10% potency)",
                ObstacleType.Void => "Void cell - cosmetic only, cannot place ingredients",
                _ => "Unknown obstacle"
            };
        }
    }
}