using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
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
        
        // Events
        public event Action OnGridChanged;
        
        // Internal data - using existing IngredientInstance structure
        private IngredientInstance[,] placed = new IngredientInstance[GRID_SIZE, GRID_SIZE];
        private bool[,] occupied = new bool[GRID_SIZE, GRID_SIZE];
        private bool[,] unlocked = new bool[GRID_SIZE, GRID_SIZE];
        
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
        
        public int PlacedCount => placedCount;
        public VisualElement viewRoot => uiDocument?.rootVisualElement;
        
        void Start()
        {
            // Auto-assign UIDocument if not set
            if (uiDocument == null)
            {
                uiDocument = FindObjectOfType<UIDocument>();
                if (uiDocument != null)
                {
                    Debug.Log("Auto-assigned UIDocument to GridCraftingManager");
                }
            }
            
            Initialize(recipeMode);
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
            
            Debug.Log($"GridCraftingManager initialized in {(recipeMode ? "Recipe" : "Free Crafting")} mode");
        }
        
        void SetupUI()
        {
            var root = uiDocument.rootVisualElement;
            
            // Find UI elements
            gridViewport = root.Q<VisualElement>("grid-viewport");
            craftButton = root.Q<Button>("craft-button");
            rightContainer = root.Q<VisualElement>("right-container");
            inventoryContainer = root.Q<VisualElement>("inventory-container");
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
            clearButton?.RegisterCallback<ClickEvent>(evt => ClearGridKeepObstacles());
            
            var inventoryButton = uiDocument.rootVisualElement.Q<Button>("inventory-button");
            inventoryButton?.RegisterCallback<ClickEvent>(evt => ToggleInventory());
            
            craftButton?.RegisterCallback<ClickEvent>(evt => OnCraftButtonClicked());
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
                    
                    // Add coordinate data
                    tile.userData = new Vector2Int(x, y);
                    
                    // Position tile using percentage values for responsive design
                    float tilePercentage = 100f / GRID_SIZE; // Each tile takes 20% of viewport (for 5x5 grid)
                    
                    // Use percentage positioning for responsive layout
                    tile.style.left = new Length(x * tilePercentage, LengthUnit.Percent);
                    tile.style.top = new Length(y * tilePercentage, LengthUnit.Percent);
                    tile.style.width = new Length(tilePercentage, LengthUnit.Percent);
                    tile.style.height = new Length(tilePercentage, LengthUnit.Percent);
                    
                    // Add click handler
                    tile.RegisterCallback<ClickEvent>(OnTileClicked);
                    tile.RegisterCallback<MouseEnterEvent>(OnTileHover);
                    tile.RegisterCallback<MouseLeaveEvent>(OnTileExit);
                    
                    gridTiles[x, y] = tile;
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
                }
            }
            
            OnGridChanged?.Invoke();
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
        
        public bool PlaceIngredient(IngredientInstance instance, Vector2Int anchor)
        {
            var shapeData = instance.ingredient.ShapeData;
            if (!CanPlace(shapeData, anchor, out string reason))
            {
                Debug.LogWarning($"Cannot place ingredient: {reason}");
                return false;
            }
            
            var occupiedOffsets = shapeData.GetOccupiedOffsets();
            
            // Place ingredient in all occupied cells
            foreach (var offset in occupiedOffsets)
            {
                Vector2Int cell = anchor + offset;
                placed[cell.x, cell.y] = instance;
                occupied[cell.x, cell.y] = true;
            }
            
            instance.gridPosition = anchor;
            instance.placementTime = Time.time;
            placedCount++;
            
            // Update unlocked cells based on expansion offsets
            UpdateUnlockedCells();
            UpdateGridVisibility();
            UpdateCraftButton();
            
            Debug.Log($"Placed ingredient {instance.ingredient.ItemName} at {anchor}");
            
            return true;
        }
        
        public bool RemoveIngredient(Vector2Int cell)
        {
            var instance = placed[cell.x, cell.y];
            if (instance == null) return false;
            
            var shapeData = instance.ingredient.ShapeData;
            var occupiedOffsets = shapeData.GetOccupiedOffsets();
            
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
            
            // Update unlocked cells
            UpdateUnlockedCells();
            UpdateGridVisibility();
            UpdateCraftButton();
            
            Debug.Log($"Removed ingredient {instance.ingredient.ItemName}");
            
            return true;
        }
        
        public void ClearGridKeepObstacles()
        {
            // Clear all placed ingredients but keep any obstacle data
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    placed[x, y] = null;
                    occupied[x, y] = false;
                }
            }
            
            placedCount = 0;
            
            UpdateUnlockedCells();
            UpdateGridVisibility();
            UpdateCraftButton();
            
            Debug.Log("Grid cleared, obstacles preserved");
        }
        
        public void ToggleRecipeMode(bool recipeMode)
        {
            this.recipeMode = recipeMode;
            UpdateGridVisibility();
            
            Debug.Log($"Switched to {(recipeMode ? "Recipe" : "Free Crafting")} mode");
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
            
            // Calculate unlocked cells based on placed ingredients' expansion offsets
            for (int x = 0; x < GRID_SIZE; x++)
            {
                for (int y = 0; y < GRID_SIZE; y++)
                {
                    var instance = placed[x, y];
                    if (instance != null)
                    {
                        var shapeData = instance.ingredient.ShapeData;
                        var expansionOffsets = shapeData.GetExpansionOffsets();
                        
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
                    var expansionOffsets = shapeData.GetExpansionOffsets();
                    
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
                Debug.Log($"Clicked tile at {position}");
                
                // Handle tile click logic here
                // Could be placing/removing ingredients, showing context menu, etc.
            }
        }
        
        void OnTileHover(MouseEnterEvent evt)
        {
            var tile = evt.target as VisualElement;
            if (tile?.userData is Vector2Int position)
            {
                // Show hover preview if ingredient is selected
                if (hoveredShape != null)
                {
                    ShowPlacementPreview(hoveredShape, position);
                }
            }
        }
        
        void OnTileExit(MouseLeaveEvent evt)
        {
            // Clear placement preview
            ClearPlacementPreview();
        }
        
        void ShowPlacementPreview(IngredientShapeData shape, Vector2Int anchor)
        {
            ClearPlacementPreview();
            
            bool canPlace = CanPlace(shape, anchor, out string reason);
            var occupiedOffsets = shape.GetOccupiedOffsets();
            
            foreach (var offset in occupiedOffsets)
            {
                Vector2Int cell = anchor + offset;
                if (InBounds(cell))
                {
                    var tile = gridTiles[cell.x, cell.y];
                    
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
                Vector2Int expandedCell = anchor + expansion;
                if (InBounds(expandedCell) && IsOuterRing(expandedCell))
                {
                    var tile = gridTiles[expandedCell.x, expandedCell.y];
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
            bool isVisible = inventoryContainer.style.display == DisplayStyle.Flex;
            inventoryContainer.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
            
            Debug.Log($"Inventory {(isVisible ? "hidden" : "shown")}");
        }
        
        void OnCraftButtonClicked()
        {
            if (placedCount != 3)
            {
                Debug.LogWarning("Cannot craft: need exactly 3 ingredients");
                return;
            }
            
            Debug.Log("Craft button clicked - starting crafting process");
            
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
    }
}