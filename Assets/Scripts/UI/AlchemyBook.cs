using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;
using GameSystems.CraftingMenu.AlchemyBookMenu.Sections;
using FourFatesStudios.ProjectWarden.GameSystems;

namespace FourFatesStudios.ProjectWarden.UI
{
    /// <summary>
    /// Potion Brewing Guide UI that displays different categories of game information
    /// Allows players to browse through recipes, ingredients, bestiary, and help content
    /// </summary>
    public class AlchemyBook : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private GameObject bookPanel;
        [SerializeField] private VisualTreeAsset bookUXML;
        [SerializeField] private StyleSheet bookUSS;

        [Header("Settings")]
        [SerializeField] private bool enableDebugLogging = true;

        // UI Elements
        private VisualElement rootElement;
        private VisualElement leftPage;
        private VisualElement rightPage;
        private TextField searchField;
        private Button closeButton;
        
        // Tab buttons
        private Button bookmarkTabButton;
        private Button bestiaryTabButton;
        private Button recipeTabButton;
        private Button ingredientTabButton;
        private Button helpTabButton;

        // Book entries from resources
        private List<RecipeEntry> recipeEntries;
        private List<IngredientEntry> ingredientEntries;
        private List<BestiaryEntry> bestiaryEntries;
        private List<HelpEntry> helpEntries;
        
