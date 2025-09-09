// Enhanced GridMinigameController with drag and drop features - Compilation complete
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.Inventory;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu
{
    [Serializable]
    public class IngredientInteraction
    {
        public Ingredient ingredient1;
        public Ingredient ingredient2;
        public float effectMultiplier = 1.5f;
        public bool unlocksBonusEffect;
        public string interactionDescription;
    }

    [Serializable]
    public class GridCell
    {
        public bool isEmpty = true;
        public List<Ingredient> ingredients = new List<Ingredient>();
        public VisualElement visualElement;
        public bool isExpanded;
        public bool isKeyLocked;
        public Ingredient requiredIngredient;
        public bool isVisible;
        public bool isAvailable;
        public bool wasUnlockedByIngredient;
        public Vector2Int position;

        public void SetPosition(int x, int y)
        {
            position = new Vector2Int(x, y);
        }

        public void Clear()
        {
            ingredients.Clear();
            isEmpty = true;
            isExpanded = false;
            wasUnlockedByIngredient = false;
            if (visualElement != null)
            {
                visualElement.Clear();
            }
        }
    }

    public class GridMinigameController : MonoBehaviour
    {
        // Grid constants for the 5x5 expandable system
        private const int TotalGridWidth = 5;
        private const int TotalGridHeight = 5;
        private const int InitialVisibleWidth = 3;
        private const int InitialVisibleHeight = 3;
        private const int CenterOffsetX = 1; // (5-3)/2 = 1
        private const int CenterOffsetY = 1; // (5-3)/2 = 1

        [Header("UI References")] [SerializeField]
        private UIDocument uiDocument;

        public UIDocument UIDocument => uiDocument;

        [Header("Inventory Integration")]
        [SerializeField, Tooltip("Reference to the player's ingredient inventory")]
        private ItemSlotContainerHolder inventoryHolder;
        
        [SerializeField, Tooltip("Auto-populate ingredients from inventory on start")]
        private bool useInventoryIngredients = true;
        
        [SerializeField, Tooltip("Consume ingredients from inventory when crafting")]
        private bool consumeIngredientsFromInventory = true;
        
        [SerializeField, Tooltip("Add crafted results to inventory")]
        private bool addResultsToInventory = true;

        [Header("Grid Settings")] [SerializeField]
        private int baseGridWidth = 3;

        [SerializeField] private int baseGridHeight = 3;
        [SerializeField] private int maxGridWidth = 5;
        [SerializeField] private int maxGridHeight = 5;
        [SerializeField] private int currentGridWidth = 3;
        [SerializeField] private int currentGridHeight = 3;
        [SerializeField] private float cellSize = 40f;

        // Grid expansion tracking
        private bool[,] visibleGridCells;
        private bool[,] availableGridCells;

        [Header("Tetris Features")] [SerializeField]
        private bool allowOverlapping;

        [SerializeField] private int maxOverlapTiles;
        [SerializeField] private bool showIngredientInteractions = true;

        [Header("Recipe Settings")] [SerializeField]
        private List<AlchemyRecipe> availableRecipes;

        [SerializeField] private List<IngredientInteraction> knownInteractions = new List<IngredientInteraction>();

        [Header("Failure Handling")] [SerializeField]
        private bool allowSyntheticCreation = true;

        [SerializeField] private Ingredient syntheticIngredientTemplate;

        // UI Elements
        private VisualElement mainContainer;
        private Button backButton;
        private Button alchemyBookButton;
        private ScrollView ingredientPalette;
        private VisualElement gridContainer;
        private ScrollView recipeSelection;
        private Button clearButton;
        private Button craftButton;
        private Label gridStatusLabel;
        private Label patternMatchLabel;
        private Label recipeInfoLabel;

        // Game State
        private GridCell[,] craftingGrid;
        private VisualElement[,] gridCells; // UI elements for the grid
        private AlchemyRecipe selectedRecipe;
        private List<Ingredient> availableIngredients;
        private Dictionary<Vector2Int, PlacedIngredient> placedIngredients;
        private Ingredient selectedIngredient;
        private List<Vector2Int> expandedGridCells;
        private Dictionary<Ingredient, List<Ingredient>> ingredientInteractions;
        private bool isKeyRecipe;
        private bool isGridInitialized;

        // Drag and drop state
        private VisualElement draggedElement;
        private Ingredient draggedIngredient;
        private bool isDragging;
        private Vector2 dragOffset;

        // Enhanced drag features
        private VisualElement dragGhost;
        private VisualElement highlightedDropZone;
        private AudioSource audioSource;

        [Header("Audio Feedback")] public AudioClip dragStartSound;
        public AudioClip dragDropSound;
        public AudioClip dragCancelSound;
        public AudioClip snapSound;

        // Events
        public event Action OnBackPressed;
        public event Action OnAlchemyBookPressed;
        public event Action<AlchemyRecipe, Dictionary<Vector2Int, PlacedIngredient>, bool> OnCraftingCompleted;
        public event Action<Ingredient> OnSyntheticIngredientCreated;

        private void Awake()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            placedIngredients = new Dictionary<Vector2Int, PlacedIngredient>();
            expandedGridCells = new List<Vector2Int>();
            ingredientInteractions = new Dictionary<Ingredient, List<Ingredient>>();

            // Setup audio source for drag feedback
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.volume = 0.7f;
            }

            UpdateGridSizeFromSkills();
            InitializeGrid();
            LoadIngredientInteractions();
        }

        private void OnEnable()
        {
            Debug.Log($"🎮 GridMinigameController OnEnable called");
            
            if (uiDocument?.rootVisualElement != null)
            {
                SetupUI();
                
                // Initialize inventory integration if enabled
                if (useInventoryIngredients)
                {
                    LoadIngredientsFromInventory();
                }
                else if (availableIngredients == null || availableIngredients.Count == 0)
                {
                    // Fallback to demo ingredients if no inventory integration and no ingredients set
                    // SetAvailableIngredients(demoIngredients);
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ UIDocument or rootVisualElement is null in OnEnable");
            }
        }

        private void OnDisable()
        {
            CleanupUI();
        }

        private void Update()
        {
            // Handle keyboard shortcuts
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnBackPressed?.Invoke();
            }
            else if (Input.GetKeyDown(KeyCode.C) && Input.GetKey(KeyCode.LeftControl))
            {
                ClearGrid();
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (craftButton?.enabledSelf == true)
                {
                    AttemptCrafting();
                }
            }
        }

        private void InitializeGrid()
        {
            // Initialize the full 5x5 grid
            craftingGrid = new GridCell[TotalGridWidth, TotalGridHeight];
            gridCells = new VisualElement[TotalGridWidth, TotalGridHeight];
            visibleGridCells = new bool[TotalGridWidth, TotalGridHeight];
            availableGridCells = new bool[TotalGridWidth, TotalGridHeight];

            // Initialize all grid cells
            for (int x = 0; x < TotalGridWidth; x++)
            {
                for (int y = 0; y < TotalGridHeight; y++)
                {
                    craftingGrid[x, y] = new GridCell();
                    craftingGrid[x, y].SetPosition(x, y);
                }
            }

            // Set initial visible area (center 3x3)
            InitializeVisibleArea();

            isGridInitialized = true;

            Debug.Log(
                $"🔧 Grid initialized: {TotalGridWidth}x{TotalGridHeight} total, {InitialVisibleWidth}x{InitialVisibleHeight} initially visible");
        }

        private void InitializeVisibleArea()
        {
            // Clear all visibility first
            for (int x = 0; x < TotalGridWidth; x++)
            {
                for (int y = 0; y < TotalGridHeight; y++)
                {
                    visibleGridCells[x, y] = false;
                    availableGridCells[x, y] = false;
                    craftingGrid[x, y].isVisible = false;
                    craftingGrid[x, y].isAvailable = false;
                }
            }

            // Set center 3x3 as visible and available
            for (int x = CenterOffsetX; x < CenterOffsetX + InitialVisibleWidth; x++)
            {
                for (int y = CenterOffsetY; y < CenterOffsetY + InitialVisibleHeight; y++)
                {
                    visibleGridCells[x, y] = true;
                    availableGridCells[x, y] = true;
                    craftingGrid[x, y].isVisible = true;
                    craftingGrid[x, y].isAvailable = true;
                }
            }

            Debug.Log(
                $"🎯 Initial visible area set: center {InitialVisibleWidth}x{InitialVisibleHeight} at offset ({CenterOffsetX}, {CenterOffsetY})");
        }

        /// <summary>
        /// Expands the grid when an ingredient with UnlocksAdditionalSpace is placed
        /// </summary>
        private void ExpandGridForIngredient(Ingredient ingredient, Vector2Int placementPosition)
        {
            if (ingredient == null || !ingredient.UnlocksAdditionalSpace || ingredient.AdditionalSpaceCount <= 0)
                return;

            Debug.Log(
                $"🔓 Expanding grid for ingredient: {ingredient.ItemName} (unlocks {ingredient.AdditionalSpaceCount} spaces)");

            List<Vector2Int> newlyUnlockedCells = new List<Vector2Int>();
            int spacesToUnlock = ingredient.AdditionalSpaceCount;

            // Find available adjacent cells to unlock, starting from the placement position
            List<Vector2Int> candidateCells = GetAdjacentExpandableCells(placementPosition);

            // Add more distant cells if needed
            if (candidateCells.Count < spacesToUnlock)
            {
                candidateCells.AddRange(GetExpandableCellsInRadius(placementPosition, 2));
            }

            // Unlock the required number of cells
            for (int i = 0; i < Mathf.Min(spacesToUnlock, candidateCells.Count); i++)
            {
                Vector2Int cellPos = candidateCells[i];
                if (IsValidGridPosition(cellPos) && !visibleGridCells[cellPos.x, cellPos.y])
                {
                    UnlockGridCell(cellPos, ingredient);
                    newlyUnlockedCells.Add(cellPos);
                }
            }

            // Update the visual grid
            UpdateGridVisuals();

            Debug.Log($"✅ Unlocked {newlyUnlockedCells.Count} new grid cells");
        }

        /// <summary>
        /// Get cells adjacent to the given position that can be expanded
        /// </summary>
        private List<Vector2Int> GetAdjacentExpandableCells(Vector2Int center)
        {
            List<Vector2Int> cells = new List<Vector2Int>();
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (Vector2Int dir in directions)
            {
                Vector2Int checkPos = center + dir;
                if (IsValidGridPosition(checkPos) && !visibleGridCells[checkPos.x, checkPos.y])
                {
                    cells.Add(checkPos);
                }
            }

            return cells;
        }

        /// <summary>
        /// Get expandable cells within a radius of the center position
        /// </summary>
        private List<Vector2Int> GetExpandableCellsInRadius(Vector2Int center, int radius)
        {
            List<Vector2Int> cells = new List<Vector2Int>();

            for (int x = center.x - radius; x <= center.x + radius; x++)
            {
                for (int y = center.y - radius; y <= center.y + radius; y++)
                {
                    Vector2Int checkPos = new Vector2Int(x, y);
                    if (IsValidGridPosition(checkPos) && !visibleGridCells[checkPos.x, checkPos.y])
                    {
                        // Calculate distance to prioritize closer cells
                        float distance = Vector2Int.Distance(center, checkPos);
                        if (distance <= radius && distance > 1) // Exclude already-checked adjacent cells
                        {
                            cells.Add(checkPos);
                        }
                    }
                }
            }

            // Sort by distance to prioritize closer cells
            cells.Sort((a, b) => Vector2Int.Distance(center, a).CompareTo(Vector2Int.Distance(center, b)));

            return cells;
        }

        /// <summary>
        /// Unlock a specific grid cell and mark it as unlocked by an ingredient
        /// </summary>
        private void UnlockGridCell(Vector2Int position, Ingredient unlockedBy)
        {
            if (!IsValidGridPosition(position))
                return;

            visibleGridCells[position.x, position.y] = true;
            availableGridCells[position.x, position.y] = true;

            var cell = craftingGrid[position.x, position.y];
            cell.isVisible = true;
            cell.isAvailable = true;
            cell.wasUnlockedByIngredient = true;

            Debug.Log($"🔓 Unlocked grid cell ({position.x}, {position.y}) by {unlockedBy.ItemName}");
        }

        private void UpdateGridSizeFromSkills()
        {
            var skillSystem = AlchemySkillSystem.Instance;
            if (skillSystem != null)
            {
                // In the new system, skills can affect overlap and other features
                // but not the basic grid size (which is now dynamically expanded by ingredients)
                allowOverlapping = skillSystem.HasSkill("overlap_placement");
                if (allowOverlapping)
                {
                    maxOverlapTiles = 3; // Allow up to 3 overlapping tiles
                }

                // Skills could potentially affect initial visibility or other features
                if (skillSystem.HasSkill("enhanced_grid"))
                {
                    // Could unlock additional starting cells or other bonuses
                    Debug.Log("🎯 Enhanced grid skill detected - could add extra starting cells");
                }
            }
        }

        private void LoadIngredientInteractions()
        {
            ingredientInteractions.Clear();

            foreach (var interaction in knownInteractions)
            {
                if (interaction.ingredient1 != null && interaction.ingredient2 != null)
                {
                    if (!ingredientInteractions.ContainsKey(interaction.ingredient1))
                        ingredientInteractions[interaction.ingredient1] = new List<Ingredient>();
                    if (!ingredientInteractions.ContainsKey(interaction.ingredient2))
                        ingredientInteractions[interaction.ingredient2] = new List<Ingredient>();

                    ingredientInteractions[interaction.ingredient1].Add(interaction.ingredient2);
                    ingredientInteractions[interaction.ingredient2].Add(interaction.ingredient1);
                }
            }
        }

        public void SetAvailableIngredients(List<Ingredient> ingredients)
        {
            availableIngredients = new List<Ingredient>(ingredients);
            RefreshIngredientPaletteDisplay();
        }

        public void SetAvailableRecipes(List<AlchemyRecipe> recipes)
        {
            availableRecipes = new List<AlchemyRecipe>(recipes);
            RefreshRecipeSelection();
        }

        #region Inventory Integration

        /// <summary>
        /// Initialize the inventory holder reference automatically if not set
        /// </summary>
        private void InitializeInventoryReference()
        {
            if (inventoryHolder == null)
            {
                inventoryHolder = FindFirstObjectByType<ItemSlotContainerHolder>();
                if (inventoryHolder == null)
                {
                    Debug.LogWarning("GridMinigameController: No ItemSlotContainerHolder found in scene. Inventory integration disabled.");
                    useInventoryIngredients = false;
                    consumeIngredientsFromInventory = false;
                    addResultsToInventory = false;
                }
            }
        }

        /// <summary>
        /// Load available ingredients from the inventory system
        /// </summary>
        public void LoadIngredientsFromInventory()
        {
            InitializeInventoryReference();
            
            if (inventoryHolder?.Container == null)
            {
                Debug.LogWarning("GridMinigameController: No inventory container available. Using demo ingredients.");
                return;
            }

            var inventoryIngredients = new List<Ingredient>();
            
            foreach (var slot in inventoryHolder.Container.Slots)
            {
                if (slot.Item is Ingredient ingredient && slot.Quantity > 0)
                {
                    // Add one copy of each ingredient type that the player has in inventory
                    inventoryIngredients.Add(ingredient);
                    Debug.Log($"Loaded ingredient from inventory: {ingredient.ItemName} (x{slot.Quantity})");
                }
            }

            if (inventoryIngredients.Count > 0)
            {
                SetAvailableIngredients(inventoryIngredients);
                Debug.Log($"Loaded {inventoryIngredients.Count} ingredient types from inventory.");
            }
            else
            {
                Debug.LogWarning("No ingredients found in inventory. Consider adding demo ingredients.");
            }
        }

        /// <summary>
        /// Check if the player has sufficient ingredients in inventory for a recipe
        /// </summary>
        public bool HasSufficientIngredientsInInventory(Dictionary<Vector2Int, PlacedIngredient> requiredIngredients)
        {
            if (!consumeIngredientsFromInventory || inventoryHolder?.Container == null)
                return true; // If not consuming from inventory, always allow crafting

            var ingredientCounts = new Dictionary<Ingredient, int>();
            
            // Count required ingredients
            foreach (var placedIngredient in requiredIngredients.Values)
            {
                if (ingredientCounts.ContainsKey(placedIngredient.ingredient))
                    ingredientCounts[placedIngredient.ingredient]++;
                else
                    ingredientCounts[placedIngredient.ingredient] = 1;
            }

            // Check if inventory has sufficient quantities
            foreach (var requirement in ingredientCounts)
            {
                var ingredient = requirement.Key;
                var requiredQuantity = requirement.Value;
                
                var inventorySlot = inventoryHolder.Container.Slots.FirstOrDefault(slot => slot.Item == ingredient);
                var availableQuantity = inventorySlot?.Quantity ?? 0;
                
                if (availableQuantity < requiredQuantity)
                {
                    Debug.LogWarning($"Insufficient {ingredient.ItemName} in inventory. Required: {requiredQuantity}, Available: {availableQuantity}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Consume ingredients from inventory when crafting
        /// </summary>
        public void ConsumeIngredientsFromInventory(Dictionary<Vector2Int, PlacedIngredient> usedIngredients)
        {
            if (!consumeIngredientsFromInventory || inventoryHolder?.Container == null)
                return;

            var ingredientCounts = new Dictionary<Ingredient, int>();
            
            // Count used ingredients
            foreach (var placedIngredient in usedIngredients.Values)
            {
                if (ingredientCounts.ContainsKey(placedIngredient.ingredient))
                    ingredientCounts[placedIngredient.ingredient]++;
                else
                    ingredientCounts[placedIngredient.ingredient] = 1;
            }

            // Remove ingredients from inventory
            foreach (var usage in ingredientCounts)
            {
                var ingredient = usage.Key;
                var usedQuantity = usage.Value;
                
                inventoryHolder.RemoveItem(ingredient, usedQuantity);
                Debug.Log($"Consumed {usedQuantity}x {ingredient.ItemName} from inventory");
            }

            // Refresh ingredient palette to reflect inventory changes
            if (useInventoryIngredients)
            {
                LoadIngredientsFromInventory();
            }
        }

        /// <summary>
        /// Add crafted results to inventory
        /// </summary>
        public void AddResultToInventory(Item resultItem, int quantity = 1)
        {
            if (!addResultsToInventory || inventoryHolder?.Container == null)
                return;

            inventoryHolder.AddItem(resultItem, quantity);
            Debug.Log($"Added {quantity}x {resultItem.ItemName} to inventory");
        }

        /// <summary>
        /// Public method to manually refresh ingredients from inventory
        /// </summary>
        public void RefreshFromInventory()
        {
            if (useInventoryIngredients)
            {
                LoadIngredientsFromInventory();
            }
        }

        #endregion

        private void SetupUI()
        {
            var root = uiDocument.rootVisualElement;
            Debug.Log($"🔧 SetupUI called, root element: {root?.name}");

            // Get UI elements
            mainContainer = root.Q<VisualElement>("MainContainer");
            backButton = root.Q<Button>("BackButton");
            alchemyBookButton = root.Q<Button>("AlchemyBookButton");
            ingredientPalette = root.Q<ScrollView>("IngredientPalette");
            gridContainer = root.Q<VisualElement>("GridContainer");
            recipeSelection = root.Q<ScrollView>("RecipeSelection");
            clearButton = root.Q<Button>("ClearButton");
            craftButton = root.Q<Button>("CraftButton");
            gridStatusLabel = root.Q<Label>("GridStatusLabel");
            patternMatchLabel = root.Q<Label>("PatternMatchLabel");
            recipeInfoLabel = root.Q<Label>("RecipeInfoLabel");
            
            Debug.Log($"🔧 UI Elements found - GridContainer: {gridContainer != null}, MainContainer: {mainContainer != null}");

            // Setup event handlers
            if (backButton != null)
                backButton.clicked += () => OnBackPressed?.Invoke();

            if (alchemyBookButton != null)
                alchemyBookButton.clicked += () => OnAlchemyBookPressed?.Invoke();
            else
                Debug.LogWarning("Alchemy book button not found in UI!");

            if (clearButton != null)
                clearButton.clicked += ClearGrid;

            if (craftButton != null)
                craftButton.clicked += AttemptCrafting;

            // Initialize UI
            CreateGridUI();
            RefreshIngredientPaletteDisplay();
            RefreshRecipeSelection();
            UpdateUI();
        }

        private void CleanupUI()
        {
            if (backButton != null)
                backButton.clicked -= () => OnBackPressed?.Invoke();

            if (alchemyBookButton != null)
                alchemyBookButton.clicked -= () => OnAlchemyBookPressed?.Invoke();

            if (clearButton != null)
                clearButton.clicked -= ClearGrid;

            if (craftButton != null)
                craftButton.clicked -= AttemptCrafting;
        }

        private void CreateGridUI()
        {
            Debug.Log($"🎨 CreateGridUI called - gridContainer null: {gridContainer == null}");
            
            if (gridContainer == null)
            {
                Debug.LogWarning("⚠️ GridContainer is null, cannot create grid UI");
                return;
            }

            if (!isGridInitialized)
            {
                Debug.LogWarning("⚠️ Grid not initialized, calling InitializeGrid()");
                InitializeGrid();
            }

            gridContainer.Clear();

            var gridElement = new VisualElement();
            gridElement.style.flexDirection = FlexDirection.Column;
            gridElement.style.alignItems = Align.Center;
            gridElement.style.justifyContent = Justify.Center;
            gridElement.style.width = 200;
            gridElement.style.height = 200;
            gridElement.style.backgroundColor = new StyleColor(new Color(0.2f, 0.3f, 0.4f, 0.8f));
            gridElement.style.borderTopWidth = 2;
            gridElement.style.borderBottomWidth = 2;
            gridElement.style.borderLeftWidth = 2;
            gridElement.style.borderRightWidth = 2;
            gridElement.style.borderTopColor = Color.white;
            gridElement.style.borderBottomColor = Color.white;
            gridElement.style.borderLeftColor = Color.white;
            gridElement.style.borderRightColor = Color.white;

            // Create the full 5x5 grid but only show visible cells
            for (int y = 0; y < TotalGridHeight; y++)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.Center;

                for (int x = 0; x < TotalGridWidth; x++)
                {
                    var cell = CreateGridCell(x, y);
                    gridCells[x, y] = cell;
                    craftingGrid[x, y].visualElement = cell;

                    // Only add visible cells to the UI
                    if (visibleGridCells[x, y])
                    {
                        row.Add(cell);
                    }
                    else
                    {
                        // Make invisible cells hidden but keep them in the data structure
                        cell.style.display = DisplayStyle.None;
                        row.Add(cell);
                    }
                }

                gridElement.Add(row);
            }

            gridContainer.Add(gridElement);

            Debug.Log($"🎨 Grid UI created: {TotalGridWidth}x{TotalGridHeight} grid with visible cells marked. GridContainer children: {gridContainer.childCount}");
        }

        /// <summary>
        /// Update grid visuals when cells are unlocked/locked
        /// </summary>
        private void UpdateGridVisuals()
        {
            if (gridCells == null) return;

            for (int x = 0; x < TotalGridWidth; x++)
            {
                for (int y = 0; y < TotalGridHeight; y++)
                {
                    var cell = gridCells[x, y];
                    if (cell != null)
                    {
                        // Update visibility
                        cell.style.display = visibleGridCells[x, y] ? DisplayStyle.Flex : DisplayStyle.None;

                        // Update visual classes
                        cell.RemoveFromClassList("locked");
                        cell.RemoveFromClassList("unlocked");
                        cell.RemoveFromClassList("expanded");

                        if (visibleGridCells[x, y])
                        {
                            if (craftingGrid[x, y].wasUnlockedByIngredient)
                            {
                                cell.AddToClassList("unlocked");
                            }

                            if (availableGridCells[x, y])
                            {
                                cell.AddToClassList("available");
                            }
                            else
                            {
                                cell.AddToClassList("locked");
                            }
                        }
                    }
                }
            }

            Debug.Log("🔄 Grid visuals updated");
        }

        private VisualElement CreateGridCell(int x, int y)
        {
            var cell = new VisualElement();
            cell.AddToClassList("grid-cell");
            cell.AddToClassList("empty");

            cell.style.width = cellSize;
            cell.style.height = cellSize;
            cell.style.borderTopWidth = 1;
            cell.style.borderBottomWidth = 1;
            cell.style.borderLeftWidth = 1;
            cell.style.borderRightWidth = 1;
            cell.style.borderTopColor = Color.gray;
            cell.style.borderBottomColor = Color.gray;
            cell.style.borderLeftColor = Color.gray;
            cell.style.borderRightColor = Color.gray;

            var position = new Vector2Int(x, y);

            // Set initial visual state based on visibility
            if (IsValidGridPosition(position))
            {
                if (visibleGridCells[x, y])
                {
                    cell.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

                    if (availableGridCells[x, y])
                    {
                        cell.AddToClassList("available");
                    }
                    else
                    {
                        cell.AddToClassList("locked");
                        cell.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
                    }
                }
                else
                {
                    // Hidden cell
                    cell.style.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.5f);
                    cell.style.display = DisplayStyle.None;
                }
            }

            // Check if this cell should be expanded from skill bonuses
            if (IsExpandedCell(position))
            {
                cell.AddToClassList("expanded");
                if (IsValidGridPosition(position))
                {
                    craftingGrid[x, y].isExpanded = true;
                }
            }

            // Check if this is a key-locked cell for story recipes
            if (IsKeyLockedCell(position))
            {
                cell.AddToClassList("key-locked");
                if (IsValidGridPosition(position))
                {
                    craftingGrid[x, y].isKeyLocked = true;
                }
            }

            // Add click handler
            cell.RegisterCallback<ClickEvent>(evt =>
            {
                // Only handle clicks on visible and available cells
                if (!IsValidGridPosition(position) || !visibleGridCells[x, y] || !availableGridCells[x, y])
                {
                    Debug.Log($"❌ Click ignored on cell ({x}, {y}) - not available");
                    return;
                }

                if (evt.ctrlKey)
                {
                    // Remove ingredient
                    RemoveIngredient(position);
                }
                else if (selectedIngredient != null)
                {
                    // Place ingredient
                    if (CanPlaceIngredient(position, selectedIngredient))
                    {
                        PlaceIngredient(position, selectedIngredient);

                        // Check if this ingredient unlocks additional space
                        if (selectedIngredient.UnlocksAdditionalSpace)
                        {
                            ExpandGridForIngredient(selectedIngredient, position);
                        }
                    }
                    else if (allowOverlapping && CanPlaceIngredientWithOverlap(position, selectedIngredient))
                    {
                        PlaceIngredientWithOverlap(position, selectedIngredient);

                        // Check for expansion even with overlap
                        if (selectedIngredient.UnlocksAdditionalSpace)
                        {
                            ExpandGridForIngredient(selectedIngredient, position);
                        }
                    }
                    else
                    {
                        ShowPlacementFeedback(position, false);
                    }
                }
            });

            // Add hover effects for placement preview
            cell.RegisterCallback<MouseEnterEvent>(evt =>
            {
                if (selectedIngredient != null)
                {
                    ShowPlacementPreview(position, selectedIngredient);
                }
            });

            cell.RegisterCallback<MouseLeaveEvent>(evt => { HidePlacementPreview(); });

            craftingGrid[x, y].visualElement = cell;
            return cell;
        }

        private void RefreshIngredientPaletteDisplay()
        {
            if (ingredientPalette == null) return;

            ingredientPalette.Clear();

            // Get all ingredients from inventory if using inventory integration
            var inventoryIngredients = new List<(Ingredient ingredient, int quantity)>();
            
            if (useInventoryIngredients && inventoryHolder?.Container != null)
            {
                // Get all ingredients from inventory with their quantities
                foreach (var slot in inventoryHolder.Container.Slots)
                {
                    if (slot.Item is Ingredient ingredient && slot.Quantity > 0)
                    {
                        inventoryIngredients.Add((ingredient, slot.Quantity));
                    }
                }
                
                // Sort by ingredient name for consistent display
                inventoryIngredients.Sort((a, b) => string.Compare(a.ingredient.ItemName, b.ingredient.ItemName));
            }
            else
            {
                // Fallback to available ingredients for demo mode
                foreach (var ingredient in availableIngredients ?? new List<Ingredient>())
                {
                    int quantity = GetIngredientCount(ingredient);
                    if (quantity > 0)
                    {
                        inventoryIngredients.Add((ingredient, quantity));
                    }
                }
            }

            // Add inventory header
            var headerLabel = new Label("🎒 Ingredient Inventory");
            headerLabel.AddToClassList("section-title");
            headerLabel.style.fontSize = 16;
            headerLabel.style.marginBottom = 8;
            ingredientPalette.Add(headerLabel);

            // Add count summary
            var countLabel = new Label($"Total Types: {inventoryIngredients.Count}");
            countLabel.AddToClassList("ingredient-count-summary");
            countLabel.style.fontSize = 12;
            countLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            countLabel.style.marginBottom = 8;
            ingredientPalette.Add(countLabel);

            // Display ingredients in inventory style
            if (inventoryIngredients.Count > 0)
            {
                foreach (var (ingredient, quantity) in inventoryIngredients)
                {
                    var item = CreateInventoryIngredientItem(ingredient, quantity);
                    ingredientPalette.Add(item);
                }
            }
            else
            {
                // Show empty inventory message
                var emptyLabel = new Label("No ingredients in inventory");
                emptyLabel.style.fontSize = 14;
                emptyLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
                emptyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                emptyLabel.style.marginTop = 20;
                ingredientPalette.Add(emptyLabel);
                
                if (useInventoryIngredients)
                {
                    var hintLabel = new Label("Add ingredients to your inventory or call AddDemoIngredients()");
                    hintLabel.style.fontSize = 12;
                    hintLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
                    hintLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    hintLabel.style.marginTop = 5;
                    hintLabel.style.whiteSpace = WhiteSpace.Normal;
                    ingredientPalette.Add(hintLabel);
                }
            }
        }

        private VisualElement CreateInventoryIngredientItem(Ingredient ingredient, int quantity)
        {
            var item = new VisualElement();
            item.AddToClassList("inventory-ingredient-item");
            item.style.flexDirection = FlexDirection.Row;
            item.style.alignItems = Align.Center;
            item.style.justifyContent = Justify.SpaceBetween;
            item.style.paddingTop = 4;
            item.style.paddingBottom = 4;
            item.style.paddingLeft = 8;
            item.style.paddingRight = 8;
            item.style.marginBottom = 2;
            item.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            item.style.borderTopWidth = 1;
            item.style.borderBottomWidth = 1;
            item.style.borderLeftWidth = 1;
            item.style.borderRightWidth = 1;
            item.style.borderTopColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            item.style.borderBottomColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            item.style.borderLeftColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            item.style.borderRightColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            item.style.borderTopLeftRadius = 4;
            item.style.borderTopRightRadius = 4;
            item.style.borderBottomLeftRadius = 4;
            item.style.borderBottomRightRadius = 4;

            // Left side: Icon and Name container
            var leftContainer = new VisualElement();
            leftContainer.style.flexDirection = FlexDirection.Row;
            leftContainer.style.alignItems = Align.Center;
            leftContainer.style.flexGrow = 1;

            // Icon display (32px)
            var iconContainer = new VisualElement();
            iconContainer.style.width = 32;
            iconContainer.style.height = 32;
            iconContainer.style.marginRight = 8;
            iconContainer.style.borderTopWidth = 1;
            iconContainer.style.borderBottomWidth = 1;
            iconContainer.style.borderLeftWidth = 1;
            iconContainer.style.borderRightWidth = 1;
            iconContainer.style.borderTopColor = new Color(0.5f, 0.5f, 0.5f);
            iconContainer.style.borderBottomColor = new Color(0.5f, 0.5f, 0.5f);
            iconContainer.style.borderLeftColor = new Color(0.5f, 0.5f, 0.5f);
            iconContainer.style.borderRightColor = new Color(0.5f, 0.5f, 0.5f);
            iconContainer.style.borderTopLeftRadius = 3;
            iconContainer.style.borderTopRightRadius = 3;
            iconContainer.style.borderBottomLeftRadius = 3;
            iconContainer.style.borderBottomRightRadius = 3;

            if (ingredient.ItemIcon != null)
            {
                iconContainer.style.backgroundImage = new StyleBackground(ingredient.ItemIcon);
                iconContainer.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            }
            else
            {
                // Use the first letter of the ingredient name as a fallback
                var iconLabel = new Label(ingredient.ItemName.Substring(0, 1).ToUpper());
                iconLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                iconLabel.style.color = Color.white;
                iconLabel.style.fontSize = 16;
                iconLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                iconContainer.Add(iconLabel);
            }

            leftContainer.Add(iconContainer);

            // Name label
            var nameLabel = new Label(ingredient.ItemName);
            nameLabel.style.fontSize = 14;
            nameLabel.style.color = Color.white;
            nameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            nameLabel.style.flexGrow = 1;
            leftContainer.Add(nameLabel);

            item.Add(leftContainer);

            // Right side: Quantity display
            var quantityContainer = new VisualElement();
            quantityContainer.style.flexDirection = FlexDirection.Row;
            quantityContainer.style.alignItems = Align.Center;

            var quantityLabel = new Label($"x{quantity}");
            quantityLabel.style.fontSize = 14;
            quantityLabel.style.color = quantity > 0 ? new Color(0.7f, 1f, 0.7f) : new Color(1f, 0.5f, 0.5f);
            quantityLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            quantityLabel.style.minWidth = 50;
            quantityLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            quantityContainer.Add(quantityLabel);

            item.Add(quantityContainer);

            // Apply styling based on quantity and usage
            if (quantity == 0)
            {
                item.style.opacity = 0.5f;
                item.style.backgroundColor = new Color(0.4f, 0.2f, 0.2f, 0.3f);
            }
            else if (IsIngredientInUse(ingredient))
            {
                item.style.backgroundColor = new Color(0.2f, 0.4f, 0.2f, 0.4f);
                item.style.borderTopColor = new Color(0.4f, 0.8f, 0.4f, 0.8f);
                item.style.borderBottomColor = new Color(0.4f, 0.8f, 0.4f, 0.8f);
                item.style.borderLeftColor = new Color(0.4f, 0.8f, 0.4f, 0.8f);
                item.style.borderRightColor = new Color(0.4f, 0.8f, 0.4f, 0.8f);
            }

            // Add interaction only if quantity > 0
            if (quantity > 0)
            {
                // Make draggable and clickable
                item.RegisterCallback<PointerDownEvent>(OnIngredientPointerDown);
                item.RegisterCallback<PointerMoveEvent>(OnIngredientPointerMove);
                item.RegisterCallback<PointerUpEvent>(OnIngredientPointerUp);
                item.style.cursor = StyleKeyword.Auto;

                // Store ingredient reference
                item.userData = ingredient;

                // Add click handler for selection
                item.RegisterCallback<ClickEvent>(evt =>
                {
                    SelectIngredient(ingredient);
                });

                // Add hover effects
                item.RegisterCallback<PointerEnterEvent>(evt =>
                {
                    if (quantity > 0)
                    {
                        item.style.backgroundColor = new Color(0.3f, 0.3f, 0.4f, 0.5f);
                    }
                });

                item.RegisterCallback<PointerLeaveEvent>(evt =>
                {
                    if (quantity > 0)
                    {
                        if (IsIngredientInUse(ingredient))
                        {
                            item.style.backgroundColor = new Color(0.2f, 0.4f, 0.2f, 0.4f);
                        }
                        else
                        {
                            item.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
                        }
                    }
                });
            }
            else
            {
                // Disable interaction for depleted ingredients
                item.style.cursor = StyleKeyword.None;
            }

            return item;
        }

        private VisualElement CreateIngredientPreview(Ingredient ingredient)
        {
            var preview = new VisualElement();
            preview.AddToClassList("ingredient-preview");

            // Create mini grid representation showing tetris block shape
            var miniGrid = new VisualElement();
            miniGrid.style.flexDirection = FlexDirection.Column;

            for (int y = 0; y < ingredient.GridHeight; y++)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;

                for (int x = 0; x < ingredient.GridWidth; x++)
                {
                    var cell = new VisualElement();
                    cell.style.width = 6;
                    cell.style.height = 6;
                    cell.style.backgroundColor = GetAspectColor(ingredient.IngredientAspect);
                    cell.style.marginTop = 1;
                    cell.style.marginLeft = 1;
                    row.Add(cell);
                }

                miniGrid.Add(row);
            }

            preview.Add(miniGrid);
            return preview;
        }

        #region Grid Management and Placement Logic

        private bool IsExpandedCell(Vector2Int position)
        {
            // Cells beyond base grid size are considered expanded
            return position.x >= baseGridWidth || position.y >= baseGridHeight;
        }

        private bool IsKeyLockedCell(Vector2Int position)
        {
            // Key recipe cells are locked to specific positions
            if (selectedRecipe == null || !isKeyRecipe) return false;

            // For key recipes, certain positions might be locked
            // This is a placeholder - in practice, you'd define this in the recipe data
            return false;
        }

        private void ShowPlacementPreview(Vector2Int position, Ingredient ingredient)
        {
            HidePlacementPreview();

            bool canPlace = CanPlaceIngredient(position, ingredient);
            bool canOverlap = allowOverlapping && CanPlaceIngredientWithOverlap(position, ingredient);

            for (int x = 0; x < ingredient.GridWidth; x++)
            {
                for (int y = 0; y < ingredient.GridHeight; y++)
                {
                    var cellPos = new Vector2Int(position.x + x, position.y + y);
                    if (IsValidGridPosition(cellPos))
                    {
                        var cell = craftingGrid[cellPos.x, cellPos.y].visualElement;
                        if (canPlace)
                        {
                            cell.AddToClassList("preview-valid");
                        }
                        else if (canOverlap)
                        {
                            cell.AddToClassList("preview-overlap");
                        }
                        else
                        {
                            cell.AddToClassList("preview-invalid");
                        }
                    }
                }
            }
        }

        private void HidePlacementPreview()
        {
            for (int x = 0; x < currentGridWidth; x++)
            {
                for (int y = 0; y < currentGridHeight; y++)
                {
                    var cell = craftingGrid[x, y].visualElement;
                    cell.RemoveFromClassList("preview-valid");
                    cell.RemoveFromClassList("preview-overlap");
                    cell.RemoveFromClassList("preview-invalid");
                }
            }
        }

        private void ShowPlacementFeedback(Vector2Int position, bool success)
        {
            var cell = craftingGrid[position.x, position.y].visualElement;
            if (success)
            {
                cell.AddToClassList("placement-success");
                // Remove class after animation
                cell.schedule.Execute(() => cell.RemoveFromClassList("placement-success")).StartingIn(500);
            }
            else
            {
                cell.AddToClassList("placement-failed");
                cell.schedule.Execute(() => cell.RemoveFromClassList("placement-failed")).StartingIn(500);
            }
        }

        /// <summary>
        /// Check if a grid position is within the total grid bounds and is available for placement
        /// </summary>
        private bool IsValidGridPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < TotalGridWidth &&
                   position.y >= 0 && position.y < TotalGridHeight;
        }

        /// <summary>
        /// Check if a grid position is visible and available for interaction
        /// </summary>
        private bool IsAvailableGridPosition(Vector2Int position)
        {
            return IsValidGridPosition(position) &&
                   visibleGridCells[position.x, position.y] &&
                   availableGridCells[position.x, position.y];
        }

        /// <summary>
        /// Get the number of currently visible cells in the grid
        /// </summary>
        private int GetVisibleCellCount()
        {
            int count = 0;
            for (int x = 0; x < TotalGridWidth; x++)
            {
                for (int y = 0; y < TotalGridHeight; y++)
                {
                    if (visibleGridCells[x, y]) count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Get the number of currently available (visible and usable) cells in the grid
        /// </summary>
        private int GetAvailableCellCount()
        {
            int count = 0;
            for (int x = 0; x < TotalGridWidth; x++)
            {
                for (int y = 0; y < TotalGridHeight; y++)
                {
                    if (availableGridCells[x, y]) count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Get the number of occupied cells in the grid
        /// </summary>
        private int GetOccupiedCellCount()
        {
            int count = 0;
            for (int x = 0; x < TotalGridWidth; x++)
            {
                for (int y = 0; y < TotalGridHeight; y++)
                {
                    if (IsValidGridPosition(new Vector2Int(x, y)) &&
                        craftingGrid[x, y] != null &&
                        !craftingGrid[x, y].isEmpty)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Get all positions that could potentially be unlocked (adjacent to visible cells)
        /// </summary>
        private List<Vector2Int> GetPotentialExpansionCells()
        {
            List<Vector2Int> potentialCells = new List<Vector2Int>();
            HashSet<Vector2Int> checkedPositions = new HashSet<Vector2Int>();

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            for (int x = 0; x < TotalGridWidth; x++)
            {
                for (int y = 0; y < TotalGridHeight; y++)
                {
                    if (visibleGridCells[x, y])
                    {
                        // Check adjacent cells
                        foreach (Vector2Int dir in directions)
                        {
                            Vector2Int checkPos = new Vector2Int(x, y) + dir;

                            if (IsValidGridPosition(checkPos) &&
                                !visibleGridCells[checkPos.x, checkPos.y] &&
                                !checkedPositions.Contains(checkPos))
                            {
                                potentialCells.Add(checkPos);
                                checkedPositions.Add(checkPos);
                            }
                        }
                    }
                }
            }

            return potentialCells;
        }

        /// <summary>
        /// Handle removal of an ingredient and check if grid should shrink
        /// </summary>
        private void HandleIngredientRemoval(Vector2Int position, Ingredient removedIngredient)
        {
            if (removedIngredient == null) return;

            // If the removed ingredient unlocked spaces, we might need to shrink the grid
            if (removedIngredient.UnlocksAdditionalSpace)
            {
                // Find cells that were unlocked by this ingredient and check if they should be locked again
                List<Vector2Int> cellsToHide = new List<Vector2Int>();

                for (int x = 0; x < TotalGridWidth; x++)
                {
                    for (int y = 0; y < TotalGridHeight; y++)
                    {
                        var cell = craftingGrid[x, y];
                        if (cell != null && cell.wasUnlockedByIngredient)
                        {
                            // Check if this cell is still supported by other expansion ingredients
                            if (!IsCellSupportedByOtherIngredients(new Vector2Int(x, y), removedIngredient))
                            {
                                cellsToHide.Add(new Vector2Int(x, y));
                            }
                        }
                    }
                }

                // Hide unsupported cells
                foreach (Vector2Int cellPos in cellsToHide)
                {
                    HideGridCell(cellPos);
                }

                if (cellsToHide.Count > 0)
                {
                    UpdateGridVisuals();
                    Debug.Log($"🔒 Hidden {cellsToHide.Count} grid cells after removing {removedIngredient.ItemName}");
                }
            }
        }

        /// <summary>
        /// Check if a cell is still supported by other expansion ingredients
        /// </summary>
        private bool IsCellSupportedByOtherIngredients(Vector2Int cellPosition, Ingredient excludeIngredient)
        {
            // Check if this cell is within the initial 3x3 area
            if (cellPosition.x >= CenterOffsetX && cellPosition.x < CenterOffsetX + InitialVisibleWidth &&
                cellPosition.y >= CenterOffsetY && cellPosition.y < CenterOffsetY + InitialVisibleHeight)
            {
                return true; // Initial area is always supported
            }

            // Check if any other placed expansion ingredients could support this cell
            foreach (var kvp in placedIngredients)
            {
                var ingredient = kvp.Value.ingredient;
                var placementPos = kvp.Key;

                if (ingredient == excludeIngredient || !ingredient.UnlocksAdditionalSpace)
                    continue;

                // Check if this ingredient could unlock the cell in question
                float distance = Vector2Int.Distance(placementPos, cellPosition);
                if (distance <= 2) // Within reasonable expansion range
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Hide a specific grid cell
        /// </summary>
        private void HideGridCell(Vector2Int position)
        {
            if (!IsValidGridPosition(position))
                return;

            // Don't hide cells in the initial 3x3 area
            if (position.x >= CenterOffsetX && position.x < CenterOffsetX + InitialVisibleWidth &&
                position.y >= CenterOffsetY && position.y < CenterOffsetY + InitialVisibleHeight)
            {
                return;
            }

            visibleGridCells[position.x, position.y] = false;
            availableGridCells[position.x, position.y] = false;

            var cell = craftingGrid[position.x, position.y];
            cell.isVisible = false;
            cell.isAvailable = false;
            cell.wasUnlockedByIngredient = false;

            // Clear any ingredients in this cell
            if (!cell.isEmpty)
            {
                cell.Clear();
                // Remove from placed ingredients
                if (placedIngredients.ContainsKey(position))
                {
                    placedIngredients.Remove(position);
                }
            }

            Debug.Log($"🔒 Hidden grid cell ({position.x}, {position.y})");
        }

        private bool CanPlaceIngredient(Vector2Int position, Ingredient ingredient)
        {
            for (int x = 0; x < ingredient.GridWidth; x++)
            {
                for (int y = 0; y < ingredient.GridHeight; y++)
                {
                    var checkPos = new Vector2Int(position.x + x, position.y + y);

                    // Check bounds
                    if (!IsValidGridPosition(checkPos))
                        return false;

                    // Check if cell is visible and available
                    if (!IsAvailableGridPosition(checkPos))
                        return false;

                    // Check if cell is already occupied (no overlapping for normal placement)
                    if (!craftingGrid[checkPos.x, checkPos.y].isEmpty)
                        return false;

                    // Check key-locked cells
                    if (craftingGrid[checkPos.x, checkPos.y].isKeyLocked)
                    {
                        var requiredIngredient = craftingGrid[checkPos.x, checkPos.y].requiredIngredient;
                        if (requiredIngredient != null && requiredIngredient != ingredient)
                            return false;
                    }
                }
            }

            return true;
        }

        private bool CanPlaceIngredientWithOverlap(Vector2Int position, Ingredient ingredient)
        {
            if (!allowOverlapping) return false;

            int overlappingCells = 0;

            for (int x = 0; x < ingredient.GridWidth; x++)
            {
                for (int y = 0; y < ingredient.GridHeight; y++)
                {
                    var checkPos = new Vector2Int(position.x + x, position.y + y);

                    // Check bounds
                    if (!IsValidGridPosition(checkPos))
                        return false;

                    // Count overlapping cells
                    if (!craftingGrid[checkPos.x, checkPos.y].isEmpty)
                        overlappingCells++;
                }
            }

            return overlappingCells <= maxOverlapTiles;
        }

        private void RefreshRecipeSelection()
        {
            if (recipeSelection == null || availableRecipes == null) return;

            recipeSelection.Clear();

            foreach (var recipe in availableRecipes)
            {
                var item = CreateRecipeItem(recipe);
                recipeSelection.Add(item);
            }
        }

        private VisualElement CreateRecipeItem(AlchemyRecipe recipe)
        {
            var item = new VisualElement();
            item.AddToClassList("recipe-item");

            if (selectedRecipe == recipe)
                item.AddToClassList("selected");

            var nameLabel = new Label(recipe.ItemName);
            nameLabel.AddToClassList("recipe-name");
            item.Add(nameLabel);

            var difficultyLabel = new Label("Standard Difficulty");
            difficultyLabel.AddToClassList("recipe-difficulty");
            item.Add(difficultyLabel);

            item.RegisterCallback<ClickEvent>(evt => { SelectRecipe(recipe); });

            return item;
        }

        private void SelectIngredient(Ingredient ingredient)
        {
            selectedIngredient = ingredient;
            UpdateIngredientSelection();
        }

        private void SelectRecipe(AlchemyRecipe recipe)
        {
            selectedRecipe = recipe;
            UpdateRecipeSelection();
            RefreshIngredientPaletteDisplay();
            UpdateRecipeInfo();
            UpdateUI();
        }

        private void UpdateIngredientSelection()
        {
            if (ingredientPalette == null) return;

            var items = ingredientPalette.Query<VisualElement>("ingredient-item").ToList();
            foreach (var item in items)
            {
                item.RemoveFromClassList("selected");
            }
        }

        private void UpdateRecipeSelection()
        {
            if (recipeSelection == null) return;

            var items = recipeSelection.Query<VisualElement>("recipe-item").ToList();
            foreach (var item in items)
            {
                item.RemoveFromClassList("selected");
                if (selectedRecipe != null)
                {
                    var nameLabel = item.Q<Label>();
                    if (nameLabel?.text == selectedRecipe.ItemName)
                        item.AddToClassList("selected");
                }
            }
        }

        private void UpdateRecipeInfo()
        {
            if (recipeInfoLabel == null) return;

            if (selectedRecipe == null)
            {
                recipeInfoLabel.text = "Select a recipe to see pattern requirements";
            }
            else
            {
                int visibleCells = GetVisibleCellCount();
                int availableCells = GetAvailableCellCount();
                int occupiedCells = GetOccupiedCellCount();

                recipeInfoLabel.text = $"Recipe: {selectedRecipe.ItemName}\n" +
                                       $"Grid Status: {occupiedCells}/{availableCells} cells used\n" +
                                       "Place ingredients to unlock more space";
            }
        }

        private string GetRecipeInfoText(int occupiedCells, int availableCells)
        {
            if (selectedRecipe == null) return "Select a recipe to see requirements and hints";
            
            var requiredIngredients = selectedRecipe.GetRequiredIngredients();
            string difficultyText = selectedRecipe.Difficulty != RecipeDifficulty.Standard ? 
                $" [{selectedRecipe.Difficulty}]" : "";

            string infoText = $"Recipe: {selectedRecipe.ItemName}{difficultyText}\\n";
            infoText += $"Required: {string.Join(", ", requiredIngredients.Select(i => i.ItemName))}\\n";
            infoText += $"Min Efficiency: {selectedRecipe.GetAdjustedMinimumEfficiency():P}\\n";
            
            if (selectedRecipe.RequiredPositions.Count > 0)
            {
                infoText += $"Position Requirements: {selectedRecipe.RequiredPositions.Count}\\n";
            }
            
            if (selectedRecipe.BonusPatterns.Count > 0)
            {
                infoText += $"Bonus Patterns Available: {selectedRecipe.BonusPatterns.Count}\\n";
            }
            
            infoText += $"Grid: {occupiedCells}/{availableCells} cells used\\n";
            
            if (!string.IsNullOrEmpty(selectedRecipe.RecipeHints))
            {
                infoText += $"Hint: {selectedRecipe.RecipeHints}";
            }

            return infoText;
        }

        #region Enhanced Recipe and Placement System

        private void PlaceIngredient(Vector2Int position, Ingredient ingredient)
        {
            if (!CanPlaceIngredient(position, ingredient)) return;

            var placedIngredient = new PlacedIngredient(ingredient, new GridPosition(position.x, position.y));

            for (int x = 0; x < ingredient.GridWidth; x++)
            {
                for (int y = 0; y < ingredient.GridHeight; y++)
                {
                    var cellPos = new Vector2Int(position.x + x, position.y + y);
                    placedIngredient.occupiedCells.Add(new GridPosition(cellPos.x, cellPos.y));
                    placedIngredients[cellPos] = placedIngredient;

                    var cell = craftingGrid[cellPos.x, cellPos.y].visualElement;
                    var gridCell = craftingGrid[cellPos.x, cellPos.y];

                    cell.RemoveFromClassList("empty");
                    cell.AddToClassList("filled");
                    cell.AddToClassList($"aspect-{ingredient.IngredientAspect.ToString().ToLower()}");

                    gridCell.ingredients.Add(ingredient);
                    gridCell.isEmpty = false;
                }
            }

            // Check for ingredient interactions
            CheckIngredientInteractions(placedIngredient);

            // Handle grid expansion from this ingredient
            HandleGridExpansion(ingredient);

            ShowPlacementFeedback(position, true);
            UpdateUI();
        }

        private void PlaceIngredientWithOverlap(Vector2Int position, Ingredient ingredient)
        {
            if (!CanPlaceIngredientWithOverlap(position, ingredient)) return;

            var placedIngredient = new PlacedIngredient(ingredient, new GridPosition(position.x, position.y));
            placedIngredient.isOverlapping = true;

            for (int x = 0; x < ingredient.GridWidth; x++)
            {
                for (int y = 0; y < ingredient.GridHeight; y++)
                {
                    var cellPos = new Vector2Int(position.x + x, position.y + y);
                    placedIngredient.occupiedCells.Add(new GridPosition(cellPos.x, cellPos.y));

                    // Multiple ingredients can occupy the same cell when overlapping
                    if (!placedIngredients.ContainsKey(cellPos))
                    {
                        placedIngredients[cellPos] = placedIngredient;
                    }

                    var cell = craftingGrid[cellPos.x, cellPos.y].visualElement;
                    var gridCell = craftingGrid[cellPos.x, cellPos.y];

                    cell.RemoveFromClassList("empty");
                    cell.AddToClassList("filled");
                    cell.AddToClassList("overlapping");
                    cell.AddToClassList($"aspect-{ingredient.IngredientAspect.ToString().ToLower()}");

                    gridCell.ingredients.Add(ingredient);
                    gridCell.isEmpty = false;
                }
            }

            CheckIngredientInteractions(placedIngredient);
            HandleGridExpansion(ingredient);

            ShowPlacementFeedback(position, true);
            UpdateUI();
        }

        private void RemoveIngredient(Vector2Int position)
        {
            if (!placedIngredients.ContainsKey(position)) return;

            var placedIngredient = placedIngredients[position];
            var ingredient = placedIngredient.ingredient;

            // Remove from all occupied cells
            foreach (var cellPos in placedIngredient.occupiedCells)
            {
                var vectorPos = new Vector2Int(cellPos.x, cellPos.y);
                if (IsValidGridPosition(vectorPos))
                {
                    placedIngredients.Remove(vectorPos);

                    var cell = craftingGrid[cellPos.x, cellPos.y].visualElement;
                    var gridCell = craftingGrid[cellPos.x, cellPos.y];

                    cell.RemoveFromClassList("filled");
                    cell.RemoveFromClassList("overlapping");
                    cell.RemoveFromClassList($"aspect-{ingredient.IngredientAspect.ToString().ToLower()}");

                    gridCell.ingredients.Remove(ingredient);
                    if (gridCell.ingredients.Count == 0)
                    {
                        cell.AddToClassList("empty");
                        gridCell.isEmpty = true;
                    }
                }
            }

            // Handle grid contraction if this ingredient was providing expansion
            HandleIngredientRemoval(position, ingredient);

            Debug.Log($"🗑️ Removed ingredient {ingredient.ItemName} from position ({position.x}, {position.y})");

            UpdateUI();
        }

        private void CheckIngredientInteractions(PlacedIngredient placedIngredient)
        {
            var ingredient = placedIngredient.ingredient;

            if (!showIngredientInteractions || !ingredientInteractions.ContainsKey(ingredient))
                return;

            var potentialInteractions = ingredientInteractions[ingredient];

            foreach (var otherPlacedIngredient in placedIngredients.Values.Distinct())
            {
                if (otherPlacedIngredient != placedIngredient &&
                    potentialInteractions.Contains(otherPlacedIngredient.ingredient))
                {
                    // Check if ingredients are adjacent or overlapping
                    if (AreIngredientsInteracting(placedIngredient, otherPlacedIngredient))
                    {
                        placedIngredient.interactions.Add(otherPlacedIngredient.ingredient.ItemName);
                        otherPlacedIngredient.interactions.Add(ingredient.ItemName);

                        // Visual feedback for interactions
                        HighlightInteraction(placedIngredient, otherPlacedIngredient);

                        Debug.Log(
                            $"🔬 Ingredient interaction discovered: {ingredient.ItemName} + {otherPlacedIngredient.ingredient.ItemName}");
                    }
                }
            }
        }

        private bool AreIngredientsInteracting(PlacedIngredient ingredient1, PlacedIngredient ingredient2)
        {
            // Check if ingredients are adjacent or overlapping
            foreach (var cell1 in ingredient1.occupiedCells)
            {
                foreach (var cell2 in ingredient2.occupiedCells)
                {
                    // Adjacent cells (including diagonals)
                    if (Mathf.Abs(cell1.x - cell2.x) <= 1 && Mathf.Abs(cell1.y - cell2.y) <= 1)
                        return true;

                    // Overlapping cells
                    if (cell1 == cell2)
                        return true;
                }
            }

            return false;
        }

        private void HighlightInteraction(PlacedIngredient ingredient1, PlacedIngredient ingredient2)
        {
            // Add visual feedback for ingredient interactions
            foreach (var cellPos in ingredient1.occupiedCells)
            {
                var vectorPos = new Vector2Int(cellPos.x, cellPos.y);
                if (IsValidGridPosition(vectorPos))
                {
                    var cell = craftingGrid[cellPos.x, cellPos.y].visualElement;
                    cell.AddToClassList("interaction-highlight");
                }
            }

            foreach (var cellPos in ingredient2.occupiedCells)
            {
                var vectorPos = new Vector2Int(cellPos.x, cellPos.y);
                if (IsValidGridPosition(vectorPos))
                {
                    var cell = craftingGrid[cellPos.x, cellPos.y].visualElement;
                    cell.AddToClassList("interaction-highlight");
                }
            }
        }

        private void HandleGridExpansion(Ingredient ingredient)
        {
            if (!ingredient.UnlocksAdditionalSpace) return;

            int additionalCells = ingredient.AdditionalSpaceCount;

            // Simple expansion: add cells to the right and bottom
            for (int i = 0; i < additionalCells && expandedGridCells.Count < 8; i++)
            {
                Vector2Int newCell;

                if (currentGridWidth < baseGridWidth + 2)
                {
                    // Expand width
                    newCell = new Vector2Int(currentGridWidth, 0);
                    currentGridWidth++;
                }
                else if (currentGridHeight < baseGridHeight + 2)
                {
                    // Expand height
                    newCell = new Vector2Int(0, currentGridHeight);
                    currentGridHeight++;
                }
                else
                {
                    break; // Max expansion reached
                }

                expandedGridCells.Add(newCell);
            }

            // Recreate grid UI if expansion occurred
            if (additionalCells > 0)
            {
                Debug.Log($"🔧 Grid expanded by {ingredient.ItemName} (+{additionalCells} cells)");
                ReinitializeGrid();
            }
        }

        private void HandleGridContraction(Ingredient ingredient)
        {
            if (!ingredient.UnlocksAdditionalSpace) return;

            // Check if any other ingredients are also providing expansion
            bool hasOtherExpansionIngredients = false;
            foreach (var placedIngredient in placedIngredients.Values.Distinct())
            {
                if (placedIngredient.ingredient != ingredient && placedIngredient.ingredient.UnlocksAdditionalSpace)
                {
                    hasOtherExpansionIngredients = true;
                    break;
                }
            }

            // If no other expansion ingredients, contract the grid
            if (!hasOtherExpansionIngredients && expandedGridCells.Count > 0)
            {
                int cellsToRemove = ingredient.AdditionalSpaceCount;
                for (int i = 0; i < cellsToRemove && expandedGridCells.Count > 0; i++)
                {
                    expandedGridCells.RemoveAt(expandedGridCells.Count - 1);

                    if (currentGridWidth > baseGridWidth)
                        currentGridWidth--;
                    else if (currentGridHeight > baseGridHeight)
                        currentGridHeight--;
                }

                Debug.Log($"🔧 Grid contracted due to removal of {ingredient.ItemName}");
                ReinitializeGrid();
            }
        }

        private void ReinitializeGrid()
        {
            // Save current placements
            var savedPlacements = new List<PlacedIngredient>(placedIngredients.Values.Distinct());

            // Clear and recreate grid
            placedIngredients.Clear();
            InitializeGrid();
            CreateGridUI();

            // Restore valid placements
            foreach (var placement in savedPlacements)
            {
                var originVector = new Vector2Int(placement.position.x, placement.position.y);
                if (CanPlaceIngredient(originVector, placement.ingredient))
                {
                    PlaceIngredient(originVector, placement.ingredient);
                }
                else
                {
                    Debug.LogWarning(
                        $"Could not restore placement of {placement.ingredient.ItemName} after grid change");
                }
            }
        }

        #endregion

        #region Grid Utility Methods

        private void ClearGrid()
        {
            placedIngredients.Clear();

            // Clear all cells in the full 5x5 grid
            for (int x = 0; x < TotalGridWidth; x++)
            {
                for (int y = 0; y < TotalGridHeight; y++)
                {
                    if (craftingGrid[x, y] != null)
                    {
                        var gridCell = craftingGrid[x, y];
                        var cell = gridCell.visualElement;

                        if (cell != null)
                        {
                            cell.RemoveFromClassList("filled");
                            cell.RemoveFromClassList("overlapping");
                            cell.RemoveFromClassList("interaction-highlight");

                            foreach (Aspect aspect in Enum.GetValues(typeof(Aspect)))
                            {
                                cell.RemoveFromClassList($"aspect-{aspect.ToString().ToLower()}");
                            }

                            cell.AddToClassList("empty");
                        }

                        gridCell.Clear();
                    }
                }
            }

            // Reset grid to initial visible state (3x3 center)
            InitializeVisibleArea();

            // Update visuals
            UpdateGridVisuals();
            UpdateUI();

            Debug.Log("🧹 Grid cleared and reset to initial 3x3 configuration");
        }
        
        /// <summary>
        /// Debug method to test grid functionality - can be called from console or buttons
        /// </summary>
        public void TestGridDisplay()
        {
            Debug.Log("🧪 Testing Grid Display...");
            Debug.Log($"  • GameObject active: {gameObject.activeInHierarchy}");
            Debug.Log($"  • Component enabled: {enabled}");
            Debug.Log($"  • UIDocument: {uiDocument != null}");
            Debug.Log($"  • Root element: {uiDocument?.rootVisualElement != null}");
            Debug.Log($"  • Grid initialized: {isGridInitialized}");
            Debug.Log($"  • Grid container: {gridContainer != null}");
            
            if (gridContainer != null)
            {
                Debug.Log($"  • Grid container children: {gridContainer.childCount}");
                Debug.Log($"  • Grid container visible: {gridContainer.style.display != DisplayStyle.None}");
            }
            
            // Force setup if needed
            if (!isGridInitialized)
            {
                Debug.Log("🔧 Force initializing grid...");
                InitializeGrid();
            }
            
            if (gridContainer == null && uiDocument?.rootVisualElement != null)
            {
                Debug.Log("🔧 Force setting up UI...");
                SetupUI();
            }
        }

        private void AttemptCrafting()
        {
            if (selectedRecipe == null)
            {
                Debug.LogWarning("No recipe selected!");
                return;
            }

            if (placedIngredients.Count == 0)
            {
                Debug.LogWarning("No ingredients placed!");
                return;
            }

            // Check inventory for sufficient ingredients before crafting
            if (!HasSufficientIngredientsInInventory(placedIngredients))
            {
                Debug.LogWarning("Insufficient ingredients in inventory for crafting!");
                return;
            }

            // For key/story recipes, check if it would be a failure
            if (isKeyRecipe && WouldBeFailure())
            {
                ShowKeyRecipeWarning();
                return;
            }

            bool isSuccess = EvaluateCraftingSuccess();

            if (isSuccess)
            {
                // Consume ingredients from inventory before crafting
                ConsumeIngredientsFromInventory(placedIngredients);
                
                // Add crafted result to inventory
                AddResultToInventory(selectedRecipe, 1);
                
                OnCraftingCompleted?.Invoke(selectedRecipe,
                    new Dictionary<Vector2Int, PlacedIngredient>(placedIngredients), true);
                Debug.Log($"✅ Successfully crafted {selectedRecipe.ItemName}!");
            }
            else
            {
                // Handle failure - create synthetic ingredient if allowed
                if (allowSyntheticCreation && !isKeyRecipe)
                {
                    // Consume ingredients from inventory even on failure
                    ConsumeIngredientsFromInventory(placedIngredients);
                    
                    var syntheticIngredient = CreateSyntheticIngredient();
                    
                    // Add synthetic ingredient to inventory
                    AddResultToInventory(syntheticIngredient, 1);
                    
                    OnSyntheticIngredientCreated?.Invoke(syntheticIngredient);
                    OnCraftingCompleted?.Invoke(selectedRecipe,
                        new Dictionary<Vector2Int, PlacedIngredient>(placedIngredients), false);
                    Debug.Log("⚗️ Created synthetic ingredient from failed crafting attempt");
                }
                else
                {
                    Debug.LogWarning("❌ Crafting failed! Pattern requirements not met.");
                }
            }

            ClearGrid();
        }

        private bool EvaluateCraftingSuccess()
        {
            if (selectedRecipe == null)
            {
                Debug.LogWarning("No recipe selected for evaluation!");
                return false;
            }

            // Get placed ingredients as a dictionary for easier processing
            var placedIngredientsDict = new Dictionary<Vector2Int, Ingredient>();
            foreach (var kvp in placedIngredients)
            {
                placedIngredientsDict[kvp.Key] = kvp.Value.ingredient;
            }

            // Check core requirements
            bool hasRequiredIngredients = CheckRequiredIngredients(placedIngredientsDict);
            bool hasForbiddenIngredients = CheckForForbiddenIngredients(placedIngredientsDict);
            bool meetsGridRequirements = CheckGridRequirements(placedIngredientsDict);
            
            // Calculate efficiency with difficulty adjustment
            float baseEfficiency = CalculateSpaceEfficiency();
            float adjustedMinEfficiency = selectedRecipe.GetAdjustedMinimumEfficiency();
            bool meetsEfficiency = baseEfficiency >= adjustedMinEfficiency;

            // Check required positions for key recipes
            bool meetsPositionRequirements = CheckRequiredPositions(placedIngredientsDict);

            // Calculate bonuses
            float patternBonus = selectedRecipe.CalculatePatternBonus(placedIngredientsDict);
            float synergyBonus = selectedRecipe.CalculateAspectSynergyBonus(placedIngredientsDict);
            float totalBonus = patternBonus + synergyBonus;

            // Adjust efficiency with bonuses
            float finalEfficiency = baseEfficiency + totalBonus;

            // Success criteria:
            bool basicSuccess = hasRequiredIngredients && !hasForbiddenIngredients && 
                               meetsGridRequirements && meetsPositionRequirements;
            bool efficiencySuccess = finalEfficiency >= adjustedMinEfficiency;

            bool overallSuccess = basicSuccess && efficiencySuccess;

            // Log detailed evaluation for debugging
            if (enableDebugMode)
            {
                Debug.Log($"🧪 Recipe Evaluation for '{selectedRecipe.name}':");
                Debug.Log($"  - Required Ingredients: {hasRequiredIngredients}");
                Debug.Log($"  - No Forbidden Ingredients: {!hasForbiddenIngredients}");
                Debug.Log($"  - Grid Requirements: {meetsGridRequirements}");
                Debug.Log($"  - Position Requirements: {meetsPositionRequirements}");
                Debug.Log($"  - Base Efficiency: {baseEfficiency:P} (Required: {adjustedMinEfficiency:P})");
                Debug.Log($"  - Pattern Bonus: +{patternBonus:P}");
                Debug.Log($"  - Synergy Bonus: +{synergyBonus:P}");
                Debug.Log($"  - Final Efficiency: {finalEfficiency:P}");
                Debug.Log($"  - Overall Success: {overallSuccess}");
            }

            return overallSuccess;
        }

        private bool CheckRequiredIngredients(Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            if (selectedRecipe == null) return false;

            var requiredIngredients = selectedRecipe.GetRequiredIngredients();
            var placedIngredientsList = placedIngredients.Values.ToList();

            foreach (var required in requiredIngredients)
            {
                if (!placedIngredientsList.Contains(required))
                {
                    // Check if there's an acceptable alternative
                    bool hasAlternative = false;
                    foreach (var alternative in selectedRecipe.AlternativeIngredients)
                    {
                        if (placedIngredientsList.Contains(alternative))
                        {
                            hasAlternative = true;
                            break;
                        }
                    }
                    
                    if (!hasAlternative)
                        return false;
                }
            }

            return true;
        }

        private bool CheckForForbiddenIngredients(Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            if (selectedRecipe == null) return false;

            foreach (var ingredient in placedIngredients.Values)
            {
                if (selectedRecipe.IsForbiddenIngredient(ingredient))
                {
                    return true; // Found forbidden ingredient
                }
            }

            return false; // No forbidden ingredients found
        }

        private bool CheckGridRequirements(Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            if (selectedRecipe == null) return true;

            var requirements = selectedRecipe.GridRequirements;
            int ingredientCount = placedIngredients.Values.Distinct().Count();

            // Check ingredient count limits
            if (ingredientCount < requirements.minimumIngredientsUsed || 
                ingredientCount > requirements.maximumIngredientsUsed)
            {
                return false;
            }

            // Check symmetry requirement
            if (requirements.requiresSymmetry && !CheckSymmetry(placedIngredients))
            {
                return false;
            }

            // Check compactness requirement
            if (requirements.requiresCompactness && 
                CalculateCompactness(placedIngredients) < requirements.compactnessThreshold)
            {
                return false;
            }

            return true;
        }

        private bool CheckRequiredPositions(Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            if (selectedRecipe == null) return true;

            foreach (var requiredPos in selectedRecipe.RequiredPositions)
            {
                if (requiredPos.mustBeExactPosition)
                {
                    if (!placedIngredients.ContainsKey(requiredPos.gridPosition) ||
                        placedIngredients[requiredPos.gridPosition] != requiredPos.requiredIngredient)
                    {
                        return false;
                    }
                }
                else if (requiredPos.allowsAdjacency)
                {
                    // Check if ingredient is at position or adjacent
                    bool found = false;
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            var checkPos = requiredPos.gridPosition + new Vector2Int(dx, dy);
                            if (placedIngredients.ContainsKey(checkPos) &&
                                placedIngredients[checkPos] == requiredPos.requiredIngredient)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (found) break;
                    }
                    
                    if (!found) return false;
                }
            }

            return true;
        }

        private bool CheckSymmetry(Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            // Simple horizontal symmetry check
            var positions = placedIngredients.Keys.ToList();
            if (positions.Count == 0) return true;

            int centerX = (positions.Min(p => p.x) + positions.Max(p => p.x)) / 2;

            foreach (var pos in positions)
            {
                var mirrorPos = new Vector2Int(2 * centerX - pos.x, pos.y);
                if (!placedIngredients.ContainsKey(mirrorPos) ||
                    placedIngredients[mirrorPos] != placedIngredients[pos])
                {
                    return false;
                }
            }

            return true;
        }

        private float CalculateCompactness(Dictionary<Vector2Int, Ingredient> placedIngredients)
        {
            if (placedIngredients.Count == 0) return 1f;

            var positions = placedIngredients.Keys.ToList();
            
            // Calculate bounding box
            int minX = positions.Min(p => p.x);
            int maxX = positions.Max(p => p.x);
            int minY = positions.Min(p => p.y);
            int maxY = positions.Max(p => p.y);

            int boundingBoxArea = (maxX - minX + 1) * (maxY - minY + 1);
            float compactness = (float)positions.Count / boundingBoxArea;

            return compactness;
        }

        private bool WouldBeFailure()
        {
            // For key recipes, check if the arrangement would fail
            return !EvaluateCraftingSuccess();
        }

        private void ShowKeyRecipeWarning()
        {
            Debug.Log("🔮 Something feels off about this arrangement... (Key recipe failure prevented)");

            if (gridStatusLabel != null)
            {
                gridStatusLabel.text = "Something feels off about this arrangement...";
                gridStatusLabel.AddToClassList("warning-text");

                // Remove warning after delay
                gridStatusLabel.schedule.Execute(() =>
                {
                    gridStatusLabel.RemoveFromClassList("warning-text");
                    UpdateUI();
                }).StartingIn(3000);
            }
        }

        // Legacy methods removed - using enhanced versions above

        private bool HasOverridingInteraction(Ingredient ingredient1, Ingredient ingredient2)
        {
            foreach (var interaction in knownInteractions)
            {
                if ((interaction.ingredient1 == ingredient1 && interaction.ingredient2 == ingredient2) ||
                    (interaction.ingredient1 == ingredient2 && interaction.ingredient2 == ingredient1))
                {
                    return true;
                }
            }

            return false;
        }

        private float CalculateSpaceEfficiency()
        {
            int totalCells = currentGridWidth * currentGridHeight;
            int usedCells = placedIngredients.Count;

            if (totalCells == 0) return 0f;

            return (float)usedCells / totalCells;
        }

        private Ingredient CreateSyntheticIngredient()
        {
            if (syntheticIngredientTemplate == null)
            {
                Debug.LogWarning("No synthetic ingredient template assigned!");
                return null;
            }

            // Create a copy of the template with modified properties based on the failed combination
            var placedIngredientsDict = new Dictionary<Vector2Int, Ingredient>();
            foreach (var kvp in placedIngredients)
            {
                placedIngredientsDict[kvp.Key] = kvp.Value.ingredient;
            }
            var uniqueIngredients = placedIngredientsDict.Values.Distinct().ToList();

            // This would ideally create a new ScriptableObject instance
            // For now, return the template (in a real implementation, you'd create a new instance)
            Debug.Log(
                $"🧪 Synthetic ingredient created from: {string.Join(", ", uniqueIngredients.Select(i => i.ItemName))}");

            return syntheticIngredientTemplate;
        }

        private void UpdateUI()
        {
            if (gridStatusLabel != null)
            {
                int uniqueIngredients = placedIngredients.Values.Distinct().Count();
                int totalCells = placedIngredients.Count;
                string status = totalCells == 0
                    ? "Empty"
                    : $"{uniqueIngredients} ingredients, {totalCells} cells filled";

                if (allowOverlapping && placedIngredients.Values.Any(p => p.isOverlapping))
                {
                    status += " (overlapping)";
                }

                gridStatusLabel.text = $"Grid Status: {status}";
            }

            if (patternMatchLabel != null)
            {
                float efficiency = CalculateSpaceEfficiency();
                int interactions = CountActiveInteractions();

                string matchText = $"Efficiency: {efficiency:P}";
                if (interactions > 0)
                {
                    matchText += $", Interactions: {interactions}";
                }

                patternMatchLabel.text = matchText;
            }

            if (craftButton != null)
            {
                bool canCraft = selectedRecipe != null && placedIngredients.Count > 0;
                craftButton.SetEnabled(canCraft);

                if (canCraft)
                {
                    bool wouldSucceed = EvaluateCraftingSuccess();
                    craftButton.text = wouldSucceed
                        ? "Craft Potion"
                        : (allowSyntheticCreation && !isKeyRecipe ? "Create Synthetic" : "Craft (Risky)");
                }
                else
                {
                    craftButton.text = "Craft";
                }
            }
        }

        private int CountActiveInteractions()
        {
            int count = 0;
            var processedPairs = new HashSet<string>();

            foreach (var placedIngredient in placedIngredients.Values.Distinct())
            {
                foreach (var interactingIngredient in placedIngredient.interactions)
                {
                    string pairKey = string.Compare(placedIngredient.ingredient.ItemName, interactingIngredient) < 0
                        ? $"{placedIngredient.ingredient.ItemName}_{interactingIngredient}"
                        : $"{interactingIngredient}_{placedIngredient.ingredient.ItemName}";

                    if (!processedPairs.Contains(pairKey))
                    {
                        processedPairs.Add(pairKey);
                        count++;
                    }
                }
            }

            return count;
        }

        #endregion

        #region Helper Methods

        private bool IsIngredientRelevant(Ingredient ingredient)
        {
            if (selectedRecipe == null) return false;

            // Check if ingredient is in recipe requirements
            return selectedRecipe.InputIngredient1 == ingredient ||
                   selectedRecipe.InputIngredient2 == ingredient ||
                   selectedRecipe.InputIngredient3 == ingredient;
        }

        private int GetIngredientCount(Ingredient ingredient)
        {
            // Get actual count from inventory system if enabled
            if (useInventoryIngredients && inventoryHolder?.Container != null)
            {
                var inventorySlot = inventoryHolder.Container.Slots.FirstOrDefault(slot => slot.Item == ingredient);
                return inventorySlot?.Quantity ?? 0;
            }
            
            // Fallback for demo mode - unlimited ingredients
            return availableIngredients.Contains(ingredient) ? 999 : 0;
        }

        private Color GetAspectColor(Aspect aspect)
        {
            return aspect switch
            {
                Aspect.Scorch => new Color(0.8f, 0.3f, 0.3f),
                Aspect.Frigid => new Color(0.3f, 0.5f, 0.8f),
                Aspect.Corporeal => new Color(0.5f, 0.6f, 0.3f),
                Aspect.Arc => new Color(0.7f, 0.7f, 0.3f),
                Aspect.Divine => new Color(0.6f, 0.5f, 0.7f),
                Aspect.Caustic => new Color(0.9f, 0.6f, 0.2f),
                _ => Color.gray
            };
        }

        public void Show()
        {
            if (uiDocument?.rootVisualElement != null)
            {
                uiDocument.rootVisualElement.style.display = DisplayStyle.Flex;
                enabled = true;
            }
        }

        public void Hide()
        {
            if (uiDocument?.rootVisualElement != null)
            {
                uiDocument.rootVisualElement.style.display = DisplayStyle.None;
                enabled = false;
            }
        }

        #endregion

        #region Skill Integration and Recipe Management

        public void SetRecipeAsKeyRecipe(bool isKey)
        {
            isKeyRecipe = isKey;

            if (isKey)
            {
                Debug.Log("🔑 Key recipe mode enabled - failures will be prevented");
            }
        }

        public void AddIngredientInteraction(Ingredient ingredient1, Ingredient ingredient2, float multiplier = 1.5f,
            string description = "")
        {
            var interaction = new IngredientInteraction
            {
                ingredient1 = ingredient1,
                ingredient2 = ingredient2,
                effectMultiplier = multiplier,
                interactionDescription = description
            };

            knownInteractions.Add(interaction);
            LoadIngredientInteractions();
        }

        public List<PlacedIngredient> GetPlacedIngredients()
        {
            return placedIngredients.Values.Distinct().ToList();
        }

        public void ApplySkillBonuses()
        {
            UpdateGridSizeFromSkills();
            ReinitializeGrid();
        }

        #endregion

        #region Drag and Drop Functionality

        private void OnIngredientPointerDown(PointerDownEvent evt)
        {
            if (evt.target is VisualElement element && element.userData is Ingredient ingredient)
            {
                int count = GetIngredientCount(ingredient);
                if (count <= 0) return;

                isDragging = true;
                draggedElement = element;
                draggedIngredient = ingredient;

                // Calculate offset from element to pointer
                var elementRect = element.worldBound;
                dragOffset = (Vector2)evt.position - new Vector2(elementRect.x, elementRect.y);

                // Create visual feedback
                element.AddToClassList("ingredient-dragging");

                // Create drag ghost preview
                CreateDragGhost(element, ingredient);

                // Play drag start sound
                PlayAudioFeedback(dragStartSound);

                // Capture pointer to this element
                element.CapturePointer(evt.pointerId);

                // Show initial drop zone highlights
                HighlightValidDropZones(evt.position);

                evt.StopPropagation();
            }
        }

        private void OnIngredientPointerMove(PointerMoveEvent evt)
        {
            if (!isDragging || draggedElement == null) return;

            // Update visual position (optional visual drag feedback)
            // Update drag ghost position
            UpdateDragGhostPosition(evt.position);

            // Highlight valid drop zones
            HighlightValidDropZones(evt.position);

            evt.StopPropagation();
        }

        private void OnIngredientPointerUp(PointerUpEvent evt)
        {
            if (!isDragging || draggedElement == null) return;

            // Find drop target
            var dropTarget = FindGridCellAt(evt.position);
            bool successfulDrop = false;

            if (dropTarget != null && draggedIngredient != null)
            {
                // Attempt to place ingredient
                successfulDrop = TryPlaceIngredientFromDrag(dropTarget, draggedIngredient);

                if (successfulDrop)
                {
                    // Play successful drop sound and animate snapping
                    PlayAudioFeedback(dragDropSound);
                    AnimateSnapToGrid(dropTarget);
                }
                else
                {
                    // Play cancel sound for invalid drop
                    PlayAudioFeedback(dragCancelSound);
                }
            }
            else
            {
                // Play cancel sound for no valid target
                PlayAudioFeedback(dragCancelSound);
            }

            // Clean up drag state
            CleanupDragOperation(evt.pointerId);

            evt.StopPropagation();
        }

        private void HighlightValidDropZones(Vector2 pointerPosition)
        {
            if (draggedIngredient == null) return;

            // Clear previous highlights
            ClearDropZoneHighlights();

            // Check all grid positions for valid placement
            for (int x = 0; x < currentGridWidth; x++)
            {
                for (int y = 0; y < currentGridHeight; y++)
                {
                    var gridPos = new Vector2Int(x, y);
                    if (CanPlaceIngredient(gridPos, draggedIngredient))
                    {
                        var cell = craftingGrid[x, y].visualElement;
                        cell.AddToClassList("drop-zone-valid");
                    }
                }
            }
        }

        private void ClearDropZoneHighlights()
        {
            for (int x = 0; x < currentGridWidth; x++)
            {
                for (int y = 0; y < currentGridHeight; y++)
                {
                    var cell = craftingGrid[x, y].visualElement;
                    cell.RemoveFromClassList("drop-zone-valid");
                    cell.RemoveFromClassList("drop-zone-invalid");
                }
            }
        }

        #endregion

        #region Enhanced Drag Features

        private void CreateDragGhost(VisualElement original, Ingredient ingredient)
        {
            if (dragGhost != null)
                RemoveDragGhost();

            dragGhost = new VisualElement();
            dragGhost.style.position = Position.Absolute;
            dragGhost.style.width = original.resolvedStyle.width;
            dragGhost.style.height = original.resolvedStyle.height;
            dragGhost.style.backgroundColor = new Color(1f, 1f, 1f, 0.7f); // Semi-transparent white
            dragGhost.style.borderTopWidth = 2;
            dragGhost.style.borderBottomWidth = 2;
            dragGhost.style.borderLeftWidth = 2;
            dragGhost.style.borderRightWidth = 2;
            dragGhost.style.borderTopColor = Color.cyan;
            dragGhost.style.borderBottomColor = Color.cyan;
            dragGhost.style.borderLeftColor = Color.cyan;
            dragGhost.style.borderRightColor = Color.cyan;
            dragGhost.style.borderTopLeftRadius = 8;
            dragGhost.style.borderTopRightRadius = 8;
            dragGhost.style.borderBottomLeftRadius = 8;
            dragGhost.style.borderBottomRightRadius = 8;
            dragGhost.pickingMode = PickingMode.Ignore;

            // Add ingredient icon/text
            var ghostLabel = new Label(ingredient.ItemName);
            ghostLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            ghostLabel.style.fontSize = 12;
            ghostLabel.style.color = Color.black;
            dragGhost.Add(ghostLabel);

            // Add to root so it appears on top
            uiDocument.rootVisualElement.Add(dragGhost);
        }

        private void UpdateDragGhostPosition(Vector2 worldPosition)
        {
            if (dragGhost == null) return;

            // Convert world position to local position relative to root
            var rootRect = uiDocument.rootVisualElement.worldBound;
            var localPos = worldPosition - new Vector2(rootRect.x, rootRect.y);

            dragGhost.style.left = localPos.x - dragOffset.x;
            dragGhost.style.top = localPos.y - dragOffset.y;
        }

        private void RemoveDragGhost()
        {
            if (dragGhost != null)
            {
                if (dragGhost.parent != null)
                    dragGhost.parent.Remove(dragGhost);
                dragGhost = null;
            }
        }

        private void PlayAudioFeedback(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        private void AnimateSnapToGrid(VisualElement gridCell)
        {
            if (gridCell == null) return;

            // Create a brief scaling animation to show snapping
            var originalScale = gridCell.style.scale;

            // Scale up briefly
            gridCell.style.scale = new Scale(Vector3.one * 1.1f);

            // Use a coroutine to scale back down
            StartCoroutine(ScaleBackCoroutine(gridCell, originalScale));

            // Play snap sound
            PlayAudioFeedback(snapSound);
        }

        private IEnumerator ScaleBackCoroutine(VisualElement element, StyleScale originalScale)
        {
            yield return new WaitForSeconds(0.1f);
            if (element != null)
                element.style.scale = originalScale;
        }

        private void CleanupDragOperation(int pointerId)
        {
            if (draggedElement != null)
            {
                draggedElement.RemoveFromClassList("ingredient-dragging");
                draggedElement.ReleasePointer(pointerId);
            }

            RemoveDragGhost();
            ClearDropZoneHighlights();

            isDragging = false;
            draggedElement = null;
            draggedIngredient = null;
        }

        #endregion

        private VisualElement FindGridCellAt(Vector2 worldPosition)
        {
            // Find which grid cell the pointer is over
            for (int x = 0; x < TotalGridWidth; x++)
            {
                for (int y = 0; y < TotalGridHeight; y++)
                {
                    if (visibleGridCells[x, y] && craftingGrid[x, y].visualElement != null)
                    {
                        var cell = craftingGrid[x, y].visualElement;
                        if (cell.worldBound.Contains(worldPosition))
                        {
                            return cell;
                        }
                    }
                }
            }

            return null;
        }

        private bool TryPlaceIngredientFromDrag(VisualElement dropCell, Ingredient ingredient)
        {
            // Find grid position of drop cell
            for (int x = 0; x < TotalGridWidth; x++)
            {
                for (int y = 0; y < TotalGridHeight; y++)
                {
                    if (visibleGridCells[x, y] && craftingGrid[x, y].visualElement == dropCell)
                    {
                        var gridPos = new Vector2Int(x, y);
                        if (CanPlaceIngredient(gridPos, ingredient))
                        {
                            PlaceIngredient(gridPos, ingredient);

                            // Check if this ingredient unlocks additional space
                            if (ingredient.UnlocksAdditionalSpace)
                            {
                                ExpandGridForIngredient(ingredient, gridPos);
                            }

                            RefreshIngredientPaletteDisplay(); // Update the ingredient list to show new quantities
                            return true; // Successful placement
                        }

                        return false; // Can't place here
                    }
                }
            }

            return false; // Drop cell not found
        }

        private bool IsIngredientInUse(Ingredient ingredient)
        {
            // Check if this ingredient is currently placed on the grid
            return placedIngredients.Values.Any(p => p.ingredient == ingredient);
        }

        // Enhanced drag and drop implementation with ghost preview, audio feedback, and snapping animation completed

        #endregion

        #region Debug and Testing Methods

        [Header("Debug Options")] [SerializeField]
        private bool enableDebugMode = true;

        [SerializeField] private bool showDragPreview = true;

        /// <summary>
        /// Add demo ingredients for testing drag and drop functionality
        /// </summary>
        public void AddDemoIngredients()
        {
            if (!enableDebugMode) return;

            Debug.Log("🧪 Adding demo ingredients for testing drag and drop");

            // Try to find some ingredients from resources
            var demoIngredients = Resources.LoadAll<Ingredient>("Ingredients");

            if (demoIngredients.Length == 0)
            {
                Debug.LogWarning(
                    "No ingredients found in Resources/Ingredients. Create some Ingredient assets for testing.");
                return;
            }

            // If using inventory integration, add to inventory instead
            if (useInventoryIngredients && inventoryHolder?.Container != null)
            {
                for (int i = 0; i < Mathf.Min(5, demoIngredients.Length); i++)
                {
                    inventoryHolder.AddItem(demoIngredients[i], 10); // Add 10 of each for testing
                    Debug.Log($"Added {demoIngredients[i].ItemName} x10 to inventory");
                }
                
                LoadIngredientsFromInventory();
                Debug.Log($"Demo ingredients added to inventory. Available ingredient types: {availableIngredients.Count}");
            }
            else
            {
                // Add directly to available ingredients (legacy mode)
                availableIngredients.Clear();
                for (int i = 0; i < Mathf.Min(5, demoIngredients.Length); i++)
                {
                    availableIngredients.Add(demoIngredients[i]);
                    Debug.Log($"Added demo ingredient: {demoIngredients[i].ItemName}");
                }

                RefreshIngredientPaletteDisplay();
                Debug.Log($"Demo ingredients added. Available ingredients: {availableIngredients.Count}");
            }
        }

        /// <summary>
        /// Test drag and drop functionality
        /// </summary>
        public void TestDragAndDrop()
        {
            if (!enableDebugMode) return;

            Debug.Log("🔧 Testing drag and drop functionality:");
            Debug.Log($"- Available ingredients: {availableIngredients.Count}");
            Debug.Log($"- Grid initialized: {isGridInitialized}");
            Debug.Log($"- Current drag state: {draggedElement != null}");
            Debug.Log($"- Show drag preview: {showDragPreview}");

            if (availableIngredients.Count == 0)
            {
                Debug.LogWarning("No ingredients available. Call AddDemoIngredients() first.");
                AddDemoIngredients();
            }
        }

        /// <summary>
        /// Clear all placed ingredients for testing
        /// </summary>
        public void ClearAllIngredients()
        {
            if (!enableDebugMode) return;

            Debug.Log("🧹 Clearing all placed ingredients");

            for (int x = 0; x < TotalGridWidth; x++)
            {
                for (int y = 0; y < TotalGridHeight; y++)
                {
                    if (craftingGrid[x, y] != null && !craftingGrid[x, y].isEmpty)
                    {
                        craftingGrid[x, y].Clear();

                        var cell = gridCells[x, y];
                        if (cell != null)
                        {
                            cell.Clear();
                            cell.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                        }
                    }
                }
            }

            placedIngredients.Clear();
            RefreshIngredientPaletteDisplay();
            Debug.Log("All ingredients cleared from grid");
        }

        /// <summary>
        /// Test inventory integration functionality
        /// </summary>
        public void TestInventoryIntegration()
        {
            if (!enableDebugMode) return;

            Debug.Log("🎒 Testing inventory integration:");
            Debug.Log($"- Use inventory ingredients: {useInventoryIngredients}");
            Debug.Log($"- Consume from inventory: {consumeIngredientsFromInventory}");
            Debug.Log($"- Add results to inventory: {addResultsToInventory}");
            
            InitializeInventoryReference();
            
            if (inventoryHolder?.Container != null)
            {
                Debug.Log($"- Inventory slots: {inventoryHolder.Container.Count}");
                Debug.Log($"- Max slots: {inventoryHolder.Container.MaxSlots}");
                
                Debug.Log("- Current inventory contents:");
                foreach (var slot in inventoryHolder.Container.Slots)
                {
                    Debug.Log($"  • {slot.Item.ItemName} x{slot.Quantity}");
                }
                
                if (useInventoryIngredients)
                {
                    LoadIngredientsFromInventory();
                }
            }
            else
            {
                Debug.LogWarning("- No inventory holder found!");
            }
        }

        #endregion
    } // End of GridMinigameController class
}