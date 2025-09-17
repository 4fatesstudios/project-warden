using UnityEngine;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    public enum ObstacleType
    {
        Corporeal,  // Blocked cell (stone/residue)
        Frigid,     // Frozen cell, needs adjacency to unlock
        Scorch,     // Volatile cell, needs compatible aspects
        Caustic,    // Degrade cell, reduces potency by 20%
        Arc,        // Chaotic RNG effects
        Divine      // Sanctified cell, only unrefined divine aspects
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

                case ObstacleType.Frigid:
                    return false; // Frozen cells need to be melted first by adjacency

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
                case ObstacleType.Scorch:
                    // Completion is defined by placing an accepted aspect
                    isCompleted = true;
                    Debug.Log($"✅ Scorch obstacle completed with {ingredient.IngredientAspect} aspect");
                    break;

                case ObstacleType.Caustic:
                    // Completion is defined by placing any ingredient (with penalty)
                    isCompleted = true;
                    Debug.Log($"✅ Caustic obstacle completed (ingredient potency reduced by 20%)");
                    break;

                case ObstacleType.Arc:
                    // Trigger chaotic RNG effect
                    TriggerArcEffect(gridManager);
                    isCompleted = true;
                    Debug.Log($"✅ Arc obstacle completed with chaotic effect");
                    break;

                case ObstacleType.Divine:
                    // Completion is defined by placing unrefined divine aspect
                    isCompleted = true;
                    Debug.Log($"✅ Divine obstacle completed with unrefined divine aspect");
                    break;
            }

            return true;
        }

        /// <summary>
        /// Attempt to melt a frozen obstacle through adjacency
        /// </summary>
        public bool TryMeltFrozen(GridGameManager gridManager)
        {
            if (obstacleType != ObstacleType.Frigid || isCompleted)
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
                        isCompleted = true;
                        Debug.Log($"❄️ Frigid obstacle melted by adjacent {adjacentIngredient.IngredientAspect} aspect");
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Get the effective potency of an ingredient placed on this obstacle
        /// </summary>
        public int GetEffectivePotency(Ingredient ingredient)
        {
            if (ingredient == null) return 0;

            switch (obstacleType)
            {
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
        /// Get visual representation color for the obstacle
        /// </summary>
        public Color GetObstacleColor()
        {
            return obstacleType switch
            {
                ObstacleType.Corporeal => Color.grey,                        // Grey
                ObstacleType.Scorch => Color.red,                           // Red  
                ObstacleType.Frigid => new Color(0.68f, 0.85f, 0.90f, 1f),  // Light blue #ADD8E6
                ObstacleType.Arc => Color.yellow,                           // Yellow
                ObstacleType.Caustic => Color.green,                        // Green
                ObstacleType.Divine => new Color(0.5f, 0.0f, 0.5f, 1f),     // Purple #800080
                _ => Color.gray
            };
        }

        /// <summary>
        /// Get description of the obstacle for UI
        /// </summary>
        public string GetObstacleDescription()
        {
            return obstacleType switch
            {
                ObstacleType.Corporeal => "Blocked cell - cannot place ingredients",
                ObstacleType.Frigid => "Frozen cell - unlock with adjacent Scorch/Corporeal",
                ObstacleType.Scorch => "Volatile cell - requires Scorch, Caustic, or Arc aspects",
                ObstacleType.Caustic => "Degrade cell - reduces ingredient potency by 20%",
                ObstacleType.Arc => "Chaotic cell - triggers random effects",
                ObstacleType.Divine => "Sanctified cell - only unrefined Divine aspects (+10% potency)",
                _ => "Unknown obstacle"
            };
        }
    }
}