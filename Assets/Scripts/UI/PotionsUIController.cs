using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.Characters.Components;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.UI
{
    /// <summary>
    /// Controls the Potions UI Document, displaying completed potions from the inventory
    /// Features filtering, searching, sorting, and detailed potion information
    /// </summary>
    public class PotionsUIController : MonoBehaviour
    {
        [Header("UI Document")]
        [SerializeField] private UIDocument uiDocument;
        
        [Header("Data Source")]
        [SerializeField] private InventoryComponent inventoryComponent;
        
        [Header("Configuration")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool autoRefreshOnEnable = true;
        
        // UI Elements
        private VisualElement root;
        private Label titleLabel;
        private Label totalCountLabel;
        private Label categoryCountLabel;
        private TextField searchField;
        private DropdownField categoryFilter;
        private DropdownField sortDropdown;
        private ListView potionsList;
        private VisualElement potionDetails;
        private Label detailsTitle;
        private VisualElement potionIcon;
        private Label potionName;
        private Label potionDescription;
        private VisualElement effectsList;
        private VisualElement ingredientsList;
        
        // Data Management
        private List<Potion> allPotions = new List<Potion>();
        private List<Potion> filteredPotions = new List<Potion>();
        private Potion selectedPotion;
        
        // Filter and Sort Options
        private string currentSearchText = "";
        private string currentCategoryFilter = "All";
        private SortOption currentSortOption = SortOption.Name;
        
        public enum SortOption
        {
            Name,
            Category,
            DateCreated,
            Rarity
        }
        
        // Events
        public event Action<Potion> OnPotionSelected;
        public event Action<List<Potion>> OnPotionsFiltered;
        
        private void Awake()
        {
            InitializeUIElements();
        }
        
        private void OnEnable()
        {
            if (autoRefreshOnEnable)
            {
                RefreshPotionsList();
            }
        }
        
        private void InitializeUIElements()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();
                
            if (uiDocument == null)
            {
                Debug.LogError("❌ No UIDocument found on PotionsUIController");
                return;
            }
            
            root = uiDocument.rootVisualElement;
            
            // Get UI elements
            titleLabel = root.Q<Label>("potions-title");
            totalCountLabel = root.Q<Label>("total-count");
            categoryCountLabel = root.Q<Label>("category-count");
            searchField = root.Q<TextField>("search-field");
            categoryFilter = root.Q<DropdownField>("category-filter");
            sortDropdown = root.Q<DropdownField>("sort-dropdown");
            potionsList = root.Q<ListView>("potions-list");
            potionDetails = root.Q<VisualElement>("potion-details");
            detailsTitle = root.Q<Label>("details-title");
            potionIcon = root.Q<VisualElement>("potion-icon");
            potionName = root.Q<Label>("potion-name");
            potionDescription = root.Q<Label>("potion-description");
            effectsList = root.Q<VisualElement>("effects-list");
            ingredientsList = root.Q<VisualElement>("ingredients-list");
            
            SetupUIControls();
            SetupListView();
            
            if (enableDebugLogging)
                Debug.Log("✅ PotionsUIController initialized successfully");
        }
        
        private void SetupUIControls()
        {
            // Setup search field
            if (searchField != null)
            {
                searchField.RegisterValueChangedCallback(OnSearchChanged);
            }
            
            // Setup category filter
            if (categoryFilter != null)
            {
                categoryFilter.choices = new List<string> { "All", "Healing", "Damage", "Buff", "Utility", "Rare" };
                categoryFilter.value = "All";
                categoryFilter.RegisterValueChangedCallback(OnCategoryFilterChanged);
            }
            
            // Setup sort dropdown
            if (sortDropdown != null)
            {
                sortDropdown.choices = new List<string> { "Name", "Category", "Date Created", "Rarity" };
                sortDropdown.value = "Name";
                sortDropdown.RegisterValueChangedCallback(OnSortChanged);
            }
        }
        
        private void SetupListView()
        {
            if (potionsList == null) return;
            
            // Configure ListView
            potionsList.itemsSource = filteredPotions;
            potionsList.makeItem = MakePotionItem;
            potionsList.bindItem = BindPotionItem;
            potionsList.selectionChanged += OnPotionSelectionChanged;
            
            if (enableDebugLogging)
                Debug.Log("📋 ListView configured with makeItem and bindItem callbacks");
        }
        
        private VisualElement MakePotionItem()
        {
            // Create item container
            var itemContainer = new VisualElement();
            itemContainer.AddToClassList("potion-item");
            
            // Create icon
            var icon = new VisualElement();
            icon.name = "item-icon";
            icon.AddToClassList("potion-item-icon");
            itemContainer.Add(icon);
            
            // Create info container
            var infoContainer = new VisualElement();
            infoContainer.AddToClassList("potion-item-info");
            
            // Create name label
            var nameLabel = new Label();
            nameLabel.name = "item-name";
            nameLabel.AddToClassList("potion-item-name");
            infoContainer.Add(nameLabel);
            
            // Create category label
            var categoryLabel = new Label();
            categoryLabel.name = "item-category";
            categoryLabel.AddToClassList("potion-item-category");
            infoContainer.Add(categoryLabel);
            
            // Create effects preview
            var effectsLabel = new Label();
            effectsLabel.name = "item-effects";
            effectsLabel.AddToClassList("potion-item-effects");
            infoContainer.Add(effectsLabel);
            
            itemContainer.Add(infoContainer);
            
            return itemContainer;
        }
        
        private void BindPotionItem(VisualElement element, int index)
        {
            if (index < 0 || index >= filteredPotions.Count) return;
            
            var potion = filteredPotions[index];
            
            // Bind icon
            var icon = element.Q<VisualElement>("item-icon");
            if (icon != null && potion.ItemIcon != null)
            {
                icon.style.backgroundImage = new StyleBackground(potion.ItemIcon);
            }
            
            // Bind name
            var nameLabel = element.Q<Label>("item-name");
            if (nameLabel != null)
            {
                nameLabel.text = potion.ItemName;
            }
            
            // Bind category (assuming potions have a category field)
            var categoryLabel = element.Q<Label>("item-category");
            if (categoryLabel != null)
            {
                // You may need to add a category field to your Potion class
                categoryLabel.text = GetPotionCategory(potion);
            }
            
            // Bind effects preview
            var effectsLabel = element.Q<Label>("item-effects");
            if (effectsLabel != null)
            {
                effectsLabel.text = GetPotionEffectsPreview(potion);
            }
            
            // Apply rarity styling
            ApplyRarityStyleToItem(element, potion);
        }
        
        private string GetPotionCategory(Potion potion)
        {
            // This is a placeholder - you may want to add category information to your Potion class
            // or determine category based on effects or naming conventions
            if (potion.ItemName.ToLower().Contains("healing") || potion.ItemName.ToLower().Contains("health"))
                return "Healing";
            else if (potion.ItemName.ToLower().Contains("damage") || potion.ItemName.ToLower().Contains("poison"))
                return "Damage";
            else if (potion.ItemName.ToLower().Contains("buff") || potion.ItemName.ToLower().Contains("enhancement"))
                return "Buff";
            else if (potion.ItemName.ToLower().Contains("rare") || potion.ItemName.ToLower().Contains("legendary"))
                return "Rare";
            else
                return "Utility";
        }
        
        private string GetPotionEffectsPreview(Potion potion)
        {
            // This is a placeholder - you may want to add effects information to your Potion class
            // For now, we'll use the description or create a simple preview
            return !string.IsNullOrEmpty(potion.ItemDescription) 
                ? (potion.ItemDescription.Length > 50 
                    ? potion.ItemDescription.Substring(0, 47) + "..." 
                    : potion.ItemDescription)
                : "No effects information available";
        }
        
        private void ApplyRarityStyleToItem(VisualElement element, Potion potion)
        {
            // Remove existing rarity classes
            element.RemoveFromClassList("potion-common");
            element.RemoveFromClassList("potion-rare");
            element.RemoveFromClassList("potion-legendary");
            
            // Apply rarity class based on potion name or other criteria
            var potionName = potion.ItemName.ToLower();
            if (potionName.Contains("legendary"))
                element.AddToClassList("potion-legendary");
            else if (potionName.Contains("rare") || potionName.Contains("epic"))
                element.AddToClassList("potion-rare");
            else
                element.AddToClassList("potion-common");
        }
        
        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            currentSearchText = evt.newValue;
            ApplyFiltersAndSort();
        }
        
        private void OnCategoryFilterChanged(ChangeEvent<string> evt)
        {
            currentCategoryFilter = evt.newValue;
            ApplyFiltersAndSort();
        }
        
        private void OnSortChanged(ChangeEvent<string> evt)
        {
            if (Enum.TryParse<SortOption>(evt.newValue.Replace(" ", ""), out var sortOption))
            {
                currentSortOption = sortOption;
                ApplyFiltersAndSort();
            }
        }
        
        private void OnPotionSelectionChanged(IEnumerable<object> selectedItems)
        {
            var selectedPotion = selectedItems.FirstOrDefault() as Potion;
            SelectPotion(selectedPotion);
        }
        
        public void RefreshPotionsList()
        {
            if (inventoryComponent == null)
            {
                Debug.LogWarning("⚠️ No InventoryComponent assigned to PotionsUIController");
                return;
            }
            
            // Get potions from the new container system
            var potionsWithCounts = inventoryComponent.GetAllPotionsWithCounts();
            allPotions = potionsWithCounts.Keys.ToList();
            
            ApplyFiltersAndSort();
            UpdateStatistics();
            
            if (enableDebugLogging)
                Debug.Log($"🔄 Refreshed potions list with {allPotions.Count} potions");
        }
        
        private void ApplyFiltersAndSort()
        {
            filteredPotions = allPotions.Where(potion => 
                MatchesSearchFilter(potion) && MatchesCategoryFilter(potion)
            ).ToList();
            
            SortPotions();
            
            potionsList?.RefreshItems();
            UpdateStatistics();
            OnPotionsFiltered?.Invoke(filteredPotions);
        }
        
        private bool MatchesSearchFilter(Potion potion)
        {
            if (string.IsNullOrWhiteSpace(currentSearchText))
                return true;
                
            return potion.ItemName.ToLower().Contains(currentSearchText.ToLower()) ||
                   (!string.IsNullOrEmpty(potion.ItemDescription) && 
                    potion.ItemDescription.ToLower().Contains(currentSearchText.ToLower()));
        }
        
        private bool MatchesCategoryFilter(Potion potion)
        {
            if (currentCategoryFilter == "All")
                return true;
                
            return GetPotionCategory(potion) == currentCategoryFilter;
        }
        
        private void SortPotions()
        {
            switch (currentSortOption)
            {
                case SortOption.Name:
                    filteredPotions = filteredPotions.OrderBy(p => p.ItemName).ToList();
                    break;
                case SortOption.Category:
                    filteredPotions = filteredPotions.OrderBy(p => GetPotionCategory(p))
                                                   .ThenBy(p => p.ItemName).ToList();
                    break;
                case SortOption.DateCreated:
                    // This would require a creation date field in your Potion class
                    filteredPotions = filteredPotions.OrderBy(p => p.ItemName).ToList();
                    break;
                case SortOption.Rarity:
                    filteredPotions = filteredPotions.OrderBy(p => GetRarityOrder(p))
                                                   .ThenBy(p => p.ItemName).ToList();
                    break;
            }
        }
        
        private int GetRarityOrder(Potion potion)
        {
            var name = potion.ItemName.ToLower();
            if (name.Contains("legendary")) return 0;
            if (name.Contains("rare") || name.Contains("epic")) return 1;
            return 2; // Common
        }
        
        private void SelectPotion(Potion potion)
        {
            selectedPotion = potion;
            UpdatePotionDetails();
            OnPotionSelected?.Invoke(potion);
        }
        
        private void UpdatePotionDetails()
        {
            if (selectedPotion == null)
            {
                ShowEmptyDetails();
                return;
            }
            
            // Update details title
            if (detailsTitle != null)
                detailsTitle.text = selectedPotion.ItemName;
            
            // Update potion icon
            if (potionIcon != null && selectedPotion.ItemIcon != null)
                potionIcon.style.backgroundImage = new StyleBackground(selectedPotion.ItemIcon);
            
            // Update potion name
            if (potionName != null)
                potionName.text = selectedPotion.ItemName;
            
            // Update description
            if (potionDescription != null)
                potionDescription.text = selectedPotion.ItemDescription ?? "No description available";
            
            // Update effects (placeholder - you may want to add actual effects data)
            UpdateEffectsList();
            
            // Update ingredients (placeholder - you may want to add ingredients data)
            UpdateIngredientsList();
        }
        
        private void ShowEmptyDetails()
        {
            if (detailsTitle != null)
                detailsTitle.text = "Select a potion to view details";
            
            if (potionIcon != null)
                potionIcon.style.backgroundImage = null;
            
            if (potionName != null)
                potionName.text = "";
            
            if (potionDescription != null)
                potionDescription.text = "";
            
            effectsList?.Clear();
            ingredientsList?.Clear();
        }
        
        private void UpdateEffectsList()
        {
            effectsList?.Clear();
            
            // Placeholder effects - you may want to add actual effects to your Potion class
            if (selectedPotion != null)
            {
                var sampleEffects = new[]
                {
                    ("Healing", "Restores health over time"),
                    ("Buff", "Increases stats temporarily")
                };
                
                foreach (var (effectName, effectDesc) in sampleEffects)
                {
                    var effectItem = CreateEffectItem(effectName, effectDesc);
                    effectsList.Add(effectItem);
                }
            }
        }
        
        private void UpdateIngredientsList()
        {
            ingredientsList?.Clear();
            
            // Placeholder ingredients - you may want to add actual recipe data
            if (selectedPotion != null)
            {
                var sampleIngredients = new[]
                {
                    ("Red Mushroom", "x2"),
                    ("Crystal Water", "x1")
                };
                
                foreach (var (ingredientName, quantity) in sampleIngredients)
                {
                    var ingredientItem = CreateIngredientItem(ingredientName, quantity);
                    ingredientsList.Add(ingredientItem);
                }
            }
        }
        
        private VisualElement CreateEffectItem(string effectName, string effectDescription)
        {
            var container = new VisualElement();
            container.AddToClassList("effect-item");
            
            var nameLabel = new Label(effectName);
            nameLabel.AddToClassList("effect-name");
            container.Add(nameLabel);
            
            var descLabel = new Label(effectDescription);
            descLabel.AddToClassList("effect-description");
            container.Add(descLabel);
            
            return container;
        }
        
        private VisualElement CreateIngredientItem(string ingredientName, string quantity)
        {
            var container = new VisualElement();
            container.AddToClassList("ingredient-item");
            
            var nameLabel = new Label(ingredientName);
            nameLabel.AddToClassList("ingredient-name");
            container.Add(nameLabel);
            
            var quantityLabel = new Label(quantity);
            quantityLabel.AddToClassList("ingredient-quantity");
            container.Add(quantityLabel);
            
            return container;
        }
        
        private void UpdateStatistics()
        {
            if (totalCountLabel != null)
                totalCountLabel.text = $"Total: {filteredPotions.Count}";
            
            if (categoryCountLabel != null)
            {
                var categories = filteredPotions.Select(GetPotionCategory).Distinct().Count();
                categoryCountLabel.text = $"Categories: {categories}";
            }
        }
        
        // Public API methods
        public void SetInventoryComponent(InventoryComponent inventory)
        {
            inventoryComponent = inventory;
            RefreshPotionsList();
        }
        
        public void ClearSelection()
        {
            potionsList?.ClearSelection();
            selectedPotion = null;
            ShowEmptyDetails();
        }
        
        public void SelectPotionByName(string potionName)
        {
            var potion = filteredPotions.FirstOrDefault(p => 
                p.ItemName.Equals(potionName, StringComparison.OrdinalIgnoreCase));
            
            if (potion != null)
            {
                var index = filteredPotions.IndexOf(potion);
                potionsList?.SetSelection(index);
            }
        }
        
        // Context menu for testing
        [ContextMenu("Debug Potions UI")]
        private void DebugPotionsUI()
        {
            Debug.Log($"🔍 Total Potions: {allPotions.Count}");
            Debug.Log($"🔍 Filtered Potions: {filteredPotions.Count}");
            Debug.Log($"🔍 Selected Potion: {selectedPotion?.ItemName ?? "None"}");
            Debug.Log($"🔍 Current Search: '{currentSearchText}'");
            Debug.Log($"🔍 Current Category Filter: {currentCategoryFilter}");
            Debug.Log($"🔍 Current Sort: {currentSortOption}");
        }
    }
}