using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.GridDemo;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.UI
{
    public enum IngredientSortOption
    {
        New,
        Recent,
        NameAsc,
        NameDesc,
        QuantityAsc,
        QuantityDesc
    }

    public class CraftingUIController : MonoBehaviour
    {
        [Header("References")]
        public UIDocument uiDocument;
        public GridCraftingManager gridManager;
        
        [Header("Data")]
        public List<Ingredient> availableIngredients = new List<Ingredient>();
        public List<Potion> availablePotions = new List<Potion>();
        
        [Header("Settings")]
        public int itemsPerPage = 12;
        
        // UI Elements - Main
        private VisualElement root;
        private TextField searchField;
        private DropdownField sortDropdown;
        private Button inventoryToggle;
        private VisualElement slotGrid;
        private Label pageNumber;
        private Button pageNext, pagePrev;
        private Button completeButton;
        
        // UI Elements - Left Buttons
        private Button clearButton;
        private Button inventoryButton;
        
        // UI Elements - Item Banner
        private VisualElement itemBanner;
        private Label itemName, itemDescription, rarityLabel;
        private VisualElement rarityBadge;
        
        // UI Elements - Filter System
        private List<Button> filterButtons = new List<Button>();
        private Button filterCorporeal, filterFrigid, filterScorch, filterCaustic, filterArc, filterDivine;
        
        // UI Elements - Inventory
        private VisualElement inventorySection;
        private TextField inventorySearch;
        private VisualElement inventoryGrid;
        private Label invPageNumber;
        private Button invPageNext, invPagePrev;
        
        // Filter and Sort System
        private Aspect? currentFilter = null; // null means "All"
        private IngredientSortOption currentSort = IngredientSortOption.New;
        private string currentSearchQuery = "";
        private bool showingInventory = false;
        
        // Pagination
        private int currentPage = 0;
        private int invCurrentPage = 0;
        private List<Ingredient> filteredIngredients = new List<Ingredient>();
        private List<Potion> filteredPotions = new List<Potion>();
        
        // Selection tracking
        private Ingredient selectedIngredient;
        private FourFatesStudios.ProjectWarden.GridDemo.IngredientInstance selectedInstance;
        private int currentRotation = 0; // Current rotation for preview (0, 90, 180, 270)
        
        // Star tracking
        private Dictionary<int, VisualElement> newStars = new Dictionary<int, VisualElement>();
        
        // Click handler tracking to prevent accumulation
        private Dictionary<int, System.Action> currentClickHandlers = new Dictionary<int, System.Action>();
        
        // Drag and drop system
        private VisualElement dragPreview;
        private Ingredient draggedIngredient;
        private bool isDragging = false;
        private Vector2 lastMousePosition;

        private void Start()
        {
            if (uiDocument == null)
            {
                uiDocument = FindObjectOfType<UIDocument>();
                if (uiDocument != null)
                {
                    // Debug.Log("Auto-assigned UIDocument to CraftingUIController");
                }
                else
                {
                    Debug.LogError("UIDocument not found! Make sure you have a UIDocument component in your scene.");
                }
            }
            
            if (gridManager == null)
            {
                gridManager = FindFirstObjectByType<GridCraftingManager>();
                if (gridManager != null)
                {
                    // Debug.Log("Auto-assigned GridCraftingManager to CraftingUIController");
                }
                else
                {
                    Debug.LogError("GridCraftingManager not found! Please ensure the GridMinigameController GameObject has the GridCraftingManager component.");
                    return;
                }
            }
            
            // Subscribe to grid changes to update button state
            if (gridManager != null)
            {
                gridManager.OnGridChanged += UpdateCompleteButtonState;
            }
            
            InitializeUI();
            PopulateAvailableItems();
            RefreshIngredientDisplay();
            UpdateCompleteButtonState();
        }

        public void Initialize()
        {
            Debug.Log("🔄 CraftingUIController.Initialize() called");
            
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
                if (uiDocument == null)
                {
                    Debug.LogError("CraftingUIController: UIDocument not found!");
                    return;
                }
            }
            
            if (gridManager == null)
            {
                gridManager = FindFirstObjectByType<GridCraftingManager>();
                if (gridManager == null)
                {
                    Debug.LogError("CraftingUIController: GridCraftingManager not found!");
                    return;
                }
            }
            
            InitializeUI();
            PopulateAvailableItems();
            RefreshIngredientDisplay();
            UpdateCompleteButtonState();
            
            Debug.Log("🔄 CraftingUIController initialization complete");
        }

        private void Update()
        {
            // Only handle input if this GameObject is enabled and active
            if (!gameObject.activeInHierarchy || !enabled)
                return;
            
            // Handle ESC key for back navigation
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                HandleBackNavigation();
            }
            
            // Handle R key for rotation
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                HandleRotation();
            }
        }

        private void InitializeUI()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
                if (uiDocument == null)
                {
                    Debug.LogError("CraftingUIController: UIDocument is null! Please assign the UIDocument reference in the Inspector.");
                    return;
                }
            }

            root = uiDocument.rootVisualElement;
            
            // Main UI elements
            searchField = root.Q<TextField>("search-field");
            sortDropdown = root.Q<DropdownField>("sort-dropdown");
            inventoryToggle = root.Q<Button>("inventory-toggle");
            slotGrid = root.Q<VisualElement>("slot-grid");
            pageNumber = root.Q<Label>("page-number");
            pageNext = root.Q<Button>("page-next");
            pagePrev = root.Q<Button>("page-prev");
            completeButton = root.Q<Button>("complete-button");
            
            // Left buttons
            clearButton = root.Q<Button>("clear-button");
            inventoryButton = root.Q<Button>("inventory-button");
            
            // Item banner elements
            itemBanner = root.Q<VisualElement>("item-banner");
            itemName = root.Q<Label>("item-name");
            itemDescription = root.Q<Label>("item-description");
            rarityLabel = root.Q<Label>("rarity-label");
            rarityBadge = root.Q<VisualElement>("rarity-badge");
            
            // Filter buttons
            filterCorporeal = root.Q<Button>("filter-corporeal");
            filterFrigid = root.Q<Button>("filter-frigid");
            filterScorch = root.Q<Button>("filter-scorch");
            filterCaustic = root.Q<Button>("filter-caustic");
            filterArc = root.Q<Button>("filter-arc");
            filterDivine = root.Q<Button>("filter-divine");
            
            filterButtons = new List<Button> { filterCorporeal, filterFrigid, filterScorch, filterCaustic, filterArc, filterDivine };
            
            // Inventory elements
            inventorySection = root.Q<VisualElement>("inventory-section");
            inventorySearch = root.Q<TextField>("inventory-search");
            inventoryGrid = root.Q<VisualElement>("inventory-grid");
            invPageNumber = root.Q<Label>("inv-page-number");
            invPageNext = root.Q<Button>("inv-page-next");
            invPagePrev = root.Q<Button>("inv-page-prev");
            
            // Setup sort dropdown
            SetupSortDropdown();
            
            // Setup event handlers
            SetupEventHandlers();
            
            // Initialize new stars
            InitializeNewStars();
            
            if (itemName == null || itemDescription == null || rarityLabel == null)
            {
                Debug.LogError("Critical UI elements not found!");
                return;
            }
        }

        private void SetupSortDropdown()
        {
            if (sortDropdown == null) return;
            
            var sortOptions = new List<string>
            {
                "Sort by: New",
                "Sort by: Recent", 
                "Sort by: Name A-Z",
                "Sort by: Name Z-A",
                "Sort by: Quantity ↑",
                "Sort by: Quantity ↓"
            };
            
            sortDropdown.choices = sortOptions;
            sortDropdown.value = sortOptions[0];
        }

        private void SetupEventHandlers()
        {
            // Search field
            if (searchField != null)
            {
                searchField.RegisterValueChangedCallback(evt => 
                {
                    currentSearchQuery = evt.newValue;
                    currentPage = 0;
                    RefreshIngredientDisplay();
                });
            }
            
            // Sort dropdown
            if (sortDropdown != null)
            {
                sortDropdown.RegisterValueChangedCallback(evt =>
                {
                    UpdateSortOption(evt.newValue);
                    RefreshIngredientDisplay();
                });
            }
            
            // Inventory toggle
            if (inventoryToggle != null)
            {
                inventoryToggle.clicked += ToggleInventory;
            }
            
            // Pagination
            if (pageNext != null) pageNext.clicked += () => ChangePage(1);
            if (pagePrev != null) pagePrev.clicked += () => ChangePage(-1);
            if (invPageNext != null) invPageNext.clicked += () => ChangeInventoryPage(1);
            if (invPagePrev != null) invPagePrev.clicked += () => ChangeInventoryPage(-1);
            
            // Complete button
            if (completeButton != null) completeButton.clicked += OnCompleteButtonClicked;
            
            // Left buttons
            if (clearButton != null) clearButton.clicked += OnClearButtonClicked;
            if (inventoryButton != null) inventoryButton.clicked += OnInventoryButtonClicked;
            
            // Filter buttons
            if (filterCorporeal != null) filterCorporeal.clicked += () => SetFilter(Aspect.Corporeal);
            if (filterFrigid != null) filterFrigid.clicked += () => SetFilter(Aspect.Frigid);
            if (filterScorch != null) filterScorch.clicked += () => SetFilter(Aspect.Scorch);
            if (filterCaustic != null) filterCaustic.clicked += () => SetFilter(Aspect.Caustic);
            if (filterArc != null) filterArc.clicked += () => SetFilter(Aspect.Arc);
            if (filterDivine != null) filterDivine.clicked += () => SetFilter(Aspect.Divine);
            
            // Inventory search
            if (inventorySearch != null)
            {
                inventorySearch.RegisterValueChangedCallback(evt => 
                {
                    invCurrentPage = 0;
                    RefreshInventoryDisplay();
                });
            }
        }

        private void InitializeNewStars()
        {
            newStars.Clear();
            for (int i = 0; i < itemsPerPage; i++)
            {
                var star = root.Q<VisualElement>($"new-star-{i}");
                if (star != null)
                {
                    newStars[i] = star;
                }
            }
        }

        private void UpdateSortOption(string sortText)
        {
            switch (sortText)
            {
                case "Sort by: New": currentSort = IngredientSortOption.New; break;
                case "Sort by: Recent": currentSort = IngredientSortOption.Recent; break;
                case "Sort by: Name A-Z": currentSort = IngredientSortOption.NameAsc; break;
                case "Sort by: Name Z-A": currentSort = IngredientSortOption.NameDesc; break;
                case "Sort by: Quantity ↑": currentSort = IngredientSortOption.QuantityAsc; break;
                case "Sort by: Quantity ↓": currentSort = IngredientSortOption.QuantityDesc; break;
            }
        }

        private void SetFilter(Aspect? aspect)
        {
            // Toggle filter: if same filter is clicked, clear it (show all)
            if (currentFilter == aspect)
            {
                currentFilter = null; // Clear filter to show all
            }
            else
            {
                currentFilter = aspect; // Set new filter
            }
            
            currentPage = 0;
            
            // Update button states
            foreach (var button in filterButtons)
            {
                button.RemoveFromClassList("filter-tab-active");
            }
            
            // Add active class to the selected filter (if any)
            if (currentFilter == Aspect.Corporeal && filterCorporeal != null)
                filterCorporeal.AddToClassList("filter-tab-active");
            else if (currentFilter == Aspect.Frigid && filterFrigid != null)
                filterFrigid.AddToClassList("filter-tab-active");
            else if (currentFilter == Aspect.Scorch && filterScorch != null)
                filterScorch.AddToClassList("filter-tab-active");
            else if (currentFilter == Aspect.Caustic && filterCaustic != null)
                filterCaustic.AddToClassList("filter-tab-active");
            else if (currentFilter == Aspect.Arc && filterArc != null)
                filterArc.AddToClassList("filter-tab-active");
            else if (currentFilter == Aspect.Divine && filterDivine != null)
                filterDivine.AddToClassList("filter-tab-active");
            
            RefreshIngredientDisplay();
        }

        private void ToggleInventory()
        {
            showingInventory = !showingInventory;
            
            if (inventorySection != null)
            {
                inventorySection.style.display = showingInventory ? DisplayStyle.Flex : DisplayStyle.None;
            }
            
            if (showingInventory)
            {
                RefreshInventoryDisplay();
            }
        }

        private void PopulateAvailableItems()
        {
            // Load ingredients from Resources
            var ingredients = Resources.LoadAll<Ingredient>("Items/Ingredients");
            availableIngredients = new List<Ingredient>(ingredients);
            
            // Load potions from Resources  
            var potions = Resources.LoadAll<Potion>("Items/Potions");
            availablePotions = new List<Potion>(potions);
        }

        private void RefreshIngredientDisplay()
        {
            // Filter ingredients
            filteredIngredients = FilterAndSortIngredients();
            
            // Update pagination
            int totalPages = Mathf.CeilToInt((float)filteredIngredients.Count / itemsPerPage);
            currentPage = Mathf.Clamp(currentPage, 0, Mathf.Max(0, totalPages - 1));
            
            // Update page display
            if (pageNumber != null)
            {
                pageNumber.text = totalPages > 0 ? $"{currentPage + 1} / {totalPages}" : "0 / 0";
            }
            
            // Enable/disable page buttons
            if (pagePrev != null) pagePrev.SetEnabled(currentPage > 0);
            if (pageNext != null) pageNext.SetEnabled(currentPage < totalPages - 1);
            
            // Update ingredient slots
            UpdateIngredientSlots();
        }

        private List<Ingredient> FilterAndSortIngredients()
        {
            var filtered = availableIngredients.AsEnumerable();
            
            // Apply aspect filter
            if (currentFilter.HasValue)
            {
                filtered = filtered.Where(ingredient => 
                    ingredient.IngredientAspect == currentFilter.Value);
            }
            
            // Apply search filter
            if (!string.IsNullOrEmpty(currentSearchQuery))
            {
                filtered = filtered.Where(ingredient => 
                    ingredient.name.ToLower().Contains(currentSearchQuery.ToLower()));
            }
            
            // Apply sorting
            switch (currentSort)
            {
                case IngredientSortOption.New:
                    filtered = filtered.OrderByDescending(ingredient => 
                        NewIngredientTracker.Instance.IsIngredientNew(ingredient));
                    break;
                case IngredientSortOption.Recent:
                    // For now, just reverse the order - you can implement proper recent tracking later
                    filtered = filtered.Reverse();
                    break;
                case IngredientSortOption.NameAsc:
                    filtered = filtered.OrderBy(ingredient => ingredient.name);
                    break;
                case IngredientSortOption.NameDesc:
                    filtered = filtered.OrderByDescending(ingredient => ingredient.name);
                    break;
                case IngredientSortOption.QuantityAsc:
                    // For now, just use alphabetical - you can implement quantity tracking later
                    filtered = filtered.OrderBy(ingredient => ingredient.name);
                    break;
                case IngredientSortOption.QuantityDesc:
                    // For now, just use reverse alphabetical - you can implement quantity tracking later
                    filtered = filtered.OrderByDescending(ingredient => ingredient.name);
                    break;
            }
            
            return filtered.ToList();
        }

        private void UpdateIngredientSlots()
        {
            int startIndex = currentPage * itemsPerPage;
            
            for (int i = 0; i < itemsPerPage; i++)
            {
                int ingredientIndex = startIndex + i;
                var slot = root.Q<Button>($"slot-{i}");
                
                if (slot == null) continue;
                
                if (ingredientIndex < filteredIngredients.Count)
                {
                    var ingredient = filteredIngredients[ingredientIndex];
                    
                    // Setup slot content
                    slot.text = ingredient.name.Length > 8 ? ingredient.name.Substring(0, 8) + "..." : ingredient.name;
                    slot.style.display = DisplayStyle.Flex;
                    
                    // Remove previous click handler if it exists
                    if (currentClickHandlers.ContainsKey(i))
                    {
                        slot.clicked -= currentClickHandlers[i];
                    }
                    
                    // Create and store new click handler
                    System.Action clickHandler = () => OnIngredientClicked(ingredient);
                    currentClickHandlers[i] = clickHandler;
                    slot.clicked += clickHandler;
                    
                    // Note: We've removed hover handler registration here to prevent infinite loops
                    // Hover behavior is handled elsewhere or can be added with proper cleanup if needed
                    
                    // Show/hide new star
                    if (newStars.ContainsKey(i))
                    {
                        bool isNew = NewIngredientTracker.Instance.IsIngredientNew(ingredient);
                        newStars[i].style.display = isNew ? DisplayStyle.Flex : DisplayStyle.None;
                    }
                }
                else
                {
                    // Empty slot - remove click handler if it exists
                    if (currentClickHandlers.ContainsKey(i))
                    {
                        slot.clicked -= currentClickHandlers[i];
                        currentClickHandlers.Remove(i);
                    }
                    
                    // Keep slot visible but empty to maintain the 3x4 grid layout
                    slot.text = "";
                    slot.style.display = DisplayStyle.Flex;  // Keep slots visible to maintain grid structure
                    
                    if (newStars.ContainsKey(i))
                    {
                        newStars[i].style.display = DisplayStyle.None;
                    }
                }
            }
        }

        private void RefreshInventoryDisplay()
        {
            // Filter potions based on search
            string searchQuery = inventorySearch?.value ?? "";
            filteredPotions = availablePotions.Where(potion => 
                string.IsNullOrEmpty(searchQuery) || 
                potion.name.ToLower().Contains(searchQuery.ToLower())).ToList();
            
            // Update pagination
            int totalPages = Mathf.CeilToInt((float)filteredPotions.Count / itemsPerPage);
            invCurrentPage = Mathf.Clamp(invCurrentPage, 0, Mathf.Max(0, totalPages - 1));
            
            // Update page display
            if (invPageNumber != null)
            {
                invPageNumber.text = totalPages > 0 ? $"{invCurrentPage + 1} / {totalPages}" : "0 / 0";
            }
            
            // Enable/disable page buttons
            if (invPagePrev != null) invPagePrev.SetEnabled(invCurrentPage > 0);
            if (invPageNext != null) invPageNext.SetEnabled(invCurrentPage < totalPages - 1);
            
            // Update potion slots
            UpdatePotionSlots();
        }

        private void UpdatePotionSlots()
        {
            int startIndex = invCurrentPage * itemsPerPage;
            
            for (int i = 0; i < itemsPerPage; i++)
            {
                int potionIndex = startIndex + i;
                var slot = root.Q<Button>($"inv-slot-{i}");
                
                if (slot == null) continue;
                
                if (potionIndex < filteredPotions.Count)
                {
                    var potion = filteredPotions[potionIndex];
                    
                    // Setup slot content
                    slot.text = potion.name.Length > 8 ? potion.name.Substring(0, 8) + "..." : potion.name;
                    slot.style.display = DisplayStyle.Flex;
                    
                    // Setup click handler
                    slot.clicked -= () => { }; // Remove previous handlers
                    slot.clicked += () => OnPotionClicked(potion);
                }
                else
                {
                    // Empty slot
                    slot.text = "";
                    slot.style.display = DisplayStyle.None;
                }
            }
        }

        private void OnIngredientClicked(Ingredient ingredient)
        {
            Debug.Log($"🖱️ OnIngredientClicked called with ingredient: {(ingredient != null ? ingredient.name : "NULL")}");
            Debug.Log($"   Current state - isDragging: {isDragging}, draggedIngredient: {(draggedIngredient != null ? draggedIngredient.name : "NULL")}, selectedIngredient: {(selectedIngredient != null ? selectedIngredient.name : "NULL")}");
            
            if (ingredient == null) 
            {
                Debug.LogWarning("❌ Ingredient is null, aborting");
                return;
            }
            
            // Start drag operation instead of just updating banner
            StartDragOperation(ingredient);
            
            // Still update item banner for visual feedback
            Debug.Log("Updating item banner...");
            UpdateItemBanner(ingredient.name, ingredient.ItemDescription, ingredient.ItemRarity.ToString());
            
            // Mark as viewed (removed the RefreshIngredientDisplay call that was causing infinite loop)
            NewIngredientTracker.Instance.MarkIngredientAsViewed(ingredient);
            
            Debug.Log("✅ OnIngredientClicked completed successfully.");
        }

        private void OnPotionClicked(Potion potion)
        {
            if (potion == null) return;
            
            // Update item banner
            UpdateItemBanner(potion.name, potion.ItemDescription, "Potion");
        }

        private void OnCompleteButtonClicked()
        {
            Debug.Log("Complete button clicked!");
            
            if (gridManager == null)
            {
                Debug.LogWarning("GridManager is not assigned to CraftingUIController!");
                return;
            }
            
            // Try to complete the current crafting attempt
            try
            {
                bool success = gridManager.TryCompleteCrafting();
                if (success)
                {
                    Debug.Log("Crafting completed successfully!");
                    UpdateCompleteButtonState();
                }
                else
                {
                    Debug.Log("Crafting could not be completed - no valid recipe found or ingredients missing.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during crafting completion: {e.Message}");
            }
        }
        
        private void UpdateCompleteButtonState()
        {
            if (completeButton == null || gridManager == null) return;
            
            // Enable button only if there are ingredients placed in the grid
            bool hasIngredients = gridManager.HasIngredientsPlaced();
            completeButton.SetEnabled(hasIngredients);
            
            // Update button text and styling based on state
            if (hasIngredients)
            {
                completeButton.text = "Complete";
                completeButton.RemoveFromClassList("complete-button-disabled");
                completeButton.AddToClassList("complete-button-enabled");
            }
            else
            {
                completeButton.text = "Complete";
                completeButton.AddToClassList("complete-button-disabled");
                completeButton.RemoveFromClassList("complete-button-enabled");
            }
        }

        private void ChangePage(int direction)
        {
            currentPage += direction;
            RefreshIngredientDisplay();
        }

        private void ChangeInventoryPage(int direction)
        {
            invCurrentPage += direction;
            RefreshInventoryDisplay();
        }

        public void UpdateItemBanner(string name, string description, string rarity)
        {
            if (itemName == null || itemDescription == null || rarityLabel == null)
            {
                Debug.LogError("Item banner elements are null!");
                return;
            }
            
            itemName.text = name;
            itemDescription.text = description;
            rarityLabel.text = rarity;
            
            // Update rarity badge color
            if (rarityBadge != null)
            {
                rarityBadge.ClearClassList();
                rarityBadge.AddToClassList("rarity-badge");
                rarityBadge.AddToClassList($"rarity-{rarity.ToLower()}");
            }
        }
        
        private void OnClearButtonClicked()
        {
            if (gridManager == null) return;
            
            // Check if we're in recipe mode
            bool isInRecipeMode = gridManager.IsRecipeMode(); // We'll need to add this method if it doesn't exist
            
            if (isInRecipeMode)
            {
                // Recipe mode: Clear grid and place obstacles down again for the recipe
                gridManager.ClearGridKeepObstacles();
                Debug.Log("Grid cleared - obstacles preserved for recipe mode");
            }
            else
            {
                // Normal mode: Just clear the grid completely
                gridManager.ClearGrid();
                Debug.Log("Grid cleared completely");
            }
        }
        
        private void OnInventoryButtonClicked()
        {
            // Toggle the inventory display
            ToggleInventory();
            Debug.Log("Inventory button clicked - toggling inventory display");
        }

        private void HandleBackNavigation()
        {
            Debug.Log("⬅️ ===== ESC PRESSED - Navigating back =====");
            
            // Clean up any active drag state first
            if (isDragging)
            {
                Debug.Log("⬅️ 🧹 Cleaning up active drag state");
                EndDragOperation();
            }
            
            // Clear all drag-related variables
            isDragging = false;
            draggedIngredient = null;
            selectedIngredient = null;
            currentRotation = 0;
            Debug.Log("⬅️ 🧹 All drag state cleared");
            
            // Hide the CraftingUI UIDocument (the actual crafting interface)
            var craftingUIDoc = GameObject.Find("CraftingUI");
            if (craftingUIDoc != null)
            {
                craftingUIDoc.SetActive(false);
                Debug.Log("⬅️ ✅ CraftingUI UIDocument hidden");
            }
            else
            {
                Debug.LogWarning("⬅️ ⚠️ CraftingUI UIDocument not found");
            }
            
            // Show the CraftingMenuUI first (search all objects including inactive)
            var allUIDocuments = FindObjectsByType<UnityEngine.UIElements.UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            UnityEngine.UIElements.UIDocument craftingMenuUIDoc = null;
            
            Debug.Log($"⬅️ Found {allUIDocuments.Length} total UIDocuments (including inactive)");
            foreach (var doc in allUIDocuments)
            {
                Debug.Log($"⬅️ - UIDocument: {doc.gameObject.name}, active: {doc.gameObject.activeSelf}");
                if (doc.gameObject.name == "CraftingMenuUI")
                {
                    craftingMenuUIDoc = doc;
                    break;
                }
            }
            
            if (craftingMenuUIDoc != null)
            {
                bool wasActive = craftingMenuUIDoc.gameObject.activeSelf;
                Debug.Log($"⬅️ CraftingMenuUI was active: {wasActive}");
                
                if (!wasActive)
                {
                    craftingMenuUIDoc.gameObject.SetActive(true);
                    Debug.Log($"⬅️ ✅ CraftingMenuUI activated, is now active: {craftingMenuUIDoc.gameObject.activeSelf}");
                }
                else
                {
                    Debug.Log("⬅️ CraftingMenuUI already active");
                }
            }
            else
            {
                Debug.LogWarning("⬅️ ⚠️ CraftingMenuUI not found");
            }
            
            // Manually reinitialize the CraftingModeSelector (OnEnable may not fire if component was disabled)
            var craftingModeSelector = FindFirstObjectByType<FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.CraftingModeSelector>(FindObjectsInactive.Include);
            if (craftingModeSelector != null)
            {
                Debug.Log($"⬅️ Found CraftingModeSelector on GameObject: {craftingModeSelector.gameObject.name}, active: {craftingModeSelector.gameObject.activeSelf}");
                craftingModeSelector.ReinitializeAfterReturn();
                Debug.Log($"⬅️ ✅ CraftingModeSelector reinitialized, checking CraftingMenuUI status again...");
                
                // Check if CraftingMenuUI is still active after reinitialization
                if (craftingMenuUIDoc != null)
                {
                    Debug.Log($"⬅️ After reinit: CraftingMenuUI active = {craftingMenuUIDoc.gameObject.activeSelf}");
                }
            }
            else
            {
                Debug.LogWarning("⬅️ ⚠️ CraftingModeSelector not found");
            }
            
            Debug.Log("⬅️ ===== Back navigation complete - CraftingUIController remains active =====");
        }

        private void HandleRotation()
        {
            // Only rotate if we have a selected ingredient and it's rotatable
            if (selectedIngredient == null || selectedIngredient.ShapeData == null || !selectedIngredient.ShapeData.rotatable)
            {
                if (selectedIngredient == null)
                    Debug.Log("R pressed - No ingredient selected");
                else if (selectedIngredient.ShapeData == null)
                    Debug.Log("R pressed - Selected ingredient has no shape data");
                else if (!selectedIngredient.ShapeData.rotatable)
                    Debug.Log($"R pressed - Ingredient '{selectedIngredient.name}' is not rotatable");
                return;
            }
            
            // Rotate clockwise by 90 degrees
            int previousRotation = currentRotation;
            currentRotation = IngredientRotationUtility.NormalizeRotation(currentRotation + 90);
            Debug.Log($"R pressed - Rotated ingredient '{selectedIngredient.name}' from {previousRotation}° to {currentRotation}°");
            
            // Show rotation feedback to the user
            ShowRotationFeedback();
            
            // Update any visual preview if currently dragging or hovering
            if (isDragging && dragPreview != null)
            {
                UpdateDragPreviewRotation();
            }
            
            // If we have a grid manager, update the preview there too
            if (gridManager != null)
            {
                gridManager.UpdateHoverPreview(selectedIngredient, currentRotation);
            }
        }

        private void UpdateDragPreviewRotation()
        {
            if (dragPreview == null || selectedIngredient?.ShapeData == null) return;
            
            // Get the rotated offsets
            var rotatedOffsets = IngredientRotationUtility.GetRotatedOffsets(selectedIngredient.ShapeData.occupiedOffsets, currentRotation);
            
            // Clear all children from the preview
            dragPreview.Clear();
            
            // Calculate the bounds of the rotated shape to properly size the container
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;
            
            foreach (var offset in rotatedOffsets)
            {
                minX = Mathf.Min(minX, offset.x);
                maxX = Mathf.Max(maxX, offset.x);
                minY = Mathf.Min(minY, offset.y);
                maxY = Mathf.Max(maxY, offset.y);
            }
            
            int shapeWidth = maxX - minX + 1;
            int shapeHeight = maxY - minY + 1;
            
            // Get cell size from grid
            var gridViewport = root.Q<VisualElement>("grid-viewport");
            float cellSize = 40;
            
            if (gridViewport != null)
            {
                float gridWidth = gridViewport.resolvedStyle.width;
                float gridHeight = gridViewport.resolvedStyle.height;
                
                if (gridWidth > 0 && gridHeight > 0)
                {
                    cellSize = Mathf.Min(gridWidth / 5f, gridHeight / 5f);
                }
            }
            
            // Get the current mouse position BEFORE updating the preview size
            Vector2 mousePos = Input.mousePosition;
            Vector2 uiMousePos = new Vector2(mousePos.x, Screen.height - mousePos.y);
            
            // Calculate where the (0,0) anchor cell will be in the rotated shape
            float anchorCellOffsetX = (0 - minX) * cellSize + (cellSize / 2f);
            float anchorCellOffsetY = (0 - minY) * cellSize + (cellSize / 2f);
            
            // Update container size
            dragPreview.style.width = shapeWidth * cellSize;
            dragPreview.style.height = shapeHeight * cellSize;
            
            // Reposition the preview so the mouse stays on the center of the (0,0) anchor cell
            dragPreview.style.left = uiMousePos.x - anchorCellOffsetX;
            dragPreview.style.top = uiMousePos.y - anchorCellOffsetY;
            
            // Get ingredient color
            Color aspectColor = GetIngredientColor(selectedIngredient);
            
            // Create new cells with rotated positions, normalized to start from (0,0)
            foreach (var offset in rotatedOffsets)
            {
                var cellElement = new VisualElement();
                cellElement.AddToClassList("preview-cell");
                cellElement.style.position = Position.Absolute;
                cellElement.style.left = (offset.x - minX) * cellSize;
                cellElement.style.top = (offset.y - minY) * cellSize;
                cellElement.style.width = cellSize - 4;
                cellElement.style.height = cellSize - 4;
                cellElement.style.backgroundColor = new Color(aspectColor.r, aspectColor.g, aspectColor.b, 0.8f);
                cellElement.style.borderTopWidth = 2;
                cellElement.style.borderRightWidth = 2;
                cellElement.style.borderBottomWidth = 2;
                cellElement.style.borderLeftWidth = 2;
                cellElement.style.borderTopColor = aspectColor;
                cellElement.style.borderRightColor = aspectColor;
                cellElement.style.borderBottomColor = aspectColor;
                cellElement.style.borderLeftColor = aspectColor;
                cellElement.style.borderTopLeftRadius = 4;
                cellElement.style.borderTopRightRadius = 4;
                cellElement.style.borderBottomLeftRadius = 4;
                cellElement.style.borderBottomRightRadius = 4;
                
                dragPreview.Add(cellElement);
            }
            
            // Re-add the name label if there's enough space
            int previewWidth = Mathf.RoundToInt(shapeWidth * cellSize);
            int previewHeight = Mathf.RoundToInt(shapeHeight * cellSize);
            
            if (previewWidth >= 60)
            {
                var nameLabel = new Label(selectedIngredient.name);
                nameLabel.style.color = Color.white;
                nameLabel.style.fontSize = 9;
                nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                nameLabel.style.position = Position.Absolute;
                nameLabel.style.top = previewHeight + 2;
                nameLabel.style.left = 0;
                nameLabel.style.right = 0;
                nameLabel.style.height = 12;
                nameLabel.style.backgroundColor = new Color(0, 0, 0, 0.8f);
                nameLabel.style.borderTopLeftRadius = 3;
                nameLabel.style.borderTopRightRadius = 3;
                nameLabel.style.borderBottomLeftRadius = 3;
                nameLabel.style.borderBottomRightRadius = 3;
                
                dragPreview.Add(nameLabel);
            }
        }

        private void ShowRotationFeedback()
        {
            // Update the item banner with rotation info if it exists
            var itemName = root.Q<Label>("item-name");
            if (itemName != null && selectedIngredient != null)
            {
                string rotationText = currentRotation == 0 ? "" : $" (Rotated {currentRotation}°)";
                itemName.text = selectedIngredient.name + rotationText;
            }
            
            // Also show a temporary rotation indicator
            ShowTemporaryRotationIndicator();
        }

        private void ShowTemporaryRotationIndicator()
        {
            // Create a temporary rotation indicator that fades out
            var indicator = new Label($"Rotated {currentRotation}°");
            indicator.name = "rotation-indicator";
            indicator.style.position = Position.Absolute;
            indicator.style.top = 50;
            indicator.style.left = 50;
            indicator.style.color = Color.yellow;
            indicator.style.fontSize = 16;
            indicator.style.backgroundColor = new Color(0, 0, 0, 0.7f);
            indicator.style.paddingLeft = 10;
            indicator.style.paddingRight = 10;
            indicator.style.paddingTop = 5;
            indicator.style.paddingBottom = 5;
            indicator.style.borderTopLeftRadius = 5;
            indicator.style.borderTopRightRadius = 5;
            indicator.style.borderBottomLeftRadius = 5;
            indicator.style.borderBottomRightRadius = 5;
            
            root.Add(indicator);
            
            // Remove existing rotation indicators first
            var existingIndicators = root.Query<Label>("rotation-indicator").ToList();
            foreach (var existing in existingIndicators)
            {
                if (existing != indicator)
                {
                    existing.RemoveFromHierarchy();
                }
            }
            
            // Remove after a short delay
            indicator.schedule.Execute(() => {
                if (indicator.parent != null)
                    indicator.RemoveFromHierarchy();
            }).StartingIn(1500); // 1.5 seconds
        }

        private void ShowRotationInstructions()
        {
            // Only show instructions if the ingredient is rotatable
            if (selectedIngredient?.ShapeData == null || !selectedIngredient.ShapeData.rotatable)
                return;
            
            // Remove any existing instructions
            var existingInstructions = root.Query<Label>("rotation-instructions").ToList();
            foreach (var existing in existingInstructions)
            {
                existing.RemoveFromHierarchy();
            }
            
            // Create instruction text
            var instructions = new Label("Press R to rotate");
            instructions.name = "rotation-instructions";
            instructions.style.position = Position.Absolute;
            instructions.style.bottom = 20;
            instructions.style.left = 20;
            instructions.style.color = new Color(1f, 1f, 1f, 0.8f);
            instructions.style.fontSize = 14;
            instructions.style.backgroundColor = new Color(0, 0, 0, 0.7f);
            instructions.style.paddingLeft = 10;
            instructions.style.paddingRight = 10;
            instructions.style.paddingTop = 5;
            instructions.style.paddingBottom = 5;
            instructions.style.borderTopLeftRadius = 5;
            instructions.style.borderTopRightRadius = 5;
            instructions.style.borderBottomLeftRadius = 5;
            instructions.style.borderBottomRightRadius = 5;
            
            root.Add(instructions);
            
            // Auto-remove after 3 seconds
            instructions.schedule.Execute(() => {
                if (instructions.parent != null)
                    instructions.RemoveFromHierarchy();
            }).StartingIn(3000);
        }

        // Public methods for external systems
        public void SetRecipeMode(bool recipeMode)
        {
            gridManager?.ToggleRecipeMode(recipeMode);
        }
        
        public void AddIngredient(Ingredient ingredient)
        {
            if (!availableIngredients.Contains(ingredient))
            {
                availableIngredients.Add(ingredient);
                RefreshIngredientDisplay();
            }
        }
        
        public void RemoveIngredient(Ingredient ingredient)
        {
            if (availableIngredients.Remove(ingredient))
            {
                RefreshIngredientDisplay();
            }
        }

        private int GetMaxPages()
        {
            return Mathf.Max(1, Mathf.CeilToInt((float)filteredIngredients.Count / itemsPerPage));
        }
        
        // Drag and Drop System
        private void StartDragOperation(Ingredient ingredient)
        {
            if (isDragging)
            {
                Debug.LogWarning($"❌ Cannot start drag - already dragging! isDragging={isDragging}");
                return; // Prevent multiple drags
            }
            
            Debug.Log($"🎬 Starting drag operation for: {ingredient.name}");
            Debug.Log($"   Ingredient shape: {ingredient.GridWidth}x{ingredient.GridHeight}");
            
            selectedIngredient = ingredient;
            draggedIngredient = ingredient;
            isDragging = true;
            
            // Reset rotation when starting new drag
            currentRotation = 0;
            
            // Store initial mouse position
            lastMousePosition = Input.mousePosition;
            
            CreateDragPreview(ingredient);
            
            // Show rotation instructions
            ShowRotationInstructions();
            
            // Notify grid manager about the selected ingredient
            if (gridManager != null)
            {
                gridManager.UpdateHoverPreview(ingredient, currentRotation);
            }
            
            // Register global mouse events
            root.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            root.RegisterCallback<MouseUpEvent>(OnMouseUp);
            
            // Capture mouse to ensure we receive events even outside our UI elements
            root.CaptureMouse();
        }
        
        private void CreateDragPreview(Ingredient ingredient)
        {
            // Create a container for the drag preview
            dragPreview = new VisualElement();
            dragPreview.name = "drag-preview";
            dragPreview.AddToClassList("drag-preview");
            
            Color aspectColor = GetIngredientColor(ingredient);
            
            // Get the ingredient's shape data
            bool[,] shape = ingredient.GetShape();
            int shapeWidth = shape.GetLength(0);
            int shapeHeight = shape.GetLength(1);
            
            // Calculate actual grid cell size from the grid viewport
            var gridViewport = root.Q<VisualElement>("grid-viewport");
            float cellSize = 40; // Slightly larger default fallback
            
            if (gridViewport != null)
            {
                // Wait for next frame to ensure the grid is rendered and has proper dimensions
                root.schedule.Execute(() => {
                    float gridWidth = gridViewport.resolvedStyle.width;
                    float gridHeight = gridViewport.resolvedStyle.height;
                    
                    if (gridWidth > 0 && gridHeight > 0)
                    {
                        cellSize = Mathf.Min(gridWidth / 5f, gridHeight / 5f); // Use the smaller dimension to maintain square cells
                        // Debug.Log($"Deferred grid calculation - Grid viewport size: {gridWidth}x{gridHeight}, calculated cell size: {cellSize}");
                        
                        // Recreate the preview with correct sizing
                        RecreatePreviewWithCorrectSize(ingredient, cellSize, aspectColor);
                    }
                });
                
                // For immediate display, try to get current dimensions
                float gridWidth = gridViewport.resolvedStyle.width;
                float gridHeight = gridViewport.resolvedStyle.height;
                
                if (gridWidth > 0 && gridHeight > 0)
                {
                    cellSize = Mathf.Min(gridWidth / 5f, gridHeight / 5f);
                    // Debug.Log($"Immediate grid calculation - Grid viewport size: {gridWidth}x{gridHeight}, calculated cell size: {cellSize}");
                }
                else
                {
                    // Debug.LogWarning($"Grid viewport has no size yet: {gridWidth}x{gridHeight}, using default cell size: {cellSize}");
                }
            }
            else
            {
                // Debug.LogWarning("Grid viewport not found, using default cell size");
            }
            
            // Calculate preview size based on shape and actual cell size
            int previewWidth = Mathf.RoundToInt(shapeWidth * cellSize);
            int previewHeight = Mathf.RoundToInt(shapeHeight * cellSize);
            
            CreatePreviewElements(ingredient, aspectColor, shapeWidth, shapeHeight, cellSize, previewWidth, previewHeight);
        }
        
        private void RecreatePreviewWithCorrectSize(Ingredient ingredient, float cellSize, Color aspectColor)
        {
            if (dragPreview == null) return;
            
            // Clear existing content
            dragPreview.Clear();
            
            bool[,] shape = ingredient.GetShape();
            int shapeWidth = shape.GetLength(0);
            int shapeHeight = shape.GetLength(1);
            
            int previewWidth = Mathf.RoundToInt(shapeWidth * cellSize);
            int previewHeight = Mathf.RoundToInt(shapeHeight * cellSize);
            
            // Update container size
            dragPreview.style.width = previewWidth;
            dragPreview.style.height = previewHeight;
            
            // Reposition the preview so cursor stays at center of (0,0) cell
            Vector2 mousePos = Input.mousePosition;
            Vector2 uiMousePos = new Vector2(mousePos.x, Screen.height - mousePos.y);
            float anchorOffsetX = cellSize / 2f;
            float anchorOffsetY = cellSize / 2f;
            
            dragPreview.style.left = uiMousePos.x - anchorOffsetX;
            dragPreview.style.top = uiMousePos.y - anchorOffsetY;
            
            CreatePreviewElements(ingredient, aspectColor, shapeWidth, shapeHeight, cellSize, previewWidth, previewHeight);
        }
        
        private void CreatePreviewElements(Ingredient ingredient, Color aspectColor, int shapeWidth, int shapeHeight, float cellSize, int previewWidth, int previewHeight)
        {
            bool[,] shape = ingredient.GetShape();
            
            // Style the container
            dragPreview.style.position = Position.Absolute;
            dragPreview.style.width = previewWidth;
            dragPreview.style.height = previewHeight;
            dragPreview.style.backgroundColor = Color.clear; // Transparent background
            
            Debug.Log($"Creating drag preview: {shapeWidth}x{shapeHeight} shape, {cellSize} cell size, {previewWidth}x{previewHeight} total size");
            
            // Create individual cells for the shape
            for (int x = 0; x < shapeWidth; x++)
            {
                for (int y = 0; y < shapeHeight; y++)
                {
                    if (shape[x, y]) // Only create visual elements for occupied cells
                    {
                        var cellElement = new VisualElement();
                        cellElement.style.position = Position.Absolute;
                        cellElement.style.left = x * cellSize;
                        cellElement.style.top = y * cellSize;
                        cellElement.style.width = cellSize - 4; // Slightly smaller to show cell borders clearly
                        cellElement.style.height = cellSize - 4;
                        cellElement.style.backgroundColor = new Color(aspectColor.r, aspectColor.g, aspectColor.b, 0.8f);
                        cellElement.style.borderTopWidth = 2;
                        cellElement.style.borderRightWidth = 2;
                        cellElement.style.borderBottomWidth = 2;
                        cellElement.style.borderLeftWidth = 2;
                        cellElement.style.borderTopColor = aspectColor;
                        cellElement.style.borderRightColor = aspectColor;
                        cellElement.style.borderBottomColor = aspectColor;
                        cellElement.style.borderLeftColor = aspectColor;
                        cellElement.style.borderTopLeftRadius = 4;
                        cellElement.style.borderTopRightRadius = 4;
                        cellElement.style.borderBottomLeftRadius = 4;
                        cellElement.style.borderBottomRightRadius = 4;
                        
                        dragPreview.Add(cellElement);
                    }
                }
            }
            
            // Add ingredient name label
            if (previewWidth >= 60) // Only add label if there's enough space
            {
                var nameLabel = new Label(ingredient.name);
                nameLabel.style.color = Color.white;
                nameLabel.style.fontSize = 9;
                nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                nameLabel.style.position = Position.Absolute;
                nameLabel.style.top = previewHeight + 2; // Position below the shape
                nameLabel.style.left = 0;
                nameLabel.style.right = 0;
                nameLabel.style.height = 12;
                nameLabel.style.backgroundColor = new Color(0, 0, 0, 0.8f); // Semi-transparent background
                nameLabel.style.borderTopLeftRadius = 3;
                nameLabel.style.borderTopRightRadius = 3;
                nameLabel.style.borderBottomLeftRadius = 3;
                nameLabel.style.borderBottomRightRadius = 3;
                
                dragPreview.Add(nameLabel);
            }
            
            // Make sure the preview is added to root and positioned
            if (dragPreview.parent != root)
            {
                root.Add(dragPreview);
            }
            
            // Position at mouse location using initial positioning
            InitialPositionDragPreview();
        }
        
        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!isDragging || dragPreview == null) return;
            
            // Get current mouse position
            Vector2 currentMousePosition = Input.mousePosition;
            
            // Calculate the delta (how much the mouse moved)
            Vector2 mouseDelta = currentMousePosition - lastMousePosition;
            
            // Apply the delta directly to the preview position
            // Note: In UI Toolkit, Y axis might be inverted, so we flip it
            UpdateDragPreviewPosition(mouseDelta.x, -mouseDelta.y);
            
            // Store current position for next frame
            lastMousePosition = currentMousePosition;
            
            // Check if we're over a valid drop zone (grid area)
            UpdateDropZoneHighlight(currentMousePosition);
        }
        
        private void OnMouseUp(MouseUpEvent evt)
        {
            Debug.Log($"🖱️ OnMouseUp called, isDragging={isDragging}");
            
            if (!isDragging) return;
            
            Debug.Log($"🎯 Mouse up - attempting to place ingredient at {Input.mousePosition}");
            
            // Check if we're dropping over the grid
            bool placed = AttemptPlacement(Input.mousePosition);
            
            if (placed)
            {
                Debug.Log($"✅ Successfully placed {draggedIngredient.name}");
            }
            else
            {
                Debug.LogWarning($"❌ Failed to place {draggedIngredient.name}");
            }
            
            EndDragOperation();
        }
        
        private void UpdateDragPreviewPosition(float deltaX, float deltaY)
        {
            if (dragPreview == null) return;
            
            // Get current position
            float currentLeft = dragPreview.style.left.value.value;
            float currentTop = dragPreview.style.top.value.value;
            
            // Apply the delta movement directly
            dragPreview.style.left = currentLeft + deltaX;
            dragPreview.style.top = currentTop + deltaY;
            
            // Debug.Log($"Mouse delta: ({deltaX}, {deltaY}), New preview pos: ({dragPreview.style.left.value.value}, {dragPreview.style.top.value.value})");
        }
        
        private void InitialPositionDragPreview()
        {
            if (dragPreview == null) return;
            
            Vector2 mousePos = Input.mousePosition;
            Vector2 uiMousePos = new Vector2(mousePos.x, Screen.height - mousePos.y);
            
            // Calculate cell size (same logic as in CreateDragPreview)
            var gridViewport = root.Q<VisualElement>("grid-viewport");
            float cellSize = 40;
            
            if (gridViewport != null)
            {
                float gridWidth = gridViewport.resolvedStyle.width;
                float gridHeight = gridViewport.resolvedStyle.height;
                
                if (gridWidth > 0 && gridHeight > 0)
                {
                    cellSize = Mathf.Min(gridWidth / 5f, gridHeight / 5f);
                }
            }
            
            // Position so the cursor is at the center of the (0,0) anchor cell
            float anchorOffsetX = cellSize / 2f;
            float anchorOffsetY = cellSize / 2f;
            
            dragPreview.style.left = uiMousePos.x - anchorOffsetX;
            dragPreview.style.top = uiMousePos.y - anchorOffsetY;
        }
        
        private void UpdateDropZoneHighlight(Vector2 mousePosition)
        {
            // Find the grid viewport to check if we're over it
            var gridViewport = root.Q<VisualElement>("grid-viewport");
            if (gridViewport == null) return;
            
            // Simple approach: convert screen coordinates to UI coordinates
            Vector2 uiMousePos = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
            
            // Check if mouse is over the grid area
            var gridBounds = gridViewport.worldBound;
            bool overGrid = gridBounds.Contains(uiMousePos);
            
            if (overGrid && draggedIngredient != null)
            {
                // Calculate grid position for validation
                Vector2Int gridPosition = CalculateGridPosition(mousePosition);
                bool canPlace = ValidatePlacement(draggedIngredient, gridPosition);
                
                gridViewport.AddToClassList("drop-zone-active");
                
                if (dragPreview != null)
                {
                    if (canPlace)
                    {
                        dragPreview.AddToClassList("drag-preview-valid");
                        dragPreview.RemoveFromClassList("drag-preview-invalid");
                    }
                    else
                    {
                        dragPreview.RemoveFromClassList("drag-preview-valid");
                        dragPreview.AddToClassList("drag-preview-invalid");
                    }
                }
                
                // Update individual grid tile highlights
                UpdateGridTileHighlights(gridPosition, canPlace);
            }
            else
            {
                gridViewport.RemoveFromClassList("drop-zone-active");
                if (dragPreview != null)
                {
                    dragPreview.RemoveFromClassList("drag-preview-valid");
                    dragPreview.AddToClassList("drag-preview-invalid");
                }
                
                // Clear all grid tile highlights
                ClearGridTileHighlights();
            }
        }
        
        private bool ValidatePlacement(Ingredient ingredient, Vector2Int gridPosition)
        {
            if (gridManager == null || ingredient?.ShapeData == null) return false;
            
            // Create a temporary ingredient instance for validation
            var tempInstance = new FourFatesStudios.ProjectWarden.GridDemo.IngredientInstance()
            {
                ingredient = ingredient,
                gridPosition = gridPosition,
                visualObject = null,
                placementTime = Time.time
            };
            
            // Use the enhanced validation that includes obstacle checks
            return gridManager.CanPlaceIngredient(tempInstance, gridPosition, out string reason);
        }
        
        private void UpdateGridTileHighlights(Vector2Int gridPosition, bool canPlace)
        {
            if (draggedIngredient?.ShapeData == null) return;
            
            // Clear previous highlights
            ClearGridTileHighlights();
            
            // Get occupied cells for this ingredient
            var occupiedOffsets = draggedIngredient.ShapeData.GetOccupiedOffsets();
            
            foreach (var offset in occupiedOffsets)
            {
                Vector2Int cellPos = gridPosition + offset;
                if (cellPos.x >= 0 && cellPos.x < 5 && cellPos.y >= 0 && cellPos.y < 5)
                {
                    var tile = root.Q<VisualElement>($"grid-tile-{cellPos.x}-{cellPos.y}");
                    if (tile != null)
                    {
                        if (canPlace)
                        {
                            tile.AddToClassList("drop-target-valid");
                            tile.RemoveFromClassList("drop-target-invalid");
                        }
                        else
                        {
                            tile.AddToClassList("drop-target-invalid");
                            tile.RemoveFromClassList("drop-target-valid");
                        }
                    }
                }
            }
        }
        
        private void ClearGridTileHighlights()
        {
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    var tile = root.Q<VisualElement>($"grid-tile-{x}-{y}");
                    if (tile != null)
                    {
                        tile.RemoveFromClassList("drop-target-valid");
                        tile.RemoveFromClassList("drop-target-invalid");
                    }
                }
            }
        }
        
        private Vector2Int CalculateGridPosition(Vector2 mousePosition)
        {
            var gridViewport = root.Q<VisualElement>("grid-viewport");
            if (gridViewport == null) return Vector2Int.zero;
            
            Vector2 uiMousePos = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
            
            var gridBounds = gridViewport.worldBound;
            if (!gridBounds.Contains(uiMousePos)) return Vector2Int.zero;
            
            Vector2 localPos = uiMousePos - gridBounds.position;
            float cellWidth = gridBounds.width / 5;
            float cellHeight = gridBounds.height / 5;
            
            int gridX = Mathf.FloorToInt(localPos.x / cellWidth);
            int gridY = Mathf.FloorToInt(localPos.y / cellHeight);
            
            gridX = Mathf.Clamp(gridX, 0, 4);
            gridY = Mathf.Clamp(gridY, 0, 4);
            
            Debug.Log($"🎯 CalculateGridPosition: mousePos={mousePosition}, uiMousePos={uiMousePos}, localPos={localPos}, visualGridPos=({gridX},{gridY})");
            
            return new Vector2Int(gridX, gridY);
        }
        
        private bool AttemptPlacement(Vector2 mousePosition)
        {
            Debug.Log($"🎯 AttemptPlacement called, mousePosition={mousePosition}");
            
            // Find the grid viewport and check if we're dropping over it
            var gridViewport = root.Q<VisualElement>("grid-viewport");
            if (gridViewport == null) 
            {
                Debug.LogWarning("❌ Grid viewport not found");
                return false;
            }
            
            Debug.Log("✅ Grid viewport found");
            
            // Simple approach: convert screen coordinates to UI coordinates
            Vector2 uiMousePos = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
            
            var gridBounds = gridViewport.worldBound;
            if (!gridBounds.Contains(uiMousePos))
            {
                Debug.LogWarning($"❌ Mouse not over grid area. Mouse: {uiMousePos}, Grid bounds: {gridBounds}");
                return false;
            }
            
            Debug.Log($"✅ Mouse is over grid area. Mouse: {uiMousePos}, Grid bounds: {gridBounds}");
            
            // Calculate grid position using the helper method
            Vector2Int gridPosition = CalculateGridPosition(mousePosition);
            
            Debug.Log($"🎯 Attempting to place ingredient at grid position: ({gridPosition.x}, {gridPosition.y})");
            
            // Try to place the ingredient using the existing placement system
            return PlaceIngredientInGrid(draggedIngredient, gridPosition);
        }
        
        private bool PlaceIngredientInGrid(Ingredient ingredient, Vector2Int gridPosition)
        {
            if (gridManager == null)
            {
                Debug.LogError("GridManager is not assigned to CraftingUIController! Cannot place ingredient.");
                return false;
            }
            
            try
            {
                // Use the new rotation-aware placement method
                bool success = gridManager.PlaceIngredientWithRotation(ingredient, gridPosition, currentRotation);
                
                if (success)
                {
                    Debug.Log($"Placed {ingredient.name} at {gridPosition} with {currentRotation}° rotation");
                    
                    // Update the complete button state after placement
                    UpdateCompleteButtonState();
                    
                    return true;
                }
                else
                {
                    Debug.LogWarning($"GridCraftingManager refused to place {ingredient.name} at {gridPosition} with {currentRotation}° rotation");
                    return false;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to place ingredient: {e.Message}");
                return false;
            }
        }
        
        private void EndDragOperation()
        {
            Debug.Log("🛑 EndDragOperation called");
            
            isDragging = false;
            
            // Clean up drag preview
            if (dragPreview != null)
            {
                root.Remove(dragPreview);
                dragPreview = null;
            }
            
            // Reset rotation for next use
            currentRotation = 0;
            
            // Reset item banner text to remove rotation info
            if (selectedIngredient != null)
            {
                var itemName = root.Q<Label>("item-name");
                if (itemName != null)
                {
                    itemName.text = selectedIngredient.name;
                }
            }
            
            // Clear all ingredient references
            selectedIngredient = null;
            draggedIngredient = null;
            
            // Clear rotation instructions
            var existingInstructions = root.Query<Label>("rotation-instructions").ToList();
            foreach (var existing in existingInstructions)
            {
                existing.RemoveFromHierarchy();
            }
            
            // Unregister mouse events
            root.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            root.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            root.ReleaseMouse();
            
            // Clear grid highlights
            var gridViewport = root.Q<VisualElement>("grid-viewport");
            gridViewport?.RemoveFromClassList("drop-zone-active");
            
            // Clear individual tile highlights
            ClearGridTileHighlights();
            
            draggedIngredient = null;
            
            Debug.Log("Drag operation ended");
        }
        
        private Color GetIngredientColor(Ingredient ingredient)
        {
            // Return color based on ingredient aspect
            switch (ingredient.IngredientAspect)
            {
                case Aspect.Scorch: return Color.red;
                case Aspect.Frigid: return Color.cyan;
                case Aspect.Arc: return Color.yellow;
                case Aspect.Caustic: return Color.green;
                case Aspect.Corporeal: return Color.gray;
                case Aspect.Divine: return Color.magenta;
                default: return Color.white;
            }
        }
        
        private void OnDestroy()
        {
            // Clean up any ongoing drag operation
            if (isDragging)
            {
                EndDragOperation();
            }
            
            // Unsubscribe from grid events
            if (gridManager != null)
            {
                gridManager.OnGridChanged -= UpdateCompleteButtonState;
            }
        }
    }
}