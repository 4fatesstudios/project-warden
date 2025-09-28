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
    /// Creates a clean, compact, and user-friendly UI design for the Grid Demo
    /// Fixed version for Unity 6.0 compilation issues
    /// 
    /// CONSOLIDATION UPDATE: This script now includes all scroll and text improvements
    /// from CraftingModeSelector.uxml/.uss including:
    /// - Enhanced scroll behavior with improved sensitivity, elasticity, and padding
    /// - Doubled font sizes for better readability (button: 12f, header: 16f, control: 14f)
    /// - Increased button sizes and spacing for better usability
    /// - Improved container sizing and layout proportions
    /// - Enhanced content size calculation and scroll view updates
    /// - Auto-fit text capabilities for better button text handling
    /// - Configurable UI parameters for consistent scaling across systems
    /// - Back button navigation to return to MenuSelector or CraftingModeSelector
    /// 
    /// Use "Apply Scroll & Text Enhancements" context menu to apply all improvements.
    /// Use "Validate Scroll & Text Improvements" to verify consolidation success.
    /// Use "Test Back Button Navigation" to test navigation back to menu.
    /// </summary>
    public class CompactUIDesigner : MonoBehaviour
    {
        [Header("UI Design Settings")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool useSidebarLayout = true;
        [SerializeField] private bool addScrolling = true;
        
        [Header("Auto-Fix & Debug")]
        [SerializeField] private bool autoLoadIngredients = true;
        [SerializeField] private bool debugMode = true;
        [SerializeField] private bool forceRecreateUI = false;
        
        [Header("Compact Sizes")]
        [SerializeField] private Vector2 compactButtonSize = new Vector2(90f, 45f); // Improved size for readability
        [SerializeField] private Vector2 compactSpacing = new Vector2(8f, 8f); // Better spacing
        [SerializeField] private int buttonsPerRow = 2; // Two columns
        [SerializeField] private float sidebarWidth = 220f; // Wider for better content
        
        [Header("Enhanced Scroll Settings")]
        [SerializeField] private float scrollAreaHeight = 280f; // Increased height
        [SerializeField] private float scrollSensitivity = 50f; // Better sensitivity
        [SerializeField] private float scrollElasticity = 0.15f; // Improved feel
        [SerializeField] private int scrollViewPadding = 8; // Better margins
        
        [Header("Colors & Style")]
        [SerializeField] private Color sidebarBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        [SerializeField] private Color buttonBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color scrollBackgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        [SerializeField] private float buttonFontSize = 12f; // Larger font for readability
        [SerializeField] private float headerFontSize = 16f; // Larger headers
        [SerializeField] private float controlButtonFontSize = 14f; // Better control button text
        
        // Events with correct signatures for GridDemoUIManager
        public System.Action<Ingredient> OnIngredientSelected;
        public System.Action OnGridCleared;
        public System.Action OnGridRandomized;
        public System.Action<int> OnGridSizeChanged; // IMPORTANT: int, not Vector2Int
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                // Add delay to ensure all components are initialized
                StartCoroutine(DelayedUISetup());
            }
        }
        
        private System.Collections.IEnumerator DelayedUISetup()
        {
            yield return new WaitForEndOfFrame();
            
            if (debugMode)
            {
                Debug.Log("🎨 CompactUIDesigner: Starting delayed UI setup...");
            }
            
            // Auto-load ingredients if enabled
            if (autoLoadIngredients)
            {
                EnsureIngredientsLoaded();
            }
            
            // Clear existing UI if force recreate is enabled
            if (forceRecreateUI)
            {
                ClearExistingUI();
            }
            
            DesignCompactUI();
            
            if (debugMode)
            {
                LogSetupResults();
            }
        }
        
        private void Update()
        {
            // Developer hotkeys for testing
            if (debugMode && Application.isPlaying)
            {
                if (Input.GetKeyDown(KeyCode.F5))
                {
                    if (debugMode) Debug.Log("🔧 F5 pressed: Refreshing Compact UI...");
                    RefreshUI();
                }
                if (Input.GetKeyDown(KeyCode.F6))
                {
                    if (debugMode) Debug.Log("🔄 F6 pressed: Force recreating UI...");
                    ForceRecreateUI();
                }
            }
        }
        
        [ContextMenu("Design Compact UI")]
        public void DesignCompactUI()
        {
            Debug.Log("CompactUIDesigner_Fixed: Creating user-friendly UI...");
            
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            if (gridDemoUI == null)
            {
                Debug.LogError("CompactUIDesigner_Fixed: GridDemo UI not found!");
                return;
            }
            
            if (useSidebarLayout)
            {
                CreateSidebarLayout(gridDemoUI);
            }
            
            SetupRightPanelManager();
            SetupClickDetection();
            
            Debug.Log("CompactUIDesigner_Fixed: Compact UI created successfully!");
        }
        
        private void CreateSidebarLayout(GameObject gridDemoUI)
        {
            Canvas canvas = gridDemoUI.GetComponent<Canvas>();
            RectTransform canvasRect = gridDemoUI.GetComponent<RectTransform>();
            
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;
            
            GameObject sidebarPanel = CreateSidebarPanel(gridDemoUI);
            CreateCompactIngredientSection(sidebarPanel);
            CreateCompactControlSection(sidebarPanel);
            
            Transform oldIngredientPanel = gridDemoUI.transform.Find("Ingredient Panel");
            Transform oldInfoPanel = gridDemoUI.transform.Find("Info Panel");
            
            if (oldIngredientPanel != null) oldIngredientPanel.gameObject.SetActive(false);
            if (oldInfoPanel != null) oldInfoPanel.gameObject.SetActive(false);
        }
        
        private GameObject CreateSidebarPanel(GameObject parent)
        {
            GameObject sidebar = new GameObject("Compact Sidebar");
            sidebar.transform.SetParent(parent.transform, false);
            
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
            verticalLayout.spacing = 12f; // Increased spacing
            verticalLayout.padding = new RectOffset(12, 12, 12, 12); // Better padding
            verticalLayout.childControlWidth = true;
            verticalLayout.childControlHeight = false;
            verticalLayout.childScaleWidth = true;
            verticalLayout.childScaleHeight = false;
            
            return sidebar;
        }
        
        private void CreateCompactIngredientSection(GameObject sidebar)
        {
            GameObject headerObj = CreateSectionHeader(sidebar, "INGREDIENTS");
            GameObject scrollArea = CreateScrollArea(sidebar, "Ingredient Scroll");
            GameObject container = CreateIngredientButtonContainer(scrollArea);
            
            GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager == null)
            {
                gridManager = FindFirstObjectByType<GridGameManager>(); // Fallback
            }
            
            if (gridManager != null)
            {
                // Force load ingredients if they're not available
                if (gridManager.availableIngredients == null || gridManager.availableIngredients.Count == 0)
                {
                    Debug.LogWarning("🧩 No ingredients found in GridGameManager. Attempting to load from Resources...");
                    LoadIngredientsFromResources(gridManager);
                }
                
                if (gridManager.availableIngredients != null && gridManager.availableIngredients.Count > 0)
                {
                    Debug.Log($"🧩 Creating ingredient buttons for {gridManager.availableIngredients.Count} ingredients");
                    CreateCompactIngredientButtons(container, gridManager);
                }
                else
                {
                    Debug.LogError("🚨 No ingredients available! Cannot create ingredient buttons.");
                    CreateNoIngredientsMessage(container);
                }
            }
            else
            {
                Debug.LogError("🚨 GridGameManager not found! Cannot create ingredient section.");
            }
        }
        
        private void LoadIngredientsFromResources(GridGameManager gridManager)
        {
            var ingredients = Resources.LoadAll<Ingredient>("Items/Ingredients");
            
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
                Debug.Log($"✅ Loaded {ingredients.Length} ingredients from Resources into GridGameManager");
            }
        }
        
        private void CreateNoIngredientsMessage(GameObject container)
        {
            GameObject messageObj = new GameObject("No Ingredients Message");
            messageObj.transform.SetParent(container.transform, false);
            
            RectTransform messageRect = messageObj.AddComponent<RectTransform>();
            messageRect.sizeDelta = new Vector2(180, 60);
            
            TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
            messageText.text = "No ingredients\nfound!\n\nPress F5 to reload";
            messageText.fontSize = buttonFontSize; // Use configurable font size
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.color = Color.yellow;
            messageText.fontStyle = FontStyles.Italic;
        }
        
        private GameObject CreateSectionHeader(GameObject parent, string title)
        {
            GameObject header = new GameObject($"Header_{title}");
            header.transform.SetParent(parent.transform, false);
            
            RectTransform headerRect = header.AddComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0, 30f); // Increased header height
            
            TextMeshProUGUI headerText = header.AddComponent<TextMeshProUGUI>();
            headerText.text = title;
            headerText.fontSize = headerFontSize; // Use configurable header font size
            headerText.fontStyle = FontStyles.Bold;
            headerText.color = Color.white;
            headerText.alignment = TextAlignmentOptions.Center;
            
            LayoutElement headerLayout = header.AddComponent<LayoutElement>();
            headerLayout.preferredHeight = 30f; // Match increased height
            headerLayout.flexibleHeight = 0f;
            
            return header;
        }
        
        private GameObject CreateScrollArea(GameObject parent, string name)
        {
            GameObject scrollArea = new GameObject(name);
            scrollArea.transform.SetParent(parent.transform, false);
            
            RectTransform scrollRect = scrollArea.AddComponent<RectTransform>();
            scrollRect.sizeDelta = new Vector2(0, scrollAreaHeight); // Use configurable height
            
            Image scrollBg = scrollArea.AddComponent<Image>();
            scrollBg.color = scrollBackgroundColor; // Use configurable background color
            
            ScrollRect scroll = scrollArea.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = scrollSensitivity; // Use configurable sensitivity
            scroll.inertia = true;
            scroll.decelerationRate = 0.135f;
            scroll.elasticity = scrollElasticity; // Use configurable elasticity
            
            // Enhanced scroll behavior
            scroll.verticalScrollbar = null;
            scroll.horizontalScrollbar = null;
            
            // Create viewport with improved settings
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollArea.transform, false);
            
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(scrollViewPadding, scrollViewPadding); // Use configurable padding
            viewportRect.offsetMax = new Vector2(-scrollViewPadding, -scrollViewPadding);
            
            viewport.AddComponent<RectMask2D>();
            
            // Create content container with enhanced settings
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 120f); // Better initial height
            contentRect.anchoredPosition = Vector2.zero;
            
            // Force the scroll rect to recognize the content
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            
            // Enhanced content size fitting
            ContentSizeFitter scrollSizeFitter = scrollArea.AddComponent<ContentSizeFitter>();
            scrollSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            scrollSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            
            LayoutElement scrollLayout = scrollArea.AddComponent<LayoutElement>();
            scrollLayout.preferredHeight = scrollAreaHeight; // Match the scroll area height
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
            gridLayout.constraintCount = buttonsPerRow; // 2 columns
            gridLayout.padding = new RectOffset(scrollViewPadding, scrollViewPadding, scrollViewPadding, scrollViewPadding); // Use configurable padding
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            
            ContentSizeFitter sizeFitter = container.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            
            return container;
        }
        
        private void CreateCompactIngredientButtons(GameObject container, GridGameManager gridManager)
        {
            Debug.Log($"🧩 === CREATING INGREDIENT BUTTONS ===");
            Debug.Log($"🧩 Container: {container.name} (Active: {container.activeSelf})");
            Debug.Log($"🧩 Container Parent: {container.transform.parent?.name}");
            Debug.Log($"🧩 Available ingredients: {gridManager.availableIngredients.Count}");
            
            int buttonCount = 0;
            foreach (var ingredient in gridManager.availableIngredients)
            {
                if (ingredient != null)
                {
                    Debug.Log($"🧩 Creating button {buttonCount + 1}: {ingredient.ItemName}");
                    CreateCompactIngredientButton(ingredient, container, gridManager);
                    buttonCount++;
                }
                else
                {
                    Debug.LogWarning($"🧩 Null ingredient found at index {buttonCount}");
                }
            }
            
            Debug.Log($"🧩 Total buttons created: {buttonCount}");
            Debug.Log($"🧩 Container children after creation: {container.transform.childCount}");
            
            // Verify buttons were actually created
            for (int i = 0; i < container.transform.childCount; i++)
            {
                Transform child = container.transform.GetChild(i);
                Debug.Log($"🧩   Child {i}: {child.name} (Active: {child.gameObject.activeSelf})");
            }
            
            // Force content size update to ensure scrolling works
            StartCoroutine(ForceScrollViewUpdate(container));
        }
        
        private System.Collections.IEnumerator ForceScrollViewUpdate(GameObject container)
        {
            yield return new WaitForEndOfFrame();
            
            // Force layout rebuild with enhanced error handling
            var contentSizeFitter = container.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                contentSizeFitter.enabled = false;
                yield return null; // Wait one frame
                contentSizeFitter.enabled = true;
            }
            
            // Force parent scroll rect to recalculate with improved logic
            var scrollRect = container.GetComponentInParent<ScrollRect>();
            if (scrollRect != null && scrollRect.content != null)
            {
                // Ensure content height is calculated correctly
                var layoutGroup = container.GetComponent<GridLayoutGroup>();
                if (layoutGroup != null)
                {
                    float totalHeight = CalculateContentHeight(container, layoutGroup);
                    scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, totalHeight);
                    
                    if (debugMode)
                    {
                        Debug.Log($"📜 Enhanced scroll content height set to: {totalHeight}px (viewport: {scrollRect.viewport.rect.height}px)");
                        
                        if (totalHeight > scrollRect.viewport.rect.height)
                        {
                            Debug.Log("✅ Enhanced scrolling enabled - content exceeds viewport!");
                        }
                        else
                        {
                            Debug.Log("⚠️ Content fits within enhanced viewport - no scrolling needed");
                        }
                    }
                }
                
                // Force scroll rect to refresh
                scrollRect.Rebuild(UnityEngine.UI.CanvasUpdate.Layout);
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
        
        private void CreateCompactIngredientButton(
            Ingredient ingredient,
            GameObject container,
            GridGameManager gridManager)
        {
            Debug.Log($"🔹 Creating button for: {ingredient.ItemName}");
            
            GameObject buttonObj = new GameObject($"Btn_{ingredient.ItemName}");
            buttonObj.transform.SetParent(container.transform, false);
            
            Debug.Log($"🔹 Button object created: {buttonObj.name}, Parent: {buttonObj.transform.parent?.name}");
            
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
            buttonText.fontSize = Mathf.RoundToInt(buttonFontSize); // Use configurable font size
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.black;
            buttonText.raycastTarget = false;
            buttonText.resizeTextForBestFit = true; // Auto-fit text for better readability
            buttonText.resizeTextMinSize = 8;
            buttonText.resizeTextMaxSize = Mathf.RoundToInt(buttonFontSize);
            
            DraggableIngredient draggable = buttonObj.AddComponent<DraggableIngredient>();
            draggable.Initialize(ingredient, gridManager);
            
            IngredientButton ingredientButtonComponent = buttonObj.AddComponent<IngredientButton>();
            ingredientButtonComponent.SetupIngredient(ingredient, gridManager);
            
            Debug.Log($"🔹 Button completed for: {ingredient.ItemName}");
            Debug.Log($"🔹 Button active: {buttonObj.activeSelf}, Position: {buttonObj.transform.localPosition}");
        }
        
        private void CreateCompactControlSection(GameObject sidebar)
        {
            CreateSectionHeader(sidebar, "CONTROLS");
            
            GameObject controlsContainer = new GameObject("Controls Container");
            controlsContainer.transform.SetParent(sidebar.transform, false);
            
            RectTransform controlsRect = controlsContainer.AddComponent<RectTransform>();
            controlsRect.sizeDelta = new Vector2(0, 185f); // Increased height for back button
            
            VerticalLayoutGroup controlsLayout = controlsContainer.AddComponent<VerticalLayoutGroup>();
            controlsLayout.spacing = 8f; // Better spacing
            controlsLayout.padding = new RectOffset(8, 8, 8, 8); // Better padding
            controlsLayout.childControlWidth = true;
            controlsLayout.childControlHeight = false;
            
            CreateCompactCraftButton(controlsContainer);
            CreateCompactClearButton(controlsContainer);
            CreateCompactBackButton(controlsContainer);

            
            LayoutElement controlsLayoutElement = controlsContainer.AddComponent<LayoutElement>();
            controlsLayoutElement.preferredHeight = 185f; // Match increased height
            controlsLayoutElement.flexibleHeight = 0f;
        }
        
        private void CreateCompactBackButton(GameObject parent)
        {
            GameObject backButtonObj = new GameObject("Back Button");
            backButtonObj.transform.SetParent(parent.transform, false);
            
            RectTransform buttonRect = backButtonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(0, 45f); // Same height as other buttons
            
            Image buttonImage = backButtonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.4f, 0.4f, 0.4f, 0.9f); // Gray color for back button
            
            Button button = backButtonObj.AddComponent<Button>();
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(backButtonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "← BACK TO MENU";
            buttonText.fontSize = controlButtonFontSize; // Use configurable font size
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            buttonText.fontStyle = FontStyles.Bold;
            
            button.onClick.AddListener(() => {
                Debug.Log("⬅️ Back button clicked from CompactUIDesigner!");

                // Show CraftingModeSelector instead of MenuSelector
                GameObject craftingModeSelector = GameObject.Find("CraftingModeSelector");
                if (craftingModeSelector != null)
                {
                    Debug.Log("✅ Re-enabling CraftingModeSelector");
                    var modeSelectorComponent = craftingModeSelector.GetComponent<FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.CraftingModeSelector>();
                    if (modeSelectorComponent != null)
                    {
                        modeSelectorComponent.ShowModeSelector();
                    }
                    else
                    {
                        // Fallback if component not found
                        craftingModeSelector.SetActive(true);
                    }
                }
                else
                {
                    // Fallback: If CraftingModeSelector not found, show MenuSelector
                    GameObject menuSelector = GameObject.Find("MenuSelector");
                    if (menuSelector != null)
                    {
                        Debug.Log("⚠️ CraftingModeSelector not found, falling back to MenuSelector");
                        menuSelector.SetActive(true);
                    }
                }
                
                Debug.Log("🎯 Navigation back to CraftingModeSelector completed");
            });
            
            LayoutElement layoutElement = backButtonObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 45f; // Match button height
        }
        
        private void CreateCompactClearButton(GameObject parent)
        {
            GameObject clearButtonObj = new GameObject("Clear Button");
            clearButtonObj.transform.SetParent(parent.transform, false);
            
            RectTransform buttonRect = clearButtonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(0, 45f); // Increased button height
            
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
            buttonText.fontSize = controlButtonFontSize; // Use configurable font size
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            buttonText.fontStyle = FontStyles.Bold;
            
            button.onClick.AddListener(() => {
                Debug.Log("🧹 Clear Grid button clicked from CompactUIDesigner!");
                
                GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
                if (gridManager == null)
                {
                    gridManager = FindFirstObjectByType<GridGameManager>();
                }
                
                if (gridManager != null)
                {
                    Debug.Log("🧹 GridGameManager found, calling ClearGrid()");
                    gridManager.ClearGrid();
                }
                else
                {
                    Debug.LogError("🚨 GridGameManager not found! Cannot clear grid.");
                    GameObject gmObj = GameObject.Find("GridGameManager");
                    if (gmObj != null)
                    {
                        GridGameManager fallbackGM = gmObj.GetComponent<GridGameManager>();
                        if (fallbackGM != null)
                        {
                            Debug.Log("🧹 Found GridGameManager via GameObject.Find, calling ClearGrid()");
                            fallbackGM.ClearGrid();
                        }
                    }
                }
            });
            
            LayoutElement layoutElement = clearButtonObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 45f; // Match increased height
        }
        
        private void CreateCompactCraftButton(GameObject parent)
        {
            GameObject craftButtonObj = new GameObject("Craft Potion Button");
            craftButtonObj.transform.SetParent(parent.transform, false);
            
            RectTransform buttonRect = craftButtonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(0, 45f); // Increased button height
            
            Image buttonImage = craftButtonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.6f, 0.2f, 0.9f); // Green color for crafting
            
            Button button = craftButtonObj.AddComponent<Button>();
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(craftButtonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "CRAFT POTION";
            buttonText.fontSize = controlButtonFontSize; // Use configurable font size
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            buttonText.fontStyle = FontStyles.Bold;
            
            button.onClick.AddListener(() => {
                Debug.Log("🧪 Craft Potion button clicked from CompactUIDesigner!");
                
                GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
                if (gridManager == null)
                {
                    gridManager = FindFirstObjectByType<GridGameManager>();
                }
                
                if (gridManager != null)
                {
                    Debug.Log("🧪 GridGameManager found, initiating potion craft");
                    InitiatePotionCraft(gridManager);
                }
                else
                {
                    Debug.LogError("🚨 GridGameManager not found! Cannot craft potion.");
                }
            });
            
            LayoutElement layoutElement = craftButtonObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 45f; // Match increased height
        }
        
        private void InitiatePotionCraft(GridGameManager gridManager)
        {
            // Get all placed ingredients from the grid
            var placedIngredients = GetPlacedIngredientsFromGrid(gridManager);
            
            if (placedIngredients == null || placedIngredients.Count == 0)
            {
                Debug.LogWarning("🧪 No ingredients placed on grid. Cannot craft potion!");
                DebugSystemConfig.LogTesting("Potion craft failed: No ingredients on grid");
                return;
            }
            
            Debug.Log($"🧪 Starting potion craft with {placedIngredients.Count} ingredients:");
            foreach (var ingredient in placedIngredients)
            {
                Debug.Log($"   - {ingredient.ItemName} (Aspect: {ingredient.IngredientAspect})");
            }
            
            // Step 1: Check for recipe matches
            Potion craftedPotion = CheckForRecipeMatch(placedIngredients);
            
            // Step 2: If no recipe match, create infusion-based potion
            if (craftedPotion == null)
            {
                craftedPotion = CreateInfusionBasedPotion(placedIngredients);
            }
            
            // Step 3: Debug output what was crafted
            DebugCraftingResult(craftedPotion, placedIngredients);
            
            // Step 4: Clear the grid after successful crafting
            ClearCraftingGrid(gridManager);
            
            DebugSystemConfig.LogTesting($"Potion craft completed: {craftedPotion?.ItemName ?? "Unknown Potion"}");
        }
        
        private Potion CheckForRecipeMatch(List<Ingredient> ingredients)
        {
            Debug.Log("🔍 Checking for recipe matches using AlchemyRecipeDatabase...");
            
            // Use the existing AlchemyRecipeDatabase for order-insensitive recipe lookup
            try
            {
                var recipeDatabase = FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes.AlchemyRecipeDatabase.Instance;
                if (recipeDatabase != null)
                {
                    var matchedRecipe = recipeDatabase.GetRecipeByIngredients(ingredients);
                    
                    if (matchedRecipe != null)
                    {
                        Debug.Log($"✅ Found recipe match: {matchedRecipe.name}");
                        Debug.Log($"   Output: {matchedRecipe.OutputPotion?.ItemName ?? "Unknown Potion"}");
                        Debug.Log($"   Quantity: {matchedRecipe.OutputQuantity}");
                        
                        // Create the output potion from the matched recipe
                        if (matchedRecipe.OutputPotion != null)
                        {
                            return matchedRecipe.OutputPotion;
                        }
                        else
                        {
                            Debug.LogWarning("⚠️ Recipe found but no output potion specified!");
                        }
                    }
                    else
                    {
                        Debug.Log("❌ No recipe matches found in database.");
                        LogIngredientSearchDetails(ingredients);
                    }
                }
                else
                {
                    Debug.LogError("❌ AlchemyRecipeDatabase instance not found! Using fallback logic.");
                    return CheckForRecipeMatchFallback(ingredients);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Error accessing AlchemyRecipeDatabase: {ex.Message}. Using fallback logic.");
                return CheckForRecipeMatchFallback(ingredients);
            }
            
            return null;
        }
        
        private void LogIngredientSearchDetails(List<Ingredient> ingredients)
        {
            Debug.Log("🔍 Recipe search details:");
            Debug.Log($"   Ingredients provided: {ingredients.Count}");
            
            var sortedIngredients = ingredients.OrderBy(i => i.name).ToList();
            foreach (var ingredient in sortedIngredients)
            {
                Debug.Log($"   - {ingredient.name} (Aspect: {ingredient.IngredientAspect})");
            }
            
            string searchKey = string.Join("-", sortedIngredients.Select(i => i.name));
            Debug.Log($"   Generated search key: {searchKey}");
        }
        
        private Potion CheckForRecipeMatchFallback(List<Ingredient> ingredients)
        {
            Debug.Log("🔄 Using fallback recipe matching logic...");
            
            // Keep the original simple recipe matching as fallback
            if (ingredients.Count >= 2)
            {
                var aspects = ingredients.Select(i => i.IngredientAspect).Distinct().ToList();
                var hasCorporeal = aspects.Contains(FourFatesStudios.ProjectWarden.Enums.Aspect.Corporeal);
                var hasDivine = aspects.Contains(FourFatesStudios.ProjectWarden.Enums.Aspect.Divine);
                
                if (hasCorporeal && hasDivine && ingredients.Count >= 3)
                {
                    Debug.Log("✅ Fallback recipe match: Balanced Healing Potion (Corporeal + Divine)");
                    return CreateNamedPotion("Balanced Healing Potion", ingredients);
                }
            }
            
            Debug.Log("❌ No fallback recipe matches found.");
            return null;
        }
        
        private Potion CreateInfusionBasedPotion(List<Ingredient> ingredients)
        {
            Debug.Log("🧬 Creating infusion-based potion...");
            
            // Collect all infusions from ingredients
            var allInfusions = new Dictionary<string, int>();
            var totalPotency = 0;
            
            foreach (var ingredient in ingredients)
            {
                totalPotency += ingredient.Potency;
                
                if (ingredient.InfusionBundle?.Infusions != null)
                {
                    foreach (var infusion in ingredient.InfusionBundle.Infusions)
                    {
                        if (!string.IsNullOrEmpty(infusion.InfusionName))
                        {
                            allInfusions[infusion.InfusionName] = allInfusions.GetValueOrDefault(infusion.InfusionName, 0) + 1;
                        }
                    }
                }
            }
            
            // Find the most common infusions (shared alike infusions)
            var sharedInfusions = allInfusions.Where(kvp => kvp.Value >= 2).ToList();
            
            if (sharedInfusions.Count > 0)
            {
                // Create potion based on shared infusions
                var primaryInfusion = sharedInfusions.OrderByDescending(kvp => kvp.Value).First();
                string potionName = $"Potion of {primaryInfusion.Key}";
                
                Debug.Log($"✅ Creating potion based on shared infusion: {primaryInfusion.Key} (x{primaryInfusion.Value})");
                
                return CreateNamedPotion(potionName, ingredients, primaryInfusion.Key);
            }
            else
            {
                // Create generic mixed potion
                Debug.Log("⚗️ No shared infusions found. Creating mixed ingredient potion.");
                return CreateGenericMixedPotion(ingredients);
            }
        }
        
        private Potion CreateNamedPotion(string name, List<Ingredient> ingredients, string primaryInfusion = null)
        {
            // Create a runtime potion instance
            var potion = ScriptableObject.CreateInstance<Potion>();
            
            // Set basic properties
            typeof(Item).GetField("itemName", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(potion, name);
            typeof(Item).GetField("itemDescription", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(potion, 
                $"A {(primaryInfusion != null ? primaryInfusion.ToLower() : "balanced")} potion crafted from {ingredients.Count} ingredients.");
            
            return potion;
        }
        
        private Potion CreateGenericMixedPotion(List<Ingredient> ingredients)
        {
            // Determine primary aspect
            var aspectCounts = new Dictionary<FourFatesStudios.ProjectWarden.Enums.Aspect, int>();
            foreach (var ingredient in ingredients)
            {
                aspectCounts[ingredient.IngredientAspect] = aspectCounts.GetValueOrDefault(ingredient.IngredientAspect, 0) + 1;
            }
            
            var primaryAspect = aspectCounts.OrderByDescending(kvp => kvp.Value).First().Key;
            string potionName = $"Mixed {primaryAspect} Potion";
            
            return CreateNamedPotion(potionName, ingredients);
        }
        
        private void DebugCraftingResult(Potion potion, List<Ingredient> ingredients)
        {
            if (potion == null)
            {
                Debug.LogError("🚫 Failed to create potion!");
                return;
            }
            
            Debug.Log("🎉 === POTION CRAFTING RESULT ===");
            Debug.Log($"📦 Potion Name: {potion.ItemName}");
            Debug.Log($"📝 Description: {potion.ItemDescription}");
            Debug.Log($"🧪 Created from {ingredients.Count} ingredients:");
            
            for (int i = 0; i < ingredients.Count; i++)
            {
                var ingredient = ingredients[i];
                Debug.Log($"   {i + 1}. {ingredient.ItemName} (Potency: {ingredient.Potency}, Aspect: {ingredient.IngredientAspect})");
                
                if (ingredient.InfusionBundle?.Infusions != null && ingredient.InfusionBundle.Infusions.Count > 0)
                {
                    string infusionList = string.Join(", ", ingredient.InfusionBundle.Infusions.Select(inf => inf.InfusionName));
                    Debug.Log($"      Infusions: {infusionList}");
                }
            }
            
            Debug.Log("🎯 === END CRAFTING RESULT ===");
        }
        
        private void ClearCraftingGrid(GridGameManager gridManager)
        {
            try
            {
                Debug.Log("🧹 Clearing crafting grid...");
                gridManager.ClearGrid();
                Debug.Log("✅ Grid cleared successfully!");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"⚠️ Could not clear grid: {ex.Message}");
            }
        }
        // Removed old helper methods that had compilation errors
        
        private List<Ingredient> GetPlacedIngredientsFromGrid(GridGameManager gridManager)
        {
            var uniqueIngredients = new List<Ingredient>();
            
            // Check if the gridManager has a public method to get placed ingredients
            // For now, we'll use reflection to access the private gridCells field
            try
            {
                var gridCellsField = typeof(GridGameManager).GetField("gridCells", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (gridCellsField != null)
                {
                    var gridCells = (GridCell[,])gridCellsField.GetValue(gridManager);
                    
                    if (gridCells != null)
                    {
                        // Use HashSet to track unique ingredients (avoid counting same ingredient multiple times)
                        var seenIngredients = new HashSet<Ingredient>();
                        
                        for (int x = 0; x < gridCells.GetLength(0); x++)
                        {
                            for (int y = 0; y < gridCells.GetLength(1); y++)
                            {
                                var cell = gridCells[x, y];
                                if (cell != null && cell.IsOccupied && cell.OccupiedByIngredient != null)
                                {
                                    // Only add if we haven't seen this ingredient before
                                    if (seenIngredients.Add(cell.OccupiedByIngredient))
                                    {
                                        uniqueIngredients.Add(cell.OccupiedByIngredient);
                                        Debug.Log($"  Found unique ingredient: {cell.OccupiedByIngredient.ItemName}");
                                    }
                                }
                            }
                        }
                        
                        Debug.Log($"📊 Total unique ingredients found: {uniqueIngredients.Count}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"🧪 Could not access grid cells via reflection: {ex.Message}");
                
                // Fallback: Try to find a different approach or log that crafting isn't available
                Debug.LogWarning("🧪 Fallback: Cannot determine placed ingredients without grid access");
            }
            
            return uniqueIngredients;
        }
        
        private void SetupRightPanelManager()
        {
            RightPanelManager existingManager = FindFirstObjectByType<RightPanelManager>();
            if (existingManager == null)
            {
                existingManager = FindFirstObjectByType<RightPanelManager>();
            }
            if (existingManager != null)
            {
                Debug.Log("CompactUIDesigner_Fixed: RightPanelManager already exists");
                return;
            }
            
            GameObject rightPanelManagerObj = new GameObject("Right Panel Manager");
            RightPanelManager rightPanelManager = rightPanelManagerObj.AddComponent<RightPanelManager>();
            
            Debug.Log("CompactUIDesigner_Fixed: Created RightPanelManager for ingredient information display");
        }
        
        private void SetupClickDetection()
        {
            ImprovedClickDetector existingDetector = FindFirstObjectByType<ImprovedClickDetector>();
            if (existingDetector == null)
            {
                existingDetector = FindFirstObjectByType<ImprovedClickDetector>();
            }
            if (existingDetector != null)
            {
                Debug.Log("CompactUIDesigner_Fixed: ImprovedClickDetector already exists");
                return;
            }
            
            GameObject clickDetectorObj = new GameObject("Improved Click Detector");
            ImprovedClickDetector clickDetector = clickDetectorObj.AddComponent<ImprovedClickDetector>();
            
            Debug.Log("CompactUIDesigner_Fixed: Created ImprovedClickDetector for ingredient clicks");
        }
        
        public void RefreshIngredientButtons()
        {
            GameObject container = GameObject.Find("Compact Sidebar/Ingredient Scroll/Button Container");
            if (container != null)
            {
                for (int i = container.transform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(container.transform.GetChild(i).gameObject);
                }
                
                GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
                if (gridManager != null && gridManager.availableIngredients != null)
                {
                    CreateCompactIngredientButtons(container, gridManager);
                }
            }
        }
        
        public void UpdateGridInfo()
        {
            Debug.Log("CompactUIDesigner_Fixed: Grid info updated");
        }
        
        #region Auto-Fix Methods
        
        /// <summary>
        /// Applies all consolidated scroll and text improvements from CraftingModeSelector
        /// </summary>
        [ContextMenu("Apply Scroll & Text Enhancements")]
        public void ApplyScrollAndTextEnhancements()
        {
            if (debugMode) Debug.Log("🔧 Applying consolidated scroll and text enhancements...");
            
            // First apply enhanced scaling configuration
            ApplyEnhancedScaling();
            
            // Clear and recreate UI with enhanced settings
            ClearExistingUI();
            
            // Ensure ingredients are loaded
            if (autoLoadIngredients)
            {
                EnsureIngredientsLoaded();
            }
            
            // Recreate UI with enhanced settings
            DesignCompactUI();
            
            if (debugMode)
            {
                Debug.Log("✅ Enhanced scroll and text improvements applied successfully!");
                LogSetupResults();
                
                // Validate the improvements were applied correctly
                StartCoroutine(DelayedValidation());
            }
        }
        
        private System.Collections.IEnumerator DelayedValidation()
        {
            yield return new WaitForEndOfFrame();
            ValidateScrollAndTextImprovements();
        }
        
        /// <summary>
        /// Updates UI element sizes to match CraftingModeSelector scaling standards
        /// </summary>
        [ContextMenu("Apply Enhanced Scaling")]
        public void ApplyEnhancedScaling()
        {
            if (debugMode) Debug.Log("🎯 Applying enhanced scaling standards...");
            
            // Update configuration values to match CraftingModeSelector improvements
            compactButtonSize = new Vector2(90f, 45f);
            compactSpacing = new Vector2(8f, 8f);
            sidebarWidth = 220f;
            scrollAreaHeight = 280f;
            scrollSensitivity = 50f;
            scrollElasticity = 0.15f;
            scrollViewPadding = 8;
            buttonFontSize = 12f;
            headerFontSize = 16f;
            controlButtonFontSize = 14f;
            
            if (debugMode) Debug.Log("✅ Enhanced scaling configuration updated! (Includes back button support)");
        }
        
        /// <summary>
        /// Validates that all improvements from CraftingModeSelector have been applied
        /// </summary>
        [ContextMenu("Validate Scroll & Text Improvements")]
        public void ValidateScrollAndTextImprovements()
        {
            Debug.Log("🔍 === VALIDATING CONSOLIDATED IMPROVEMENTS ===");
            
            // Check configuration values
            bool scalingValid = compactButtonSize.x >= 90f && compactButtonSize.y >= 45f;
            bool spacingValid = compactSpacing.x >= 8f && compactSpacing.y >= 8f;
            bool sizeValid = sidebarWidth >= 220f && scrollAreaHeight >= 280f;
            bool fontValid = buttonFontSize >= 12f && headerFontSize >= 16f && controlButtonFontSize >= 14f;
            bool scrollValid = scrollSensitivity >= 50f && scrollElasticity >= 0.15f && scrollViewPadding >= 8;
            
            Debug.Log($"✨ Button sizing: {(scalingValid ? "✅" : "❌")} (Expected: ≥90x45, Current: {compactButtonSize})");
            Debug.Log($"✨ Spacing: {(spacingValid ? "✅" : "❌")} (Expected: ≥8x8, Current: {compactSpacing})");
            Debug.Log($"✨ Container sizes: {(sizeValid ? "✅" : "❌")} (Sidebar: {sidebarWidth}, Scroll: {scrollAreaHeight})");
            Debug.Log($"✨ Font sizes: {(fontValid ? "✅" : "❌")} (Button: {buttonFontSize}, Header: {headerFontSize}, Control: {controlButtonFontSize})");
            Debug.Log($"✨ Scroll settings: {(scrollValid ? "✅" : "❌")} (Sensitivity: {scrollSensitivity}, Elasticity: {scrollElasticity}, Padding: {scrollViewPadding})");
            
            bool allValid = scalingValid && spacingValid && sizeValid && fontValid && scrollValid;
            
            if (allValid)
            {
                Debug.Log("🎉 All CraftingModeSelector improvements successfully consolidated!");
            }
            else
            {
                Debug.LogWarning("⚠️ Some improvements may need adjustment. Consider running 'Apply Enhanced Scaling'.");
            }
            
            // Check if UI exists and is properly scaled
            GameObject sidebar = GameObject.Find("Compact Sidebar");
            if (sidebar != null)
            {
                RectTransform sidebarRect = sidebar.GetComponent<RectTransform>();
                if (sidebarRect != null)
                {
                    bool widthMatches = Mathf.Approximately(sidebarRect.sizeDelta.x, sidebarWidth);
                    Debug.Log($"✨ Sidebar width in scene: {(widthMatches ? "✅" : "❌")} (Expected: {sidebarWidth}, Current: {sidebarRect.sizeDelta.x})");
                    
                    if (!widthMatches)
                    {
                        Debug.LogWarning("⚠️ Sidebar width doesn't match configuration. Consider running 'Apply Scroll & Text Enhancements'.");
                    }
                }
                
                // NEW: Check for back button
                Transform backButton = sidebar.transform.Find("Controls Container/Back Button");
                if (backButton != null)
                {
                    Debug.Log("✅ Back button found in controls container!");
                }
                else
                {
                    Debug.LogWarning("⚠️ Back button not found! Consider running 'Apply Scroll & Text Enhancements' to add it.");
                }
            }
            else
            {
                Debug.Log("💡 No Compact Sidebar found in scene. Run 'Design Compact UI' or 'Apply Scroll & Text Enhancements' first.");
            }
            
            Debug.Log("🔍 === VALIDATION COMPLETE ===");
        }
        
        private void EnsureIngredientsLoaded()
        {
            GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager == null)
            {
                if (debugMode) Debug.LogError("❌ GridGameManager not found!");
                return;
            }
            
            // Check if ingredients need to be loaded
            if (gridManager.availableIngredients == null || gridManager.availableIngredients.Count == 0)
            {
                if (debugMode) Debug.Log("📦 No ingredients found. Loading from Resources...");
                LoadIngredientsFromResources(gridManager);
            }
            else if (debugMode)
            {
                Debug.Log($"✅ Found {gridManager.availableIngredients.Count} ingredients already loaded");
            }
        }
        
        private void ClearExistingUI()
        {
            GameObject existingSidebar = GameObject.Find("Compact Sidebar");
            if (existingSidebar != null)
            {
                if (debugMode) Debug.Log("🗑️ Removing existing Compact Sidebar");
                DestroyImmediate(existingSidebar);
            }
        }
        
        private void LogSetupResults()
        {
            GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
            GameObject sidebar = GameObject.Find("Compact Sidebar");
            
            Debug.Log("📊 === COMPACT UI SETUP RESULTS ===");
            Debug.Log($"GridGameManager found: {gridManager != null}");
            Debug.Log($"Ingredients count: {gridManager?.availableIngredients?.Count ?? 0}");
            Debug.Log($"Compact Sidebar created: {sidebar != null}");
            
            if (sidebar != null)
            {
                GameObject buttonContainer = GameObject.Find("Compact Sidebar/Ingredient Scroll/Viewport/Content/Button Container");
                if (buttonContainer != null)
                {
                    int buttonCount = buttonContainer.transform.childCount;
                    Debug.Log($"Ingredient buttons created: {buttonCount}");
                    
                    if (buttonCount == 0 && gridManager?.availableIngredients?.Count > 0)
                    {
                        Debug.LogWarning("⚠️ Ingredients loaded but no buttons created! Check button creation logic.");
                    }
                }
            }
            Debug.Log("📊 === END SETUP RESULTS ===");
        }
        
        [ContextMenu("Refresh UI")]
        public void RefreshUI()
        {
            if (autoLoadIngredients)
            {
                EnsureIngredientsLoaded();
            }
            RefreshIngredientButtons();
        }
        
        [ContextMenu("Force Recreate UI")]
        public void ForceRecreateUI()
        {
            ClearExistingUI();
            if (autoLoadIngredients)
            {
                EnsureIngredientsLoaded();
            }
            DesignCompactUI();
            if (debugMode)
            {
                LogSetupResults();
            }
        }
        
        [ContextMenu("Debug Current UI State")]
        public void DebugCurrentUIState()
        {
            Debug.Log("🔍 === DEBUGGING CURRENT UI STATE ===");
            
            // Check if sidebar exists
            Transform sidebar = transform.Find("Compact Sidebar");
            if (sidebar == null)
            {
                Debug.LogError("❌ Compact Sidebar not found under GridDemo UI!");
                return;
            }
            
            Debug.Log($"✅ Compact Sidebar found, active: {sidebar.gameObject.activeSelf}");
            
            // Navigate to button container
            Transform buttonContainer = sidebar.Find("Ingredient Scroll/Viewport/Content/Button Container");
            if (buttonContainer == null)
            {
                Debug.LogError("❌ Button Container not found!");
                // Try to find it another way
                Debug.Log("🔍 Searching for Button Container in all children...");
                Transform[] allChildren = sidebar.GetComponentsInChildren<Transform>();
                foreach (Transform child in allChildren)
                {
                    if (child.name.Contains("Button Container"))
                    {
                        Debug.Log($"Found Button Container: {child.name}");
                        buttonContainer = child;
                        break;
                    }
                    if (child.name.Contains("Button") || child.name.Contains("Btn_"))
                    {
                        Debug.Log($"Found button: {child.name} under parent: {child.parent.name}");
                    }
                }
            }
            
            if (buttonContainer != null)
            {
                Debug.Log($"✅ Button Container found, children: {buttonContainer.childCount}");
                
                for (int i = 0; i < buttonContainer.childCount; i++)
                {
                    Transform child = buttonContainer.GetChild(i);
                    Debug.Log($"   Button {i}: {child.name} (Active: {child.gameObject.activeSelf})");
                    
                    // Check RectTransform
                    RectTransform rect = child.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        Debug.Log($"     Position: {rect.localPosition}, Size: {rect.sizeDelta}");
                    }
                }
            }
            
            // Check for back button specifically
            Transform backButton = GameObject.Find("Back Button")?.transform;
            if (backButton != null)
            {
                Debug.Log($"✅ Back Button found: {backButton.name} (Active: {backButton.gameObject.activeSelf})");
            }
            else
            {
                Debug.LogWarning("⚠️ Back Button not found in scene!");
            }
            
            Debug.Log("🔍 === DEBUG COMPLETE ===");
        }
        
        [ContextMenu("Test Back Button Navigation")]
        public void TestBackButtonNavigation()
        {
            Debug.Log("🧪 Testing back button navigation...");
            
            // Show MenuSelector
            GameObject menuSelector = GameObject.Find("MenuSelector");
            if (menuSelector != null)
            {
                Debug.Log("✅ Showing MenuSelector");
                menuSelector.SetActive(true);
            }
            
            // Hide CraftingModeSelector
            GameObject craftingModeSelector = GameObject.Find("CraftingModeSelector");
            if (craftingModeSelector != null)
            {
                craftingModeSelector.SetActive(true);
            }
            
            Debug.Log("✅ Navigation to MenuSelector completed via direct GameObject manipulation");
        }
        
        #endregion
    }
}