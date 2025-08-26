using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.GameSystems.AlchemyMenu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FourFatesStudios.ProjectWarden.UI
{
    /// <summary>
    /// Alchemy Book UI that displays available potions and recipes
    /// Allows players to select recipes and auto-add ingredients to crafting systems
    /// </summary>
    public class AlchemyBook : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private GameObject bookPanel;
        [SerializeField] private VisualTreeAsset bookUXML;
        [SerializeField] private StyleSheet bookUSS;

        [Header("Integration")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private float autoCloseDelay = 5f;

        // UI Elements
        private VisualElement rootElement;
        private VisualElement recipesContainer;
        private VisualElement potionsContainer;
        private VisualElement selectedRecipeDetails;
        private TextField searchField;
        private Button closeButton;
        private Button recipesTabButton;
        private Button potionsTabButton;

        // State
        private List<AlchemyRecipe> availableRecipes;
        private List<Potion> knownPotions;
        private AlchemyRecipe selectedRecipe;
        private Potion selectedPotion;
        private string currentTab = "recipes"; // "recipes" or "potions"
        private string previousCraftingPanel = "";
        private string searchQuery = "";

        // Integration references
        private CraftingUIManager craftingUIManager;
        private PotionCraftingController potionCraftingController;
        private BulkCraftingController bulkCraftingController;
        private ItemSlotContainerHolder inventory;

        public static AlchemyBook Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeReferences();
        }

        private void Start()
        {
            SetupUI();
            LoadRecipesAndPotions();
            HideBook(); // Start hidden
        }

        private void InitializeReferences()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            // Find integration components
            craftingUIManager = FindFirstObjectByType<CraftingUIManager>();
            potionCraftingController = FindFirstObjectByType<PotionCraftingController>();
            bulkCraftingController = FindFirstObjectByType<BulkCraftingController>();
            inventory = FindFirstObjectByType<ItemSlotContainerHolder>();

            if (enableDebugLogging)
            {
                Debug.Log($"📚 AlchemyBook initialized:");
                Debug.Log($"  • CraftingUIManager: {(craftingUIManager != null ? "✅" : "❌")}");
                Debug.Log($"  • PotionCraftingController: {(potionCraftingController != null ? "✅" : "❌")}");
                Debug.Log($"  • BulkCraftingController: {(bulkCraftingController != null ? "✅" : "❌")}");
                Debug.Log($"  • Inventory: {(inventory != null ? "✅" : "❌")}");
            }
        }

        private void SetupUI()
        {
            if (uiDocument == null) return;

            // Load UXML and USS if provided
            if (bookUXML != null)
            {
                uiDocument.visualTreeAsset = bookUXML;
            }

            if (bookUSS != null && uiDocument.rootVisualElement != null)
            {
                uiDocument.rootVisualElement.styleSheets.Add(bookUSS);
            }

            rootElement = uiDocument.rootVisualElement;
            if (rootElement == null) return;

            // Find UI elements
            recipesContainer = rootElement.Q<ScrollView>("recipes-container");
            potionsContainer = rootElement.Q<ScrollView>("potions-container");
            selectedRecipeDetails = rootElement.Q<VisualElement>("recipe-details");
            searchField = rootElement.Q<TextField>("search-field");
            closeButton = rootElement.Q<Button>("close-button");
            recipesTabButton = rootElement.Q<Button>("recipes-tab");
            potionsTabButton = rootElement.Q<Button>("potions-tab");

            // Setup event handlers
            closeButton?.RegisterCallback<ClickEvent>(_ => HideBook());
            recipesTabButton?.RegisterCallback<ClickEvent>(_ => ShowRecipesTab());
            potionsTabButton?.RegisterCallback<ClickEvent>(_ => ShowPotionsTab());
            searchField?.RegisterValueChangedCallback(OnSearchChanged);

            // Setup keyboard shortcuts
            rootElement?.RegisterCallback<KeyDownEvent>(OnKeyDown);

            if (enableDebugLogging)
                Debug.Log("📚 AlchemyBook UI setup complete");
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.Escape:
                    HideBook();
                    break;
                case KeyCode.Tab:
                    ToggleTab();
                    evt.PreventDefault();
                    break;
            }
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            searchQuery = evt.newValue?.ToLower() ?? "";
            RefreshUI();
        }

        private void LoadRecipesAndPotions()
        {
            // Load all available recipes
            availableRecipes = Resources.LoadAll<AlchemyRecipe>("Recipes").ToList();
            
            // Load all known potions
            knownPotions = Resources.LoadAll<Potion>("Potions").ToList();

            if (enableDebugLogging)
            {
                Debug.Log($"📚 Loaded {availableRecipes.Count} recipes and {knownPotions.Count} potions");
            }

            RefreshUI();
        }

        private void RefreshUI()
        {
            if (currentTab == "recipes")
            {
                RefreshRecipesList();
            }
            else
            {
                RefreshPotionsList();
            }
        }

        private void RefreshRecipesList()
        {
            recipesContainer?.Clear();

            foreach (var recipe in availableRecipes)
            {
                if (recipe == null) continue;

                // Apply search filter
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    bool matchesSearch = recipe.name.ToLower().Contains(searchQuery) ||
                                       (recipe.OutputPotion != null && recipe.OutputPotion.ItemName.ToLower().Contains(searchQuery)) ||
                                       (recipe.InputIngredient1 != null && recipe.InputIngredient1.ItemName.ToLower().Contains(searchQuery)) ||
                                       (recipe.InputIngredient2 != null && recipe.InputIngredient2.ItemName.ToLower().Contains(searchQuery)) ||
                                       (recipe.InputIngredient3 != null && recipe.InputIngredient3.ItemName.ToLower().Contains(searchQuery));

                    if (!matchesSearch) continue;
                }

                var recipeElement = CreateRecipeElement(recipe);
                recipesContainer?.Add(recipeElement);
            }
        }

        private void RefreshPotionsList()
        {
            potionsContainer?.Clear();

            foreach (var potion in knownPotions)
            {
                if (potion == null) continue;

                // Apply search filter
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    bool matchesSearch = potion.ItemName.ToLower().Contains(searchQuery) ||
                                       potion.ItemDescription.ToLower().Contains(searchQuery) ||
                                       (potion.PotionEffects != null && potion.PotionEffects.Any(e => e.name.ToLower().Contains(searchQuery)));

                    if (!matchesSearch) continue;
                }

                var potionElement = CreatePotionElement(potion);
                potionsContainer?.Add(potionElement);
            }
        }

        private VisualElement CreateRecipeElement(AlchemyRecipe recipe)
        {
            var container = new VisualElement();
            container.AddToClassList("recipe-item");

            // Recipe name and info
            var titleLabel = new Label(recipe.name);
            titleLabel.AddToClassList("recipe-title");
            container.Add(titleLabel);

            // Ingredients preview
            var ingredientsContainer = new VisualElement();
            ingredientsContainer.AddToClassList("ingredients-preview");

            if (recipe.InputIngredient1 != null)
                ingredientsContainer.Add(new Label($"• {recipe.InputIngredient1.ItemName}"));
            if (recipe.InputIngredient2 != null)
                ingredientsContainer.Add(new Label($"• {recipe.InputIngredient2.ItemName}"));
            if (recipe.InputIngredient3 != null)
                ingredientsContainer.Add(new Label($"• {recipe.InputIngredient3.ItemName}"));

            container.Add(ingredientsContainer);

            // Result potion
            if (recipe.OutputPotion != null)
            {
                var resultLabel = new Label($"→ {recipe.OutputPotion.ItemName}");
                resultLabel.AddToClassList("recipe-result");
                container.Add(resultLabel);
            }

            // Click handler
            container.RegisterCallback<ClickEvent>(_ => SelectRecipe(recipe));

            // Check if player has ingredients
            bool hasIngredients = CheckPlayerHasIngredients(recipe);
            if (hasIngredients)
            {
                container.AddToClassList("recipe-available");
            }
            else
            {
                container.AddToClassList("recipe-unavailable");
            }

            return container;
        }

        private VisualElement CreatePotionElement(Potion potion)
        {
            var container = new VisualElement();
            container.AddToClassList("potion-item");

            // Potion name and type
            var titleLabel = new Label(potion.ItemName);
            titleLabel.AddToClassList("potion-title");
            container.Add(titleLabel);

            var typeLabel = new Label($"Type: {potion.ItemPotionType}");
            typeLabel.AddToClassList("potion-type");
            container.Add(typeLabel);

            // Effects
            if (potion.PotionEffects != null && potion.PotionEffects.Count > 0)
            {
                var effectsLabel = new Label($"Effects: {string.Join(", ", potion.PotionEffects.Select(e => e.name))}");
                effectsLabel.AddToClassList("potion-effects");
                container.Add(effectsLabel);
            }

            // Click handler
            container.RegisterCallback<ClickEvent>(_ => SelectPotion(potion));

            return container;
        }

        private bool CheckPlayerHasIngredients(AlchemyRecipe recipe)
        {
            if (inventory == null) return false;

            var requiredIngredients = new List<Ingredient>();
            if (recipe.InputIngredient1 != null) requiredIngredients.Add(recipe.InputIngredient1);
            if (recipe.InputIngredient2 != null) requiredIngredients.Add(recipe.InputIngredient2);
            if (recipe.InputIngredient3 != null) requiredIngredients.Add(recipe.InputIngredient3);

            // Simple check - this would need to be integrated with your actual inventory system
            // For now, assume player has ingredients if they exist
            return requiredIngredients.Count > 0;
        }

        #region Public Interface

        public void ShowBook(string fromPanel = "")
        {
            previousCraftingPanel = fromPanel;
            
            if (bookPanel != null)
                bookPanel.SetActive(true);

            // Update references in case they changed
            InitializeReferences();

            if (enableDebugLogging)
                Debug.Log($"📚 Opening Alchemy Book (from: {fromPanel})");
        }

        public void HideBook()
        {
            if (bookPanel != null)
                bookPanel.SetActive(false);

            if (enableDebugLogging)
                Debug.Log("📚 Closing Alchemy Book");
        }

        public void ToggleBook(string fromPanel = "")
        {
            if (bookPanel != null && bookPanel.activeSelf)
            {
                HideBook();
            }
            else
            {
                ShowBook(fromPanel);
            }
        }

        #endregion

        #region Tab Management

        public void ShowRecipesTab()
        {
            currentTab = "recipes";
            recipesContainer?.RemoveFromClassList("hidden");
            potionsContainer?.AddToClassList("hidden");
            recipesTabButton?.AddToClassList("active");
            potionsTabButton?.RemoveFromClassList("active");
            RefreshRecipesList();
        }

        private void ShowPotionsTab()
        {
            currentTab = "potions";
            potionsContainer?.RemoveFromClassList("hidden");
            recipesContainer?.AddToClassList("hidden");
            potionsTabButton?.AddToClassList("active");
            recipesTabButton?.RemoveFromClassList("active");
            RefreshPotionsList();
        }

        private void ToggleTab()
        {
            if (currentTab == "recipes")
                ShowPotionsTab();
            else
                ShowRecipesTab();
        }

        #endregion

        #region Selection Handlers

        private void SelectRecipe(AlchemyRecipe recipe)
        {
            selectedRecipe = recipe;
            selectedPotion = null;

            ShowRecipeDetails(recipe);

            // Try to auto-add ingredients to active crafting system
            TryAutoAddIngredients(recipe);
        }

        private void SelectPotion(Potion potion)
        {
            selectedPotion = potion;
            selectedRecipe = null;

            ShowPotionDetails(potion);

            // Try to find a recipe that creates this potion
            var recipe = availableRecipes.FirstOrDefault(r => r.OutputPotion == potion);
            if (recipe != null)
            {
                TryAutoAddIngredients(recipe);
            }
        }

        private void ShowRecipeDetails(AlchemyRecipe recipe)
        {
            selectedRecipeDetails?.Clear();

            var titleLabel = new Label($"Recipe: {recipe.name}");
            titleLabel.AddToClassList("details-title");
            selectedRecipeDetails?.Add(titleLabel);

            // Ingredients section
            var ingredientsSection = new VisualElement();
            ingredientsSection.AddToClassList("details-section");
            
            var ingredientsTitle = new Label("Required Ingredients:");
            ingredientsTitle.AddToClassList("section-title");
            ingredientsSection.Add(ingredientsTitle);

            if (recipe.InputIngredient1 != null)
                ingredientsSection.Add(new Label($"• {recipe.InputIngredient1.ItemName}"));
            if (recipe.InputIngredient2 != null)
                ingredientsSection.Add(new Label($"• {recipe.InputIngredient2.ItemName}"));
            if (recipe.InputIngredient3 != null)
                ingredientsSection.Add(new Label($"• {recipe.InputIngredient3.ItemName}"));

            selectedRecipeDetails?.Add(ingredientsSection);

            // Result section
            if (recipe.OutputPotion != null)
            {
                var resultSection = new VisualElement();
                resultSection.AddToClassList("details-section");
                
                var resultTitle = new Label("Creates:");
                resultTitle.AddToClassList("section-title");
                resultSection.Add(resultTitle);

                var resultLabel = new Label($"• {recipe.OutputPotion.ItemName}");
                resultSection.Add(resultLabel);

                selectedRecipeDetails?.Add(resultSection);
            }

            // Action button
            var craftButton = new Button(() => TryAutoAddIngredients(recipe));
            craftButton.text = "Add to Crafting";
            craftButton.AddToClassList("craft-button");
            selectedRecipeDetails?.Add(craftButton);
        }

        private void ShowPotionDetails(Potion potion)
        {
            selectedRecipeDetails?.Clear();

            var titleLabel = new Label($"Potion: {potion.ItemName}");
            titleLabel.AddToClassList("details-title");
            selectedRecipeDetails?.Add(titleLabel);

            var descLabel = new Label(potion.ItemDescription);
            selectedRecipeDetails?.Add(descLabel);

            // Effects
            if (potion.PotionEffects != null && potion.PotionEffects.Count > 0)
            {
                var effectsSection = new VisualElement();
                effectsSection.AddToClassList("details-section");
                
                var effectsTitle = new Label("Effects:");
                effectsTitle.AddToClassList("section-title");
                effectsSection.Add(effectsTitle);

                foreach (var effect in potion.PotionEffects)
                {
                    if (effect != null)
                        effectsSection.Add(new Label($"• {effect.name}"));
                }

                selectedRecipeDetails?.Add(effectsSection);
            }
        }

        #endregion

        #region Auto-Add Ingredients Integration

        private void TryAutoAddIngredients(AlchemyRecipe recipe)
        {
            if (recipe == null) return;

            // Determine which crafting system is currently active
            bool success = false;

            if (IsCurrentlyInPotionCrafting())
            {
                success = AddIngredientsToPotionCrafting(recipe);
            }
            else if (IsCurrentlyInBulkCrafting())
            {
                success = AddIngredientsToBulkCrafting(recipe);
            }

            if (success)
            {
                if (enableDebugLogging)
                    Debug.Log($"✅ Added ingredients for {recipe.name} to crafting system");
                
                // Optionally close the book after successful addition
                HideBook();
            }
            else
            {
                if (enableDebugLogging)
                    Debug.LogWarning($"⚠️ Failed to add ingredients for {recipe.name} - crafting system not active or missing ingredients");
            }
        }

        private bool IsCurrentlyInPotionCrafting()
        {
            if (craftingUIManager != null)
            {
                var activePanel = craftingUIManager.GetActivePanel();
                return activePanel == "PotionCraftingUI" || previousCraftingPanel == "PotionCraftingUI";
            }
            
            return previousCraftingPanel == "PotionCrafting";
        }

        private bool IsCurrentlyInBulkCrafting()
        {
            if (craftingUIManager != null)
            {
                var activePanel = craftingUIManager.GetActivePanel();
                return activePanel == "BulkCraftingUI" || previousCraftingPanel == "BulkCraftingUI";
            }
            
            return previousCraftingPanel == "BulkCrafting";
        }

        private bool AddIngredientsToPotionCrafting(AlchemyRecipe recipe)
        {
            if (potionCraftingController == null) return false;

            try
            {
                // Get the private selectedIngredients array using reflection
                var controllerType = typeof(PotionCraftingController);
                var selectedIngredientsField = controllerType.GetField("selectedIngredients", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var ingredientSlotsField = controllerType.GetField("ingredientSlots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (selectedIngredientsField != null && ingredientSlotsField != null)
                {
                    var selectedIngredients = (Ingredient[])selectedIngredientsField.GetValue(potionCraftingController);
                    var ingredientSlots = (Button[])ingredientSlotsField.GetValue(potionCraftingController);

                    // Clear existing ingredients
                    for (int i = 0; i < selectedIngredients.Length; i++)
                    {
                        selectedIngredients[i] = null;
                        if (ingredientSlots[i] != null)
                            ingredientSlots[i].text = "+";
                    }

                    // Add new ingredients from recipe
                    int slotIndex = 0;
                    if (recipe.InputIngredient1 != null && slotIndex < selectedIngredients.Length)
                    {
                        selectedIngredients[slotIndex] = recipe.InputIngredient1;
                        if (ingredientSlots[slotIndex] != null)
                            ingredientSlots[slotIndex].text = recipe.InputIngredient1.ItemName;
                        slotIndex++;
                    }
                    if (recipe.InputIngredient2 != null && slotIndex < selectedIngredients.Length)
                    {
                        selectedIngredients[slotIndex] = recipe.InputIngredient2;
                        if (ingredientSlots[slotIndex] != null)
                            ingredientSlots[slotIndex].text = recipe.InputIngredient2.ItemName;
                        slotIndex++;
                    }
                    if (recipe.InputIngredient3 != null && slotIndex < selectedIngredients.Length)
                    {
                        selectedIngredients[slotIndex] = recipe.InputIngredient3;
                        if (ingredientSlots[slotIndex] != null)
                            ingredientSlots[slotIndex].text = recipe.InputIngredient3.ItemName;
                        slotIndex++;
                    }

                    if (enableDebugLogging)
                    {
                        Debug.Log($"🧪 Added ingredients to Potion Crafting:");
                        if (recipe.InputIngredient1 != null) Debug.Log($"  • Slot 1: {recipe.InputIngredient1.ItemName}");
                        if (recipe.InputIngredient2 != null) Debug.Log($"  • Slot 2: {recipe.InputIngredient2.ItemName}");
                        if (recipe.InputIngredient3 != null) Debug.Log($"  • Slot 3: {recipe.InputIngredient3.ItemName}");
                    }

                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to add ingredients to potion crafting: {e.Message}");
            }

            return false;
        }

        private bool AddIngredientsToBulkCrafting(AlchemyRecipe recipe)
        {
            if (bulkCraftingController == null) return false;

            // This would integrate with your actual bulk crafting system
            if (enableDebugLogging)
            {
                Debug.Log($"🏭 Adding to Bulk Crafting:");
                if (recipe.InputIngredient1 != null) Debug.Log($"  • {recipe.InputIngredient1.ItemName}");
                if (recipe.InputIngredient2 != null) Debug.Log($"  • {recipe.InputIngredient2.ItemName}");
                if (recipe.InputIngredient3 != null) Debug.Log($"  • {recipe.InputIngredient3.ItemName}");
            }

            // TODO: Implement actual ingredient addition to bulk crafting system
            return true;
        }

        #endregion

        #region Unity Events

        private void Update()
        {
            // Handle keyboard shortcuts when book is open
            if (bookPanel != null && bookPanel.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    HideBook();
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion
    }
}