        // State
        private string currentTab = "recipe"; // "bookmark", "bestiary", "recipe", "ingredient", "help"
        private string previousCraftingPanel = "";
        private string searchQuery = "";

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
        }

        private void Start()
        {
            SetupUI();
            LoadBookEntries();
            HideBook(); // Start hidden
        }

        private void SetupUI()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

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
            leftPage = rootElement.Q<VisualElement>("left-page");
            rightPage = rootElement.Q<VisualElement>("right-page");
            searchField = rootElement.Q<TextField>("search-field");
            closeButton = rootElement.Q<Button>("close-button");
            
            if (enableDebugLogging)
            {
                Debug.Log($"📚 Close button found: {closeButton != null}");
                if (closeButton != null)
                    Debug.Log($"📚 Close button name: {closeButton.name}");
            }
            
            // Tab buttons
            bookmarkTabButton = rootElement.Q<Button>("bookmark-tab");
            bestiaryTabButton = rootElement.Q<Button>("bestiary-tab");
            recipeTabButton = rootElement.Q<Button>("recipe-tab");
            ingredientTabButton = rootElement.Q<Button>("ingredient-tab");
            helpTabButton = rootElement.Q<Button>("help-tab");

            // Setup event handlers
            closeButton?.RegisterCallback<ClickEvent>(_ => {
                if (enableDebugLogging)
                    Debug.Log("📚 Close button (X) clicked - returning to crafting menu!");
                HideBook();
            });
            
            // Tab button handlers
            bookmarkTabButton?.RegisterCallback<ClickEvent>(_ => ShowTab("bookmark"));
            bestiaryTabButton?.RegisterCallback<ClickEvent>(_ => ShowTab("bestiary"));
            recipeTabButton?.RegisterCallback<ClickEvent>(_ => ShowTab("recipe"));
            ingredientTabButton?.RegisterCallback<ClickEvent>(_ => ShowTab("ingredient"));
            helpTabButton?.RegisterCallback<ClickEvent>(_ => ShowTab("help"));
            searchField?.RegisterValueChangedCallback(OnSearchChanged);

            // Setup keyboard shortcuts
            rootElement?.RegisterCallback<KeyDownEvent>(OnKeyDown);

            if (enableDebugLogging)
                Debug.Log("📚 Potion Brewing Guide UI setup complete");
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
                    evt.StopPropagation();
                    break;
                case KeyCode.Backspace:
                    GoBackToPreviousPanel();
                    evt.StopPropagation();
                    break;
                // Quick tab switching with number keys
                case KeyCode.Alpha1:
                    ShowTab("bookmark");
                    evt.StopPropagation();
                    break;
                case KeyCode.Alpha2:
                    ShowTab("bestiary");
                    evt.StopPropagation();
                    break;
                case KeyCode.Alpha3:
                    ShowTab("recipe");
                    evt.StopPropagation();
                    break;
                case KeyCode.Alpha4:
                    ShowTab("ingredient");
                    evt.StopPropagation();
                    break;
                case KeyCode.Alpha5:
                    ShowTab("help");
                    evt.StopPropagation();
                    break;
            }
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            searchQuery = evt.newValue?.ToLower() ?? "";
            RefreshUI();
        }

        private void LoadBookEntries()
        {
            // Load all book entries from the PotionBrewingGuide resource folders
            recipeEntries = Resources.LoadAll<RecipeEntry>("AlchemyBook/Recipes").ToList();
            ingredientEntries = Resources.LoadAll<IngredientEntry>("AlchemyBook/Ingredients").ToList();
            bestiaryEntries = Resources.LoadAll<BestiaryEntry>("AlchemyBook/Bestiary").ToList();
            helpEntries = Resources.LoadAll<HelpEntry>("AlchemyBook/Help").ToList();

            if (enableDebugLogging)
            {
                Debug.Log($"📚 Loaded {recipeEntries?.Count ?? 0} recipe entries, {ingredientEntries?.Count ?? 0} ingredient entries, {bestiaryEntries?.Count ?? 0} bestiary entries, {helpEntries?.Count ?? 0} help entries");
            }

            RefreshUI();
        }

        private void RefreshUI()
        {
            switch (currentTab)
            {
                case "bookmark":
                    ShowBookmarkedEntries();
                    break;
                case "bestiary":
                    ShowBestiaryEntries();
                    break;
                case "recipe":
                    ShowRecipeEntries();
                    break;
                case "ingredient":
                    ShowIngredientEntries();
                    break;
                case "help":
                    ShowHelpEntries();
                    break;
                default:
                    ShowRecipeEntries();
                    break;
            }
        }

        public void ShowTab(string tabName)
        {
            currentTab = tabName;
            UpdateTabButtons();
            RefreshUI();
            
            if (enableDebugLogging)
                Debug.Log($"📚 Switched to {tabName} tab");
        }

        private void UpdateTabButtons()
        {
            // Remove active class from all tabs
            bookmarkTabButton?.RemoveFromClassList("active");
            bestiaryTabButton?.RemoveFromClassList("active");
            recipeTabButton?.RemoveFromClassList("active");
            ingredientTabButton?.RemoveFromClassList("active");
            helpTabButton?.RemoveFromClassList("active");

            // Add active class to current tab
            switch (currentTab)
            {
                case "bookmark":
                    bookmarkTabButton?.AddToClassList("active");
                    break;
                case "bestiary":
                    bestiaryTabButton?.AddToClassList("active");
                    break;
                case "recipe":
                    recipeTabButton?.AddToClassList("active");
                    break;
                case "ingredient":
                    ingredientTabButton?.AddToClassList("active");
                    break;
                case "help":
                    helpTabButton?.AddToClassList("active");
                    break;
            }
        }

        private void ShowRecipeEntries()
        {
            leftPage?.Clear();
            rightPage?.Clear();

            if (recipeEntries == null || !recipeEntries.Any())
            {
                ShowEmptyPage("No recipe entries found.");
                return;
            }

            var filteredEntries = recipeEntries.Where(entry => 
                string.IsNullOrEmpty(searchQuery) || 
                entry.title.ToLower().Contains(searchQuery) ||
                entry.description.ToLower().Contains(searchQuery)
            ).ToList();

            DisplayEntries(filteredEntries.Cast<BaseEntry>().ToList(), "Recipe");
        }

        private void ShowIngredientEntries()
        {
            leftPage?.Clear();
            rightPage?.Clear();

            if (ingredientEntries == null || !ingredientEntries.Any())
            {
                ShowEmptyPage("No ingredient entries found.");
                return;
            }

            var filteredEntries = ingredientEntries.Where(entry => 
                string.IsNullOrEmpty(searchQuery) || 
                entry.title.ToLower().Contains(searchQuery) ||
                entry.description.ToLower().Contains(searchQuery)
            ).ToList();

            DisplayEntries(filteredEntries.Cast<BaseEntry>().ToList(), "Ingredient");
        }

        private void ShowBestiaryEntries()
        {
            leftPage?.Clear();
            rightPage?.Clear();

            if (bestiaryEntries == null || !bestiaryEntries.Any())
            {
                ShowEmptyPage("No bestiary entries found.");
                return;
            }

            var filteredEntries = bestiaryEntries.Where(entry => 
                string.IsNullOrEmpty(searchQuery) || 
                entry.title.ToLower().Contains(searchQuery) ||
                entry.description.ToLower().Contains(searchQuery)
            ).ToList();

            DisplayEntries(filteredEntries.Cast<BaseEntry>().ToList(), "Bestiary");
        }

        private void ShowHelpEntries()
        {
            leftPage?.Clear();
            rightPage?.Clear();

            if (helpEntries == null || !helpEntries.Any())
            {
                ShowEmptyPage("No help entries found.");
                return;
            }

            var filteredEntries = helpEntries.Where(entry => 
                string.IsNullOrEmpty(searchQuery) || 
                entry.title.ToLower().Contains(searchQuery) ||
                entry.description.ToLower().Contains(searchQuery)
            ).ToList();

            DisplayEntries(filteredEntries.Cast<BaseEntry>().ToList(), "Help");
        }

        private void ShowBookmarkedEntries()
        {
            leftPage?.Clear();
            rightPage?.Clear();

            var bookmarkedEntries = new List<BaseEntry>();
            
            // Collect all bookmarked entries from all categories
            if (recipeEntries != null)
                bookmarkedEntries.AddRange(recipeEntries.Where(e => e.isBookmarked).Cast<BaseEntry>());
            if (ingredientEntries != null)
                bookmarkedEntries.AddRange(ingredientEntries.Where(e => e.isBookmarked).Cast<BaseEntry>());
            if (bestiaryEntries != null)
                bookmarkedEntries.AddRange(bestiaryEntries.Where(e => e.isBookmarked).Cast<BaseEntry>());
            if (helpEntries != null)
                bookmarkedEntries.AddRange(helpEntries.Where(e => e.isBookmarked).Cast<BaseEntry>());

            if (!bookmarkedEntries.Any())
            {
                ShowEmptyPage("No bookmarked entries found.\nClick the bookmark icon on any entry to add it here.");
                return;
            }

            DisplayEntries(bookmarkedEntries, "Bookmarked");
        }

        private void ShowEmptyPage(string message)
        {
            var messageLabel = new Label(message);
            messageLabel.AddToClassList("empty-page-message");
            leftPage?.Add(messageLabel);
        }

        private void DisplayEntries(List<BaseEntry> entries, string categoryName)
        {
            if (!entries.Any())
            {
                ShowEmptyPage($"No {categoryName.ToLower()} entries found.");
                return;
            }

            // Display the first entry on the left page, second on right
            for (int i = 0; i < Math.Min(entries.Count, 2); i++)
            {
                var targetPage = i == 0 ? leftPage : rightPage;
                var entry = entries[i];
                var entryElement = CreateEntryElement(entry);
                targetPage?.Add(entryElement);
            }

            if (enableDebugLogging)
                Debug.Log($"📚 Displaying {Math.Min(entries.Count, 2)} {categoryName} entries");
        }

        private VisualElement CreateEntryElement(BaseEntry entry)
        {
            var container = new VisualElement();
            container.AddToClassList("book-entry");

            // Add bookmark button at the top
            var headerContainer = new VisualElement();
            headerContainer.AddToClassList("entry-header");
            
            var bookmarkButton = new Button();
            bookmarkButton.AddToClassList("bookmark-button");
            bookmarkButton.text = entry.isBookmarked ? "★" : "☆";
            if (entry.isBookmarked)
                bookmarkButton.AddToClassList("bookmarked");
            
            bookmarkButton.clicked += () => ToggleBookmark(entry);
            headerContainer.Add(bookmarkButton);
            container.Add(headerContainer);

            // Use the entry's built-in visual creation method
            var entryVisual = entry.CreateEntryVisual();
            container.Add(entryVisual);

            return container;
        }

        private void ToggleBookmark(BaseEntry entry)
        {
            entry.isBookmarked = !entry.isBookmarked;
            RefreshUI();
            if (enableDebugLogging)
                Debug.Log($"📚 Toggled bookmark for {entry.GetEntryType()}: {entry.title}");
        }

        #region Public Interface

        public void ShowBook(string fromPanel = "")
        {
            previousCraftingPanel = fromPanel;
            
            if (bookPanel != null)
                bookPanel.SetActive(true);
            
            // Make sure we start with recipe tab and update buttons
            currentTab = "recipe";
            UpdateTabButtons();
            RefreshUI();

            if (enableDebugLogging)
                Debug.Log($"📚 Opening Potion Brewing Guide (from: {fromPanel})");
        }

        public void HideBook()
        {
            if (bookPanel != null)
                bookPanel.SetActive(false);

            if (enableDebugLogging)
                Debug.Log("📚 Closing Potion Brewing Guide");
        }

        public void GoBackToPreviousPanel()
        {
            if (enableDebugLogging)
                Debug.Log("📚 GoBackToPreviousPanel called - starting back navigation");
            
            // Hide the book first
            HideBook();
            
            // Use the CraftingNavigationController's back navigation system
            var navigationController = FindFirstObjectByType<CraftingNavigationController>();
            if (navigationController != null)
            {
                if (enableDebugLogging)
                    Debug.Log("📚 Found CraftingNavigationController");
                
                // If we have a specific panel to go back to, navigate there
                if (!string.IsNullOrEmpty(previousCraftingPanel))
                {
                    navigationController.ShowPanel(previousCraftingPanel);
                    if (enableDebugLogging)
                        Debug.Log($"📚 Navigating back to: {previousCraftingPanel}");
                }
                else
                {
                    // Use the navigation controller's built-in back functionality
                    navigationController.GoBack();
                    if (enableDebugLogging)
                        Debug.Log("📚 Using navigation controller's back functionality");
                }
            }
            else
            {
                if (enableDebugLogging)
                    Debug.Log("📚 CraftingNavigationController not found, trying fallback");
                
                // Fallback - try to find a basic crafting manager
                var craftingManager = FindFirstObjectByType<CraftingMenuManager>();
                if (craftingManager != null)
                {
                    if (enableDebugLogging)
                        Debug.Log("📚 Fallback: Using CraftingMenuManager back navigation");
                    craftingManager.NavigateBack();
                }
                else
                {
                    if (enableDebugLogging)
                        Debug.LogWarning("⚠️ No navigation controller found - staying in book");
                }
            }
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
            ShowTab("recipe");
        }

        public void OpenToRecipes()
        {
            ShowBook();
            ShowTab("recipe");
        }

        private void ToggleTab()
        {
            // Cycle through available tabs
            switch (currentTab)
            {
                case "bookmark":
                    ShowTab("bestiary");
                    break;
                case "bestiary":
                    ShowTab("recipe");
                    break;
                case "recipe":
                    ShowTab("ingredient");
                    break;
                case "ingredient":
                    ShowTab("help");
                    break;
                case "help":
                    ShowTab("bookmark");
                    break;
                default:
                    ShowTab("recipe");
                    break;
            }
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #region Debug Methods
        
        [ContextMenu("Test Back Navigation")]
        private void TestBackNavigation()
        {
            Debug.Log("🧪 Testing Potion Brewing Guide back navigation...");
            
            var navigationController = FindFirstObjectByType<CraftingNavigationController>();
            var craftingManager = FindFirstObjectByType<CraftingMenuManager>();
            
            Debug.Log($"  • CraftingNavigationController: {(navigationController != null ? "✅ Found" : "❌ Missing")}");
            Debug.Log($"  • CraftingMenuManager: {(craftingManager != null ? "✅ Found" : "❌ Missing")}");
            Debug.Log($"  • Previous Panel: {(string.IsNullOrEmpty(previousCraftingPanel) ? "❌ Not Set" : $"✅ {previousCraftingPanel}")}");
            
            // Test the actual navigation
            GoBackToPreviousPanel();
        }

        [ContextMenu("Test Open From Main Menu")]
        private void TestOpenFromMainMenu()
        {
            ShowBook("CraftingMenuSystem");
        }

        [ContextMenu("Test Complete Navigation Flow")]
        private void TestCompleteNavigationFlow()
        {
            Debug.Log("🧪 Testing complete navigation flow...");
            
            // Start from main menu
            var navigationController = FindFirstObjectByType<CraftingNavigationController>();
            if (navigationController != null)
            {
                navigationController.ShowMainMenu();
                Debug.Log("  1. ✅ Showing main menu");
                
                // Open book
                ShowBook("CraftingMenuSystem");
                Debug.Log("  2. ✅ Opening Potion Brewing Guide from main menu");
                
                // Test back navigation
                Debug.Log("  3. 🧪 Testing back navigation...");
                GoBackToPreviousPanel();
            }
            else
            {
                Debug.LogError("  ❌ CraftingNavigationController not found!");
            }
        }

        #endregion
    }
}