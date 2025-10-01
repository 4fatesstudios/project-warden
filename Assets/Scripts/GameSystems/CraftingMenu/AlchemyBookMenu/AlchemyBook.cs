using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.Enums;
using GameSystems.CraftingMenu.AlchemyBookMenu.Sections;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameSystems.CraftingMenu.AlchemyBookMenu
{
    public class AlchemyBook : MonoBehaviour
    {
        [Header("UI References")]
        public UIDocument uiDocument;
        public VisualTreeAsset entryTemplate;
        public StyleSheet bookStyleSheet;

        [Header("Camera Settings")]
        [SerializeField] private float distanceFromCamera = 5f;

        [Header("Book Content")]
        public List<BestiaryEntry> bestiaryEntries = new List<BestiaryEntry>();
        public List<RecipeEntry> recipeEntries = new List<RecipeEntry>();
        public List<IngredientEntry> ingredientEntries = new List<IngredientEntry>();
        public List<HelpEntry> helpEntries = new List<HelpEntry>();

        // UI Elements
        private VisualElement root;
        private VisualElement tabsContainer;
        private VisualElement leftPage;
        private VisualElement rightPage;
        private VisualElement frontCover;
        private VisualElement backCover;
        private Button nextButton;
        private Button prevButton;
        private TextField searchField;
        private Button searchButton;

        // State
        private EntryType currentSection = EntryType.Bestiary;
        private int currentPageIndex;
        private bool showingCover = true;
        private bool showingFrontCover = true;
        private List<BaseEntry> filteredEntries = new List<BaseEntry>();

        void Start()
        {
            Debug.Log("📖 AlchemyBook Start() called");
            
            // Auto-assign UIDocument if not set
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
                Debug.Log($"📖 Auto-assigned UIDocument: {uiDocument != null}");
            }
            
            InitializeHelpEntries();
            SetupCameraPosition();
            SetupUI();
            
            // Load alchemy recipes and ingredients from the project
            LoadAllAlchemyRecipes();
            LoadAllIngredients();
            
            // Load entries from Resources/AlchemyBook folder structure
            LoadEntriesFromResources();
            
            UpdateDisplay();
            
            Debug.Log($"📖 AlchemyBook initialization complete. Total entries - Bestiary: {bestiaryEntries.Count}, Recipes: {recipeEntries.Count}, Ingredients: {ingredientEntries.Count}, Help: {helpEntries.Count}");
        }

        void OnEnable()
        {
            Debug.Log("📖 AlchemyBook OnEnable called");
        }

        void OnDisable()
        {
            Debug.Log("📖 AlchemyBook OnDisable called");
        }

        void SetupCameraPosition()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                var cameraTransform = mainCamera.transform;
                transform.position = cameraTransform.position + cameraTransform.forward * distanceFromCamera;
                transform.LookAt(cameraTransform.position); // Use Vector3 to avoid ambiguity
            }
        }

        void InitializeHelpEntries()
        {
            helpEntries = new List<HelpEntry>();
            
            // Create help entries using ScriptableObject.CreateInstance
            var refinementGuide = ScriptableObject.CreateInstance<HelpEntry>();
            refinementGuide.title = "Refinements Guide";
            refinementGuide.description = "Learn how to use the refinement system to process raw materials.";
            refinementGuide.category = HelpCategory.AdvancedTechniques;
            refinementGuide.steps = new string[] { "Select raw materials", "Choose refinement type", "Play the minigame", "Collect refined materials" };
            refinementGuide.tips = new string[] { "Focus on timing for best results", "Different materials require different techniques" };
            refinementGuide.isSeen = true;
            helpEntries.Add(refinementGuide);
            
            var potionCrafting = ScriptableObject.CreateInstance<HelpEntry>();
            potionCrafting.title = "Potion Crafting";
            potionCrafting.description = "Master the art of brewing potions using ingredients and recipes.";
            potionCrafting.category = HelpCategory.PotionBrewing;
            potionCrafting.steps = new string[] { "Gather ingredients", "Select recipe", "Follow brewing process", "Complete crafting minigame" };
            potionCrafting.tips = new string[] { "Check ingredient quality", "Match recipe requirements exactly" };
            potionCrafting.isSeen = true;
            helpEntries.Add(potionCrafting);
            
            var harvesting = ScriptableObject.CreateInstance<HelpEntry>();
            harvesting.title = "Harvesting";
            harvesting.description = "Learn to efficiently gather ingredients from the world.";
            harvesting.category = HelpCategory.IngredientHarvesting;
            harvesting.steps = new string[] { "Find ingredient sources", "Use proper tools", "Harvest at optimal times", "Store ingredients safely" };
            harvesting.tips = new string[] { "Different seasons affect availability", "Quality varies by location" };
            harvesting.isSeen = true;
            helpEntries.Add(harvesting);
            
            var combatSystem = ScriptableObject.CreateInstance<HelpEntry>();
            combatSystem.title = "Combat System";
            combatSystem.description = "Understanding combat mechanics and strategy.";
            combatSystem.category = HelpCategory.AdvancedTechniques;
            combatSystem.steps = new string[] { "Learn basic attacks", "Master defense timing", "Use potions strategically", "Adapt to enemy types" };
            combatSystem.tips = new string[] { "Prepare potions before fights", "Study enemy patterns" };
            combatSystem.isSeen = true;
            helpEntries.Add(combatSystem);
            
            var templatePage = ScriptableObject.CreateInstance<HelpEntry>();
            templatePage.title = "Template Page";
            templatePage.description = "This is a template for creating new help entries.";
            templatePage.category = HelpCategory.GettingStarted;
            templatePage.steps = new string[] { "Step 1: Example", "Step 2: Example", "Step 3: Example" };
            templatePage.tips = new string[] { "This is a sample tip" };
            templatePage.isSeen = true;
            helpEntries.Add(templatePage);
        }

        void SetupUI()
        {
            Debug.Log("📖 SetupUI called");
            
            if (uiDocument == null)
            {
                Debug.LogError("📖 UIDocument is null! Cannot setup UI.");
                return;
            }
            
            root = uiDocument.rootVisualElement;
            Debug.Log($"📖 Root element: {root?.name}");

            if (root == null)
            {
                Debug.LogError("📖 Root visual element is null! Check if UXML is assigned to UIDocument.");
                return;
            }

            if (bookStyleSheet)
                root.styleSheets.Add(bookStyleSheet);

            // Find main UI elements
            tabsContainer = root.Q<VisualElement>("tabs-container");
            leftPage = root.Q<VisualElement>("left-page");
            rightPage = root.Q<VisualElement>("right-page");
            frontCover = root.Q<VisualElement>("front-cover");
            backCover = root.Q<VisualElement>("back-cover");
            nextButton = root.Q<Button>("next-button");
            prevButton = root.Q<Button>("prev-button");
            searchField = root.Q<TextField>("search-field");
            searchButton = root.Q<Button>("search-button");
            
            Debug.Log($"📖 UI Elements found - leftPage: {leftPage != null}, rightPage: {rightPage != null}, tabsContainer: {tabsContainer != null}, frontCover: {frontCover != null}");

            // Find and setup the open book button
            var openBookButton = root.Q<Button>("open-book-button");
            if (openBookButton != null)
            {
                openBookButton.clicked += OpenBook;
                Debug.Log("📖 Open book button found and connected");
            }
            else
            {
                Debug.LogWarning("📖 Open book button not found in UI");
            }

            // Find and setup close book button
            var closeBookButton = root.Q<Button>("close-book-main");
            if (closeBookButton != null)
            {
                closeBookButton.clicked += CloseBook;
                Debug.Log("📖 Close book button found and connected");
            }

            SetupTabButtons();
            SetupNavigationButtons();
            SetupSearchFunctionality();
            
            Debug.Log("📖 UI setup completed");
        }

        void SetupNavigationButtons()
        {
            if (nextButton != null)
                nextButton.clicked += NextPage;
            if (prevButton != null)
                prevButton.clicked += PrevPage;
        }

        void SetupSearchFunctionality()
        {
            if (searchButton != null)
                searchButton.clicked += PerformSearch;
            if (searchField != null)
                searchField.RegisterCallback<KeyDownEvent>(OnSearchKeyDown);
        }

        void OpenBook()
        {
            Debug.Log("📖 Opening book");
            showingCover = false;
            showingFrontCover = false;
            UpdateDisplay();
        }

        public void CloseBook()
        {
            Debug.Log("📖 Closing book");
            showingCover = true;
            showingFrontCover = true;
            UpdateDisplay();
            gameObject.SetActive(false);

        }

        void SetupTabButtons()
        {
            var bestiaryTab = root.Q<Button>("bestiary-tab");
            var recipeTab = root.Q<Button>("recipes-tab");
            var ingredientTab = root.Q<Button>("ingredients-tab");
            var helpTab = root.Q<Button>("help-tab");
            var bookmarkTab = root.Q<Button>("bookmarks-tab");

            if (bestiaryTab != null)
                bestiaryTab.clicked += () => SwitchSection(EntryType.Bestiary);
            if (recipeTab != null)
                recipeTab.clicked += () => SwitchSection(EntryType.Recipe);
            if (ingredientTab != null)
                ingredientTab.clicked += () => SwitchSection(EntryType.Ingredient);
            if (helpTab != null)
                helpTab.clicked += () => SwitchSection(EntryType.Help);
            if (bookmarkTab != null)
                bookmarkTab.clicked += () => SwitchSection(EntryType.Bookmarked);
            
            Debug.Log($"📖 Tab buttons setup - Bestiary: {bestiaryTab != null}, Recipes: {recipeTab != null}, Ingredients: {ingredientTab != null}, Help: {helpTab != null}, Bookmarks: {bookmarkTab != null}");
        }

        void SwitchSection(EntryType section)
        {
            currentSection = section;
            currentPageIndex = 0;
            showingCover = false;
            UpdateFilteredEntries();
            UpdateDisplay();
        }

        void UpdateFilteredEntries()
        {
            filteredEntries.Clear();

            switch (currentSection)
            {
                case EntryType.Bestiary:
                    filteredEntries.AddRange(bestiaryEntries);
                    break;
                case EntryType.Recipe:
                    filteredEntries.AddRange(recipeEntries);
                    break;
                case EntryType.Ingredient:
                    filteredEntries.AddRange(ingredientEntries);
                    break;
                case EntryType.Help:
                    filteredEntries.AddRange(helpEntries);
                    break;
                case EntryType.Bookmarked:
                    filteredEntries.AddRange(GetBookmarkedEntries());
                    break;
            }

            for (int i = 0; i < filteredEntries.Count; i++)
            {
                filteredEntries[i].pageNumber = i + 1;
            }
        }

        List<BaseEntry> GetBookmarkedEntries()
        {
            var bookmarked = new List<BaseEntry>();
            bookmarked.AddRange(bestiaryEntries.Where(e => e.isBookmarked));
            bookmarked.AddRange(recipeEntries.Where(e => e.isBookmarked));
            bookmarked.AddRange(ingredientEntries.Where(e => e.isBookmarked));
            bookmarked.AddRange(helpEntries.Where(e => e.isBookmarked));
            return bookmarked;
        }

        void NextPage()
        {
            if (showingCover)
            {
                if (showingFrontCover)
                {
                    showingCover = false;
                    UpdateDisplay();
                }
            }
            else
            {
                if (currentPageIndex + 2 < filteredEntries.Count)
                {
                    currentPageIndex += 2;
                    UpdateDisplay();
                }
                else if (GetNextSection() != currentSection)
                {
                    SwitchSection(GetNextSection());
                }
                else
                {
                    showingCover = true;
                    showingFrontCover = false;
                    UpdateDisplay();
                }
            }
        }

        void PrevPage()
        {
            if (showingCover)
            {
                if (!showingFrontCover)
                {
                    showingCover = false;
                    SwitchSection(GetPrevSection());
                    currentPageIndex = Mathf.Max(0, (filteredEntries.Count - 1) / 2 * 2);
                    UpdateDisplay();
                }
            }
            else
            {
                if (currentPageIndex > 0)
                {
                    currentPageIndex -= 2;
                    UpdateDisplay();
                }
                else if (GetPrevSection() != currentSection)
                {
                    SwitchSection(GetPrevSection());
                    currentPageIndex = Mathf.Max(0, (filteredEntries.Count - 1) / 2 * 2);
                    UpdateDisplay();
                }
                else
                {
                    showingCover = true;
                    showingFrontCover = true;
                    UpdateDisplay();
                }
            }
        }

        EntryType GetNextSection()
        {
            switch (currentSection)
            {
                case EntryType.Bestiary: return EntryType.Recipe;
                case EntryType.Recipe: return EntryType.Ingredient;
                case EntryType.Ingredient: return EntryType.Help;
                case EntryType.Help: return EntryType.Bookmarked;
                case EntryType.Bookmarked: return EntryType.Bestiary;
                default: return EntryType.Bestiary;
            }
        }

        EntryType GetPrevSection()
        {
            switch (currentSection)
            {
                case EntryType.Bestiary: return EntryType.Bookmarked;
                case EntryType.Recipe: return EntryType.Bestiary;
                case EntryType.Ingredient: return EntryType.Recipe;
                case EntryType.Help: return EntryType.Ingredient;
                case EntryType.Bookmarked: return EntryType.Help;
                default: return EntryType.Bestiary;
            }
        }

        void PerformSearch()
        {
            var searchTerm = searchField.value.ToLower();
            if (string.IsNullOrEmpty(searchTerm))
            {
                UpdateFilteredEntries();
                UpdateDisplay();
                return;
            }

            var searchResults = new List<BaseEntry>();

            searchResults.AddRange(bestiaryEntries.Where(e =>
                e.title.ToLower().Contains(searchTerm) ||
                e.description.ToLower().Contains(searchTerm) ||
                e.habitat.ToLower().Contains(searchTerm) ||
                e.behavior.ToLower().Contains(searchTerm)
            ));

            searchResults.AddRange(recipeEntries.Where(e =>
                e.title.ToLower().Contains(searchTerm) ||
                e.description.ToLower().Contains(searchTerm) ||
                e.infusions.Any(i => i.ToLower().Contains(searchTerm)) ||
                e.requiredIngredients.Any(i => i.ingredientName.ToLower().Contains(searchTerm))
            ));

            searchResults.AddRange(ingredientEntries.Where(e =>
                e.title.ToLower().Contains(searchTerm) ||
                e.description.ToLower().Contains(searchTerm) ||
                e.aspect.ToString().ToLower().Contains(searchTerm) ||
                e.dropSources.Any(s => s.sourceName.ToLower().Contains(searchTerm))
            ));

            searchResults.AddRange(helpEntries.Where(e =>
                e.title.ToLower().Contains(searchTerm) ||
                e.description.ToLower().Contains(searchTerm)
            ));

            filteredEntries = searchResults;
            currentPageIndex = 0;
            showingCover = false;
            UpdateDisplay();
        }

        void OnSearchKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                PerformSearch();
            }
        }

        void UpdateDisplay()
        {
            tabsContainer.style.display = showingCover ? DisplayStyle.None : DisplayStyle.Flex;

            leftPage.Clear();
            rightPage.Clear();
            frontCover.style.display = DisplayStyle.None;
            backCover.style.display = DisplayStyle.None;

            if (showingCover)
            {
                if (showingFrontCover)
                    frontCover.style.display = DisplayStyle.Flex;
                else
                    backCover.style.display = DisplayStyle.Flex;
                return;
            }

            if (currentPageIndex < filteredEntries.Count)
                leftPage.Add(CreateEntryElement(filteredEntries[currentPageIndex]));

            if (currentPageIndex + 1 < filteredEntries.Count)
                rightPage.Add(CreateEntryElement(filteredEntries[currentPageIndex + 1]));

            UpdateTabStates();
        }

        /// <summary>
        /// Creates a visual element for an entry, using template if available
        /// </summary>
        private VisualElement CreateEntryElement(BaseEntry entry)
        {
            if (entryTemplate != null)
            {
                return CreateEntryFromTemplate(entry);
            }
            else
            {
                // Fallback to the entry's own CreateEntryVisual method
                return entry.CreateEntryVisual();
            }
        }

        /// <summary>
        /// Creates an entry using the UXML template
        /// </summary>
        private VisualElement CreateEntryFromTemplate(BaseEntry entry)
        {
            var entryElement = entryTemplate.CloneTree();
            var container = entryElement.Q<VisualElement>("entry-container");
            
            if (container == null)
            {
                Debug.LogWarning("Entry template missing 'entry-container' element!");
                return entry.CreateEntryVisual(); // Fallback
            }

            // Apply entry type specific styling
            string entryTypeClass = entry switch
            {
                BestiaryEntry => "bestiary-entry",
                RecipeEntry => "recipe-entry", 
                IngredientEntry => "ingredient-entry",
                HelpEntry => "help-entry",
                _ => "generic-entry"
            };
            container.AddToClassList(entryTypeClass);

            // Populate template elements
            PopulateTemplateElements(container, entry);

            return container;
        }

        /// <summary>
        /// Populates the template elements with entry data
        /// </summary>
        private void PopulateTemplateElements(VisualElement container, BaseEntry entry)
        {
            // Title and basic info
            var title = container.Q<Label>("entry-title");
            if (title != null) title.text = entry.title;

            var subtitle = container.Q<Label>("entry-subtitle");
            if (subtitle != null) 
            {
                // Set subtitle based on entry type
                string subtitleText = entry switch
                {
                    RecipeEntry recipe => recipe.isUniquePotionRecipe ? "Unique Potion" : "Custom Infusion",
                    IngredientEntry ingredient => ingredient.aspect.ToString(),
                    BestiaryEntry => "Creature",
                    HelpEntry => "Guide",
                    _ => ""
                };
                subtitle.text = subtitleText;
            }

            var description = container.Q<Label>("entry-description");
            if (description != null) description.text = entry.description ?? "";

            // Bookmark functionality
            var bookmarkButton = container.Q<Button>("bookmark-button");
            if (bookmarkButton != null)
            {
                bookmarkButton.text = entry.isBookmarked ? "★" : "☆";
                if (entry.isBookmarked)
                    bookmarkButton.AddToClassList("bookmarked");
                bookmarkButton.clicked += () => ToggleBookmark(entry.title, entry.GetEntryType());
            }

            // Page number
            var pageNumber = container.Q<Label>("entry-page-number");
            if (pageNumber != null) pageNumber.text = $"Page {entry.pageNumber}";

            // Discovery status
            var discovered = container.Q<Label>("entry-discovered");
            if (discovered != null)
            {
                discovered.text = entry.isSeen ? "Discovered" : "Unknown";
                discovered.style.color = entry.isSeen ? new Color(0.13f, 0.55f, 0.13f) : new Color(0.7f, 0.7f, 0.7f);
            }

            // Type-specific content
            PopulateTypeSpecificContent(container, entry);
        }

        /// <summary>
        /// Populates content specific to the entry type
        /// </summary>
        private void PopulateTypeSpecificContent(VisualElement container, BaseEntry entry)
        {
            switch (entry)
            {
                case RecipeEntry recipe:
                    PopulateRecipeContent(container, recipe);
                    break;
                case IngredientEntry ingredient:
                    PopulateIngredientContent(container, ingredient);
                    break;
                case HelpEntry help:
                    PopulateHelpContent(container, help);
                    break;
                case BestiaryEntry bestiary:
                    PopulateBestiaryContent(container, bestiary);
                    break;
            }
        }

        private void PopulateRecipeContent(VisualElement container, RecipeEntry recipe)
        {
            var ingredientsSection = container.Q<VisualElement>("entry-ingredients");
            if (ingredientsSection != null && recipe.requiredIngredients != null && recipe.requiredIngredients.Count > 0)
            {
                ingredientsSection.style.display = DisplayStyle.Flex;
                var ingredientsList = ingredientsSection.Q<VisualElement>("ingredients-list");
                if (ingredientsList != null)
                {
                    foreach (var ingredient in recipe.requiredIngredients)
                    {
                        if (ingredient.isDiscovered)
                        {
                            var ingredientLabel = new Label($"• {ingredient.ingredientName} x{ingredient.quantity}");
                            ingredientLabel.AddToClassList("ingredient-item");
                            ingredientsList.Add(ingredientLabel);
                        }
                        else
                        {
                            var unknownLabel = new Label("• ??? x?");
                            unknownLabel.AddToClassList("ingredient-unknown");
                            ingredientsList.Add(unknownLabel);
                        }
                    }
                }
            }

            var effectsSection = container.Q<VisualElement>("entry-effects");
            if (effectsSection != null && recipe.discoveredInfusions != null && recipe.discoveredInfusions.Count > 0)
            {
                effectsSection.style.display = DisplayStyle.Flex;
                var effectsList = effectsSection.Q<VisualElement>("effects-list");
                if (effectsList != null)
                {
                    foreach (var infusion in recipe.discoveredInfusions)
                    {
                        var effectLabel = new Label($"• {infusion}");
                        effectLabel.AddToClassList("effect-item");
                        effectsList.Add(effectLabel);
                    }
                }
            }
        }

        private void PopulateIngredientContent(VisualElement container, IngredientEntry ingredient)
        {
            var propertiesSection = container.Q<VisualElement>("entry-properties");
            if (propertiesSection != null && ingredient.availableRefinements != null && ingredient.availableRefinements.Count > 0)
            {
                propertiesSection.style.display = DisplayStyle.Flex;
                var propertiesList = propertiesSection.Q<VisualElement>("properties-list");
                if (propertiesList != null)
                {
                    foreach (var refinement in ingredient.availableRefinements)
                    {
                        var propertyLabel = new Label($"• {refinement}");
                        propertyLabel.AddToClassList("property-item");
                        propertiesList.Add(propertyLabel);
                    }
                }
            }

            // Show drop sources in the lore section
            var loreSection = container.Q<VisualElement>("entry-lore");
            if (loreSection != null && ingredient.dropSources != null && ingredient.dropSources.Count > 0)
            {
                loreSection.style.display = DisplayStyle.Flex;
                var loreContent = loreSection.Q<Label>("lore-content");
                if (loreContent != null)
                {
                    string sourcesText = "Sources:\n";
                    foreach (var source in ingredient.dropSources)
                    {
                        sourcesText += $"• {source.sourceName} ({source.sourceType})";
                        if (!string.IsNullOrEmpty(source.location))
                            sourcesText += $" - {source.location}";
                        sourcesText += "\n";
                    }
                    loreContent.text = sourcesText;
                }
            }
        }

        private void PopulateHelpContent(VisualElement container, HelpEntry help)
        {
            var tipsSection = container.Q<VisualElement>("entry-tips");
            if (tipsSection != null && help.tips != null && help.tips.Length > 0)
            {
                tipsSection.style.display = DisplayStyle.Flex;
                var tipsList = tipsSection.Q<VisualElement>("tips-list");
                if (tipsList != null)
                {
                    foreach (var tip in help.tips)
                    {
                        var tipLabel = new Label($"• {tip}");
                        tipLabel.AddToClassList("tip-item");
                        tipsList.Add(tipLabel);
                    }
                }
            }

            // Show steps in the properties section
            var propertiesSection = container.Q<VisualElement>("entry-properties");
            if (propertiesSection != null && help.steps != null && help.steps.Length > 0)
            {
                propertiesSection.style.display = DisplayStyle.Flex;
                var propertiesList = propertiesSection.Q<VisualElement>("properties-list");
                if (propertiesList != null)
                {
                    for (int i = 0; i < help.steps.Length; i++)
                    {
                        var stepLabel = new Label($"{i + 1}. {help.steps[i]}");
                        stepLabel.AddToClassList("property-item");
                        propertiesList.Add(stepLabel);
                    }
                }
            }
        }

        private void PopulateBestiaryContent(VisualElement container, BestiaryEntry bestiary)
        {
            var loreSection = container.Q<VisualElement>("entry-lore");
            if (loreSection != null && !string.IsNullOrEmpty(bestiary.behavior))
            {
                loreSection.style.display = DisplayStyle.Flex;
                var loreContent = loreSection.Q<Label>("lore-content");
                if (loreContent != null)
                {
                    string loreText = $"Behavior: {bestiary.behavior}";
                    if (!string.IsNullOrEmpty(bestiary.habitat))
                        loreText += $"\n\nHabitat: {bestiary.habitat}";
                    loreContent.text = loreText;
                }
            }

            // Show dropped ingredients in the properties section
            var propertiesSection = container.Q<VisualElement>("entry-properties");
            if (propertiesSection != null && bestiary.droppedIngredients != null && bestiary.droppedIngredients.Length > 0)
            {
                propertiesSection.style.display = DisplayStyle.Flex;
                var propertiesList = propertiesSection.Q<VisualElement>("properties-list");
                if (propertiesList != null)
                {
                    foreach (var ingredient in bestiary.droppedIngredients)
                    {
                        var ingredientLabel = new Label($"• {ingredient}");
                        ingredientLabel.AddToClassList("property-item");
                        propertiesList.Add(ingredientLabel);
                    }
                }
            }
        }

        void UpdateTabStates()
        {
            var bestiaryTab = root.Q<Button>("bestiary-tab");
            var recipeTab = root.Q<Button>("recipe-tab");
            var ingredientTab = root.Q<Button>("ingredient-tab");
            var helpTab = root.Q<Button>("help-tab");
            var bookmarkTab = root.Q<Button>("bookmark-tab");

            bestiaryTab.RemoveFromClassList("tab-active");
            recipeTab.RemoveFromClassList("tab-active");
            ingredientTab.RemoveFromClassList("tab-active");
            helpTab.RemoveFromClassList("tab-active");
            bookmarkTab.RemoveFromClassList("tab-active");

            switch (currentSection)
            {
                case EntryType.Bestiary: bestiaryTab.AddToClassList("tab-active"); break;
                case EntryType.Recipe: recipeTab.AddToClassList("tab-active"); break;
                case EntryType.Ingredient: ingredientTab.AddToClassList("tab-active"); break;
                case EntryType.Help: helpTab.AddToClassList("tab-active"); break;
                case EntryType.Bookmarked: bookmarkTab.AddToClassList("tab-active"); break;
            }
        }

        public void AddBestiaryEntry(BestiaryEntry entry)
        {
            bestiaryEntries.Add(entry);
            if (currentSection == EntryType.Bestiary)
            {
                UpdateFilteredEntries();
                UpdateDisplay();
            }
        }

        public void AddRecipeEntry(RecipeEntry entry)
        {
            recipeEntries.Add(entry);
            if (currentSection == EntryType.Recipe)
            {
                UpdateFilteredEntries();
                UpdateDisplay();
            }
        }

        public void AddIngredientEntry(IngredientEntry entry)
        {
            ingredientEntries.Add(entry);
            if (currentSection == EntryType.Ingredient)
            {
                UpdateFilteredEntries();
                UpdateDisplay();
            }
        }

        public void MarkEntryAsSeen(string entryTitle, EntryType entryType)
        {
            BaseEntry entry = null;

            switch (entryType)
            {
                case EntryType.Bestiary:
                    entry = bestiaryEntries.FirstOrDefault(e => e.title == entryTitle);
                    break;
                case EntryType.Recipe:
                    entry = recipeEntries.FirstOrDefault(e => e.title == entryTitle);
                    break;
                case EntryType.Ingredient:
                    entry = ingredientEntries.FirstOrDefault(e => e.title == entryTitle);
                    break;
            }

            if (entry != null)
            {
                entry.isSeen = true;
                UpdateDisplay();
            }
        }

        public void ToggleBookmark(string entryTitle, EntryType entryType)
        {
            BaseEntry entry = null;

            switch (entryType)
            {
                case EntryType.Bestiary:
                    entry = bestiaryEntries.FirstOrDefault(e => e.title == entryTitle);
                    break;
                case EntryType.Recipe:
                    entry = recipeEntries.FirstOrDefault(e => e.title == entryTitle);
                    break;
                case EntryType.Ingredient:
                    entry = ingredientEntries.FirstOrDefault(e => e.title == entryTitle);
                    break;
                case EntryType.Help:
                    entry = helpEntries.FirstOrDefault(e => e.title == entryTitle);
                    break;
            }

            if (entry != null)
            {
                entry.isBookmarked = !entry.isBookmarked;
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Opens the alchemy book to a specific section. Useful for integration with crafting systems.
        /// </summary>
        /// <param name="section">The section to open to</param>
        public void OpenToSection(EntryType section)
        {
            gameObject.SetActive(true);
            SwitchSection(section);
            showingCover = false;
            currentPageIndex = 0;
            UpdateDisplay();
        }

        /// <summary>
        /// Opens the alchemy book to the recipes section for easy access from crafting UIs
        /// </summary>
        public void OpenToRecipes()
        {
            OpenToSection(EntryType.Recipe);
        }

        /// <summary>
        /// Opens the alchemy book to the ingredients section for easy access from crafting UIs
        /// </summary>
        public void OpenToIngredients()
        {
            OpenToSection(EntryType.Ingredient);
        }

        /// <summary>
        /// Adds an alchemy recipe from the ScriptableObject system to the book
        /// </summary>
        /// <param name="alchemyRecipe">The alchemy recipe ScriptableObject</param>
        /// <param name="isDiscovered">Whether this recipe has been discovered by the player</param>
        public void AddAlchemyRecipe(AlchemyRecipe alchemyRecipe, bool isDiscovered = true)
        {
            if (alchemyRecipe == null) return;

            // Check if recipe already exists
            if (recipeEntries.Any(r => r.title == alchemyRecipe.name)) return;

            // Convert AlchemyRecipe to RecipeEntry
            var recipeEntry = new RecipeEntry
            {
                title = alchemyRecipe.name,
                description = alchemyRecipe.ItemDescription ?? "A mysterious alchemical recipe.",
                isSeen = isDiscovered,
                isBookmarked = false,
                isUniquePotionRecipe = alchemyRecipe.OutputPotion != null,
                recipeType = alchemyRecipe.OutputPotion != null ? RecipeType.UniquePotion : RecipeType.CustomPotion,
                requiredIngredients = new List<IngredientRequirement>()
            };

            // Add ingredients
            if (alchemyRecipe.InputIngredient1 != null)
            {
                recipeEntry.requiredIngredients.Add(new IngredientRequirement
                {
                    ingredientName = alchemyRecipe.InputIngredient1.ItemName,
                    isDiscovered = isDiscovered
                });
            }

            if (alchemyRecipe.InputIngredient2 != null)
            {
                recipeEntry.requiredIngredients.Add(new IngredientRequirement
                {
                    ingredientName = alchemyRecipe.InputIngredient2.ItemName,
                    isDiscovered = isDiscovered
                });
            }

            if (alchemyRecipe.InputIngredient3 != null)
            {
                recipeEntry.requiredIngredients.Add(new IngredientRequirement
                {
                    ingredientName = alchemyRecipe.InputIngredient3.ItemName,
                    isDiscovered = isDiscovered
                });
            }

            // Add result potion info to description
            if (alchemyRecipe.OutputPotion != null)
            {
                recipeEntry.description += $"\n\nCreates: {alchemyRecipe.OutputPotion.ItemName}";
                if (!string.IsNullOrEmpty(alchemyRecipe.OutputPotion.ItemDescription))
                {
                    recipeEntry.description += $"\n{alchemyRecipe.OutputPotion.ItemDescription}";
                }
            }

            AddRecipeEntry(recipeEntry);
        }

        /// <summary>
        /// Adds an ingredient from the ScriptableObject system to the book
        /// </summary>
        /// <param name="ingredient">The ingredient ScriptableObject</param>
        /// <param name="isDiscovered">Whether this ingredient has been discovered by the player</param>
        public void AddIngredient(Ingredient ingredient, bool isDiscovered = true)
        {
            if (ingredient == null) return;

            // Check if ingredient already exists
            if (ingredientEntries.Any(i => i.title == ingredient.ItemName)) return;

            // Convert Ingredient to IngredientEntry
            var ingredientEntry = new IngredientEntry
            {
                title = ingredient.ItemName,
                description = ingredient.ItemDescription ?? "A mysterious alchemical ingredient.",
                isSeen = isDiscovered,
                isBookmarked = false,
                aspect = Aspect.Corporeal, // Default, could be expanded based on ingredient properties
                dropSources = new List<DropSource>()
            };

            // Add some basic drop source info if available
            ingredientEntry.dropSources.Add(new DropSource
            {
                sourceName = "Various Locations",
                dropChance = "Common"
            });

            AddIngredientEntry(ingredientEntry);
        }

        /// <summary>
        /// Loads all available alchemy recipes from Resources folder
        /// </summary>
        public void LoadAllAlchemyRecipes()
        {
            var alchemyRecipes = Resources.LoadAll<AlchemyRecipe>("");
            foreach (var recipe in alchemyRecipes)
            {
                AddAlchemyRecipe(recipe, true); // All loaded recipes are considered discovered
            }

            Debug.Log($"📚 Loaded {alchemyRecipes.Length} alchemy recipes into the book");
        }

        /// <summary>
        /// Loads all available ingredients from Resources folder
        /// </summary>
        public void LoadAllIngredients()
        {
            var ingredients = Resources.LoadAll<Ingredient>("");
            foreach (var ingredient in ingredients)
            {
                AddIngredient(ingredient, true); // All loaded ingredients are considered discovered
            }

            Debug.Log($"📚 Loaded {ingredients.Length} ingredients into the book");
        }

        /// <summary>
        /// Load entries from Resources/AlchemyBook folder structure
        /// </summary>
        private void LoadEntriesFromResources()
        {
            // Load entries from each subfolder
            LoadBestiaryEntries();
            LoadRecipeEntries();
            LoadIngredientEntries();
            LoadHelpEntries();

            Debug.Log($"📚 Loaded entries from Resources/AlchemyBook - Bestiary: {bestiaryEntries.Count}, Recipes: {recipeEntries.Count}, Ingredients: {ingredientEntries.Count}, Help: {helpEntries.Count}");
        }

        private void LoadBestiaryEntries()
        {
            var entries = Resources.LoadAll<BestiaryEntry>("AlchemyBook/Bestiary");
            foreach (var entry in entries)
            {
                if (!bestiaryEntries.Contains(entry))
                {
                    bestiaryEntries.Add(entry);
                }
            }
        }

        private void LoadRecipeEntries()
        {
            var entries = Resources.LoadAll<RecipeEntry>("AlchemyBook/Recipes");
            foreach (var entry in entries)
            {
                if (!recipeEntries.Contains(entry))
                {
                    recipeEntries.Add(entry);
                }
            }
        }

        private void LoadIngredientEntries()
        {
            var entries = Resources.LoadAll<IngredientEntry>("AlchemyBook/Ingredients");
            foreach (var entry in entries)
            {
                if (!ingredientEntries.Contains(entry))
                {
                    ingredientEntries.Add(entry);
                }
            }
        }

        private void LoadHelpEntries()
        {
            var entries = Resources.LoadAll<HelpEntry>("AlchemyBook/Help");
            foreach (var entry in entries)
            {
                if (!helpEntries.Contains(entry))
                {
                    helpEntries.Add(entry);
                }
            }
        }
    }
}
