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
            InitializeHelpEntries();
            SetupCameraPosition();
            SetupUI();
            
            // Load alchemy recipes and ingredients from the project
            LoadAllAlchemyRecipes();
            LoadAllIngredients();
            
            UpdateDisplay();
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
            root = uiDocument.rootVisualElement;

            if (bookStyleSheet)
                root.styleSheets.Add(bookStyleSheet);

            tabsContainer = root.Q<VisualElement>("tabs-container");
            leftPage = root.Q<VisualElement>("left-page");
            rightPage = root.Q<VisualElement>("right-page");
            frontCover = root.Q<VisualElement>("front-cover");
            backCover = root.Q<VisualElement>("back-cover");
            nextButton = root.Q<Button>("next-button");
            prevButton = root.Q<Button>("prev-button");
            searchField = root.Q<TextField>("search-field");
            searchButton = root.Q<Button>("search-button");

            SetupTabButtons();

            nextButton.clicked += NextPage;
            prevButton.clicked += PrevPage;

            searchButton.clicked += PerformSearch;
            searchField.RegisterCallback<KeyDownEvent>(OnSearchKeyDown);
        }

        void SetupTabButtons()
        {
            var bestiaryTab = root.Q<Button>("bestiary-tab");
            var recipeTab = root.Q<Button>("recipe-tab");
            var ingredientTab = root.Q<Button>("ingredient-tab");
            var helpTab = root.Q<Button>("help-tab");
            var bookmarkTab = root.Q<Button>("bookmark-tab");

            bestiaryTab.clicked += () => SwitchSection(EntryType.Bestiary);
            recipeTab.clicked += () => SwitchSection(EntryType.Recipe);
            ingredientTab.clicked += () => SwitchSection(EntryType.Ingredient);
            helpTab.clicked += () => SwitchSection(EntryType.Help);
            bookmarkTab.clicked += () => SwitchSection(EntryType.Bookmarked);
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
                leftPage.Add(filteredEntries[currentPageIndex].CreateEntryVisual());

            if (currentPageIndex + 1 < filteredEntries.Count)
                rightPage.Add(filteredEntries[currentPageIndex + 1].CreateEntryVisual());

            UpdateTabStates();
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
        /// Closes the alchemy book
        /// </summary>
        public void CloseBook()
        {
            gameObject.SetActive(false);
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
    }
}
