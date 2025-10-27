using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.UI;
using FourFatesStudios.ProjectWarden.GridDemo;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu
{
    /// <summary>
    /// Main crafting mode selector that presents two crafting options:
    /// 1. Free Crafting - Traditional 3x3 grid layout
    /// 2. Recipe Crafting - Select from available recipes with saved grid patterns
    /// </summary>
    public class CraftingModeSelector : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private VisualTreeAsset recipeScrollItemTemplate;
        
        [Header("Navigation")]
        [SerializeField] private bool enableDebugLogging = true;
        
        [Header("Grid Configuration")]
        [SerializeField] private Vector2Int freeCraftingGridSize = new Vector2Int(3, 3);
        
        // UI Elements
        private Button freeCraftingButton;
        private Button recipeCraftingButton;
        private Button backButton;
        private VisualElement mainModeSelection;
        private VisualElement recipeModePanel;
        private ScrollView recipeScrollView;
        private Button recipeBackButton;
        private Label selectedRecipeLabel;
        private Button confirmRecipeButton;
        private VisualElement recipePreviewContainer;
        private TextField recipeSearchField;
        private DropdownField difficultyFilter;
        private Label recipeCountLabel;
        
        // State
        private AlchemyRecipe selectedRecipe;
        private List<AlchemyRecipe> availableRecipes = new List<AlchemyRecipe>();
        private List<AlchemyRecipe> filteredRecipes = new List<AlchemyRecipe>();
        private CraftingMode currentMode = CraftingMode.None;
        private string currentSearchText = "";
        private FourFatesStudios.ProjectWarden.Enums.RecipeDifficulty currentDifficultyFilter = FourFatesStudios.ProjectWarden.Enums.RecipeDifficulty.Standard;
        
        public enum CraftingMode
        {
            None,
            Free,
            Recipe
        }
        
        void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
                
            // If UIDocument is still null, try to add it and configure it
            if (uiDocument == null)
            {
                Debug.LogWarning("🔧 UIDocument component missing on CraftingModeSelector - attempting to add and configure...");
                uiDocument = gameObject.AddComponent<UIDocument>();
                
                // Try to find and assign the UXML asset automatically
                var assets = Resources.FindObjectsOfTypeAll<VisualTreeAsset>();
                foreach (var asset in assets)
                {
                    if (asset.name == "CraftingModeSelector")
                    {
                        uiDocument.visualTreeAsset = asset;
                        Debug.Log("✅ Auto-assigned CraftingModeSelector UXML asset");
                        break;
                    }
                }
                
                // Try to find and assign the USS style sheet
                var styleSheets = Resources.FindObjectsOfTypeAll<StyleSheet>();
                foreach (var styleSheet in styleSheets)
                {
                    if (styleSheet.name == "CraftingModeSelector")
                    {
                        // In Unity 6, StyleSheets are typically set in the UXML or via Inspector
                        Debug.Log("✅ Found CraftingModeSelector USS style sheet (set via UXML)");
                        break;
                    }
                }
            }
                
            InitializeUI();
            LoadAvailableRecipes();
        }
        
        void OnDisable()
        {
            UnregisterCallbacks();
        }
        
        #region UI Initialization
        
        private void InitializeUI()
        {
            if (uiDocument == null)
            {
                Debug.LogError("🎮 CraftingModeSelector: UIDocument is null! Cannot initialize UI.");
                return;
            }
            
            var root = uiDocument.rootVisualElement;
            
            if (root == null)
            {
                Debug.LogError("🎮 CraftingModeSelector: Root VisualElement is null! UIDocument might not have a valid UXML asset assigned.");
                Debug.LogError("   Please ensure the CraftingModeSelector.uxml file is assigned to the UIDocument component.");
                return;
            }
            
            // Main mode selection elements
            mainModeSelection = root.Q<VisualElement>("main-mode-selection");
            freeCraftingButton = root.Q<Button>("free-crafting-button");
            recipeCraftingButton = root.Q<Button>("recipe-crafting-button");
            backButton = root.Q<Button>("back-button");
            
            // Recipe mode elements
            recipeModePanel = root.Q<VisualElement>("recipe-mode-panel");
            recipeScrollView = root.Q<ScrollView>("recipe-scroll-view");
            recipeBackButton = root.Q<Button>("recipe-back-button");
            selectedRecipeLabel = root.Q<Label>("selected-recipe-label");
            confirmRecipeButton = root.Q<Button>("confirm-recipe-button");
            recipePreviewContainer = root.Q<VisualElement>("recipe-preview-container");
            recipeSearchField = root.Q<TextField>("recipe-search");
            difficultyFilter = root.Q<DropdownField>("difficulty-filter");
            recipeCountLabel = root.Q<Label>("recipe-count-label");
            
            // Validate critical UI elements
            bool hasRequiredElements = freeCraftingButton != null && recipeCraftingButton != null && mainModeSelection != null;
            
            if (!hasRequiredElements)
            {
                Debug.LogError("🎮 CraftingModeSelector: Critical UI elements not found!");
                Debug.LogError($"   - Free Crafting Button: {freeCraftingButton != null}");
                Debug.LogError($"   - Recipe Crafting Button: {recipeCraftingButton != null}");
                Debug.LogError($"   - Main Mode Selection: {mainModeSelection != null}");
                Debug.LogError("   This suggests the UXML file structure doesn't match expected element names.");
                return;
            }
            
            if (enableDebugLogging)
            {
                Debug.Log("🎮 CraftingModeSelector: UI elements found successfully");
                Debug.Log($"   - Main Mode Selection: ✅");
                Debug.Log($"   - Recipe Mode Panel: {(recipeModePanel != null ? "✅" : "❌")}");
                Debug.Log($"   - Recipe Scroll View: {(recipeScrollView != null ? "✅" : "❌")}");
            }
            
            RegisterCallbacks();
            SetupDifficultyFilter();
            SetupInitialState();
            
            if (enableDebugLogging)
                Debug.Log("🎮 CraftingModeSelector initialized successfully");
        }
        
        private void RegisterCallbacks()
        {
            // First unregister any existing callbacks to prevent duplicates
            UnregisterCallbacks();
            
            // Register fresh callbacks
            freeCraftingButton?.RegisterCallback<ClickEvent>(OnFreeCraftingClicked);
            recipeCraftingButton?.RegisterCallback<ClickEvent>(OnRecipeCraftingClicked);
            backButton?.RegisterCallback<ClickEvent>(OnBackClicked);
            recipeBackButton?.RegisterCallback<ClickEvent>(OnRecipeBackClicked);
            confirmRecipeButton?.RegisterCallback<ClickEvent>(OnConfirmRecipeClicked);
            
            // Search and filter callbacks
            recipeSearchField?.RegisterCallback<ChangeEvent<string>>(OnSearchTextChanged);
            difficultyFilter?.RegisterCallback<ChangeEvent<string>>(OnDifficultyFilterChanged);
        }
        
        private void UnregisterCallbacks()
        {
            // Safely unregister all callbacks, handling null references gracefully
            try
            {
                freeCraftingButton?.UnregisterCallback<ClickEvent>(OnFreeCraftingClicked);
                recipeCraftingButton?.UnregisterCallback<ClickEvent>(OnRecipeCraftingClicked);
                backButton?.UnregisterCallback<ClickEvent>(OnBackClicked);
                recipeBackButton?.UnregisterCallback<ClickEvent>(OnRecipeBackClicked);
                confirmRecipeButton?.UnregisterCallback<ClickEvent>(OnConfirmRecipeClicked);
                
                // Search and filter callbacks
                recipeSearchField?.UnregisterCallback<ChangeEvent<string>>(OnSearchTextChanged);
                difficultyFilter?.UnregisterCallback<ChangeEvent<string>>(OnDifficultyFilterChanged);
            }
            catch (System.Exception ex)
            {
                if (enableDebugLogging)
                    Debug.LogWarning($"🔧 Warning during callback unregistration: {ex.Message}");
            }
        }
        
        private void SetupDifficultyFilter()
        {
            if (difficultyFilter != null)
            {
                try
                {
                    // Set up the dropdown choices for difficulty filter
                    var difficultyChoices = new List<string>
                    {
                        "All",
                        "Standard",
                        "Advanced", 
                        "Expert"
                    };
                    
                    difficultyFilter.choices = difficultyChoices;
                    difficultyFilter.value = "All"; // Set default value
                    
                    if (enableDebugLogging)
                        Debug.Log($"🎮 Difficulty filter setup with {difficultyChoices.Count} choices");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"❌ Error setting up difficulty filter: {ex.Message}");
                    // Disable the dropdown if setup fails
                    difficultyFilter.SetEnabled(false);
                }
            }
        }
        
        private void SetupInitialState()
        {
            ShowMainSelection();
            confirmRecipeButton?.SetEnabled(false);
            
            if (selectedRecipeLabel != null)
                selectedRecipeLabel.text = "Select a recipe to continue";
        }
        
        #endregion
        
        #region Recipe Loading
        
        private void LoadAvailableRecipes()
        {
            availableRecipes.Clear();
            
            // Get recipes from the database
            var database = AlchemyRecipeDatabase.Instance;
            if (database != null)
            {
                // Only include recipes that have custom grid data
                foreach (var recipe in database.Recipes)
                {
                    if (recipe != null && recipe.HasCustomGridData())
                    {
                        availableRecipes.Add(recipe);
                    }
                }
                
                if (enableDebugLogging)
                    Debug.Log($"📜 Loaded {availableRecipes.Count} recipes with custom grid patterns");
            }
            else
            {
                Debug.LogWarning("📜 AlchemyRecipeDatabase not found - recipe crafting will be limited");
            }
            
            PopulateRecipeScrollView();
        }
        
        private void FilterRecipes()
        {
            filteredRecipes.Clear();
            
            foreach (var recipe in availableRecipes)
            {
                bool matchesSearch = string.IsNullOrEmpty(currentSearchText) || 
                                   recipe.ItemName.ToLower().Contains(currentSearchText.ToLower());
                
                bool matchesDifficulty = currentDifficultyFilter == FourFatesStudios.ProjectWarden.Enums.RecipeDifficulty.Standard || // "All" option
                                        recipe.Difficulty.ToString() == currentDifficultyFilter.ToString();
                
                if (matchesSearch && matchesDifficulty)
                {
                    filteredRecipes.Add(recipe);
                }
            }
            
            if (enableDebugLogging)
                Debug.Log($"📜 Filtered recipes: {filteredRecipes.Count}/{availableRecipes.Count} recipes match criteria");
                
            PopulateRecipeScrollView();
        }
        
        private void OnSearchTextChanged(ChangeEvent<string> evt)
        {
            currentSearchText = evt.newValue;
            FilterRecipes();
        }
        
        private void OnDifficultyFilterChanged(ChangeEvent<string> evt)
        {
            if (evt.newValue == "All")
            {
                currentDifficultyFilter = FourFatesStudios.ProjectWarden.Enums.RecipeDifficulty.Standard; // Use Standard as "All"
            }
            else if (System.Enum.TryParse<FourFatesStudios.ProjectWarden.Enums.RecipeDifficulty>(evt.newValue, out var difficulty))
            {
                currentDifficultyFilter = difficulty;
            }
            else
            {
                currentDifficultyFilter = FourFatesStudios.ProjectWarden.Enums.RecipeDifficulty.Standard; // Default to "All"
            }
            
            if (enableDebugLogging)
                Debug.Log($"🎮 Difficulty filter changed to: {evt.newValue} (enum: {currentDifficultyFilter})");
                
            FilterRecipes();
        }
        
        private void PopulateRecipeScrollView()
        {
            if (recipeScrollView == null) return;
            
            recipeScrollView.Clear();
            
            // Use filtered recipes if any filters are applied, otherwise use all recipes
            var recipesToShow = filteredRecipes.Count > 0 || !string.IsNullOrEmpty(currentSearchText) || currentDifficultyFilter != FourFatesStudios.ProjectWarden.Enums.RecipeDifficulty.Standard
                ? filteredRecipes 
                : availableRecipes;
            
            foreach (var recipe in recipesToShow)
            {
                var recipeItem = CreateRecipeScrollItem(recipe);
                if (recipeItem != null)
                {
                    recipeScrollView.Add(recipeItem);
                }
            }
            
            // Update recipe count label
            if (recipeCountLabel != null)
            {
                string countText = recipesToShow.Count == availableRecipes.Count 
                    ? $"{recipesToShow.Count} recipes available"
                    : $"{recipesToShow.Count} of {availableRecipes.Count} recipes shown";
                recipeCountLabel.text = countText;
            }
            
            if (enableDebugLogging)
                Debug.Log($"📜 Populated recipe scroll view with {recipesToShow.Count} items");
        }
        
        private VisualElement CreateRecipeScrollItem(AlchemyRecipe recipe)
        {
            // Create a recipe item with enhanced styling
            var item = new VisualElement();
            item.AddToClassList("recipe-scroll-item");
            
            // Add difficulty-based styling
            string difficultyName = recipe.Difficulty.ToString();
            switch (difficultyName)
            {
                case "Beginner":
                    item.AddToClassList("difficulty-beginner");
                    break;
                case "Advanced":
                    item.AddToClassList("difficulty-advanced");
                    break;
                case "Master":
                    item.AddToClassList("difficulty-master");
                    break;
                default:
                    item.AddToClassList("difficulty-standard");
                    break;
            }
            
            // Recipe name
            var nameLabel = new Label(recipe.ItemName);
            nameLabel.AddToClassList("recipe-name");
            item.Add(nameLabel);
            
            // Recipe description/difficulty
            var difficultyLabel = new Label($"Difficulty: {recipe.Difficulty}");
            difficultyLabel.AddToClassList("recipe-difficulty");
            item.Add(difficultyLabel);
            
            // Grid info
            if (recipe.HasCustomGridData())
            {
                var gridInfo = recipe.GetGridInfo();
                var gridLabel = new Label($"Grid: {gridInfo.gridWidth}x{gridInfo.gridHeight} ({gridInfo.obstacleCount} obstacles)");
                gridLabel.AddToClassList("recipe-grid-info");
                item.Add(gridLabel);
            }
            
            // Output information
            if (recipe.OutputPotion != null)
            {
                var outputLabel = new Label($"Creates: {recipe.OutputPotion.ItemName} x{recipe.OutputQuantity}");
                outputLabel.AddToClassList("recipe-output-info");
                item.Add(outputLabel);
            }
            
            // Click handler
            item.RegisterCallback<ClickEvent>(evt => OnRecipeSelected(recipe));
            
            // Hover effects
            item.RegisterCallback<MouseEnterEvent>(evt => item.AddToClassList("recipe-item-hovered"));
            item.RegisterCallback<MouseLeaveEvent>(evt => item.RemoveFromClassList("recipe-item-hovered"));
            
            return item;
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnFreeCraftingClicked(ClickEvent evt)
        {
            if (enableDebugLogging)
                Debug.Log("🎮 Free Crafting mode selected");
                
            currentMode = CraftingMode.Free;
            StartFreeCrafting();
        }
        
        private void OnRecipeCraftingClicked(ClickEvent evt)
        {
            if (enableDebugLogging)
            {
                Debug.Log("📜 Recipe Crafting mode selected");
                Debug.Log($"📜 UI Elements Status - mainModeSelection: {mainModeSelection != null}, recipeModePanel: {recipeModePanel != null}");
                Debug.Log($"📜 Available recipes: {availableRecipes.Count}");
            }
                
            currentMode = CraftingMode.Recipe;
            ShowRecipeSelection();
        }
        
        private void OnBackClicked(ClickEvent evt)
        {
            if (enableDebugLogging)
                Debug.Log("🔙 Back to main menu");
                
            NavigateToMainMenu();
        }
        
        private void OnRecipeBackClicked(ClickEvent evt)
        {
            if (enableDebugLogging)
                Debug.Log("🔙 Back to mode selection");
                
            ShowMainSelection();
        }
        
        private void OnRecipeSelected(AlchemyRecipe recipe)
        {
            selectedRecipe = recipe;
            
            if (selectedRecipeLabel != null)
                selectedRecipeLabel.text = $"Selected: {recipe.ItemName}";
                
            confirmRecipeButton?.SetEnabled(true);
            
            ShowRecipePreview(recipe);
            
            if (enableDebugLogging)
                Debug.Log($"📜 Recipe selected: {recipe.ItemName}");
        }
        
        private void OnConfirmRecipeClicked(ClickEvent evt)
        {
            if (selectedRecipe == null)
            {
                Debug.LogWarning("📜 No recipe selected");
                return;
            }
            
            if (enableDebugLogging)
                Debug.Log($"📜 Confirming recipe: {selectedRecipe.ItemName}");
                
            StartRecipeCrafting(selectedRecipe);
        }
        
        #endregion
        
        #region Navigation Methods
        
        private void ShowMainSelection()
        {
            if (mainModeSelection != null)
                mainModeSelection.style.display = DisplayStyle.Flex;
            if (recipeModePanel != null)
                recipeModePanel.style.display = DisplayStyle.None;
            currentMode = CraftingMode.None;
            
            if (enableDebugLogging)
                Debug.Log("📜 ShowMainSelection completed - showing main mode selection");
        }
        
        private void ShowRecipeSelection()
        {
            if (enableDebugLogging)
            {
                Debug.Log("📜 ShowRecipeSelection called");
                Debug.Log($"📜 mainModeSelection null?: {mainModeSelection == null}");
                Debug.Log($"📜 recipeModePanel null?: {recipeModePanel == null}");
                Debug.Log($"📜 recipeScrollView null?: {recipeScrollView == null}");
            }
            
            // Use style.display instead of SetDisplayed extension method
            if (mainModeSelection != null)
                mainModeSelection.style.display = DisplayStyle.None;
            if (recipeModePanel != null)
                recipeModePanel.style.display = DisplayStyle.Flex;
            
            if (enableDebugLogging)
            {
                Debug.Log("📜 Panel visibility updated using style.display");
            }
            
            // Clear selection
            selectedRecipe = null;
            confirmRecipeButton?.SetEnabled(false);
            if (selectedRecipeLabel != null)
                selectedRecipeLabel.text = "Select a recipe to continue";
                
            ClearRecipePreview();
            
            if (enableDebugLogging)
            {
                Debug.Log("📜 ShowRecipeSelection completed");
            }
        }
        
        private void ShowRecipePreview(AlchemyRecipe recipe)
        {
            if (recipePreviewContainer == null) return;
            
            recipePreviewContainer.Clear();
            
            // Create a simple preview showing recipe info
            var previewTitle = new Label("Recipe Preview");
            previewTitle.AddToClassList("preview-title");
            recipePreviewContainer.Add(previewTitle);
            
            // Recipe details
            var detailsContainer = new VisualElement();
            detailsContainer.AddToClassList("recipe-details");
            
            detailsContainer.Add(new Label($"Name: {recipe.ItemName}"));
            detailsContainer.Add(new Label($"Difficulty: {recipe.Difficulty}"));
            
            if (recipe.OutputPotion != null)
                detailsContainer.Add(new Label($"Output: {recipe.OutputPotion.ItemName}"));
                
            // Grid pattern info
            if (recipe.HasCustomGridData())
            {
                var gridInfo = recipe.GetGridInfo();
                detailsContainer.Add(new Label($"Grid Size: {gridInfo.gridWidth}x{gridInfo.gridHeight}"));
                detailsContainer.Add(new Label($"Obstacles: {gridInfo.obstacleCount}"));
            }
            
            recipePreviewContainer.Add(detailsContainer);
        }
        
        private void ClearRecipePreview()
        {
            recipePreviewContainer?.Clear();
        }
        
        #endregion
        
        #region Crafting Mode Launching
        
        private void StartFreeCrafting()
        {
            if (enableDebugLogging)
                Debug.Log("🎮 Starting Free Crafting Mode");
            
            // Hide the mode selector UI
            if (uiDocument != null)
            {
                uiDocument.gameObject.SetActive(false);
            }
            
            // Launch the grid minigame with free crafting configuration
            var gridGameManager = FindFirstObjectByType<FourFatesStudios.ProjectWarden.GridDemo.GridGameManager>();
            if (gridGameManager != null)
            {
                // Configure for free crafting (3x3 grid, no preset obstacles)
                SetupFreeCraftingGrid(gridGameManager);
                
                if (enableDebugLogging)
                    Debug.Log("🎮 Free crafting grid configured successfully");
            }
            else
            {
                Debug.LogError("🎮 GridGameManager not found - cannot start free crafting");
            }
        }
        
        private void StartRecipeCrafting(AlchemyRecipe recipe)
        {
            if (enableDebugLogging)
                Debug.Log($"📜 Starting Recipe Crafting Mode with recipe: {recipe.ItemName}");
            
            // Hide the mode selector UI
            if (uiDocument != null)
            {
                uiDocument.gameObject.SetActive(false);
            }
            
            // Also trigger the GridDemoUIController to start
            GridDemoUIController.StartGridUI();
            
            // Launch the grid minigame with recipe configuration
            var gridGameManager = FindFirstObjectByType<GridGameManager>();
            if (gridGameManager != null)
            {
                // Configure for recipe crafting (use saved grid pattern)
                SetupRecipeCraftingGrid(gridGameManager, recipe);
                
                if (enableDebugLogging)
                    Debug.Log("📜 Recipe crafting grid configured successfully");
            }
            else
            {
                Debug.LogError("📜 GridGameManager not found - cannot start recipe crafting");
            }
        }
        
        private void SetupFreeCraftingGrid(FourFatesStudios.ProjectWarden.GridDemo.GridGameManager gridManager)
        {
            if (enableDebugLogging)
                Debug.Log($"🎮 Configuring grid for free crafting: {freeCraftingGridSize.x}x{freeCraftingGridSize.y}");
            
            // Store the old size
            int oldWidth = gridManager.gridWidth;
            int oldHeight = gridManager.gridHeight;
            
            // Set grid size for free crafting (3x3)
            gridManager.gridWidth = freeCraftingGridSize.x;
            gridManager.gridHeight = freeCraftingGridSize.y;
            
            // Disable obstacles for free crafting
            gridManager.enableObstacles = false;
            
            // Force the grid to recreate if size changed
            if (oldWidth != gridManager.gridWidth || oldHeight != gridManager.gridHeight)
            {
                ForceGridRecreation(gridManager);
            }
            
            // Now clear the grid after recreation
            gridManager.ClearGrid();
            
            // Center camera on the new grid
            gridManager.CenterCameraOnGrid();
            
            if (enableDebugLogging)
                Debug.Log($"🎮 Free crafting grid configured: {freeCraftingGridSize.x}x{freeCraftingGridSize.y}, obstacles disabled");
        }
        
        private void SetupRecipeCraftingGrid(FourFatesStudios.ProjectWarden.GridDemo.GridGameManager gridManager, AlchemyRecipe recipe)
        {
            if (enableDebugLogging)
                Debug.Log($"📜 Configuring grid for recipe crafting with {recipe.ItemName}");
            
            // Store the old size
            int oldWidth = gridManager.gridWidth;
            int oldHeight = gridManager.gridHeight;
            
            // Load the recipe's custom grid pattern
            if (recipe.HasCustomGridData())
            {
                var gridInfo = recipe.GetGridInfo();
                
                // Set grid size from recipe (typically 5x5 for recipes)
                gridManager.gridWidth = gridInfo.gridWidth;
                gridManager.gridHeight = gridInfo.gridHeight;
                
                if (enableDebugLogging)
                    Debug.Log($"📜 Recipe grid configured: {gridInfo.gridWidth}x{gridInfo.gridHeight} with {gridInfo.obstacleCount} obstacles");
            }
            else
            {
                // Default recipe grid size if no custom data
                gridManager.gridWidth = 5;
                gridManager.gridHeight = 5;
                
                if (enableDebugLogging)
                    Debug.Log("📜 Using default recipe grid: 5x5 with obstacles enabled");
            }
            
            // Enable obstacles for recipe crafting
            gridManager.enableObstacles = true;
            
            // Force the grid to recreate if size changed
            if (oldWidth != gridManager.gridWidth || oldHeight != gridManager.gridHeight)
            {
                ForceGridRecreation(gridManager);
            }
            
            // Now clear the grid after recreation
            gridManager.ClearGrid();
            
            // CRITICAL FIX: Clear existing obstacles before loading recipe obstacles
            ClearExistingObstacles(gridManager);

            // Load the specific obstacle pattern from the recipe
            LoadRecipeObstaclePattern(gridManager, recipe);

            // DO NOT spawn random obstacles for recipe crafting - they would overwrite recipe obstacles!
            // Random obstacles are only for free crafting mode
            if (enableDebugLogging)
                Debug.Log("📜 Skipping random obstacle spawn for recipe crafting - using recipe-defined obstacles only");

            // Center camera on the new grid
            gridManager.CenterCameraOnGrid();
        }
        
        /// <summary>
        /// Force the grid to recreate its cells and visualization
        /// </summary>
        private void ForceGridRecreation(FourFatesStudios.ProjectWarden.GridDemo.GridGameManager gridManager)
        {
            if (enableDebugLogging)
                Debug.Log($"🔄 Force recreating grid with new size: {gridManager.gridWidth}x{gridManager.gridHeight}");
            
            // Use the new public method to force grid recreation
            gridManager.ForceRecreateGrid();
            
            if (enableDebugLogging)
                Debug.Log("🔄 Grid recreation completed successfully");
        }
        
        /// <summary>
        /// Clear existing obstacles from the grid manager to prepare for recipe obstacles
        /// </summary>
        private void ClearExistingObstacles(FourFatesStudios.ProjectWarden.GridDemo.GridGameManager gridManager)
        {
            if (enableDebugLogging)
                Debug.Log("🧹 Clearing existing obstacles before loading recipe obstacles");

            // Clear obstacles directly - aspectObstacles is a public field
            int clearedCount = gridManager.aspectObstacles.Count;
            gridManager.aspectObstacles.Clear();
                    
            if (enableDebugLogging)
                Debug.Log($"🧹 Cleared {clearedCount} existing obstacles");
        }
        
        private void LoadRecipeObstaclePattern(FourFatesStudios.ProjectWarden.GridDemo.GridGameManager gridManager, AlchemyRecipe recipe)
        {
            if (enableDebugLogging)
                Debug.Log($"📜 Loading obstacle pattern for recipe: {recipe.ItemName}");
            
            if (recipe.HasCustomGridData())
            {
                // Get the custom grid cells from the recipe
                var customCells = recipe.CustomGridCells;
                
                if (enableDebugLogging)
                    Debug.Log($"📜 Recipe has {customCells.Count} custom grid cells");
                
                // Filter cells that have obstacles
                var obstacleCells = customCells.Where(cell => cell.hasObstacle).ToList();
                
                if (obstacleCells.Count > 0)
                {
                    if (enableDebugLogging)
                        Debug.Log($"📜 Found {obstacleCells.Count} obstacle cells to load");
                    
                    foreach (var cell in obstacleCells)
                    {
                        // Get the obstacle position
                        var obstaclePosition = cell.position;
                        
                        // Verify position is within grid bounds
                        if (obstaclePosition.x >= 0 && obstaclePosition.x < gridManager.gridWidth &&
                            obstaclePosition.y >= 0 && obstaclePosition.y < gridManager.gridHeight)
                        {
                            // Use the obstacle type from the recipe cell
                            var obstacleType = cell.obstacleType;
                            
                            // Create the obstacle and add it to the grid directly
                            var obstacle = new FourFatesStudios.ProjectWarden.GridDemo.AspectObstacle(obstacleType, obstaclePosition);
                            gridManager.aspectObstacles.Add(obstacle);
                            
                            // Also set the obstacle on the grid cell so it knows it has an obstacle
                            var gridCell = gridManager.GetCell(obstaclePosition.x, obstaclePosition.y);
                            if (gridCell != null)
                            {
                                gridCell.SetObstacle(obstacle);
                            }
                            
                            if (enableDebugLogging)
                                Debug.Log($"📜 Added {obstacleType} obstacle at position ({obstaclePosition.x}, {obstaclePosition.y})");
                        }
                        else
                        {
                            Debug.LogWarning($"📜 Obstacle position ({obstaclePosition.x}, {obstaclePosition.y}) is outside grid bounds ({gridManager.gridWidth}x{gridManager.gridHeight})");
                        }
                    }
                    
                    // Refresh the grid visualization to show the new obstacles
                    var visualizer = gridManager.GetComponent<FourFatesStudios.ProjectWarden.GridDemo.GridVisualizer>();
                    if (visualizer != null)
                    {
                        visualizer.RefreshGrid();
                        if (enableDebugLogging)
                            Debug.Log("📜 Grid visualization refreshed to show recipe obstacles");
                    }
                    else
                    {
                        Debug.LogWarning("📜 Grid visualizer not found - obstacles may not be visible");
                    }
                    
                    if (enableDebugLogging)
                        Debug.Log($"✅ Successfully loaded {obstacleCells.Count} obstacles from recipe {recipe.ItemName}");
                }
                else
                {
                    if (enableDebugLogging)
                        Debug.Log($"📜 Recipe {recipe.ItemName} has no obstacle data");
                }
            }
            else
            {
                if (enableDebugLogging)
                    Debug.Log($"📜 Recipe {recipe.ItemName} has no custom grid data");
            }
        }
        
        #endregion
        
        #region Navigation Integration
        
        private void NavigateToMainMenu()
        {
            var navigationController = FindFirstObjectByType<CraftingNavigationController>();
            if (navigationController != null)
            {
                navigationController.ShowMainMenu();
            }
            else
            {
                Debug.LogWarning("🔙 CraftingNavigationController not found - cannot navigate to main menu");
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Get the currently selected crafting mode
        /// </summary>
        public CraftingMode GetCurrentMode()
        {
            return currentMode;
        }
        
        /// <summary>
        /// Get the currently selected recipe (if in recipe mode)
        /// </summary>
        public AlchemyRecipe GetSelectedRecipe()
        {
            return selectedRecipe;
        }
        
        /// <summary>
        /// Force refresh the available recipes list
        /// </summary>
        public void RefreshRecipes()
        {
            LoadAvailableRecipes();
        }
        
        /// <summary>
        /// Set the free crafting grid size
        /// </summary>
        public void SetFreeCraftingGridSize(Vector2Int size)
        {
            freeCraftingGridSize = size;
            
            if (enableDebugLogging)
                Debug.Log($"🎮 Free crafting grid size set to: {size.x}x{size.y}");
        }
        
        /// <summary>
        /// Show the mode selector UI again (useful for returning from grid)
        /// </summary>
        public void ShowModeSelector()
        {
            if (uiDocument != null)
            {
                uiDocument.gameObject.SetActive(true);
                
                // Re-initialize UI to ensure callbacks are properly registered
                InitializeUI();
                
                // Only reload recipes if we don't have any loaded yet
                if (availableRecipes.Count == 0)
                {
                    LoadAvailableRecipes();
                }
                
                if (enableDebugLogging)
                    Debug.Log("🎮 Mode selector UI shown and re-initialized");
            }
        }
        
        /// <summary>
        /// Hide the mode selector UI
        /// </summary>
        public void HideModeSelector()
        {
            if (uiDocument != null)
            {
                uiDocument.gameObject.SetActive(false);
                
                if (enableDebugLogging)
                    Debug.Log("🎮 Mode selector UI hidden");
            }
        }
        
        /// <summary>
        /// Debug method to check the current state of UI elements and callbacks
        /// </summary>
        [ContextMenu("Debug UI State")]
        public void DebugUIState()
        {
            Debug.Log("🔍 === CraftingModeSelector UI State Debug ===");
            Debug.Log($"UIDocument: {uiDocument != null}");
            Debug.Log($"UIDocument Active: {uiDocument?.gameObject.activeSelf}");
            Debug.Log($"Root Element: {uiDocument?.rootVisualElement != null}");
            
            Debug.Log("--- Button References ---");
            Debug.Log($"Free Crafting Button: {freeCraftingButton != null}");
            Debug.Log($"Recipe Crafting Button: {recipeCraftingButton != null}");
            Debug.Log($"Back Button: {backButton != null}");
            Debug.Log($"Recipe Back Button: {recipeBackButton != null}");
            Debug.Log($"Confirm Recipe Button: {confirmRecipeButton != null}");
            
            Debug.Log("--- Panel References ---");
            Debug.Log($"Main Mode Selection: {mainModeSelection != null}");
            Debug.Log($"Recipe Mode Panel: {recipeModePanel != null}");
            Debug.Log($"Recipe Scroll View: {recipeScrollView != null}");
            
            if (mainModeSelection != null)
                Debug.Log($"Main Mode Selection Display: {mainModeSelection.style.display}");
            if (recipeModePanel != null)
                Debug.Log($"Recipe Mode Panel Display: {recipeModePanel.style.display}");
            
            Debug.Log($"Current Mode: {currentMode}");
            Debug.Log($"Selected Recipe: {selectedRecipe?.ItemName ?? "None"}");
            Debug.Log($"Available Recipes: {availableRecipes.Count}");
            Debug.Log("🔍 === End Debug ===");
        }
        
        /// <summary>
        /// Test method to manually trigger recipe crafting button for debugging
        /// </summary>
        [ContextMenu("Test Recipe Crafting Button")]
        public void TestRecipeCraftingButton()
        {
            Debug.Log("🧪 Testing Recipe Crafting Button manually...");
            OnRecipeCraftingClicked(null);
        }
        
        /// <summary>
        /// Manual setup method to configure UIDocument if missing
        /// </summary>
        [ContextMenu("Force Setup UIDocument")]
        public void ForceSetupUIDocument()
        {
            Debug.Log("🔧 Force setup UIDocument triggered...");
            
            if (uiDocument == null)
            {
                Debug.Log("🔧 UIDocument is null - attempting to add and configure...");
                uiDocument = gameObject.AddComponent<UIDocument>();
                
                // Try to find and assign the UXML asset automatically
                var assets = Resources.FindObjectsOfTypeAll<VisualTreeAsset>();
                VisualTreeAsset foundAsset = null;
                foreach (var asset in assets)
                {
                    Debug.Log($"🔍 Found UXML asset: {asset.name}");
                    if (asset.name == "CraftingModeSelector")
                    {
                        foundAsset = asset;
                        uiDocument.visualTreeAsset = asset;
                        Debug.Log("✅ Auto-assigned CraftingModeSelector UXML asset");
                        break;
                    }
                }
                
                if (foundAsset == null)
                {
                    Debug.LogError("❌ Could not find CraftingModeSelector UXML asset!");
                    foreach (var asset in assets)
                    {
                        Debug.Log($"   Available UXML: {asset.name}");
                    }
                }
                
                // Try to find and assign the USS style sheet
                var styleSheets = Resources.FindObjectsOfTypeAll<StyleSheet>();
                StyleSheet foundStyleSheet = null;
                foreach (var styleSheet in styleSheets)
                {
                    Debug.Log($"🔍 Found USS asset: {styleSheet.name}");
                    if (styleSheet.name == "CraftingModeSelector")
                    {
                        foundStyleSheet = styleSheet;
                        // In Unity 6, StyleSheets are typically set in the UXML or via Inspector
                        Debug.Log("✅ Found CraftingModeSelector USS style sheet (style is set via UXML)");
                        break;
                    }
                }
                
                if (foundStyleSheet == null)
                {
                    Debug.LogWarning("⚠️ Could not find CraftingModeSelector USS style sheet");
                    foreach (var styleSheet in styleSheets)
                    {
                        Debug.Log($"   Available USS: {styleSheet.name}");
                    }
                }
            }
            else
            {
                Debug.Log("🔧 UIDocument already exists");
                Debug.Log($"   UXML Asset: {uiDocument.visualTreeAsset?.name ?? "None"}");
                Debug.Log($"   StyleSheets are managed via UXML in Unity 6");
            }
            
            // Force re-initialization
            enabled = false;
            enabled = true;
            
            Debug.Log("🔄 Force setup completed and component re-enabled");
        }
        
        #endregion
    }
}