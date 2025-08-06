using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyBookMenu.Sections;
using GameSystems.CraftingMenu.AlchemyBookMenu.Sections;
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
            UpdateDisplay();
        }

        void SetupCameraPosition()
        {
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera != null)
            {
                var cameraTransform = mainCamera.transform;
                transform.position = cameraTransform.position + cameraTransform.forward * distanceFromCamera;
                transform.LookAt(cameraTransform.position); // Use Vector3 to avoid ambiguity
            }
        }

        void InitializeHelpEntries()
        {
            helpEntries = new List<HelpEntry>
            {
                new HelpEntry
                {
                    title = "Refinements Guide",
                    topicType = HelpTopicType.Refinements,
                    content = "Lorem ipsum...",
                    isSeen = true
                },
                new HelpEntry
                {
                    title = "Potion Crafting",
                    topicType = HelpTopicType.PotionCrafting,
                    content = "Lorem ipsum...",
                    isSeen = true
                },
                new HelpEntry
                {
                    title = "Harvesting",
                    topicType = HelpTopicType.Harvesting,
                    content = "Lorem ipsum...",
                    isSeen = true
                },
                new HelpEntry
                {
                    title = "Combat System",
                    topicType = HelpTopicType.CombatSystem,
                    content = "Lorem ipsum...",
                    isSeen = true
                },
                new HelpEntry
                {
                    title = "Template Page",
                    topicType = HelpTopicType.Template,
                    content = "Lorem ipsum...",
                    isSeen = true
                }
            };
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
                e.encounterLocations.Any(l => l.ToLower().Contains(searchTerm)) ||
                e.skills.Any(s => s.ToLower().Contains(searchTerm))
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
                e.content.ToLower().Contains(searchTerm)
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
    }
}
