using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FourFatesStudios.ProjectWarden.GridDemo.UI;

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
        [SerializeField] private Vector2 compactButtonSize = new Vector2(120f, 35f);
        [SerializeField] private Vector2 compactSpacing = new Vector2(5f, 5f);
        [SerializeField] private int buttonsPerRow = 1;
        [SerializeField] private float sidebarWidth = 200f;
        
        [Header("Colors & Style")]
        [SerializeField] private Color sidebarBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        [SerializeField] private Color buttonBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private float buttonFontSize = 10f;
        
        // Events with correct signatures for GridDemoUIManager
        public System.Action<FourFatesStudios.ProjectWarden.ScriptableObjects.Items.Ingredient> OnIngredientSelected;
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
            scrollRect.sizeDelta = new Vector2(0, 200f);
            
            Image scrollBg = scrollArea.AddComponent<Image>();
            scrollBg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            
            ScrollRect scroll = scrollArea.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;
            scroll.inertia = true;
            scroll.decelerationRate = 0.135f;
            
            scroll.verticalScrollbar = null;
            scroll.horizontalScrollbar = null;
            
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollArea.transform, false);
            
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(5, 5);
            viewportRect.offsetMax = new Vector2(-5, -5);
            
            viewport.AddComponent<Mask>();
            
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            contentRect.anchoredPosition = Vector2.zero;
            
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            
            LayoutElement scrollLayout = scrollArea.AddComponent<LayoutElement>();
            scrollLayout.preferredHeight = 200f;
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
            gridLayout.padding = new RectOffset(10, 10, 10, 10);
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
        }
        
        private void CreateCompactIngredientButton(
            FourFatesStudios.ProjectWarden.ScriptableObjects.Items.Ingredient ingredient,
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
            buttonText.fontSize = Mathf.RoundToInt(buttonFontSize);
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
            controlsRect.sizeDelta = new Vector2(0, 80f);
            
            VerticalLayoutGroup controlsLayout = controlsContainer.AddComponent<VerticalLayoutGroup>();
            controlsLayout.spacing = 5f;
            controlsLayout.padding = new RectOffset(5, 5, 5, 5);
            controlsLayout.childControlWidth = true;
            controlsLayout.childControlHeight = false;
            
            CreateCompactClearButton(controlsContainer);
            
            LayoutElement controlsLayoutElement = controlsContainer.AddComponent<LayoutElement>();
            controlsLayoutElement.preferredHeight = 80f;
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
                GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
                if (gridManager != null)
                {
                    gridManager.ClearGrid();
                    Debug.Log("Grid cleared!");
                }
            });
            
            LayoutElement layoutElement = clearButtonObj.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 35f;
        }
        
        private void SetupRightPanelManager()
        {
            RightPanelManager existingManager = FindFirstObjectByType<RightPanelManager>();
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
            if (existingDetector != null)
            {
                Debug.Log("CompactUIDesigner_Fixed: ImprovedClickDetector already exists");
                return;
            }
            
            GameObject clickDetectorObj = new GameObject("Improved Click Detector");
            ImprovedClickDetector clickDetector = clickDetectorObj.AddComponent<ImprovedClickDetector>();
            
            Debug.Log("CompactUIDesigner_Fixed: Created ImprovedClickDetector for ingredient clicks");
        }
        
        // Legacy compatibility methods for GridDemoUIManager
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