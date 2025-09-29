using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FourFatesStudios.ProjectWarden.GridDemo.UI;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Consolidated Grid Demo UI Controller - Single component managing all UI functionality
    /// Replaces GridDemoUI, GridDemoUIManager, and CompactUIDesigner to prevent conflicts
    /// </summary>
    public class GridDemoUIController : MonoBehaviour
    {
        [Header("Singleton & Auto-Setup")]
        [SerializeField] private bool createUIOnStart = false; // Changed to false - UI will be created when recipe is selected
        [SerializeField] private bool autoLoadIngredients = true;
        [SerializeField] private bool debugMode = true;
        
        [Header("UI Design Settings")]
        [SerializeField] private Vector2 compactButtonSize = new Vector2(80f, 35f);
        [SerializeField] private Vector2 compactSpacing = new Vector2(5f, 5f);
        [SerializeField] private int buttonsPerRow = 2;
        [SerializeField] private float sidebarWidth = 200f;
        
        [Header("Colors & Style")]
        [SerializeField] private Color sidebarBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        [SerializeField] private Color buttonBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private float buttonFontSize = 10f;
        
        // Singleton instance
        private static GridDemoUIController instance;
        
        // Core references
        private GridGameManager gridManager;
        private GameObject currentSidebar;
        private RightPanelManager rightPanel;
        
        // Events
        public System.Action<Ingredient> OnIngredientSelected;
        public System.Action OnGridCleared;
        public System.Action OnGridRandomized;
        public System.Action<int> OnGridSizeChanged;
        
        // Crafting state management
        private bool isCrafting = false;
        private UnityEngine.UI.Button craftButton;
        
        #region Singleton & Initialization
        
        private void Awake()
        {
            // Singleton pattern - prevent duplicate UI controllers
            if (instance != null && instance != this)
            {
                Debug.LogWarning("🚨 Multiple GridDemoUIController instances found! Destroying duplicate to prevent conflicts.");
                Destroy(this);
                return;
            }
            instance = this;
            
            // Find core components
            gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager == null)
            {
                Debug.LogError("❌ GridGameManager not found! GridDemoUIController requires GridGameManager.");
            }
        }
        
        private void Start()
        {
            if (createUIOnStart)
            {
                StartCoroutine(SetupCompleteUI());
            }
        }
        
        private IEnumerator SetupCompleteUI()
        {
            yield return new WaitForEndOfFrame();
            
            if (debugMode)
            {
                Debug.Log("🎨 GridDemoUIController: Starting consolidated UI setup...");
            }
            
            // Step 1: Clear any existing duplicate UI elements
            ClearExistingUI();
            
            // Step 2: Load ingredients if needed
            if (autoLoadIngredients)
            {
                EnsureIngredientsLoaded();
            }
            
            // Step 3: Create the consolidated UI
            CreateConsolidatedUI();
            
            // Step 4: Setup additional components #temporarily disabled
            //SetupSupportingComponents();
            
            if (debugMode)
            {
                LogSetupResults();
            }
        }
        
        #endregion
        
        #region UI Creation
        
        private void ClearExistingUI()
        {
            // Find and remove any existing compact sidebars to prevent duplicates
            GameObject[] existingSidebars = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go => go.name.Contains("Compact Sidebar"))
                .ToArray();
                
            foreach (var sidebar in existingSidebars)
            {
                if (debugMode)
                {
                    Debug.Log($"🗑️ Removing existing sidebar: {sidebar.name}");
                }
                DestroyImmediate(sidebar);
            }
            
            // Also hide any old UI panels
            Transform oldIngredientPanel = transform.Find("Ingredient Panel");
            Transform oldInfoPanel = transform.Find("Info Panel");
            
            if (oldIngredientPanel != null) oldIngredientPanel.gameObject.SetActive(false);
            if (oldInfoPanel != null) oldInfoPanel.gameObject.SetActive(false);
        }
        
        [ContextMenu("Create Consolidated UI")]
        public void CreateConsolidatedUI()
        {
            if (debugMode)
            {
                Debug.Log("🎨 Creating consolidated Grid Demo UI...");
            }
            
            // Setup canvas properties
            SetupCanvas();
            
            // Create the main sidebar
            currentSidebar = CreateMainSidebar();
            
            // Create sections
            CreateIngredientSection(currentSidebar);
            CreateControlsSection(currentSidebar);
            
            if (debugMode)
            {
                Debug.Log("✅ Consolidated UI created successfully!");
            }
        }
        
        private void SetupCanvas()
        {
            Canvas canvas = GetComponent<Canvas>();
            RectTransform canvasRect = GetComponent<RectTransform>();
            
            if (canvas != null && canvasRect != null)
            {
                canvasRect.anchorMin = Vector2.zero;
                canvasRect.anchorMax = Vector2.one;
                canvasRect.offsetMin = Vector2.zero;
                canvasRect.offsetMax = Vector2.zero;
            }
        }
        
        private GameObject CreateMainSidebar()
        {
            GameObject sidebar = new GameObject("Compact Sidebar");
            sidebar.transform.SetParent(transform, false);
            
            RectTransform sidebarRect = sidebar.AddComponent<RectTransform>();
            sidebarRect.anchorMin = new Vector2(0, 0);
            sidebarRect.anchorMax = new Vector2(0, 1);
            sidebarRect.pivot = new Vector2(0, 0.5f);
            sidebarRect.sizeDelta = new Vector2(sidebarWidth, 0);
            sidebarRect.anchoredPosition = Vector2.zero;
            
            Image sidebarBg = sidebar.AddComponent<Image>();
            sidebarBg.color = sidebarBackgroundColor;
            
            VerticalLayoutGroup verticalLayout = sidebar.AddComponent<VerticalLayoutGroup>();
            verticalLayout.childAlignment = TextAnchor.UpperCenter;
            verticalLayout.spacing = 10f;
            verticalLayout.padding = new RectOffset(10, 10, 10, 10);
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = false;
            verticalLayout.childScaleWidth = true;
            verticalLayout.childScaleHeight = false;
            
            return sidebar;
        }
        
        #endregion
        
        #region Ingredient Section
        
        private void CreateIngredientSection(GameObject sidebar)
        {
            CreateSectionHeader(sidebar, "INGREDIENTS");
            GameObject scrollArea = CreateScrollArea(sidebar, "Ingredient Scroll");
            GameObject container = CreateIngredientButtonContainer(scrollArea);
            
            if (gridManager != null && gridManager.availableIngredients != null && gridManager.availableIngredients.Count > 0)
            {
                if (debugMode)
                {
                    Debug.Log($"🧩 Creating ingredient buttons for {gridManager.availableIngredients.Count} ingredients");
                }
                CreateIngredientButtons(container);
            }
            else
            {
                if (debugMode)
                {
                    Debug.LogWarning("🚨 No ingredients available! Creating placeholder message.");
                }
                CreateNoIngredientsMessage(container);
            }
        }
        
        private GameObject CreateSectionHeader(GameObject parent, string title)
        {
            GameObject header = new GameObject($"Header_{title}");
            header.transform.SetParent(parent.transform, false);
            
            RectTransform headerRect = header.AddComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0, 25f);
            
            TextMeshProUGUI headerText = header.AddComponent<TextMeshProUGUI>();
            headerText.text = title;
            headerText.fontSize = 14f;
            headerText.fontStyle = FontStyles.Bold;
            headerText.color = Color.white;
            headerText.alignment = TextAlignmentOptions.Center;
            
            LayoutElement headerLayout = header.AddComponent<LayoutElement>();
            headerLayout.preferredHeight = 25f;
            headerLayout.flexibleHeight = 0f;
            
            return header;
        }
        
        private GameObject CreateScrollArea(GameObject parent, string name)
        {
            GameObject scrollArea = new GameObject(name);
            scrollArea.transform.SetParent(parent.transform, false);
            
            RectTransform scrollRect = scrollArea.AddComponent<RectTransform>();
            scrollRect.sizeDelta = new Vector2(0, 250f);
            
            Image scrollBg = scrollArea.AddComponent<Image>();
            scrollBg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            
            ScrollRect scroll = scrollArea.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;
            scroll.inertia = true;
            scroll.decelerationRate = 0.135f;
            scroll.elasticity = 0.1f;
            
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollArea.transform, false);
            
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(2, 2);
            viewportRect.offsetMax = new Vector2(-2, -2);
            
            viewport.AddComponent<RectMask2D>();
            
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 100f);
            contentRect.anchoredPosition = Vector2.zero;
            
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            
            LayoutElement scrollLayout = scrollArea.AddComponent<LayoutElement>();
            scrollLayout.preferredHeight = 250f;
            scrollLayout.flexibleHeight = 1f;
            
            return content;
        }
        
        private GameObject CreateIngredientButtonContainer(GameObject parent)
        {
            GameObject container = new GameObject("Button Container");
            container.transform.SetParent(parent.transform, false);
            
            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 1);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.pivot = new Vector2(0.5f, 1);
            containerRect.anchoredPosition = Vector2.zero;
            
            GridLayoutGroup gridLayout = container.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = compactButtonSize;
            gridLayout.spacing = compactSpacing;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = buttonsPerRow;
            gridLayout.padding = new RectOffset(5, 5, 5, 5);
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            
            ContentSizeFitter sizeFitter = container.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            
            return container;
        }
        
        private void CreateIngredientButtons(GameObject container)
        {
            foreach (var ingredient in gridManager.availableIngredients)
            {
                if (ingredient != null)
                {
                    CreateIngredientButton(ingredient, container);
                }
            }
            
            // Force content size update
            StartCoroutine(UpdateScrollViewContent(container));
        }
        
        private void CreateIngredientButton(Ingredient ingredient, GameObject container)
        {
            GameObject buttonObj = new GameObject($"Btn_{ingredient.ItemName}");
            buttonObj.transform.SetParent(container.transform, false);
            
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = Color.white;
            
            Button button = buttonObj.AddComponent<Button>();
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = ingredient.ItemName;
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = Mathf.RoundToInt(buttonFontSize * 0.8f);
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.black;
            buttonText.raycastTarget = false;
            
            // Add ingredient functionality
            DraggableIngredient draggable = buttonObj.AddComponent<DraggableIngredient>();
            draggable.Initialize(ingredient, gridManager);
            
            IngredientButton ingredientButtonComponent = buttonObj.AddComponent<IngredientButton>();
            ingredientButtonComponent.SetupIngredient(ingredient, gridManager);
            
            // Add click listener
            button.onClick.AddListener(() => SelectIngredient(ingredient));
        }
        
        private void CreateNoIngredientsMessage(GameObject container)
        {
            GameObject messageObj = new GameObject("No Ingredients Message");
            messageObj.transform.SetParent(container.transform, false);
            
            RectTransform messageRect = messageObj.AddComponent<RectTransform>();
            messageRect.sizeDelta = new Vector2(180, 60);
            
            TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
            messageText.text = "No ingredients\nfound!\n\nPress F5 to reload";
            messageText.fontSize = 12f;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.color = Color.yellow;
            messageText.fontStyle = FontStyles.Italic;
        }
        
        private IEnumerator UpdateScrollViewContent(GameObject container)
        {
            yield return new WaitForEndOfFrame();
            
            var scrollRect = container.GetComponentInParent<ScrollRect>();
            if (scrollRect != null && scrollRect.content != null)
            {
                var layoutGroup = container.GetComponent<GridLayoutGroup>();
                if (layoutGroup != null)
                {
                    float totalHeight = CalculateContentHeight(container, layoutGroup);
                    scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, totalHeight);
                    
                    if (debugMode)
                    {
                        Debug.Log($"📜 Scroll content height set to: {totalHeight}px");
                    }
                }
            }
        }
        
        private float CalculateContentHeight(GameObject container, GridLayoutGroup layoutGroup)
        {
            int childCount = container.transform.childCount;
            int rows = Mathf.CeilToInt((float)childCount / layoutGroup.constraintCount);
            
            float totalHeight = layoutGroup.padding.top + layoutGroup.padding.bottom;
            totalHeight += rows * layoutGroup.cellSize.y;
            totalHeight += (rows - 1) * layoutGroup.spacing.y;
            
            return totalHeight;
        }
        
        #endregion
        
        #region Controls Section
        
        private void CreateControlsSection(GameObject sidebar)
        {
            CreateSectionHeader(sidebar, "CONTROLS");
            
            GameObject controlsContainer = new GameObject("Controls Container");
            controlsContainer.transform.SetParent(sidebar.transform, false);
            
            RectTransform controlsRect = controlsContainer.AddComponent<RectTransform>();
            controlsRect.sizeDelta = new Vector2(0, 120f);
            
            VerticalLayoutGroup controlsLayout = controlsContainer.AddComponent<VerticalLayoutGroup>();
            controlsLayout.spacing = 5f;
            controlsLayout.padding = new RectOffset(5, 5, 5, 5);
            controlsLayout.childControlWidth = true;
            controlsLayout.childControlHeight = false;
            
            CreateCraftButton(controlsContainer);
            CreateClearButton(controlsContainer);
            
            LayoutElement controlsLayoutElement = controlsContainer.AddComponent<LayoutElement>();
            controlsLayoutElement.preferredHeight = 120f;
            controlsLayoutElement.flexibleHeight = 0f;
        }
        
        private void CreateCraftButton(GameObject parent)
        {
            GameObject craftButtonObj = new GameObject("Craft Potion Button");
            craftButtonObj.transform.SetParent(parent.transform, false);
            
            RectTransform buttonRect = craftButtonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(0, 35f);
            
            Image buttonImage = craftButtonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 0.9f);
            
            Button button = craftButtonObj.AddComponent<Button>();
            craftButton = button; // Store reference for state management
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(craftButtonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "CRAFT POTION";
            buttonText.fontSize = 12f;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            buttonText.fontStyle = FontStyles.Bold;
            
            button.onClick.AddListener(CraftPotion);
            
            LayoutElement layoutElement = craftButtonObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 35f;
        }
        
        private void CreateClearButton(GameObject parent)
        {
            GameObject clearButtonObj = new GameObject("Clear Button");
            clearButtonObj.transform.SetParent(parent.transform, false);
            
            RectTransform buttonRect = clearButtonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(0, 35f);
            
            Image buttonImage = clearButtonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);
            
            Button button = clearButtonObj.AddComponent<Button>();
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(clearButtonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "CLEAR GRID";
            buttonText.fontSize = 12f;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            buttonText.fontStyle = FontStyles.Bold;
            
            button.onClick.AddListener(ClearGrid);
            
            LayoutElement layoutElement = clearButtonObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 35f;
        }
        
        #endregion
        
        #region Core Functionality
        
        private void EnsureIngredientsLoaded()
        {
            if (gridManager == null) return;
            
            if (gridManager.availableIngredients == null || gridManager.availableIngredients.Count == 0)
            {
                if (debugMode)
                {
                    Debug.Log("📦 Loading ingredients from Resources...");
                }
                LoadIngredientsFromResources();
            }
        }
        
        private void LoadIngredientsFromResources()
        {
            var ingredients = Resources.LoadAll<Ingredient>("Items/Ingredients");
            
            if (ingredients.Length == 0)
            {
                ingredients = Resources.LoadAll<Ingredient>("Ingredients");
            }
            
            if (ingredients.Length == 0)
            {
                ingredients = Resources.LoadAll<Ingredient>("");
            }
            
            if (ingredients.Length > 0)
            {
                if (gridManager.availableIngredients == null)
                {
                    gridManager.availableIngredients = new List<Ingredient>();
                }
                gridManager.availableIngredients.Clear();
                gridManager.availableIngredients.AddRange(ingredients);
                
                if (debugMode)
                {
                    Debug.Log($"✅ Loaded {ingredients.Length} ingredients from Resources");
                }
            }
            else if (debugMode)
            {
                Debug.LogWarning("⚠️ No ingredients found in Resources!");
            }
        }
        
        private void SetupSupportingComponents()
        {
            // Setup right panel manager if it doesn't exist
            rightPanel = GetComponentInChildren<RightPanelManager>();
            if (rightPanel == null)
            {
                GameObject rightPanelObj = new GameObject("Right Panel Manager");
                rightPanelObj.transform.SetParent(transform, false);
                rightPanel = rightPanelObj.AddComponent<RightPanelManager>();
            }
        }
        
        private void SelectIngredient(Ingredient ingredient)
        {
            if (gridManager != null)
            {
                gridManager.SelectIngredient(ingredient);
                OnIngredientSelected?.Invoke(ingredient);
                
                if (debugMode)
                {
                    Debug.Log($"🧩 Selected ingredient: {ingredient.ItemName}");
                }
            }
        }
        
        private void ClearGrid()
        {
            if (gridManager != null)
            {
                // Player manual clear - preserve obstacles
                gridManager.ClearGridPreserveObstacles();
                OnGridCleared?.Invoke();
                
                if (debugMode)
                {
                    Debug.Log("🧹 Grid cleared (obstacles preserved)!");
                }
            }
        }
        
        private void CraftPotion()
        {
            // Prevent spam-clicking by checking if crafting is already in progress
            if (isCrafting)
            {
                if (debugMode)
                {
                    Debug.Log("🧪 Crafting already in progress - ignoring button click");
                }
                return;
            }
            
            if (gridManager != null)
            {
                // Set crafting state and disable button
                isCrafting = true;
                if (craftButton != null)
                {
                    craftButton.interactable = false;
                    
                    // Update button text to show crafting in progress
                    var buttonText = craftButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = "CRAFTING...";
                        buttonText.color = Color.yellow;
                    }
                }
                
                // Delegate to the proper GridGameManager recipe system instead of generating fake names
                if (debugMode)
                {
                    Debug.Log("🧪 UI Craft Button: Starting crafting process - delegating to GridGameManager recipe system");
                }
                
                // Use reflection to call the proper recipe checking method
                try
                {
                    var checkMethod = typeof(GridGameManager).GetMethod("CheckForRecipeMatches", 
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    if (checkMethod != null)
                    {
                        checkMethod.Invoke(gridManager, null);
                    }
                    else
                    {
                        Debug.LogWarning("🧪 Could not find CheckForRecipeMatches method in GridGameManager");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"🧪 Error calling recipe system: {ex.Message}");
                }
                finally
                {
                    // Re-enable the button after a longer delay to ensure atomic processing completes
                    StartCoroutine(ResetCraftingStateAfterDelay(2.0f)); // Extended from 1.0f to 2.0f
                }
                
                return;
            }
            
            Debug.LogWarning("🧪 GridGameManager not found - cannot craft potion");
        }
        
        /// <summary>
        /// Coroutine to reset the crafting state after a delay to prevent spam-clicking
        /// Extended delay to ensure atomic processing completes
        /// </summary>
        private IEnumerator ResetCraftingStateAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            // Additional safety check - ensure GridGameManager is not still processing
            bool needsExtraWait = false;
            if (gridManager != null)
            {
                // Use reflection to check if the GridGameManager is still processing
                try
                {
                    var processingField = typeof(GridGameManager).GetField("isProcessingRecipe", 
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    if (processingField != null)
                    {
                        bool stillProcessing = (bool)processingField.GetValue(gridManager);
                        if (stillProcessing)
                        {
                            if (debugMode)
                            {
                                Debug.Log("🧪 GridGameManager still processing - extending UI cooldown...");
                            }
                            
                            needsExtraWait = true;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    if (debugMode)
                    {
                        Debug.LogWarning($"🧪 Could not check GridGameManager processing state: {ex.Message}");
                    }
                }
            }
            
            // Wait additional time if still processing (moved outside try-catch)
            if (needsExtraWait)
            {
                yield return new WaitForSeconds(1.0f);
            }
            
            // Reset crafting state
            isCrafting = false;
            
            // Re-enable button and restore original text
            if (craftButton != null)
            {
                craftButton.interactable = true;
                
                var buttonText = craftButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "CRAFT POTION";
                    buttonText.color = Color.white;
                }
            }
            
            if (debugMode)
            {
                Debug.Log("🧪 Crafting state reset - button ready for next use");
            }
        }
        
        private string GeneratePotionName(List<Ingredient> ingredients)
        {
            if (ingredients.Count == 0) return "Empty Potion";
            
            // Simple name generation based on primary ingredient
            var primaryIngredient = ingredients[0];
            return $"{primaryIngredient.ItemName} Potion";
        }
        
        private string GenerateRecipeKey(List<Ingredient> ingredients)
        {
            if (ingredients.Count == 0) return "empty";
            
            // Generate a simple key based on sorted ingredient names
            var sortedNames = ingredients.Select(i => i.ItemName).OrderBy(name => name);
            return string.Join("_", sortedNames).ToLower().Replace(" ", "");
        }
        
        private List<Ingredient> GetPlacedIngredientsFromGrid()
        {
            var uniqueIngredients = new List<Ingredient>();
            
            if (gridManager == null) return uniqueIngredients;
            
            try
            {
                var gridCellsField = typeof(GridGameManager).GetField("gridCells", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (gridCellsField != null)
                {
                    var gridCells = (GridCell[,])gridCellsField.GetValue(gridManager);
                    
                    if (gridCells != null)
                    {
                        var seenIngredients = new HashSet<Ingredient>();
                        
                        for (int x = 0; x < gridCells.GetLength(0); x++)
                        {
                            for (int y = 0; y < gridCells.GetLength(1); y++)
                            {
                                var cell = gridCells[x, y];
                                if (cell != null && cell.IsOccupied && cell.OccupiedByIngredient != null)
                                {
                                    if (seenIngredients.Add(cell.OccupiedByIngredient))
                                    {
                                        uniqueIngredients.Add(cell.OccupiedByIngredient);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                if (debugMode)
                {
                    Debug.LogWarning($"🧪 Could not access grid cells: {ex.Message}");
                }
            }
            
            return uniqueIngredients;
        }
        
        #endregion
        
        #region Public Methods & Context Menus
        
        [ContextMenu("🔄 Refresh UI")]
        public void RefreshUI()
        {
            if (autoLoadIngredients)
            {
                EnsureIngredientsLoaded();
            }
            
            // Clear and recreate UI
            ClearExistingUI();
            CreateConsolidatedUI();
            SetupSupportingComponents();
            
            if (debugMode)
            {
                Debug.Log("🔄 UI refreshed successfully!");
            }
        }
        
        /// <summary>
        /// Starts the Grid UI for recipe crafting
        /// This should be called when a recipe is selected from the menu
        /// </summary>
        public void StartGridUIForRecipe()
        {
            if (debugMode)
            {
                Debug.Log("🎯 GridDemoUIController: Starting Grid UI for recipe crafting...");
            }
            
            // Ensure this GameObject is active
            gameObject.SetActive(true);
            
            // Setup the complete UI if it hasn't been created yet
            if (currentSidebar == null)
            {
                StartCoroutine(SetupCompleteUI());
            }
            else if (debugMode)
            {
                Debug.Log("✅ Grid UI already exists, activating...");
            }
        }
        
        /// <summary>
        /// Public static method to start grid UI from external systems
        /// </summary>
        public static void StartGridUI()
        {
            GridDemoUIController controller = FindFirstObjectByType<GridDemoUIController>();
            if (controller != null)
            {
                controller.StartGridUIForRecipe();
            }
            else
            {
                Debug.LogError("❌ GridDemoUIController not found! Cannot start Grid UI.");
            }
        }
        
        [ContextMenu("🧹 Force Clear All UI")]
        public void ForceClearAllUI()
        {
            ClearExistingUI();
            
            if (debugMode)
            {
                Debug.Log("🧹 All UI elements cleared!");
            }
        }
        
        [ContextMenu("📊 Debug UI State")]
        public void DebugUIState()
        {
            LogSetupResults();
        }
        
        private void LogSetupResults()
        {
            Debug.Log("📊 === GridDemoUIController Status ===");
            Debug.Log($"Grid Manager: {gridManager != null}");
            Debug.Log($"Right Panel: {rightPanel != null}");
            Debug.Log($"Current Sidebar: {currentSidebar != null}");
            Debug.Log($"Ingredients Count: {gridManager?.availableIngredients?.Count ?? 0}");
            
            GameObject sidebar = GameObject.Find("Compact Sidebar");
            Debug.Log($"Sidebar in Scene: {sidebar != null}");
            
            if (sidebar != null)
            {
                var buttonContainer = sidebar.transform.Find("Ingredient Scroll/Viewport/Content/Button Container");
                if (buttonContainer != null)
                {
                    Debug.Log($"Ingredient Buttons: {buttonContainer.childCount}");
                }
            }
            
            Debug.Log("📊 === End Status ===");
        }
        
        #endregion
        
        #region Update & Shortcuts
        
        private void Update()
        {
            // Developer shortcuts
            if (debugMode && Application.isPlaying)
            {
                if (Input.GetKeyDown(KeyCode.F5))
                {
                    RefreshUI();
                }
                
                if (Input.GetKeyDown(KeyCode.F6))
                {
                    ForceClearAllUI();
                    CreateConsolidatedUI();
                }
                
                // Clear grid shortcut
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
                {
                    ClearGrid();
                }
            }
        }
        
        #endregion
        
        #region Cleanup
        
        private void OnDestroy()
        {
            // Clear singleton reference
            if (instance == this)
            {
                instance = null;
            }
        }
        
        #endregion
    }
}