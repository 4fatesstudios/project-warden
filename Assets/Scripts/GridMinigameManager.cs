using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    public class GridCraftingManager : MonoBehaviour
    {
        public const int GRID_SIZE = 5;
        
        [Header("UI References")]
        public UIDocument uiDocument;
        
        [Header("Grid Settings")]
        [SerializeField] private bool recipeMode = false;
        
        [Header("Recipe System")]
        private AlchemyRecipe currentRecipe;
        private bool isProcessingRecipe = false;
        
        // Events
        public event Action OnGridChanged;
        
        // Internal data - using existing IngredientInstance structure
        private IngredientInstance[,] placed = new IngredientInstance[GRID_SIZE, GRID_SIZE];
        private bool[,] occupied = new bool[GRID_SIZE, GRID_SIZE];
        private bool[,] unlocked = new bool[GRID_SIZE, GRID_SIZE];
        
        // Obstacle system
        [Header("Obstacle System")]
        [SerializeField] private AspectObstacle[,] obstacles = new AspectObstacle[GRID_SIZE, GRID_SIZE];
        [SerializeField] private bool enableObstacles = true;
        
        // Initialize some test obstacles for demonstration
        [Space]
        [Header("Test Configuration")]
        [SerializeField] private bool createTestObstacles = true;
        #pragma warning disable 0414
        [SerializeField, Tooltip("Force refresh flag")] private bool refreshFlag = true;
        #pragma warning restore 0414
        
        // UI Elements
        private VisualElement gridViewport;
        private VisualElement craftButton;
        private VisualElement rightContainer;
        private VisualElement inventoryContainer;
        private VisualElement itemBanner;
        
        // Grid tiles
        private VisualElement[,] gridTiles = new VisualElement[GRID_SIZE, GRID_SIZE];
        
        // State tracking
        private int placedCount = 0;
        private IngredientShapeData hoveredShape;
        private Vector2Int hoveredAnchor;
        private int hoveredRotation = 0;
        
        // Quantity tracking - pending changes before craft
        private Dictionary<Ingredient, int> pendingQuantityChanges = new Dictionary<Ingredient, int>();
        
        public int PlacedCount => placedCount;
        public VisualElement viewRoot => uiDocument?.rootVisualElement;
        public int gridWidth => GRID_SIZE;
        public int gridHeight => GRID_SIZE;
        
        public AlchemyRecipe CurrentRecipe 
        { 
            get => currentRecipe; 
            set => currentRecipe = value; 
        }
        
        public bool IsProcessingRecipe => isProcessingRecipe;
        
        public Dictionary<Vector2Int, Ingredient> GetPlacedIngredients()
        {
            var result = new Dictionary<Vector2Int, Ingredient>();
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    if (placed[x, y] != null)
                    {
                        result[new Vector2Int(x, y)] = placed[x, y].ingredient;
                    }
                }
            }
            return result;
        }
        
        public List<AspectObstacle> GetAspectObstacles()
        {
            var result = new List<AspectObstacle>();
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    if (obstacles[x, y] != null)
                    {
                        result.Add(obstacles[x, y]);
                    }
                }
            }
            return result;
        }
        
        public List<Vector2Int> GetIngredientCells(Ingredient ingredient, Vector2Int anchor)
        {
            var cells = new List<Vector2Int>();
            var shape = ingredient.GetShape();
            if (shape == null) return cells;
            
            int width = shape.GetLength(0);
            int height = shape.GetLength(1);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (shape[x, y])
                    {
                        cells.Add(new Vector2Int(anchor.x + x, anchor.y + y));
                    }
                }
            }
            
            return cells;
        }
        
        public int GetPendingQuantityChange(Ingredient ingredient)
        {
            return pendingQuantityChanges.ContainsKey(ingredient) ? pendingQuantityChanges[ingredient] : 0;
        }
        
        public void CommitQuantityChanges()
        {
            var inventory = FindFirstObjectByType<FourFatesStudios.ProjectWarden.ItemSlotContainerHolder>();
            if (inventory == null || inventory.Container == null)
            {
                Debug.LogWarning("Cannot commit quantity changes - inventory not found");
                return;
            }
            
            foreach (var kvp in pendingQuantityChanges)
            {
                Ingredient ingredient = kvp.Key;
                int change = kvp.Value;
                
                if (change > 0)
                {
                    inventory.RemoveItem(ingredient, change);
                    Debug.Log($"Committed: Removed {change} of {ingredient.ItemName} from inventory");
                }
            }
            
            pendingQuantityChanges.Clear();
        }
        
        public void ResetPendingQuantityChanges()
        {
            pendingQuantityChanges.Clear();
            OnGridChanged?.Invoke();
        }
        
        void Start()
        {
            // Auto-assign UIDocument if not set
            if (uiDocument == null)
            {
                uiDocument = FindFirstObjectByType<UIDocument>();
                if (uiDocument != null)
                {
                    Debug.Log($"GridCraftingManager: Auto-found UIDocument on '{uiDocument.gameObject.name}'");
                }
            }
            
            // Initialize obstacles before UI
            InitializeObstacles();
            
            // Wait for UIDocument to be ready
            if (uiDocument != null && uiDocument.rootVisualElement != null)
            {
                Initialize(recipeMode);
            }
            else
            {
                StartCoroutine(WaitForUIDocument());
            }
        }
        
        private System.Collections.IEnumerator WaitForUIDocument()
        {
            while (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                yield return null;
            }
            
            Debug.Log("GridCraftingManager: UIDocument is now ready, initializing");
            Initialize(recipeMode);
        }
        
        private void InitializeObstacles()
        {
            if (!enableObstacles || !createTestObstacles) return;
            
            // Create some test obstacles for demonstration
            obstacles[1, 1] = new AspectObstacle(ObstacleType.Corporeal, new Vector2Int(1, 1)); // Blocked cell
            obstacles[3, 2] = new AspectObstacle(ObstacleType.FrigidFrozen, new Vector2Int(3, 2));
            obstacles[2, 3] = new AspectObstacle(ObstacleType.Scorch, new Vector2Int(2, 3)); // Scorch cell
            obstacles[4, 0] = new AspectObstacle(ObstacleType.Caustic, new Vector2Int(4, 0)); // Caustic cell
            
            // Debug.Log("🛡️ Initialized test obstacles:");
            // Debug.Log("  • Corporeal (blocked) at (1,1) - No ingredients allowed");
            // Debug.Log("  • Frozen at (3,2) - Requires melting by adjacent Scorch/Corporeal");
            // Debug.Log("  • Scorch at (2,3) - Only Scorch/Caustic/Arc aspects allowed");
            // Debug.Log("  • Caustic at (4,0) - Any ingredient allowed (reduced potency)");
        }
        
        public void Initialize(bool recipeMode)
        {
            this.recipeMode = recipeMode;
            
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument not assigned to GridCraftingManager! Please assign the CraftingUI GameObject's UIDocument component to this field in the inspector.");
                return;
            }
            
            SetupUI();
            
            // Only proceed if UI setup was successful
            if (gridViewport == null)
            {
                Debug.LogError("Failed to initialize GridCraftingManager: UI elements not found");
                return;
            }
            
            CreateGridTiles();
            UpdateGridVisibility();
            
            // Debug.Log($"GridCraftingManager initialized in {(recipeMode ? "Recipe" : "Free Crafting")} mode");
        }
        
        void SetupUI()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
                if (uiDocument == null)
                {
                    uiDocument = FindFirstObjectByType<UIDocument>();
                    if (uiDocument != null)
                    {
                        Debug.Log($"GridCraftingManager: Automatically found UIDocument on GameObject '{uiDocument.gameObject.name}'");
                    }
                }
            }
            
            if (uiDocument == null)
            {
                Debug.LogError("GridCraftingManager: UIDocument not found! Please assign the UIDocument reference in the Inspector or ensure a UIDocument exists in the scene.");
                return;
            }
            
            var root = uiDocument.rootVisualElement;
            
            if (root == null)
            {
                Debug.LogError("GridCraftingManager: UIDocument.rootVisualElement is null! The UI may not have been loaded yet.");
                return;
            }
            
            // Find UI elements
            gridViewport = root.Q<VisualElement>("grid-viewport");
            craftButton = root.Q<Button>("craft-button");
            rightContainer = root.Q<VisualElement>("right-container");
            inventoryContainer = root.Q<VisualElement>("inventory-section");
            itemBanner = root.Q<VisualElement>("item-banner");
            
            // Validate critical UI elements
            if (gridViewport == null)
            {
                Debug.LogError("Grid viewport not found! Make sure the UIDocument is assigned and contains 'grid-viewport' element.");
                return;
            }
            
            // Initialize containers
            inventoryContainer?.SetEnabled(false);
            craftButton?.SetEnabled(false);
            
            // Setup event handlers
            SetupEventHandlers();
        }
        
        void SetupEventHandlers()
        {
            var clearButton = uiDocument.rootVisualElement.Q<Button>("clear-button");
            clearButton?.RegisterCallback<ClickEvent>(evt => ClearGridPreserveObstacles());
            
            var inventoryButton = uiDocument.rootVisualElement.Q<Button>("inventory-button");
            inventoryButton?.RegisterCallback<ClickEvent>(evt => ToggleInventory());
            
            var closeInventoryButton = uiDocument.rootVisualElement.Q<Button>("close-inventory-button");
            closeInventoryButton?.RegisterCallback<ClickEvent>(evt => ToggleInventory());
            
            craftButton?.RegisterCallback<ClickEvent>(evt => OnCraftButtonClicked());
            
            uiDocument.rootVisualElement.RegisterCallback<ClickEvent>(OnRootClicked);
        }
        
        void CreateGridTiles()
        {
            if (gridViewport == null) 
            {
                Debug.LogError("Cannot create grid tiles: gridViewport is null");
                return;
            }
            
            gridViewport.Clear();
            
            for (int y = 0; y < GRID_SIZE; y++)
            {
                for (int x = 0; x < GRID_SIZE; x++)
                {
                    var tile = new VisualElement();
                    tile.AddToClassList("grid-tile");
                    
                    // Calculate the visual Y position (flipped)
                    int visualY = GRID_SIZE - 1 - y;
                    
                    // Store coordinates that match the visual position
                    tile.userData = new Vector2Int(x, visualY);
                    
                    // Position tile using percentage values for responsive design
                    float tilePercentage = 100f / GRID_SIZE; // Each tile takes 20% of viewport (for 5x5 grid)
                    
                    // Use percentage positioning for responsive layout
                    // Flip Y coordinate so Y=0 is at bottom, Y=4 is at top (matching game grid expectations)
                    tile.style.left = new Length(x * tilePercentage, LengthUnit.Percent);
                    tile.style.top = new Length(visualY * tilePercentage, LengthUnit.Percent);
                    tile.style.width = new Length(tilePercentage, LengthUnit.Percent);
                    tile.style.height = new Length(tilePercentage, LengthUnit.Percent);
                    
                    // Add click handler
                    tile.RegisterCallback<ClickEvent>(OnTileClicked);
                    tile.RegisterCallback<MouseEnterEvent>(OnTileHover);
                    tile.RegisterCallback<MouseLeaveEvent>(OnTileExit);
                    
                    // Store tile using the visual coordinates for easy access
                    gridTiles[x, visualY] = tile;
                    gridViewport.Add(tile);
                }
            }
        }
        
        void UpdateGridVisibility()
        {
            if (gridTiles == null)
            {
                Debug.LogError("Cannot update grid visibility: gridTiles array is null");
                return;
            }
            
            Debug.Log($"🔄 UpdateGridVisibility: Checking {GRID_SIZE}x{GRID_SIZE} tiles, enableObstacles={enableObstacles}");
            
            for (int y = 0; y < GRID_SIZE; y++)
            {
                for (int x = 0; x < GRID_SIZE; x++)
                {
                    var tile = gridTiles[x, y];
                    if (tile == null)
                    {
                        Debug.LogError($"Grid tile at [{x},{y}] is null");
                        continue;
                    }
                    
                    bool isOuterRing = IsOuterRing(new Vector2Int(x, y));
                    
                    if (recipeMode)
                    {
                        // Show all tiles in recipe mode
                        tile.SetEnabled(true);
                        tile.RemoveFromClassList("outer-ring-locked");
                    }
                    else
                    {
                        if (isOuterRing)
                        {
                            // Outer ring - check if unlocked
                            bool isUnlocked = unlocked[x, y];
                            tile.SetEnabled(isUnlocked);
                            
                            if (isUnlocked)
                            {
                                tile.RemoveFromClassList("outer-ring-locked");
                                tile.AddToClassList("outer-ring-unlocked");
                            }
                            else
                            {
                                tile.AddToClassList("outer-ring-locked");
                                tile.RemoveFromClassList("outer-ring-unlocked");
                            }
                        }
                        else
                        {
                            // Center 3x3 - always enabled
                            tile.SetEnabled(true);
                            tile.RemoveFromClassList("outer-ring-locked");
                        }
                    }
                    
                    // Update ingredient visuals first (highest priority)
                    UpdateTileIngredientVisual(tile, x, y);
                    
                    // Update obstacle visuals
                    UpdateTileObstacleVisual(tile, x, y);
                }
            }
            
            OnGridChanged?.Invoke();
        }
        
        private void UpdateTileIngredientVisual(VisualElement tile, int x, int y)
        {
            // Clear all ingredient state classes first
            tile.RemoveFromClassList("empty");
            tile.RemoveFromClassList("filled");
            tile.RemoveFromClassList("aspect-scorch");
            tile.RemoveFromClassList("aspect-frigid");
            tile.RemoveFromClassList("aspect-corporeal");
            tile.RemoveFromClassList("aspect-arc");
            tile.RemoveFromClassList("aspect-divine");
            tile.RemoveFromClassList("aspect-caustic");
            
            bool isOccupied = occupied[x, y];
            IngredientInstance placedIngredient = placed[x, y];
            
            // Check if there's a placed ingredient
            if (isOccupied && placedIngredient != null)
            {
                tile.AddToClassList("filled");
                
                // Add aspect-specific styling for visual differentiation
                string aspectClass = GetAspectCSSClass(placedIngredient.ingredient.IngredientAspect);
                if (!string.IsNullOrEmpty(aspectClass))
                {
                    tile.AddToClassList(aspectClass);
                }
                
                // Update tooltip to show ingredient information
                string ingredientTooltip = $"🧪 {placedIngredient.ingredient.ItemName}\n";
                ingredientTooltip += $"Aspect: {placedIngredient.ingredient.IngredientAspect}\n";
                ingredientTooltip += $"Potency: {placedIngredient.ingredient.Potency}\n";
                ingredientTooltip += $"Placed at: {placedIngredient.placementTime:F1}s\n";
                ingredientTooltip += $"Position: ({x}, {y})";
                
                tile.tooltip = ingredientTooltip;
                
                // Debug.Log($"Updated tile visual at ({x},{y}) with {placedIngredient.ingredient.ItemName} ({placedIngredient.ingredient.IngredientAspect})");
            }
            else
            {
                // Empty cell
                tile.AddToClassList("empty");
                tile.tooltip = $"Empty grid position: ({x}, {y})";
            }
        }
        
        private string GetAspectCSSClass(Aspect aspect)
        {
            return aspect switch
            {
                Aspect.Scorch => "aspect-scorch",
                Aspect.Frigid => "aspect-frigid", 
                Aspect.Corporeal => "aspect-corporeal",
                Aspect.Arc => "aspect-arc",
                Aspect.Divine => "aspect-divine",
                Aspect.Caustic => "aspect-caustic",
                _ => ""
            };
        }
        
        private void UpdateTileObstacleVisual(VisualElement tile, int x, int y)
        {
            // Remove all obstacle classes first
            tile.RemoveFromClassList("obstacle-corporeal");
            tile.RemoveFromClassList("obstacle-frigid-frozen");
            tile.RemoveFromClassList("obstacle-frigid-melted");
            tile.RemoveFromClassList("obstacle-scorch");
            tile.RemoveFromClassList("obstacle-caustic");
            tile.RemoveFromClassList("obstacle-arc");
            tile.RemoveFromClassList("obstacle-divine");
            tile.RemoveFromClassList("obstacle-void");
            
            if (!enableObstacles)
            {
                Debug.Log($"🚫 Obstacles disabled, skipping visual update for ({x},{y})");
                return;
            }
            
            var obstacle = obstacles[x, y];
            if (obstacle != null)
            {
                // Add appropriate obstacle class for styling
                string obstacleClass = obstacle.ObstacleType switch
                {
                    ObstacleType.Corporeal => "obstacle-corporeal",
                    ObstacleType.FrigidFrozen => "obstacle-frigid-frozen",
                    ObstacleType.FrigidMelted => "obstacle-frigid-melted",
                    ObstacleType.Scorch => "obstacle-scorch",
                    ObstacleType.Caustic => "obstacle-caustic",
                    ObstacleType.Arc => "obstacle-arc",
                    ObstacleType.Divine => "obstacle-divine",
                    ObstacleType.Void => "obstacle-void",
                    _ => ""
                };
                
                if (!string.IsNullOrEmpty(obstacleClass))
                {
                    tile.AddToClassList(obstacleClass);
                    Debug.Log($"🎨 Applied obstacle class '{obstacleClass}' to tile ({x},{y}), Tile classes: {string.Join(", ", tile.GetClasses())}");
                    
                    // Add tooltip with obstacle description and placement rules
                    string tooltipText = $"{obstacle.GetObstacleDescription()}";
                    
                    // Add placement rules to tooltip
                    switch (obstacle.ObstacleType)
                    {
                        case ObstacleType.Corporeal:
                            tooltipText += "\n❌ No ingredients can be placed here";
                            break;
                        case ObstacleType.FrigidFrozen:
                            tooltipText += "\n❄️ Blocked until melted by adjacent Scorch/Corporeal";
                            break;
                        case ObstacleType.FrigidMelted:
                            tooltipText += "\n💧 Any ingredient can be placed";
                            break;
                        case ObstacleType.Scorch:
                            tooltipText += "\n🔥 Only Scorch, Caustic, or Arc ingredients";
                            break;
                        case ObstacleType.Caustic:
                            tooltipText += "\n🧪 Any ingredient (reduced potency)";
                            break;
                        case ObstacleType.Divine:
                            tooltipText += "\n✨ Only unrefined Divine ingredients";
                            break;
                        case ObstacleType.Void:
                            tooltipText += "\n🌑 Nothing can exist here";
                            break;
                    }
                    
                    tile.tooltip = tooltipText;
                }
            }
        }
        
        // Alias method for obstacle visual updates
        private void UpdateGridVisuals()
        {
            UpdateGridVisibility();
        }
        
        public bool CanPlaceIngredient(IngredientInstance instance, Vector2Int anchor, out string reason)
        {
            var shapeData = instance.ingredient.ShapeData;
            if (!CanPlace(shapeData, anchor, out reason))
            {
                return false;
            }
            
            // Additional validation for obstacles with actual ingredient
            if (enableObstacles)
            {
                var occupiedOffsets = shapeData.GetOccupiedOffsets();
                foreach (var offset in occupiedOffsets)
                {
                    Vector2Int cell = anchor + offset;
                    var obstacle = obstacles[cell.x, cell.y];
                    
                    if (obstacle != null && !obstacle.CanPlaceIngredient(instance.ingredient))
                    {
                        reason = $"🛡️ {obstacle.GetObstacleDescription()} at ({cell.x}, {cell.y}) - incompatible with {instance.ingredient.IngredientAspect} aspect";
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        public bool CanPlace(IngredientShapeData shape, Vector2Int anchor, out string reason)
        {
            reason = "";
            
            if (shape == null)
            {
                reason = "Invalid shape data";
                return false;
            }
            
            var occupiedOffsets = shape.GetOccupiedOffsets();
            
            foreach (var offset in occupiedOffsets)
            {
                Vector2Int cell = anchor + offset;
                
                if (!InBounds(cell))
                {
                    reason = "Out of bounds";
                    return false;
                }
                
                if (occupied[cell.x, cell.y])
                {
                    reason = "Cell already occupied";
                    return false;
                }
                
                // Check for obstacles
                if (enableObstacles && obstacles[cell.x, cell.y] != null)
                {
                    var obstacle = obstacles[cell.x, cell.y];
                    // Note: We can't validate ingredient aspect here since we only have shape data
                    // The actual ingredient validation will happen in PlaceIngredient
                    if (obstacle.ObstacleType == ObstacleType.Corporeal || 
                        obstacle.ObstacleType == ObstacleType.FrigidFrozen ||
                        obstacle.ObstacleType == ObstacleType.Void)
                    {
                        reason = $"Obstacle at ({cell.x}, {cell.y}): {obstacle.GetObstacleDescription()}";
                        return false;
                    }
                }
                
                if (IsOuterRing(cell) && !recipeMode)
                {
                    // Check if outer ring cell is unlocked by adjacent expansion
                    if (!IsOuterRingAllowed(cell))
                    {
                        reason = "Outer cell not unlocked by adjacent ingredient";
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Check if a single cell can be placed at (helper for preview)
        /// </summary>
        /// <param name="cell">Internal cell position</param>
        /// <returns>True if the cell can be placed at</returns>
        private bool CanPlaceAtCell(Vector2Int cell)
        {
            if (!InBounds(cell)) return false;
            
            if (occupied[cell.x, cell.y]) return false;
            
            // Check for blocking obstacles
            if (enableObstacles && obstacles[cell.x, cell.y] != null)
            {
                var obstacle = obstacles[cell.x, cell.y];
                if (obstacle.ObstacleType == ObstacleType.Corporeal || 
                    obstacle.ObstacleType == ObstacleType.FrigidFrozen ||
                    obstacle.ObstacleType == ObstacleType.Void)
                {
                    return false;
                }
            }
            
            if (IsOuterRing(cell) && !recipeMode)
            {
                return IsOuterRingAllowed(cell);
            }
            
            return true;
        }
        
        // Obstacle management methods
        public AspectObstacle GetObstacleAt(Vector2Int position)
        {
            if (!InBounds(position)) return null;
            return obstacles[position.x, position.y];
        }
        
        public bool HasObstacleAt(Vector2Int position)
        {
            return GetObstacleAt(position) != null;
        }
        
        public void SetObstacle(Vector2Int position, AspectObstacle obstacle)
        {
            if (!InBounds(position)) return;
            
            // Note: position comes from recipe which uses Y=0 at bottom (visual coords)
            // The obstacles array uses the same coordinate system as gridTiles (visual coords)
            obstacles[position.x, position.y] = obstacle;
            Debug.Log($"🔷 SetObstacle at ({position.x},{position.y}): {obstacle.ObstacleType}. GridTiles null? {gridTiles == null}, Tile at pos null? {(gridTiles != null ? (gridTiles[position.x, position.y] == null).ToString() : "N/A")}");
            UpdateGridVisuals();
        }
        
        public void RemoveObstacle(Vector2Int position)
        {
            if (!InBounds(position)) return;
            obstacles[position.x, position.y] = null;
            UpdateGridVisuals();
        }
        
        public void ClearAllObstacles()
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    obstacles[x, y] = null;
                }
            }
            UpdateGridVisuals();
        }
        
        // Helper methods to convert between visual coordinates (Y=0 at bottom) and internal array coordinates (Y=0 at index 0)
        private Vector2Int VisualToInternal(Vector2Int visualCoords)
        {
            return new Vector2Int(visualCoords.x, GRID_SIZE - 1 - visualCoords.y);
        }
        
        private Vector2Int InternalToVisual(Vector2Int internalCoords)
        {
            return new Vector2Int(internalCoords.x, GRID_SIZE - 1 - internalCoords.y);
        }
        
        public void DebugGridState()
        {
            Debug.Log("=== GRID STATE DEBUG ===");
            int obstacleCount = 0;
            for (int y = 4; y >= 0; y--) // Top to bottom for readability
            {
                string row = $"Row {y}: ";
                for (int x = 0; x < 5; x++)
                {
                    string cell = "";
                    
                    // Check if occupied and what's placed there
                    if (occupied[x, y]) 
                    {
                        var ingredient = placed[x, y];
                        if (ingredient != null)
                        {
                            // Use first letter of aspect for compactness
                            char aspectChar = ingredient.ingredient.IngredientAspect.ToString()[0];
                            cell += $"{aspectChar}";
                        }
                        else
                        {
                            cell += "?"; // Occupied but no ingredient data
                        }
                    }
                    else 
                    {
                        cell += "."; // Empty
                    }
                    
                    // Add unlock status
                    if (unlocked[x, y]) cell += "U"; 
                    else cell += "L";
                    
                    // Add obstacle info
                    if (obstacles[x, y] != null) 
                    {
                        cell += $"({obstacles[x, y].ObstacleType.ToString()[0]})";
                        obstacleCount++;
                    }
                    
                    row += $"[{cell}] ";
                }
                Debug.Log(row);
            }
            Debug.Log($"Total placed ingredients: {placedCount}");
            Debug.Log($"Total obstacles: {obstacleCount}");
            Debug.Log($"Ingredients in placed array: {GetAllPlacedIngredients().Count}");
            Debug.Log("========================");
        }
        
        public void ForceVisualRefresh()
        {
            // Debug.Log("🔄 Forcing visual refresh of all grid tiles");
            UpdateGridVisibility();
            OnGridChanged?.Invoke();
        }
        
        public void TestIngredientVisuals()
        {
            // Debug.Log("🧪 Testing ingredient visual system:");
            for (int y = 0; y < GRID_SIZE; y++)
            {
                for (int x = 0; x < GRID_SIZE; x++)
                {
                    if (occupied[x, y] && placed[x, y] != null)
                    {
                        var ingredient = placed[x, y];
                        // Debug.Log($"  - Position ({x},{y}): {ingredient.ingredient.ItemName} ({ingredient.ingredient.IngredientAspect})");
                        
                        var tile = gridTiles[x, y];
                        if (tile != null)
                        {
                            bool hasFilled = tile.ClassListContains("filled");
                            string aspectClass = GetAspectCSSClass(ingredient.ingredient.IngredientAspect);
                            bool hasAspect = !string.IsNullOrEmpty(aspectClass) && tile.ClassListContains(aspectClass);
                            
                            // Debug.Log($"    Visual state: filled={hasFilled}, aspect={hasAspect} ({aspectClass})");
                        }
                        else
                        {
                            // Debug.LogWarning($"    Tile at ({x},{y}) is null!");
                        }
                    }
                }
            }
        }
        
        public void ClearGrid()
        {
            // Clear all placed ingredients and occupancy
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    placed[x, y] = null;
                    occupied[x, y] = false;
                }
            }
            
            // Reset center cell and placedCount
            placedCount = 0;
            unlocked[2, 2] = true;
            
            // Clear pending quantity changes (restore to original)
            pendingQuantityChanges.Clear();
            
            UpdateGridVisibility();
            OnGridChanged?.Invoke();
        }
        
        public bool PlaceIngredient(IngredientInstance instance, Vector2Int visualAnchor)
        {
            Vector2Int internalAnchor = VisualToInternal(visualAnchor);
            
            if (!CanPlaceIngredient(instance, internalAnchor, out string reason))
            {
                return false;
            }
            
            var occupiedOffsets = instance.ingredient.ShapeData.GetOccupiedOffsets();
            
            if (enableObstacles)
            {
                foreach (var offset in occupiedOffsets)
                {
                    Vector2Int cell = internalAnchor + offset;
                    var obstacle = obstacles[cell.x, cell.y];
                    
                    if (obstacle != null)
                    {
                        if (!obstacle.TryPlaceIngredient(instance.ingredient, null))
                        {
                            return false;
                        }
                    }
                }
                
                CheckAndMeltFrigidObstacles();
                CheckAndEruptCorporealObstacles();
            }
            
            foreach (var offset in occupiedOffsets)
            {
                Vector2Int cell = internalAnchor + offset;
                placed[cell.x, cell.y] = instance;
                occupied[cell.x, cell.y] = true;
            }
            
            instance.gridPosition = internalAnchor;
            instance.placementTime = Time.time;
            placedCount++;
            
            if (!pendingQuantityChanges.ContainsKey(instance.ingredient))
            {
                pendingQuantityChanges[instance.ingredient] = 0;
            }
            pendingQuantityChanges[instance.ingredient]++;
            
            UpdateUnlockedCells();
            UpdateGridVisibility();
            UpdateCraftButton();
            
            OnGridChanged?.Invoke();
            
            return true;
        }
        
        private void CheckAndMeltFrigidObstacles()
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    var obstacle = obstacles[x, y];
                    if (obstacle != null && obstacle.ObstacleType == ObstacleType.FrigidFrozen)
                    {
                        if (obstacle.TryMeltFrozen(this))
                        {
                            UpdateGridVisuals();
                        }
                    }
                }
            }
        }
        
        private void CheckAndEruptCorporealObstacles()
        {
            var currentIngredients = GetAllPlacedIngredients().Select(inst => inst.ingredient).ToList();
            
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    var obstacle = obstacles[x, y];
                    if (obstacle != null && obstacle.ObstacleType == ObstacleType.Corporeal)
                    {
                        if (!obstacle.IsErupted)
                        {
                            if (obstacle.TryEruptCorporeal(this, currentIngredients))
                            {
                                UpdateGridVisuals();
                            }
                        }
                        else
                        {
                            obstacle.CheckEruptionIngredientsStillPresent(currentIngredients);
                            UpdateGridVisuals();
                        }
                    }
                }
            }
        }
        
        public bool RemoveIngredient(Vector2Int cell)
        {
            var instance = placed[cell.x, cell.y];
            if (instance == null) return false;
            
            var shapeData = instance.ingredient.ShapeData;
            
            // Get the rotated offsets based on the instance's rotation
            var occupiedOffsets = IngredientRotationUtility.GetRotatedOffsets(shapeData.occupiedOffsets, instance.rotation);
            
            // Debug.Log($"🗑️ Removing {instance.ingredient.ItemName} at {cell} with rotation {instance.rotation}°");
            
            // Restore pending quantity change (decrement because we're removing from grid)
            if (pendingQuantityChanges.ContainsKey(instance.ingredient))
            {
                pendingQuantityChanges[instance.ingredient]--;
                // Debug.Log($"📊 Decremented pending change for {instance.ingredient.ItemName}: {pendingQuantityChanges[instance.ingredient]} now on grid");
                
                if (pendingQuantityChanges[instance.ingredient] == 0)
                {
                    pendingQuantityChanges.Remove(instance.ingredient);
                    // Debug.Log($"📊 Removed {instance.ingredient.ItemName} from pending changes (count is 0)");
                }
            }
            
            // Remove ingredient from all occupied cells
            foreach (var offset in occupiedOffsets)
            {
                Vector2Int occupiedCell = instance.gridPosition + offset;
                if (InBounds(occupiedCell))
                {
                    placed[occupiedCell.x, occupiedCell.y] = null;
                    occupied[occupiedCell.x, occupiedCell.y] = false;
                }
            }
            
            placedCount--;
            
            if (enableObstacles)
            {
                CheckAndEruptCorporealObstacles();
                CheckAndMeltFrigidObstacles();
            }
            
            UpdateUnlockedCells();
            UpdateGridVisibility();
            UpdateCraftButton();
            OnGridChanged?.Invoke();
            
            return true;
        }
        
        public void ToggleRecipeMode(bool recipeMode)
        {
            this.recipeMode = recipeMode;
            UpdateGridVisibility();
            
            // Debug.Log($"Switched to {(recipeMode ? "Recipe" : "Free Crafting")} mode");
        }
        
        public bool IsRecipeMode()
        {
            return recipeMode;
        }
        
        void UpdateUnlockedCells()
        {
            // Reset unlocked state
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    unlocked[x, y] = false;
                }
            }
            
            // Calculate unlocked cells based on placed ingredients' expansion offsets with rotation
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    var instance = placed[x, y];
                    if (instance != null)
                    {
                        var shapeData = instance.ingredient.ShapeData;
                        var expansionOffsets = shapeData.GetExpansionOffsets(instance.rotation);
                        
                        foreach (var expansion in expansionOffsets)
                        {
                            Vector2Int expandedCell = instance.gridPosition + expansion;
                            if (InBounds(expandedCell))
                            {
                                unlocked[expandedCell.x, expandedCell.y] = true;
                            }
                        }
                    }
                }
            }
        }
        
        void UpdateCraftButton()
        {
            bool canCraft = placedCount == 3;
            craftButton?.SetEnabled(canCraft);
            
            if (canCraft)
            {
                craftButton?.RemoveFromClassList("craft-button-disabled");
                craftButton?.AddToClassList("craft-button-enabled");
            }
            else
            {
                craftButton?.AddToClassList("craft-button-disabled");
                craftButton?.RemoveFromClassList("craft-button-enabled");
            }
        }
        
        bool IsOuterRing(Vector2Int cell)
        {
            return cell.x == 0 || cell.x == GRID_SIZE - 1 || cell.y == 0 || cell.y == GRID_SIZE - 1;
        }
        
        bool IsOuterRingAllowed(Vector2Int target)
        {
            var neighbors = GetOrthogonalAndDiagonalNeighbors(target);
            
            foreach (var neighbor in neighbors)
            {
                var instance = placed[neighbor.x, neighbor.y];
                if (instance != null)
                {
                    Vector2Int relative = target - instance.gridPosition;
                    var shapeData = instance.ingredient.ShapeData;
                    var expansionOffsets = shapeData.GetExpansionOffsets(instance.rotation);
                    
                    foreach (var expansion in expansionOffsets)
                    {
                        if (expansion == relative)
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
        
        List<Vector2Int> GetOrthogonalAndDiagonalNeighbors(Vector2Int cell)
        {
            var neighbors = new List<Vector2Int>();
            
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    
                    Vector2Int neighbor = new Vector2Int(cell.x + dx, cell.y + dy);
                    if (InBounds(neighbor))
                    {
                        neighbors.Add(neighbor);
                    }
                }
            }
            
            return neighbors;
        }
        
        bool InBounds(Vector2Int cell)
        {
            return cell.x >= 0 && cell.x < GRID_SIZE && cell.y >= 0 && cell.y < GRID_SIZE;
        }
        
        void OnTileClicked(ClickEvent evt)
        {
            var tile = evt.target as VisualElement;
            if (tile?.userData is Vector2Int position)
            {
                // Handle tile click logic here
                // Could be placing/removing ingredients, showing context menu, etc.
            }
        }
        
        void OnTileHover(MouseEnterEvent evt)
        {
            var tile = evt.target as VisualElement;
            if (tile?.userData is Vector2Int position)
            {
                hoveredAnchor = position;
                
                // Show hover preview if ingredient is selected
                if (hoveredShape != null)
                {
                    ShowPlacementPreviewWithRotation(hoveredShape, position, hoveredRotation);
                }
            }
        }
        
        void OnTileExit(MouseLeaveEvent evt)
        {
            // Clear placement preview
            ClearPlacementPreview();
        }
        
        void ShowPlacementPreview(IngredientShapeData shape, Vector2Int visualAnchor)
        {
            ClearPlacementPreview();
            
            // Convert visual coordinates to internal coordinates for game logic
            Vector2Int internalAnchor = VisualToInternal(visualAnchor);
            
            bool canPlace = CanPlace(shape, internalAnchor, out string reason);
            var occupiedOffsets = shape.GetOccupiedOffsets();
            
            foreach (var offset in occupiedOffsets)
            {
                // Calculate internal cell position
                Vector2Int internalCell = internalAnchor + offset;
                
                // Convert back to visual coordinates for UI highlighting
                Vector2Int visualCell = InternalToVisual(internalCell);
                
                if (InBounds(internalCell))
                {
                    var tile = gridTiles[visualCell.x, visualCell.y];
                    
                    if (canPlace)
                    {
                        tile.AddToClassList("placement-preview-valid");
                    }
                    else
                    {
                        tile.AddToClassList("placement-preview-invalid");
                    }
                }
            }
            
            // Show expansion preview
            var expansionOffsets = shape.GetExpansionOffsets();
            foreach (var expansion in expansionOffsets)
            {
                // Calculate internal expanded cell position
                Vector2Int internalExpandedCell = internalAnchor + expansion;
                
                // Convert to visual coordinates for UI highlighting
                Vector2Int visualExpandedCell = InternalToVisual(internalExpandedCell);
                
                if (InBounds(internalExpandedCell) && IsOuterRing(internalExpandedCell))
                {
                    var tile = gridTiles[visualExpandedCell.x, visualExpandedCell.y];
                    tile.AddToClassList("expansion-highlight");
                }
            }
        }
        
        void ClearPlacementPreview()
        {
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    var tile = gridTiles[x, y];
                    tile.RemoveFromClassList("placement-preview-valid");
                    tile.RemoveFromClassList("placement-preview-invalid");
                    tile.RemoveFromClassList("expansion-highlight");
                }
            }
        }
        
        void ToggleInventory()
        {
            if (inventoryContainer == null)
            {
                Debug.LogError("❌ Inventory container is null!");
                return;
            }
            
            bool isVisible = inventoryContainer.style.display == DisplayStyle.Flex;
            inventoryContainer.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
            inventoryContainer.SetEnabled(!isVisible);
            
            Debug.Log($"📦 Inventory {(isVisible ? "hidden" : "shown")} - Display style set to: {inventoryContainer.style.display.value}, Enabled: {inventoryContainer.enabledInHierarchy}");
            Debug.Log($"📦 Inventory position: {inventoryContainer.style.position.value}, Visible: {inventoryContainer.visible}, Opacity: {inventoryContainer.style.opacity.value}");
        }
        
        void OnRootClicked(ClickEvent evt)
        {
            if (inventoryContainer == null || inventoryContainer.style.display != DisplayStyle.Flex)
            {
                return;
            }
            
            var clickedElement = evt.target as VisualElement;
            if (clickedElement == null)
            {
                return;
            }
            
            if (clickedElement == inventoryContainer || inventoryContainer.Contains(clickedElement))
            {
                return;
            }
            
            var inventoryButton = uiDocument.rootVisualElement.Q<Button>("inventory-button");
            if (clickedElement == inventoryButton || (inventoryButton != null && inventoryButton.Contains(clickedElement)))
            {
                return;
            }
            
            inventoryContainer.style.display = DisplayStyle.None;
        }
        
        void OnCraftButtonClicked()
        {
            if (placedCount != 3)
            {
                // Debug.LogWarning("Cannot craft: need exactly 3 ingredients");
                return;
            }
            
            // Debug.Log("Craft button clicked - starting crafting process");
            
            // TODO: Implement actual crafting logic
            // This would typically call a CraftPotion() method or show rhythm minigame
        }
        
        // Public methods for external interaction
        public void SetHoveredShape(IngredientShapeData shape)
        {
            hoveredShape = shape;
        }
        
        public void ClearHoveredShape()
        {
            hoveredShape = null;
            ClearPlacementPreview();
        }
        
        public IngredientInstance GetIngredientAt(Vector2Int cell)
        {
            if (InBounds(cell))
            {
                return placed[cell.x, cell.y];
            }
            return null;
        }
        
        public List<IngredientInstance> GetAllPlacedIngredients()
        {
            var instances = new List<IngredientInstance>();
            var seen = new HashSet<IngredientInstance>();
            
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    var instance = placed[x, y];
                    if (instance != null && !seen.Contains(instance))
                    {
                        instances.Add(instance);
                        seen.Add(instance);
                    }
                }
            }
            
            return instances;
        }
        
        public IngredientInstance GetPlacedAt(int x, int y)
        {
            if (x < 0 || x >= GRID_SIZE || y < 0 || y >= GRID_SIZE)
                return null;
            
            return placed[x, y];
        }
        
        public bool HasIngredientsPlaced()
        {
            return placedCount > 0;
        }
        
        public bool TryCompleteCrafting()
        {
            if (placedCount == 0)
            {
                Debug.Log("No ingredients placed to craft with.");
                return false;
            }
            
            if (isProcessingRecipe)
            {
                Debug.Log("🧪 Recipe processing already in progress");
                return false;
            }
            
            try
            {
                isProcessingRecipe = true;
                
                var placedInstances = GetAllPlacedIngredients();
                if (placedInstances == null || placedInstances.Count < 2)
                {
                    Debug.Log("🧪 Not enough ingredients placed for recipes (need at least 2)");
                    isProcessingRecipe = false;
                    return false;
                }

                var placedIngredientsSnapshot = new List<IngredientInstance>(placedInstances);
                var uniqueIngredients = new HashSet<Ingredient>();
                
                foreach (var instance in placedIngredientsSnapshot)
                {
                    if (instance?.ingredient != null)
                    {
                        uniqueIngredients.Add(instance.ingredient);
                    }
                }

                Debug.Log($"🧪 Attempting to craft with {uniqueIngredients.Count} unique ingredients");

                var database = AlchemyRecipeDatabase.Instance;
                if (database == null)
                {
                    Debug.LogError("🧪 AlchemyRecipeDatabase not found");
                    isProcessingRecipe = false;
                    return false;
                }

                AlchemyRecipe matchedRecipe = null;
                foreach (var recipe in database.Recipes)
                {
                    if (DoesRecipeMatch(recipe, uniqueIngredients))
                    {
                        matchedRecipe = recipe;
                        break;
                    }
                }

                if (matchedRecipe != null)
                {
                    Debug.Log($"🧪 ✅ RECIPE MATCH FOUND: {matchedRecipe.ItemName}");
                    ExecuteRecipeAtomic(matchedRecipe, uniqueIngredients, placedIngredientsSnapshot);
                    return true;
                }
                else
                {
                    Debug.Log("🧪 ❌ No recipe matches found");
                    
                    var dynamicPotion = CheckForInfusionBasedCrafting(uniqueIngredients, placedIngredientsSnapshot);
                    if (dynamicPotion != null)
                    {
                        Debug.Log($"🧪 ✨ DYNAMIC POTION CREATED: {dynamicPotion.ItemName}");
                        ExecuteDynamicPotionCrafting(dynamicPotion, uniqueIngredients, placedIngredientsSnapshot);
                        return true;
                    }
                    else
                    {
                        Debug.Log("🧪 💥 No valid combinations found - destroying ingredients");
                        ExecuteIngredientDestruction(uniqueIngredients, placedIngredientsSnapshot);
                        return false;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"🧪 Error during crafting: {ex.Message}");
                isProcessingRecipe = false;
                return false;
            }
            finally
            {
                isProcessingRecipe = false;
            }
        }
        
        /// <summary>
        /// Update hover preview with rotation support
        /// </summary>
        /// <param name="ingredient">The ingredient to preview</param>
        /// <param name="rotation">Rotation in degrees (0, 90, 180, 270)</param>
        public void UpdateHoverPreview(Ingredient ingredient, int rotation)
        {
            if (ingredient?.ShapeData == null) return;
            
            hoveredShape = ingredient.ShapeData;
            hoveredRotation = IngredientRotationUtility.NormalizeRotation(rotation);
            
            // If we have a current hover position, update the preview
            if (hoveredAnchor != Vector2Int.zero)
            {
                ShowPlacementPreviewWithRotation(hoveredShape, hoveredAnchor, hoveredRotation);
            }
        }
        
        /// <summary>
        /// Show placement preview with rotation support
        /// </summary>
        /// <param name="shape">The ingredient shape</param>
        /// <param name="anchor">The anchor position</param>
        /// <param name="rotation">Rotation in degrees</param>
        void ShowPlacementPreviewWithRotation(IngredientShapeData shape, Vector2Int anchor, int rotation)
        {
            if (shape == null) return;
            
            ClearPlacementPreview();
            
            // Get rotated offsets
            var rotatedOffsets = IngredientRotationUtility.GetRotatedOffsets(shape.occupiedOffsets, rotation);
            
            Vector2Int internalAnchor = VisualToInternal(anchor);
            
            // Show occupied cells preview with rotation
            foreach (var offset in rotatedOffsets)
            {
                Vector2Int internalCell = internalAnchor + offset;
                Vector2Int visualCell = InternalToVisual(internalCell);
                
                if (InBounds(internalCell))
                {
                    var tile = gridTiles[visualCell.x, visualCell.y];
                    
                    if (CanPlaceAtCell(internalCell))
                    {
                        tile.AddToClassList("placement-preview-valid");
                    }
                    else
                    {
                        tile.AddToClassList("placement-preview-invalid");
                    }
                }
            }
            
            // Show expansion preview with rotation if applicable
            var expansionOffsets = shape.expansionRotatesWithIngredient 
                ? IngredientRotationUtility.GetRotatedOffsets(shape.expansionOffsets, rotation)
                : shape.expansionOffsets;
                
            foreach (var expansion in expansionOffsets)
            {
                Vector2Int internalExpandedCell = internalAnchor + expansion;
                Vector2Int visualExpandedCell = InternalToVisual(internalExpandedCell);
                
                if (InBounds(internalExpandedCell) && IsOuterRing(internalExpandedCell))
                {
                    var tile = gridTiles[visualExpandedCell.x, visualExpandedCell.y];
                    tile.AddToClassList("expansion-highlight");
                }
            }
        }
        
        /// <summary>
        /// Check if ingredient can be placed at position with rotation
        /// </summary>
        /// <param name="ingredient">The ingredient to place</param>
        /// <param name="anchor">The anchor position</param>
        /// <param name="rotation">Rotation in degrees</param>
        /// <returns>True if placement is valid</returns>
        public bool CanPlaceIngredientWithRotation(Ingredient ingredient, Vector2Int anchor, int rotation)
        {
            if (ingredient?.ShapeData == null) return false;
            
            var rotatedOffsets = IngredientRotationUtility.GetRotatedOffsets(ingredient.ShapeData.occupiedOffsets, rotation);
            
            foreach (var offset in rotatedOffsets)
            {
                Vector2Int cell = anchor + offset;
                if (!CanPlaceAtCell(cell))
                {
                    return false;
                }
                
                // Additional check: verify ingredient is compatible with obstacle
                if (enableObstacles && obstacles[cell.x, cell.y] != null)
                {
                    var obstacle = obstacles[cell.x, cell.y];
                    if (!obstacle.CanPlaceIngredient(ingredient))
                    {
                        Debug.Log($"❌ Cannot place {ingredient.ItemName} on {obstacle.ObstacleType} obstacle at {cell}");
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Place ingredient with rotation support
        /// </summary>
        /// <param name="ingredient">The ingredient to place</param>
        /// <param name="anchor">The anchor position</param>
        /// <param name="rotation">Rotation in degrees</param>
        /// <returns>True if placement was successful</returns>
        public bool PlaceIngredientWithRotation(Ingredient ingredient, Vector2Int anchor, int rotation)
        {
            Debug.Log($"🎯 PlaceIngredientWithRotation: ingredient={ingredient.name}, anchor={anchor}, rotation={rotation}°");
            
            if (!CanPlaceIngredientWithRotation(ingredient, anchor, rotation))
            {
                Debug.LogWarning($"❌ Cannot place {ingredient.name} at {anchor} with rotation {rotation}°");
                return false;
            }
            
            var rotatedOffsets = IngredientRotationUtility.GetRotatedOffsets(ingredient.ShapeData.occupiedOffsets, rotation);
            
            Debug.Log($"✅ Can place {ingredient.name} at {anchor}, rotated offsets: {string.Join(", ", rotatedOffsets)}");
            
            // Create ingredient instance with rotation
            var instance = new IngredientInstance
            {
                ingredient = ingredient,
                gridPosition = anchor,
                placementTime = Time.time,
                rotation = IngredientRotationUtility.NormalizeRotation(rotation)
            };
            
            // Place in all occupied cells
            foreach (var offset in rotatedOffsets)
            {
                Vector2Int cell = anchor + offset;
                // Debug.Log($"  - Placing at cell {cell} (anchor {anchor} + offset {offset})");
                placed[cell.x, cell.y] = instance;
                occupied[cell.x, cell.y] = true;
            }
            
            placedCount++;
            
            // Track pending quantity change (positive = placed on grid)
            if (!pendingQuantityChanges.ContainsKey(ingredient))
            {
                pendingQuantityChanges[ingredient] = 0;
            }
            pendingQuantityChanges[ingredient]++;
            // Debug.Log($"📊 Tracked pending change for {ingredient.ItemName}: {pendingQuantityChanges[ingredient]} placed on grid");
            
            UpdateUnlockedCells();
            UpdateGridVisibility();
            UpdateCraftButton();
            OnGridChanged?.Invoke();
            // Debug.Log($"📡 OnGridChanged invoked after placing {ingredient.ItemName}");
            
            // Handle obstacle interactions after placement
            HandleObstacleInteractionsForPlacement(ingredient, anchor, rotatedOffsets);
            
            return true;
        }
        
        #region Obstacle Interaction Logic
        
        /// <summary>
        /// Handle obstacle interactions when placing ingredients
        /// </summary>
        private void HandleObstacleInteractionsForPlacement(Ingredient ingredient, Vector2Int anchor, Vector2Int[] rotatedOffsets)
        {
            // Check each cell the ingredient occupies for obstacles
            foreach (var offset in rotatedOffsets)
            {
                Vector2Int cellPos = anchor + offset;
                var obstacle = GetObstacleAt(cellPos);
                
                if (obstacle != null)
                {
                    HandleObstacleInteraction(ingredient, cellPos, obstacle);
                }
            }
            
            // After placement, check adjacent cells for frigid obstacles that might melt
            foreach (var offset in rotatedOffsets)
            {
                Vector2Int cellPos = anchor + offset;
                CheckAdjacentFrigidObstacles(cellPos);
            }
        }
        
        /// <summary>
        /// Handle interaction with a specific obstacle
        /// </summary>
        private bool HandleObstacleInteraction(Ingredient ingredient, Vector2Int position, AspectObstacle obstacle)
        {
            if (obstacle == null) return true;

            bool canPlace = obstacle.CanPlaceIngredient(ingredient);
            if (canPlace)
            {
                // Note: We're using 'this' but GridCraftingManager isn't a GridGameManager
                // The AspectObstacle expects GridGameManager, so we'll skip the TryPlaceIngredient call
                // and just handle the effects directly
                
                Debug.Log($"✅ Placed {ingredient.ItemName} on {obstacle.ObstacleType} obstacle at {position}");
                
                // Process special obstacle effects
                ProcessObstacleEffects(obstacle, ingredient);
                
                return true;
            }

            Debug.Log($"❌ Cannot place {ingredient.ItemName} on {obstacle.ObstacleType} obstacle at {position}");
            return false;
        }

        /// <summary>
        /// Process special effects from obstacle interactions
        /// </summary>
        private void ProcessObstacleEffects(AspectObstacle obstacle, Ingredient ingredient)
        {
            switch (obstacle.ObstacleType)
            {
                case ObstacleType.Scorch:
                    bool isCompatible = (ingredient.IngredientAspect == Aspect.Scorch ||
                                       ingredient.IngredientAspect == Aspect.Caustic ||
                                       ingredient.IngredientAspect == Aspect.Arc);
                    if (isCompatible)
                    {
                        Debug.Log($"🔥 Scorch obstacle: {ingredient.ItemName} potency enhanced by 50%");
                    }
                    else
                    {
                        Debug.Log($"🔥 Scorch obstacle: {ingredient.ItemName} burned, potency reduced");
                    }
                    break;
                    
                case ObstacleType.Caustic:
                    if (ingredient.IngredientArchetype == IngredientArchetype.Herb)
                    {
                        Debug.Log($"🧪 Caustic obstacle: Herb {ingredient.ItemName} reduces negative effects by 60%");
                    }
                    else if (ingredient.IngredientAspect == Aspect.Caustic)
                    {
                        Debug.Log($"🧪 Caustic obstacle: Caustic {ingredient.ItemName} potency increased by 20%");
                    }
                    else
                    {
                        Debug.Log($"🧪 Caustic obstacle: {ingredient.ItemName} potency reduced by 20%");
                    }
                    break;
                    
                case ObstacleType.Arc:
                    Debug.Log($"⚡ Arc obstacle: {ingredient.ItemName} creates static field");
                    break;
                    
                case ObstacleType.Divine:
                    if (ingredient.IngredientAspect == Aspect.Divine && ingredient.IsUnrefined)
                    {
                        Debug.Log($"✨ Divine obstacle: Unrefined Divine {ingredient.ItemName} potency increased by 10%");
                    }
                    break;
                    
                case ObstacleType.FrigidMelted:
                    if (ingredient.IngredientAspect == Aspect.Scorch)
                    {
                        Debug.Log($"❄️ FrigidMelted obstacle: Scorch {ingredient.ItemName} evaporates, potency increased by 30%");
                    }
                    else if (ingredient.IngredientAspect == Aspect.Frigid)
                    {
                        Debug.Log($"❄️ FrigidMelted obstacle: Frigid {ingredient.ItemName} refreezes the cell");
                        // Revert to frozen
                        SetObstacle(obstacle.Position, new AspectObstacle(ObstacleType.FrigidFrozen, obstacle.Position));
                    }
                    else if (ingredient.IngredientAspect == Aspect.Corporeal)
                    {
                        Debug.Log($"❄️ FrigidMelted obstacle: Corporeal {ingredient.ItemName} stabilizes the melted cell, potency increased by 10%");
                    }
                    else
                    {
                        Debug.Log($"❄️ FrigidMelted obstacle: {ingredient.ItemName} gets chilled, potency reduced by 10%");
                    }
                    break;
            }
        }

        /// <summary>
        /// Check adjacent cells for frigid obstacles that might be melted
        /// </summary>
        private void CheckAdjacentFrigidObstacles(Vector2Int position)
        {
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            
            var placedIngredient = placed[position.x, position.y];
            if (placedIngredient == null) return;

            foreach (var direction in directions)
            {
                Vector2Int adjacentPos = position + direction;
                if (!InBounds(adjacentPos)) continue;
                
                var obstacle = GetObstacleAt(adjacentPos);
                
                if (obstacle != null && obstacle.ObstacleType == ObstacleType.FrigidFrozen)
                {
                    // Check if the placed ingredient can melt it
                    if (placedIngredient.ingredient.IngredientAspect == Aspect.Scorch ||
                        placedIngredient.ingredient.IngredientAspect == Aspect.Corporeal)
                    {
                        // Melt the frozen obstacle
                        Debug.Log($"❄️ Frigid obstacle melted by adjacent {placedIngredient.ingredient.IngredientAspect} aspect - now FrigidMelted");
                        SetObstacle(adjacentPos, new AspectObstacle(ObstacleType.FrigidMelted, adjacentPos));
                    }
                }
            }
        }
        
        #endregion
        
        #region Recipe System
        
        /// <summary>
        /// Clear all ingredients and obstacles, then reload recipe obstacles from current recipe
        /// </summary>
        public void ClearGridPreserveObstacles()
        {
            Debug.Log("ClearGridPreserveObstacles called - clearing ingredients and reapplying recipe obstacles");
            
            // Clear all placed ingredients
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    placed[x, y] = null;
                    occupied[x, y] = false;
                }
            }
            
            placedCount = 0;
            
            // Clear pending quantity changes (restore to original)
            pendingQuantityChanges.Clear();
            
            // Clear all obstacles and reload from the current recipe pattern
            LoadRecipePattern();
            
            UpdateUnlockedCells();
            UpdateGridVisibility();
            UpdateCraftButton();
            
            OnGridChanged?.Invoke();
            
            Debug.Log("Grid cleared and recipe obstacles reapplied from current recipe pattern");
        }
        
        /// <summary>
        /// Clears all obstacles and reloads them from the current recipe's custom grid data
        /// This ensures the grid matches the recipe's initial obstacle layout
        /// </summary>
        private void LoadRecipePattern()
        {
            if (currentRecipe == null || !currentRecipe.HasCustomGridData())
            {
                Debug.Log("No current recipe or custom grid data available - clearing all obstacles");
                ClearAllObstacles();
                return;
            }
            
            Debug.Log($"Clearing all obstacles and reloading pattern from {currentRecipe.name}");
            
            // First, clear all existing obstacles
            ClearAllObstacles();
            
            // Then, apply obstacles from the recipe's custom grid data
            foreach (var customCell in currentRecipe.CustomGridCells)
            {
                if (customCell.hasObstacle && 
                    customCell.position.x >= 0 && customCell.position.x < GRID_SIZE &&
                    customCell.position.y >= 0 && customCell.position.y < GRID_SIZE)
                {
                    // Recipe positions need Y-flip to match visual coordinates
                    int flippedY = GRID_SIZE - 1 - customCell.position.y;
                    Vector2Int obstaclePosition = new Vector2Int(customCell.position.x, flippedY);
                    
                    var obstacle = new AspectObstacle(customCell.obstacleType, obstaclePosition);
                    SetObstacle(obstaclePosition, obstacle);
                    
                    Debug.Log($"Applied obstacle at {obstaclePosition} (recipe pos {customCell.position}) with type {customCell.obstacleType}");
                }
            }
            
            Debug.Log($"Recipe pattern loaded: obstacles cleared and reapplied from recipe");
        }
        
        /// <summary>
        /// Check for recipe matches and execute if found
        /// </summary>
        public void CheckForRecipeMatches()
        {
            if (isProcessingRecipe)
            {
                Debug.Log("🧪 ⚠️ Recipe processing already in progress - ignoring additional calls");
                return;
            }
            
            Debug.Log("🧪 === CHECKING FOR RECIPE MATCHES ===");
            
            try
            {
                isProcessingRecipe = true;
                
                var placedInstances = GetAllPlacedIngredients();
                if (placedInstances == null || placedInstances.Count < 2)
                {
                    Debug.Log("🧪 Not enough ingredients placed for recipes (need at least 2)");
                    return;
                }

                var placedIngredientsSnapshot = new List<IngredientInstance>(placedInstances);
                Debug.Log($"🧪 📸 Created snapshot of {placedIngredientsSnapshot.Count} placed ingredients");

                var uniqueIngredients = new HashSet<Ingredient>();
                foreach (var instance in placedIngredientsSnapshot)
                {
                    if (instance?.ingredient != null)
                    {
                        uniqueIngredients.Add(instance.ingredient);
                    }
                }

                Debug.Log($"🧪 Found {uniqueIngredients.Count} unique ingredients on grid");
                foreach (var ingredient in uniqueIngredients)
                {
                    Debug.Log($"   - {ingredient.ItemName}");
                }

                var database = AlchemyRecipeDatabase.Instance;
                if (database == null)
                {
                    Debug.LogError("🧪 AlchemyRecipeDatabase not found");
                    return;
                }

                Debug.Log($"🧪 Checking against {database.Recipes.Count} recipes in database");

                AlchemyRecipe matchedRecipe = null;
                foreach (var recipe in database.Recipes)
                {
                    if (DoesRecipeMatch(recipe, uniqueIngredients))
                    {
                        matchedRecipe = recipe;
                        break;
                    }
                }

                if (matchedRecipe != null)
                {
                    Debug.Log($"🧪 ✅ RECIPE MATCH FOUND: {matchedRecipe.ItemName}");
                    ExecuteRecipeAtomic(matchedRecipe, uniqueIngredients, placedIngredientsSnapshot);
                }
                else
                {
                    Debug.Log("🧪 ❌ No recipe matches found for current ingredients");
                    
                    // Check for dynamic infusion-based crafting
                    var dynamicPotion = CheckForInfusionBasedCrafting(uniqueIngredients, placedIngredientsSnapshot);
                    if (dynamicPotion != null)
                    {
                        Debug.Log($"🧪 ✨ DYNAMIC POTION CREATED: {dynamicPotion.ItemName}");
                        ExecuteDynamicPotionCrafting(dynamicPotion, uniqueIngredients, placedIngredientsSnapshot);
                    }
                    else
                    {
                        Debug.Log("🧪 💥 No valid combinations found - destroying ingredients");
                        ExecuteIngredientDestruction(uniqueIngredients, placedIngredientsSnapshot);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"🧪 💥 Error during recipe processing: {ex.Message}");
                Debug.LogError($"🧪 Stack trace: {ex.StackTrace}");
            }
            finally
            {
                isProcessingRecipe = false;
                Debug.Log("🧪 Recipe processing state reset");
            }
        }
        
        private bool DoesRecipeMatch(AlchemyRecipe recipe, HashSet<Ingredient> placedIngredients)
        {
            var requiredIngredients = new List<Ingredient>();
            
            if (recipe.InputIngredient1 != null) requiredIngredients.Add(recipe.InputIngredient1);
            if (recipe.InputIngredient2 != null) requiredIngredients.Add(recipe.InputIngredient2);
            if (recipe.InputIngredient3 != null) requiredIngredients.Add(recipe.InputIngredient3);

            Debug.Log($"🧪 Checking recipe '{recipe.ItemName}' requiring {requiredIngredients.Count} ingredients");

            foreach (var required in requiredIngredients)
            {
                bool found = placedIngredients.Any(placed => placed == required);
                if (!found)
                {
                    Debug.Log($"🧪 Missing required ingredient: {required.ItemName}");
                    return false;
                }
            }

            if (placedIngredients.Count != requiredIngredients.Count)
            {
                Debug.Log($"🧪 Wrong number of ingredients: have {placedIngredients.Count}, need {requiredIngredients.Count}");
                return false;
            }

            return true;
        }
        
        private void ExecuteRecipeAtomic(AlchemyRecipe recipe, HashSet<Ingredient> usedIngredientsSnapshot, List<IngredientInstance> placedIngredientsSnapshot)
        {
            Debug.Log($"🧪 ⚗️ EXECUTING RECIPE ATOMICALLY: {recipe.ItemName}");
            Debug.Log($"🧪 Using ingredients: {string.Join(", ", usedIngredientsSnapshot.Select(i => i.ItemName))}");
            Debug.Log($"🧪 Output: {recipe.OutputPotion?.ItemName ?? "Unknown Potion"} x{recipe.OutputQuantity}");
            
            try
            {
                Debug.Log($"🧪 💰 Committing quantity changes to inventory");
                CommitQuantityChanges();
                
                Debug.Log($"🧪 🧹 Clearing grid immediately to prevent additional ingredient placement");
                ClearGridPreserveObstacles();
                
                if (recipe.OutputPotion != null)
                {
                    var inventory = FindFirstObjectByType<FourFatesStudios.ProjectWarden.ItemSlotContainerHolder>();
                    if (inventory != null)
                    {
                        inventory.AddItem(recipe.OutputPotion, recipe.OutputQuantity);
                        Debug.Log($"🎒 ✅ Added {recipe.OutputQuantity}x {recipe.OutputPotion.ItemName} to inventory");
                    }
                    else
                    {
                        Debug.LogWarning("🎒 ⚠️ ItemSlotContainerHolder not found - potion not added to inventory");
                    }
                }
                
                Debug.Log($"🧪 ✅ SUCCESS Atomically created {recipe.OutputPotion?.ItemName ?? "Unknown Potion"}");
                Debug.Log($"🧪 🎉 Recipe execution complete! Enjoy your new {recipe.OutputPotion?.ItemName ?? "potion"}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"🧪 💥 Error during atomic recipe execution: {ex.Message}");
                Debug.LogError($"🧪 Stack trace: {ex.StackTrace}");
                
                ClearGridPreserveObstacles();
            }
        }
        
        private void ExecuteIngredientDestruction(HashSet<Ingredient> usedIngredients, List<IngredientInstance> placedIngredients)
        {
            Debug.Log($"🧪 💥 === EXECUTING INGREDIENT DESTRUCTION ===");
            Debug.Log($"🧪 Destroying ingredients: {string.Join(", ", usedIngredients.Select(i => i.ItemName))}");
            
            try
            {
                Debug.Log($"🧪 💰 Committing quantity changes to inventory");
                CommitQuantityChanges();
                
                Debug.Log($"🧪 🗑️ Clearing grid - ingredients destroyed");
                ClearGridPreserveObstacles();
                
                Debug.Log($"🧪 💀 The ingredients react violently and are destroyed! No useful potion was created");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"🧪 💥 Error during ingredient destruction: {ex.Message}");
            }
        }
        
        private Potion CheckForInfusionBasedCrafting(HashSet<Ingredient> uniqueIngredients, List<IngredientInstance> placedIngredients)
        {
            Debug.Log($"🧪 ✨ === CHECKING FOR INFUSION-BASED CRAFTING ===");
            
            if (uniqueIngredients.Count < 2)
            {
                Debug.Log("🧪 Need at least 2 ingredients for infusion-based crafting");
                return null;
            }

            var sharedInfusions = FindSharedInfusions(uniqueIngredients);
            
            if (sharedInfusions.Count == 0)
            {
                Debug.Log("🧪 No shared infusions found between ingredients");
                return null;
            }

            Debug.Log($"🧪 ✨ Found {sharedInfusions.Count} shared infusion types");
            foreach (var infusionType in sharedInfusions.Keys)
            {
                var infusions = sharedInfusions[infusionType];
                Debug.Log($"   - {infusionType.Name}: {infusions.Count} instances from {infusions.Select(i => i.sourceIngredient.ItemName).Distinct().Count()} ingredients");
            }

            return CreateDynamicPotion(sharedInfusions, uniqueIngredients);
        }
        
        private Dictionary<System.Type, List<(object infusion, Ingredient sourceIngredient)>> FindSharedInfusions(HashSet<Ingredient> ingredients)
        {
            var infusionMap = new Dictionary<System.Type, List<(object infusion, Ingredient sourceIngredient)>>();
            
            foreach (var ingredient in ingredients)
            {
                if (ingredient.InfusionBundle?.Infusions != null)
                {
                    foreach (var infusion in ingredient.InfusionBundle.Infusions)
                    {
                        if (infusion != null)
                        {
                            var infusionType = infusion.GetType();
                            
                            if (!infusionMap.ContainsKey(infusionType))
                            {
                                infusionMap[infusionType] = new List<(object, Ingredient)>();
                            }
                            
                            infusionMap[infusionType].Add((infusion, ingredient));
                        }
                    }
                }
            }
            
            var sharedInfusions = new Dictionary<System.Type, List<(object infusion, Ingredient sourceIngredient)>>();
            
            foreach (var kvp in infusionMap)
            {
                var infusionType = kvp.Key;
                var infusionList = kvp.Value;
                
                var uniqueIngredients = infusionList.Select(tuple => tuple.sourceIngredient).Distinct().Count();
                
                if (uniqueIngredients >= 2)
                {
                    sharedInfusions[infusionType] = infusionList;
                    Debug.Log($"🧪 🔗 Shared infusion detected: {infusionType.Name} (found in {uniqueIngredients} ingredients)");
                }
            }
            
            return sharedInfusions;
        }
        
        private Potion CreateDynamicPotion(Dictionary<System.Type, List<(object infusion, Ingredient sourceIngredient)>> sharedInfusions, HashSet<Ingredient> sourceIngredients)
        {
            Debug.Log($"🧪 🎨 Creating dynamic potion from shared infusions");
            
            var potionName = GenerateDynamicPotionName(sharedInfusions, sourceIngredients);
            var dynamicPotion = CreateRuntimePotion(potionName, sharedInfusions, sourceIngredients);
            
            return dynamicPotion;
        }
        
        private string GenerateDynamicPotionName(Dictionary<System.Type, List<(object infusion, Ingredient sourceIngredient)>> sharedInfusions, HashSet<Ingredient> sourceIngredients)
        {
            var infusionNames = new List<string>();
            
            foreach (var sharedType in sharedInfusions.Keys)
            {
                var firstInfusion = sharedInfusions[sharedType].First().infusion;
                if (firstInfusion != null)
                {
                    try
                    {
                        var nameProperty = firstInfusion.GetType().GetProperty("InfusionName");
                        if (nameProperty != null)
                        {
                            var infusionName = nameProperty.GetValue(firstInfusion) as string;
                            if (!string.IsNullOrEmpty(infusionName))
                            {
                                infusionNames.Add(infusionName);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"🧪 ⚠️ Could not get infusion name: {ex.Message}");
                        infusionNames.Add(sharedType.Name.Replace("Infusion", ""));
                    }
                }
            }
            
            if (infusionNames.Count == 0)
            {
                infusionNames = sharedInfusions.Keys.Select(type => type.Name.Replace("Infusion", "")).ToList();
            }
            
            string infusionPart = string.Join(" & ", infusionNames);
            
            return $"Potion of {infusionPart}";
        }
        
        private Potion CreateRuntimePotion(string potionName, Dictionary<System.Type, List<(object infusion, Ingredient sourceIngredient)>> sharedInfusions, HashSet<Ingredient> sourceIngredients)
        {
            Debug.Log($"🧪 ⚡ Creating runtime potion: {potionName}");
            
            var dynamicPotion = ScriptableObject.CreateInstance<Potion>();
            
            var generatedName = GenerateDynamicPotionName(sharedInfusions, sourceIngredients);
            var sortedIngredientNames = sourceIngredients.Select(i => i.ItemName).OrderBy(name => name).ToList();
            var generatedDescription = $"A dynamic potion crafted from: {string.Join(", ", sortedIngredientNames)}";
            
            SetPotionProperty(dynamicPotion, "itemName", generatedName);
            SetPotionProperty(dynamicPotion, "itemDescription", generatedDescription);
            
            dynamicPotion.name = generatedName;
            
            Debug.Log($"🧪 🔍 Potion.ItemName: {dynamicPotion.ItemName}");
            Debug.Log($"🧪 🔍 Potion.ItemDescription: {dynamicPotion.ItemDescription}");
            
            var averagePotency = sourceIngredients.Average(i => i.Potency);
            SetPotionProperty(dynamicPotion, "potency", Mathf.RoundToInt((float)averagePotency));
            
            SetPotionProperty(dynamicPotion, "isCustomPotion", true);
            SetPotionProperty(dynamicPotion, "rarity", PotionRarity.Uncommon);
            
            var potionInfusionBundle = new InfusionBundle();
            
            foreach (var sharedInfusionType in sharedInfusions.Keys)
            {
                var infusionList = sharedInfusions[sharedInfusionType];
                var representativeInfusion = infusionList.First().infusion;
                
                if (representativeInfusion != null)
                {
                    try
                    {
                        var addInfusionMethod = typeof(InfusionBundle).GetMethod("AddInfusion");
                        if (addInfusionMethod != null)
                        {
                            addInfusionMethod.Invoke(potionInfusionBundle, new object[] { representativeInfusion });
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"🧪 ⚠️ Could not add infusion {representativeInfusion.GetType().Name}: {ex.Message}");
                    }
                }
            }
            
            SetPotionProperty(dynamicPotion, "infusionBundle", potionInfusionBundle);
            SetPotionProperty(dynamicPotion, "sourceIngredients", sourceIngredients.ToList());
            
            SetDynamicPotionVisuals(dynamicPotion, sharedInfusions);
            
            Debug.Log($"🧪 ✨ Runtime potion created with {potionInfusionBundle.Infusions.Count} infusions");
            
            return dynamicPotion;
        }
        
        private void SetPotionProperty(Potion potion, string propertyName, object value)
        {
            try
            {
                var field = typeof(Potion).GetField(propertyName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field == null)
                {
                    field = typeof(Item).GetField(propertyName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                }
                
                if (field != null)
                {
                    field.SetValue(potion, value);
                    Debug.Log($"🧪 ✅ Set {propertyName} = {value}");
                }
                else
                {
                    Debug.LogWarning($"🧪 ⚠️ Could not find field '{propertyName}' in Potion or Item class");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"🧪 ⚠️ Error setting potion property '{propertyName}': {ex.Message}");
            }
        }
        
        private void SetDynamicPotionVisuals(Potion potion, Dictionary<System.Type, List<(object infusion, Ingredient sourceIngredient)>> sharedInfusions)
        {
            Color primaryColor = Color.blue;
            Color secondaryColor = Color.white;
            bool hasGlow = false;
            bool hasBubbles = false;
            bool hasParticles = false;
            
            foreach (var infusionType in sharedInfusions.Keys)
            {
                string infusionName = infusionType.Name.ToLower();
                
                if (infusionName.Contains("fire"))
                {
                    primaryColor = Color.red;
                    secondaryColor = new Color(1f, 0.4f, 0f, 1f);
                    hasGlow = true;
                    hasBubbles = true;
                }
                else if (infusionName.Contains("ice") || infusionName.Contains("frost"))
                {
                    primaryColor = Color.cyan;
                    secondaryColor = Color.white;
                    hasParticles = true;
                }
                else if (infusionName.Contains("poison") || infusionName.Contains("toxic"))
                {
                    primaryColor = Color.green;
                    secondaryColor = Color.yellow;
                    hasBubbles = true;
                }
                else if (infusionName.Contains("lightning") || infusionName.Contains("shock"))
                {
                    primaryColor = Color.magenta;
                    secondaryColor = Color.white;
                    hasGlow = true;
                    hasParticles = true;
                }
                else if (infusionName.Contains("divine") || infusionName.Contains("holy"))
                {
                    primaryColor = Color.white;
                    secondaryColor = Color.yellow;
                    hasGlow = true;
                    hasParticles = true;
                }
                else
                {
                    primaryColor = new Color(1f, 0.4f, 0f, 0.3f);
                    secondaryColor = Color.gray;
                    hasGlow = true;
                }
            }
            
            SetPotionProperty(potion, "primaryColor", primaryColor);
            SetPotionProperty(potion, "secondaryColor", secondaryColor);
            SetPotionProperty(potion, "hasGlow", hasGlow);
            SetPotionProperty(potion, "hasBubbles", hasBubbles);
            SetPotionProperty(potion, "hasParticles", hasParticles);
            
            Debug.Log($"🧪 🎨 Set dynamic potion visuals: Primary={primaryColor}, Glow={hasGlow}, Bubbles={hasBubbles}, Particles={hasParticles}");
        }
        
        private void ExecuteDynamicPotionCrafting(Potion dynamicPotion, HashSet<Ingredient> usedIngredients, List<IngredientInstance> placedIngredients)
        {
            Debug.Log($"🧪 ✨ === EXECUTING DYNAMIC POTION CRAFTING ===");
            Debug.Log($"🧪 Creating: {dynamicPotion.ItemName}");
            Debug.Log($"🧪 From ingredients: {string.Join(", ", usedIngredients.Select(i => i.ItemName))}");
            
            try
            {
                Debug.Log($"🧪 💰 Committing quantity changes to inventory");
                CommitQuantityChanges();
                
                Debug.Log($"🧪 🧹 Clearing grid");
                ClearGridPreserveObstacles();
                
                var inventory = FindFirstObjectByType<FourFatesStudios.ProjectWarden.ItemSlotContainerHolder>();
                if (inventory != null)
                {
                    var existingSlot = inventory.Container.Slots.FirstOrDefault(s => 
                        s.Item != null && 
                        s.Item is Potion potion && 
                        potion.ItemName == dynamicPotion.ItemName);
                    
                    if (existingSlot != null)
                    {
                        Debug.Log($"🧪 📦 Found existing potion '{dynamicPotion.ItemName}' in inventory, increasing quantity");
                        inventory.AddItem(existingSlot.Item, 1);
                        Debug.Log($"🎒 ✅ Increased quantity of existing potion: {dynamicPotion.ItemName}");
                    }
                    else
                    {
                        Debug.Log($"🧪 📦 No existing potion found, adding new instance to inventory");
                        Debug.Log($"🧪 📦 - Potion.ItemName: '{dynamicPotion.ItemName}'");
                        Debug.Log($"🧪 📦 - Potion.ItemDescription: '{dynamicPotion.ItemDescription}'");
                        
                        inventory.AddItem(dynamicPotion, 1);
                        Debug.Log($"🎒 ✅ Added new dynamic potion to inventory: {dynamicPotion.ItemName}");
                    }
                }
                else
                {
                    Debug.LogWarning("🎒 ⚠️ ItemSlotContainerHolder not found - dynamic potion not added to inventory");
                }
                
                Debug.Log($"🧪 🎉 DISCOVERY! You created a new potion: {dynamicPotion.ItemName}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"🧪 💥 Error during dynamic potion crafting: {ex.Message}");
            }
        }
        
        [ContextMenu("🔧 Force Reset Processing State")]
        public void ForceResetProcessingState()
        {
            bool wasProcessing = isProcessingRecipe;
            isProcessingRecipe = false;
            
            Debug.Log($"🔧 Processing state manually reset. Was processing: {wasProcessing}");
        }
        
        #endregion
    }
}