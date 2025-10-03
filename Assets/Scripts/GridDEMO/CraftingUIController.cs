using UnityEngine;
using UnityEngine.UIElements;
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

    public class IngredientInstance
    {
        public Ingredient ingredient;
        public Vector2Int gridPosition;
        public GameObject visualObject;
        public float placementTime;
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
        
        // UI Elements - Item Banner
        private VisualElement itemBanner;
        private Label itemName, itemDescription, rarityLabel;
        private VisualElement rarityBadge;
        
        // UI Elements - Filter System
        private List<Button> filterButtons = new List<Button>();
        private Button filterAll, filterCorporeal, filterFrigid, filterScorch, filterCaustic, filterArc, filterDivine;
        
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
        private IngredientInstance selectedInstance;
        
        // Star tracking
        private Dictionary<int, VisualElement> newStars = new Dictionary<int, VisualElement>();

        private void Start()
        {
            if (uiDocument == null)
            {
                uiDocument = FindObjectOfType<UIDocument>();
                Debug.Log("Auto-assigned UIDocument to CraftingUIController");
            }
            
            InitializeUI();
            PopulateAvailableItems();
            RefreshIngredientDisplay();
        }

        private void InitializeUI()
        {
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument is null! Cannot initialize UI.");
                return;
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
            
            // Item banner elements
            itemBanner = root.Q<VisualElement>("item-banner");
            itemName = root.Q<Label>("item-name");
            itemDescription = root.Q<Label>("item-description");
            rarityLabel = root.Q<Label>("rarity-label");
            rarityBadge = root.Q<VisualElement>("rarity-badge");
            
            // Filter buttons
            filterAll = root.Q<Button>("filter-all");
            filterCorporeal = root.Q<Button>("filter-corporeal");
            filterFrigid = root.Q<Button>("filter-frigid");
            filterScorch = root.Q<Button>("filter-scorch");
            filterCaustic = root.Q<Button>("filter-caustic");
            filterArc = root.Q<Button>("filter-arc");
            filterDivine = root.Q<Button>("filter-divine");
            
            filterButtons = new List<Button> { filterAll, filterCorporeal, filterFrigid, filterScorch, filterCaustic, filterArc, filterDivine };
            
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
            
            // Filter buttons
            if (filterAll != null) filterAll.clicked += () => SetFilter(null);
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
            currentFilter = aspect;
            currentPage = 0;
            
            // Update button states
            foreach (var button in filterButtons)
            {
                button.RemoveFromClassList("filter-tab-active");
            }
            
            if (aspect == null && filterAll != null)
                filterAll.AddToClassList("filter-tab-active");
            else if (aspect == Aspect.Corporeal && filterCorporeal != null)
                filterCorporeal.AddToClassList("filter-tab-active");
            else if (aspect == Aspect.Frigid && filterFrigid != null)
                filterFrigid.AddToClassList("filter-tab-active");
            else if (aspect == Aspect.Scorch && filterScorch != null)
                filterScorch.AddToClassList("filter-tab-active");
            else if (aspect == Aspect.Caustic && filterCaustic != null)
                filterCaustic.AddToClassList("filter-tab-active");
            else if (aspect == Aspect.Arc && filterArc != null)
                filterArc.AddToClassList("filter-tab-active");
            else if (aspect == Aspect.Divine && filterDivine != null)
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
                    
                    // Setup click handler
                    slot.clicked -= () => { }; // Remove previous handlers
                    slot.clicked += () => OnIngredientClicked(ingredient);
                    
                    // Setup hover handler for new ingredient tracking
                    slot.RegisterCallback<MouseEnterEvent>(evt => OnIngredientHovered(ingredient));
                    
                    // Show/hide new star
                    if (newStars.ContainsKey(i))
                    {
                        bool isNew = NewIngredientTracker.Instance.IsIngredientNew(ingredient);
                        newStars[i].style.display = isNew ? DisplayStyle.Flex : DisplayStyle.None;
                    }
                }
                else
                {
                    // Empty slot
                    slot.text = "";
                    slot.style.display = DisplayStyle.None;
                    
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
            if (ingredient == null) return;
            
            // Update item banner
            UpdateItemBanner(ingredient.name, ingredient.ItemDescription, ingredient.ItemRarity.ToString());
            
            // Mark as viewed
            NewIngredientTracker.Instance.MarkIngredientAsViewed(ingredient);
            
            // Refresh display to hide star
            RefreshIngredientDisplay();
        }

        private void OnIngredientHovered(Ingredient ingredient)
        {
            if (ingredient == null) return;
            
            // Mark as viewed when hovered
            NewIngredientTracker.Instance.MarkIngredientAsViewed(ingredient);
            
            // Refresh display to hide star
            RefreshIngredientDisplay();
        }

        private void OnPotionClicked(Potion potion)
        {
            if (potion == null) return;
            
            // Update item banner
            UpdateItemBanner(potion.name, potion.ItemDescription, "Potion");
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
    }
}