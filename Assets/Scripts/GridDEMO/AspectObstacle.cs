using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    public enum ObstacleType
    {
        Corporeal,      // Blocked cells - can be erupted and removed with specific ingredient combinations
        FrigidFrozen,   // Frozen cells - must be melted before placement
        FrigidMelted,   // Melted cells - unlocked and can place ingredients
        Scorch,         // Enflamed cells - HP-based burn/enhance mechanics
        Caustic,        // Deteriorating cells - multi-state potency modifiers
        Arc,            // Fulminating cells - Volatile/Static states with spawning
        Divine,         // Sanctified cells - Divine aspects only, global potency bonus
        Void            // Cosmetic void - disables visuals, blocks placement, not counted for completion
    }
    
    public enum CausticState
    {
        Deteriorated,  // Non-Caustic ingredients: -20% potency
        Controlled,    // Herb ingredients: -60% negative effects
        Hyperactive    // Caustic ingredients: +20% potency
    }
    
    public enum ArcState
    {
        Volatile,  // Default: -20% total potion potency
        Static     // Placed: +20% ingredient potency, spawns children
    }

    [System.Serializable]
    public class AspectObstacle
    {
        [SerializeField] private ObstacleType obstacleType;
        [SerializeField] private Vector2Int position;
        [SerializeField] private bool isCompleted = false;
        [SerializeField] private Ingredient placedIngredient;
        
        [SerializeField] private int currentHP = 2;
        [SerializeField] private int maxHP = 2;
        
        [SerializeField] private List<CausticState> activeCausticStates = new List<CausticState>();
        
        [SerializeField] private ArcState arcState = ArcState.Volatile;
        [SerializeField] private List<AspectObstacle> childArcObstacles = new List<AspectObstacle>();
        [SerializeField] private AspectObstacle parentArcObstacle = null;
        [SerializeField] private int arcSpawnCount = 0;
        
        [SerializeField] private List<Ingredient> eruptionIngredients = new List<Ingredient>();
        [SerializeField] private bool isErupted = false;

        public ObstacleType ObstacleType => obstacleType;
        public Vector2Int Position => position;
        public bool IsCompleted => isCompleted;
        public Ingredient PlacedIngredient => placedIngredient;
        public int CurrentHP => currentHP;
        public int MaxHP => maxHP;
        public ArcState ArcState => arcState;
        public List<CausticState> ActiveCausticStates => activeCausticStates;
        public bool IsErupted => isErupted;
        public bool IsFrozenUnlocked => obstacleType == ObstacleType.FrigidMelted;

        public AspectObstacle(ObstacleType type, Vector2Int pos)
        {
            obstacleType = type;
            position = pos;
            isCompleted = false;
            placedIngredient = null;
            
            if (type == ObstacleType.Scorch)
            {
                currentHP = 2;
                maxHP = 2;
            }
            
            if (type == ObstacleType.Arc)
            {
                arcState = ArcState.Volatile;
            }
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
                    return isErupted;

                case ObstacleType.FrigidFrozen:
                    return false;

                case ObstacleType.FrigidMelted:
                    return true;

                case ObstacleType.Scorch:
                    return currentHP > 0;

                case ObstacleType.Caustic:
                    return true;

                case ObstacleType.Arc:
                    return true;

                case ObstacleType.Divine:
                    return ingredient.IngredientAspect == Aspect.Divine;

                case ObstacleType.Void:
                    return false;

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
                    if (isErupted)
                    {
                        Debug.Log($"🪨 Corporeal: Placed {ingredient.ItemName} on erupted blocked cell");
                        isCompleted = true;
                    }
                    break;

                case ObstacleType.FrigidMelted:
                    Debug.Log($"❄️ Frigid: Placed {ingredient.ItemName} on melted frozen cell");
                    isCompleted = true;
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

            bool isIncompatible = (ingredient.IngredientAspect == Aspect.Corporeal ||
                                 ingredient.IngredientAspect == Aspect.Frigid);

            int ingredientSize = GetIngredientSize(ingredient);

            if (isIncompatible)
            {
                float potencyReduction = (1.1f / ingredientSize) * 100f;
                Debug.Log($"🔥 Scorch: {ingredient.ItemName} burned! Potency reduced by {potencyReduction:F1}%");
                
                currentHP = Mathf.Max(0, currentHP - 1);
                Debug.Log($"🔥 Scorch: Cell HP reduced to {currentHP}/{maxHP}");
                
                if (currentHP == 0)
                {
                    isCompleted = true;
                    Debug.Log($"🔥 Scorch: Flame quenched!");
                }
            }
            else if (isCompatible)
            {
                float potencyBonus = (0.5f / ingredientSize) * 100f;
                Debug.Log($"🔥 Scorch: {ingredient.ItemName} enhanced! Potency increased by {potencyBonus:F1}%");
                isCompleted = true;
            }
        }
        
        /// <summary>
        /// Handle Caustic (Poison/Acid) obstacle interactions  
        /// </summary>
        private void HandleCausticObstacle(Ingredient ingredient, GridGameManager gridManager)
        {
            activeCausticStates.Clear();
            
            if (ingredient.IngredientArchetype == IngredientArchetype.Herb)
            {
                activeCausticStates.Add(CausticState.Controlled);
                Debug.Log($"🧪 Caustic: Herb {ingredient.ItemName} → Controlled state (-60% negative effects)");
            }
            
            if (ingredient.IngredientAspect == Aspect.Caustic)
            {
                activeCausticStates.Add(CausticState.Hyperactive);
                Debug.Log($"🧪 Caustic: Caustic {ingredient.ItemName} → Hyperactive state (+20% potency)");
            }
            
            if (activeCausticStates.Count == 0)
            {
                activeCausticStates.Add(CausticState.Deteriorated);
                Debug.Log($"🧪 Caustic: {ingredient.ItemName} → Deteriorated state (-20% potency)");
            }
            
            isCompleted = true;
        }
        
        /// <summary>
        /// Handle Arc (Lightning) obstacle interactions
        /// </summary>
        private void HandleArcObstacle(Ingredient ingredient, GridGameManager gridManager)
        {
            if (arcState == ArcState.Volatile)
            {
                arcState = ArcState.Static;
                Debug.Log($"⚡ Arc: Volatile → Static, {ingredient.ItemName} potency increased by 20%");
                
                if (arcSpawnCount < 2)
                {
                    int spawnCount = Mathf.Min(2 - arcSpawnCount, 2);
                    SpawnChildArcObstacles(gridManager, spawnCount);
                    arcSpawnCount += spawnCount;
                }
                
                isCompleted = true;
            }
        }
        
        private void SpawnChildArcObstacles(GridGameManager gridManager, int count)
        {
            var emptyCells = new List<Vector2Int>();
            
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
            
            int spawned = 0;
            for (int i = 0; i < count && emptyCells.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, emptyCells.Count);
                Vector2Int spawnPos = emptyCells[randomIndex];
                emptyCells.RemoveAt(randomIndex);
                
                var childObstacle = new AspectObstacle(ObstacleType.Arc, spawnPos);
                childObstacle.parentArcObstacle = this;
                childArcObstacles.Add(childObstacle);
                
                Debug.Log($"⚡ Arc: Spawned child Fulminating cell at ({spawnPos.x}, {spawnPos.y})");
                spawned++;
            }
            
            Debug.Log($"⚡ Arc: Spawned {spawned} child cells (total spawns: {arcSpawnCount})");
        }
        
        public void OnIngredientRemoved()
        {
            placedIngredient = null;
            
            if (obstacleType == ObstacleType.Arc && arcState == ArcState.Static)
            {
                arcState = ArcState.Volatile;
                Debug.Log($"⚡ Arc: Static → Volatile, child cells reset");
                
                foreach (var child in childArcObstacles)
                {
                    child.arcState = ArcState.Volatile;
                    child.isCompleted = false;
                }
            }
            else if (obstacleType == ObstacleType.Scorch)
            {
                currentHP = Mathf.Min(currentHP + 1, maxHP);
                Debug.Log($"🔥 Scorch: Ingredient returned to inventory, HP restored to {currentHP}/{maxHP}");
            }
        }
        
        /// <summary>
        /// Handle Divine (Holy) obstacle interactions
        /// </summary>
        private void HandleDivineObstacle(Ingredient ingredient, GridGameManager gridManager)
        {
            if (ingredient.IngredientAspect == Aspect.Divine)
            {
                Debug.Log($"✨ Divine: {ingredient.ItemName} → +10% overall potion potency");
                isCompleted = true;
            }
            else
            {
                Debug.LogWarning($"⚠️ Divine: {ingredient.ItemName} cannot be placed (requires Divine aspect)");
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

        public bool TryMeltFrozen(GridGameManager gridManager)
        {
            if (obstacleType != ObstacleType.FrigidFrozen)
                return false;

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            
            int adjacentCorporealCount = 0;
            bool hasAdjacentScorch = false;
            
            foreach (var direction in directions)
            {
                Vector2Int adjacentPos = position + direction;
                var adjacentCell = gridManager.GetCell(adjacentPos.x, adjacentPos.y);
                
                if (adjacentCell != null && adjacentCell.IsOccupied)
                {
                    var adjacentIngredient = adjacentCell.OccupiedByIngredient;
                    
                    if (adjacentIngredient.IngredientAspect == Aspect.Scorch)
                    {
                        hasAdjacentScorch = true;
                    }
                    else if (adjacentIngredient.IngredientAspect == Aspect.Corporeal)
                    {
                        adjacentCorporealCount++;
                    }
                }
            }
            
            if (hasAdjacentScorch)
            {
                obstacleType = ObstacleType.FrigidMelted;
                Debug.Log($"❄️ FrigidFrozen: Melted by adjacent Scorch ingredient");
                return true;
            }
            
            if (adjacentCorporealCount >= 3)
            {
                obstacleType = ObstacleType.FrigidMelted;
                Debug.Log($"❄️ FrigidFrozen: Melted by {adjacentCorporealCount} adjacent Corporeal ingredients");
                return true;
            }
            
            return false;
        }
        
        public bool TryMeltFrozen(GridCraftingManager gridManager)
        {
            if (obstacleType != ObstacleType.FrigidFrozen)
                return false;

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            
            int adjacentCorporealCount = 0;
            bool hasAdjacentScorch = false;
            
            foreach (var direction in directions)
            {
                Vector2Int adjacentPos = position + direction;
                
                if (adjacentPos.x < 0 || adjacentPos.x >= GridCraftingManager.GRID_SIZE ||
                    adjacentPos.y < 0 || adjacentPos.y >= GridCraftingManager.GRID_SIZE)
                    continue;
                
                var placedIngredient = gridManager.GetPlacedAt(adjacentPos.x, adjacentPos.y);
                
                if (placedIngredient != null && placedIngredient.ingredient != null)
                {
                    if (placedIngredient.ingredient.IngredientAspect == Aspect.Scorch)
                    {
                        hasAdjacentScorch = true;
                    }
                    else if (placedIngredient.ingredient.IngredientAspect == Aspect.Corporeal)
                    {
                        adjacentCorporealCount++;
                    }
                }
            }
            
            if (hasAdjacentScorch)
            {
                obstacleType = ObstacleType.FrigidMelted;
                Debug.Log($"❄️ FrigidFrozen: Melted by adjacent Scorch ingredient");
                return true;
            }
            
            if (adjacentCorporealCount >= 3)
            {
                obstacleType = ObstacleType.FrigidMelted;
                Debug.Log($"❄️ FrigidFrozen: Melted by {adjacentCorporealCount} adjacent Corporeal ingredients");
                return true;
            }
            
            return false;
        }
        
        public int GetEffectivePotency(Ingredient ingredient)
        {
            if (ingredient == null)
                return 0;
                
            float potency = ingredient.Potency;
            
            switch (obstacleType)
            {
                case ObstacleType.Scorch:
                    bool isCompatible = (ingredient.IngredientAspect == Aspect.Scorch ||
                                       ingredient.IngredientAspect == Aspect.Caustic ||
                                       ingredient.IngredientAspect == Aspect.Arc);
                    bool isIncompatible = (ingredient.IngredientAspect == Aspect.Corporeal ||
                                         ingredient.IngredientAspect == Aspect.Frigid);
                    int ingredientSize = GetIngredientSize(ingredient);
                    
                    if (isIncompatible)
                    {
                        potency -= (1.1f / ingredientSize);
                    }
                    else if (isCompatible)
                    {
                        potency += (0.5f / ingredientSize);
                    }
                    break;
                    
                case ObstacleType.Caustic:
                    if (activeCausticStates.Contains(CausticState.Hyperactive))
                    {
                        potency *= 1.2f;
                    }
                    else if (activeCausticStates.Contains(CausticState.Deteriorated))
                    {
                        potency *= 0.8f;
                    }
                    break;
                    
                case ObstacleType.Arc:
                    if (arcState == ArcState.Static)
                    {
                        potency *= 1.2f;
                    }
                    else
                    {
                        potency *= 0.8f;
                    }
                    break;
                    
                case ObstacleType.Divine:
                    if (ingredient.IngredientAspect == Aspect.Divine)
                    {
                        potency *= 1.1f;
                    }
                    break;
            }
            
            return Mathf.RoundToInt(potency);
        }
        
        public bool TryEruptCorporeal(GridGameManager gridManager, List<Ingredient> placedIngredients)
        {
            if (obstacleType != ObstacleType.Corporeal || isErupted)
                return false;

            Vector2Int[] adjacentDirections = new Vector2Int[]
            {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
                Vector2Int.up + Vector2Int.left, Vector2Int.up + Vector2Int.right,
                Vector2Int.down + Vector2Int.left, Vector2Int.down + Vector2Int.right
            };
            
            bool hasFrigid = false;
            bool hasCaustic = false;
            bool hasScorch = false;
            bool hasArc = false;
            
            List<Ingredient> triggeringIngredients = new List<Ingredient>();
            
            foreach (var direction in adjacentDirections)
            {
                Vector2Int adjacentPos = position + direction;
                var adjacentCell = gridManager.GetCell(adjacentPos.x, adjacentPos.y);
                
                if (adjacentCell != null && adjacentCell.IsOccupied)
                {
                    var ingredient = adjacentCell.OccupiedByIngredient;
                    
                    if (ingredient.IngredientAspect == Aspect.Frigid)
                    {
                        hasFrigid = true;
                        if (!triggeringIngredients.Contains(ingredient))
                            triggeringIngredients.Add(ingredient);
                    }
                    else if (ingredient.IngredientAspect == Aspect.Caustic)
                    {
                        hasCaustic = true;
                        if (!triggeringIngredients.Contains(ingredient))
                            triggeringIngredients.Add(ingredient);
                    }
                    else if (ingredient.IngredientAspect == Aspect.Scorch)
                    {
                        hasScorch = true;
                        if (!triggeringIngredients.Contains(ingredient))
                            triggeringIngredients.Add(ingredient);
                    }
                    else if (ingredient.IngredientAspect == Aspect.Arc)
                    {
                        hasArc = true;
                        if (!triggeringIngredients.Contains(ingredient))
                            triggeringIngredients.Add(ingredient);
                    }
                }
            }
            
            bool canErupt = (hasFrigid && hasCaustic) || (hasScorch && hasArc);
            
            if (canErupt)
            {
                isErupted = true;
                eruptionIngredients = new List<Ingredient>(triggeringIngredients);
                
                string combo = (hasFrigid && hasCaustic) ? "Frigid+Caustic" : "Scorch+Arc";
                Debug.Log($"🪨 Corporeal: Erupted by {combo} combination!");
                Debug.Log($"🪨 Corporeal: Tracking {eruptionIngredients.Count} triggering ingredients");
                
                return true;
            }
            
            return false;
        }
        
        public bool TryEruptCorporeal(GridCraftingManager gridManager, List<Ingredient> placedIngredients)
        {
            if (obstacleType != ObstacleType.Corporeal || isErupted)
                return false;

            Vector2Int[] adjacentDirections = new Vector2Int[]
            {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
                Vector2Int.up + Vector2Int.left, Vector2Int.up + Vector2Int.right,
                Vector2Int.down + Vector2Int.left, Vector2Int.down + Vector2Int.right
            };
            
            bool hasFrigid = false;
            bool hasCaustic = false;
            bool hasScorch = false;
            bool hasArc = false;
            
            List<Ingredient> triggeringIngredients = new List<Ingredient>();
            
            foreach (var direction in adjacentDirections)
            {
                Vector2Int adjacentPos = position + direction;
                
                if (adjacentPos.x < 0 || adjacentPos.x >= GridCraftingManager.GRID_SIZE ||
                    adjacentPos.y < 0 || adjacentPos.y >= GridCraftingManager.GRID_SIZE)
                    continue;
                
                var placedInstance = gridManager.GetPlacedAt(adjacentPos.x, adjacentPos.y);
                
                if (placedInstance != null && placedInstance.ingredient != null)
                {
                    var ingredient = placedInstance.ingredient;
                    
                    if (ingredient.IngredientAspect == Aspect.Frigid)
                    {
                        hasFrigid = true;
                        if (!triggeringIngredients.Contains(ingredient))
                            triggeringIngredients.Add(ingredient);
                    }
                    else if (ingredient.IngredientAspect == Aspect.Caustic)
                    {
                        hasCaustic = true;
                        if (!triggeringIngredients.Contains(ingredient))
                            triggeringIngredients.Add(ingredient);
                    }
                    else if (ingredient.IngredientAspect == Aspect.Scorch)
                    {
                        hasScorch = true;
                        if (!triggeringIngredients.Contains(ingredient))
                            triggeringIngredients.Add(ingredient);
                    }
                    else if (ingredient.IngredientAspect == Aspect.Arc)
                    {
                        hasArc = true;
                        if (!triggeringIngredients.Contains(ingredient))
                            triggeringIngredients.Add(ingredient);
                    }
                }
            }
            
            bool canErupt = (hasFrigid && hasCaustic) || (hasScorch && hasArc);
            
            if (canErupt)
            {
                isErupted = true;
                eruptionIngredients = new List<Ingredient>(triggeringIngredients);
                
                string combo = (hasFrigid && hasCaustic) ? "Frigid+Caustic" : "Scorch+Arc";
                Debug.Log($"🪨 Corporeal: Erupted by {combo} combination!");
                Debug.Log($"🪨 Corporeal: Tracking {eruptionIngredients.Count} triggering ingredients");
                
                return true;
            }
            
            return false;
        }
        
        public bool CheckEruptionIngredientsStillPresent(List<Ingredient> currentIngredients)
        {
            if (obstacleType != ObstacleType.Corporeal || !isErupted)
                return true;

            foreach (var ingredient in eruptionIngredients)
            {
                if (!currentIngredients.Contains(ingredient))
                {
                    Debug.Log($"🪨 Corporeal: Eruption ingredient {ingredient.ItemName} removed - regenerating block!");
                    isErupted = false;
                    eruptionIngredients.Clear();
                    isCompleted = false;
                    return false;
                }
            }
            
            return true;
        }

        public Color GetObstacleColor()
        {
            return obstacleType switch
            {
                ObstacleType.Corporeal => new Color(0.8f, 0.6f, 0.4f, 0.9f),
                ObstacleType.Scorch => new Color(1.0f, 0.4f, 0.2f, 0.9f),
                ObstacleType.FrigidFrozen => new Color(0.2f, 0.4f, 1.0f, 0.9f),
                ObstacleType.FrigidMelted => new Color(0.4f, 0.8f, 1.0f, 0.9f),
                ObstacleType.Arc => new Color(1.0f, 1.0f, 0.4f, 0.9f),
                ObstacleType.Caustic => new Color(0.6f, 1.0f, 0.2f, 0.9f),
                ObstacleType.Divine => new Color(1.0f, 0.8f, 1.0f, 0.9f),
                ObstacleType.Void => new Color(0.1f, 0.1f, 0.1f, 0.95f),
                _ => new Color(0.7f, 0.7f, 0.7f, 0.9f)
            };
        }

        public bool SubtractsFromAvailableSpace()
        {
            if (obstacleType == ObstacleType.Corporeal)
                return !isErupted;
            
            if (obstacleType == ObstacleType.FrigidFrozen)
                return true;
            
            return obstacleType == ObstacleType.Void;
        }

        public bool CountsForCompletion()
        {
            return obstacleType != ObstacleType.Void && obstacleType != ObstacleType.Corporeal;
        }

        public string GetObstacleDescription()
        {
            return obstacleType switch
            {
                ObstacleType.Corporeal => isErupted 
                    ? "Blocked cell - erupted (can now place ingredients)" 
                    : "Blocked cell - erupt with Frigid+Caustic or Scorch+Arc proximity",
                ObstacleType.FrigidFrozen => "Frozen cell - melt with 1 Scorch or 3 Corporeal adjacent",
                ObstacleType.FrigidMelted => "Melted cell - unlocked (can place ingredients)",
                ObstacleType.Scorch => $"Enflamed cell ({currentHP}/{maxHP} HP) - burns incompatible, enhances compatible",
                ObstacleType.Caustic => "Deteriorating cell - multi-state potency modifiers",
                ObstacleType.Arc => arcState == ArcState.Volatile 
                    ? "Fulminating cell (Volatile) - reduces total potency by 20%" 
                    : "Fulminating cell (Static) - ingredient +20% potency, spawns children",
                ObstacleType.Divine => "Sanctified cell - Divine aspects only (+10% overall potency)",
                ObstacleType.Void => "Void cell - cosmetic only, cannot place ingredients",
                _ => "Unknown obstacle"
            };
        }
    }
}