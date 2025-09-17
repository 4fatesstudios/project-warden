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
    /// </summary>
    public class CompactUIDesigner : MonoBehaviour
    {
        [Header("UI Design Settings")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool useSidebarLayout = true;
        [SerializeField] private bool addScrolling = true;
        
        [Header("Compact Sizes")]
        [SerializeField] private Vector2 compactButtonSize = new Vector2(80f, 35f); // Smaller width for 2 columns
        [SerializeField] private Vector2 compactSpacing = new Vector2(5f, 5f);
        [SerializeField] private int buttonsPerRow = 2; // Two columns
        [SerializeField] private float sidebarWidth = 200f;
        
        [Header("Colors & Style")]
        [SerializeField] private Color sidebarBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        [SerializeField] private Color buttonBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private float buttonFontSize = 10f;
        
        // Events with correct signatures for GridDemoUIManager
        public System.Action<Ingredient> OnIngredientSelected;
        public System.Action OnGridCleared;
        public System.Action OnGridRandomized;
        public System.Action<int> OnGridSizeChanged; // IMPORTANT: int, not Vector2Int
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                DesignCompactUI();
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
            verticalLayout.spacing = 10f;
            verticalLayout.padding = new RectOffset(10, 10, 10, 10);
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
                gridManager = FindObjectOfType<GridGameManager>(); // Fallback
            }
            
            if (gridManager != null && gridManager.availableIngredients != null)
            {
                CreateCompactIngredientButtons(container, gridManager);
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
            scrollRect.sizeDelta = new Vector2(0, 250f); // Increased height for better scrolling
            
            Image scrollBg = scrollArea.AddComponent<Image>();
            scrollBg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            
            ScrollRect scroll = scrollArea.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f; // Increased for better sensitivity
            scroll.inertia = true;
            scroll.decelerationRate = 0.135f;
            scroll.elasticity = 0.1f; // Add some elasticity for better feel
            
            scroll.verticalScrollbar = null;
            scroll.horizontalScrollbar = null;
            
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollArea.transform, false);
            
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(2, 2); // Smaller margins for more space
            viewportRect.offsetMax = new Vector2(-2, -2);
            
            viewport.AddComponent<RectMask2D>();
            
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 100f); // Start with minimum height
            contentRect.anchoredPosition = Vector2.zero;
            
            // Force the scroll rect to recognize the content
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            
            // Add a ContentSizeFitter to the scroll area itself for better handling
            ContentSizeFitter scrollSizeFitter = scrollArea.AddComponent<ContentSizeFitter>();
            scrollSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            scrollSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            
            LayoutElement scrollLayout = scrollArea.AddComponent<LayoutElement>();
            scrollLayout.preferredHeight = 250f; // Match the scroll area height
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
            gridLayout.padding = new RectOffset(5, 5, 5, 5); // Smaller padding
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            
            ContentSizeFitter sizeFitter = container.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            
            return container;
        }
        
        private void CreateCompactIngredientButtons(GameObject container, GridGameManager gridManager)
        {
            foreach (var ingredient in gridManager.availableIngredients)
            {
                if (ingredient != null)
                {
                    CreateCompactIngredientButton(ingredient, container, gridManager);
                }
            }
            
            // Force content size update to ensure scrolling works
            StartCoroutine(ForceScrollViewUpdate(container));
        }
        
        private System.Collections.IEnumerator ForceScrollViewUpdate(GameObject container)
        {
            yield return new WaitForEndOfFrame();
            
            // Force layout rebuild
            var contentSizeFitter = container.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                contentSizeFitter.enabled = false;
                contentSizeFitter.enabled = true;
            }
            
            // Force parent scroll rect to recalculate
            var scrollRect = container.GetComponentInParent<ScrollRect>();
            if (scrollRect != null && scrollRect.content != null)
            {
                // Ensure content height is calculated correctly
                var layoutGroup = container.GetComponent<GridLayoutGroup>();
                if (layoutGroup != null)
                {
                    float totalHeight = CalculateContentHeight(container, layoutGroup);
                    scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, totalHeight);
                    
                    Debug.Log($"📜 Scroll content height set to: {totalHeight}px (viewport: {scrollRect.viewport.rect.height}px)");
                    
                    if (totalHeight > scrollRect.viewport.rect.height)
                    {
                        Debug.Log("✅ Scrolling should now be enabled - content exceeds viewport!");
                    }
                    else
                    {
                        Debug.Log("⚠️ Content fits within viewport - no scrolling needed");
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
        
        private void CreateCompactIngredientButton(
            Ingredient ingredient,
            GameObject container,
            GridGameManager gridManager)
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
            buttonText.fontSize = Mathf.RoundToInt(buttonFontSize * 0.8f); // Smaller font for 2 columns
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.black;
            buttonText.raycastTarget = false;
            
            DraggableIngredient draggable = buttonObj.AddComponent<DraggableIngredient>();
            draggable.Initialize(ingredient, gridManager);
            
            IngredientButton ingredientButtonComponent = buttonObj.AddComponent<IngredientButton>();
            ingredientButtonComponent.SetupIngredient(ingredient, gridManager);
        }
        
        private void CreateCompactControlSection(GameObject sidebar)
        {
            CreateSectionHeader(sidebar, "CONTROLS");
            
            GameObject controlsContainer = new GameObject("Controls Container");
            controlsContainer.transform.SetParent(sidebar.transform, false);
            
            RectTransform controlsRect = controlsContainer.AddComponent<RectTransform>();
            controlsRect.sizeDelta = new Vector2(0, 120f); // Increased height for two buttons
            
            VerticalLayoutGroup controlsLayout = controlsContainer.AddComponent<VerticalLayoutGroup>();
            controlsLayout.spacing = 5f;
            controlsLayout.padding = new RectOffset(5, 5, 5, 5);
            controlsLayout.childControlWidth = true;
            controlsLayout.childControlHeight = false;
            
            CreateCompactCraftButton(controlsContainer);
            CreateCompactClearButton(controlsContainer);
            
            LayoutElement controlsLayoutElement = controlsContainer.AddComponent<LayoutElement>();
            controlsLayoutElement.preferredHeight = 120f;
            controlsLayoutElement.flexibleHeight = 0f;
        }
        
        private void CreateCompactClearButton(GameObject parent)
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
            
            button.onClick.AddListener(() => {
                Debug.Log("🧹 Clear Grid button clicked from CompactUIDesigner!");
                
                GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
                if (gridManager == null)
                {
                    gridManager = FindObjectOfType<GridGameManager>();
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
            layoutElement.preferredHeight = 35f;
        }
        
        private void CreateCompactCraftButton(GameObject parent)
        {
            GameObject craftButtonObj = new GameObject("Craft Potion Button");
            craftButtonObj.transform.SetParent(parent.transform, false);
            
            RectTransform buttonRect = craftButtonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(0, 35f);
            
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
            buttonText.fontSize = 12f;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            buttonText.fontStyle = FontStyles.Bold;
            
            button.onClick.AddListener(() => {
                Debug.Log("🧪 Craft Potion button clicked from CompactUIDesigner!");
                
                GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
                if (gridManager == null)
                {
                    gridManager = FindObjectOfType<GridGameManager>();
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
            layoutElement.preferredHeight = 35f;
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
                existingManager = FindObjectOfType<RightPanelManager>();
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
                existingDetector = FindObjectOfType<ImprovedClickDetector>();
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
    }
}