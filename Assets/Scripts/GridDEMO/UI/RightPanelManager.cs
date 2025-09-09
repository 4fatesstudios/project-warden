using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Manages the right-side information panel for ingredient details
    /// Shows/hides based on what the user clicks on
    /// </summary>
    public class RightPanelManager : MonoBehaviour
    {
        [Header("Panel Settings")]
        [SerializeField] private float panelWidth = 200f;
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool slideAnimation = true;
        [SerializeField] private float animationSpeed = 5f;
        
        [Header("Panel Colors")]
        [SerializeField] private Color panelBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        [SerializeField] private Color headerColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        // UI Components
        private GameObject rightPanel;
        private RectTransform panelRect;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI descriptionText;
        private TextMeshProUGUI propertiesText;
        private Button removeButton;
        private Image aspectColorBar;
        
        // State
        private bool isPanelVisible = false;
        private Ingredient currentIngredient;
        private Vector2Int currentGridPosition;
        private Vector3 targetPosition;
        private bool isAnimating = false;
        
        // References
        private GridGameManager gridManager;
        private ImprovedClickDetector clickDetector;
        
        public static RightPanelManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupRightPanel();
                SetupClickDetection();
            }
            
            gridManager = FindFirstObjectByType<GridGameManager>();
        }
        
        private void Update()
        {
            HandlePanelAnimation();
        }
        
        private void SetupRightPanel()
        {
            // Find or create canvas
            Canvas canvas = FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("RightPanelManager: No canvas found!");
                return;
            }
            
            // Create right panel
            rightPanel = new GameObject("Right Info Panel");
            rightPanel.transform.SetParent(canvas.transform, false);
            
            // Setup panel rect transform - ALWAYS KEEP ON RIGHT SIDE
            panelRect = rightPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1, 0); // Anchor to right side
            panelRect.anchorMax = new Vector2(1, 1); // Anchor to right side
            panelRect.pivot = new Vector2(1, 0.5f);   // Pivot on right edge
            panelRect.sizeDelta = new Vector2(panelWidth, 0);
            
            // Start hidden (off-screen to the right)
            panelRect.anchoredPosition = new Vector2(0, 0); // Hidden position
            targetPosition = new Vector2(-panelWidth, 0);   // Visible position (slide in from right)
            
            // Add background
            Image panelBackground = rightPanel.AddComponent<Image>();
            panelBackground.color = panelBackgroundColor;
            
            // Add shadow/border effect
            Shadow shadow = rightPanel.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(-2, -2);
            
            // Setup panel content
            SetupPanelContent();
            
            // Initially hide the panel
            HidePanel(false);
            
            Debug.Log("RightPanelManager: Right panel created successfully (anchored to right side)");
        }
        
        private void SetupPanelContent()
        {
            // Create scroll area for content
            GameObject scrollArea = CreateScrollArea(rightPanel);
            
            // Create content container
            GameObject content = scrollArea.transform.Find("Viewport/Content").gameObject;
            
            // Add vertical layout to content
            VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 15f;
            contentLayout.padding = new RectOffset(20, 20, 20, 20);
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childScaleWidth = true;
            contentLayout.childScaleHeight = false;
            
            // Create header section
            CreateHeaderSection(content);
            
            // Create aspect color bar
            CreateAspectColorBar(content);
            
            // Create description section
            CreateDescriptionSection(content);
            
            // Create properties section
            CreatePropertiesSection(content);
            
            // Create action buttons section
            CreateActionButtonsSection(content);
        }
        
        private GameObject CreateScrollArea(GameObject parent)
        {
            GameObject scrollArea = new GameObject("Scroll Area");
            scrollArea.transform.SetParent(parent.transform, false);
            
            RectTransform scrollRect = scrollArea.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            ScrollRect scroll = scrollArea.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;
            
            // Create viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollArea.transform, false);
            
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = Color.clear;
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            
            // Create content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = Vector2.zero;
            contentRect.anchoredPosition = Vector2.zero;
            
            ContentSizeFitter contentFitter = content.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            
            return scrollArea;
        }
        
        private void CreateHeaderSection(GameObject parent)
        {
            GameObject header = new GameObject("Header");
            header.transform.SetParent(parent.transform, false);
            
            RectTransform headerRect = header.AddComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0, 60f);
            
            Image headerBg = header.AddComponent<Image>();
            headerBg.color = headerColor;
            
            LayoutElement headerLayout = header.AddComponent<LayoutElement>();
            headerLayout.preferredHeight = 60f;
            
            // Add title text
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(header.transform, false);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(10, 10);
            titleRect.offsetMax = new Vector2(-10, -10);
            
            titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Ingredient Details";
            titleText.fontSize = 18f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
        }
        
        private void CreateAspectColorBar(GameObject parent)
        {
            GameObject colorBar = new GameObject("Aspect Color Bar");
            colorBar.transform.SetParent(parent.transform, false);
            
            RectTransform colorRect = colorBar.AddComponent<RectTransform>();
            colorRect.sizeDelta = new Vector2(0, 8f);
            
            aspectColorBar = colorBar.AddComponent<Image>();
            aspectColorBar.color = Color.gray;
            
            LayoutElement colorLayout = colorBar.AddComponent<LayoutElement>();
            colorLayout.preferredHeight = 8f;
        }
        
        private void CreateDescriptionSection(GameObject parent)
        {
            GameObject descSection = new GameObject("Description Section");
            descSection.transform.SetParent(parent.transform, false);
            
            LayoutElement descLayout = descSection.AddComponent<LayoutElement>();
            descLayout.preferredHeight = 100f;
            descLayout.flexibleHeight = 1f;
            
            descriptionText = descSection.AddComponent<TextMeshProUGUI>();
            descriptionText.text = "Select an ingredient to view details";
            descriptionText.fontSize = 14f;
            descriptionText.color = Color.white;
            descriptionText.alignment = TextAlignmentOptions.TopLeft;
            descriptionText.fontStyle = FontStyles.Italic;
        }
        
        private void CreatePropertiesSection(GameObject parent)
        {
            GameObject propSection = new GameObject("Properties Section");
            propSection.transform.SetParent(parent.transform, false);
            
            LayoutElement propLayout = propSection.AddComponent<LayoutElement>();
            propLayout.preferredHeight = 150f;
            propLayout.flexibleHeight = 1f;
            
            propertiesText = propSection.AddComponent<TextMeshProUGUI>();
            propertiesText.text = "";
            propertiesText.fontSize = 12f;
            propertiesText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            propertiesText.alignment = TextAlignmentOptions.TopLeft;
        }
        
        private void CreateActionButtonsSection(GameObject parent)
        {
            GameObject buttonSection = new GameObject("Action Buttons");
            buttonSection.transform.SetParent(parent.transform, false);
            
            RectTransform buttonRect = buttonSection.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(0, 50f);
            
            LayoutElement buttonLayout = buttonSection.AddComponent<LayoutElement>();
            buttonLayout.preferredHeight = 50f;
            
            // Create remove button
            GameObject removeButtonObj = new GameObject("Remove Button");
            removeButtonObj.transform.SetParent(buttonSection.transform, false);
            
            RectTransform removeRect = removeButtonObj.AddComponent<RectTransform>();
            removeRect.anchorMin = Vector2.zero;
            removeRect.anchorMax = Vector2.one;
            removeRect.offsetMin = new Vector2(10, 10);
            removeRect.offsetMax = new Vector2(-10, -10);
            
            Image removeImage = removeButtonObj.AddComponent<Image>();
            removeImage.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);
            
            removeButton = removeButtonObj.AddComponent<Button>();
            
            // Add button text
            GameObject removeTextObj = new GameObject("Text");
            removeTextObj.transform.SetParent(removeButtonObj.transform, false);
            
            RectTransform removeTextRect = removeTextObj.AddComponent<RectTransform>();
            removeTextRect.anchorMin = Vector2.zero;
            removeTextRect.anchorMax = Vector2.one;
            removeTextRect.offsetMin = Vector2.zero;
            removeTextRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI removeButtonText = removeTextObj.AddComponent<TextMeshProUGUI>();
            removeButtonText.text = "REMOVE INGREDIENT";
            removeButtonText.fontSize = 12f;
            removeButtonText.fontStyle = FontStyles.Bold;
            removeButtonText.color = Color.white;
            removeButtonText.alignment = TextAlignmentOptions.Center;
            
            // Add button functionality
            removeButton.onClick.AddListener(RemoveCurrentIngredient);
        }
        
        private void SetupClickDetection()
        {
            // Create or get click detection manager
            clickDetector = FindFirstObjectByType<ImprovedClickDetector>();
            if (clickDetector == null)
            {
                GameObject clickDetectorObj = new GameObject("Improved Click Detector");
                clickDetector = clickDetectorObj.AddComponent<ImprovedClickDetector>();
            }
            
            // Subscribe to click events
            clickDetector.OnIngredientClicked += ShowIngredientDetails;
            clickDetector.OnEmptySpaceClicked += HidePanel;
        }
        
        public void ShowIngredientDetails(Ingredient ingredient, Vector2Int gridPosition)
        {
            currentIngredient = ingredient;
            currentGridPosition = gridPosition;
            
            // Update UI content
            UpdatePanelContent();
            
            // Show panel
            ShowPanel();
        }
        
        private void UpdatePanelContent()
        {
            if (currentIngredient == null) return;
            
            // Update title
            titleText.text = currentIngredient.ItemName;
            
            // Update aspect color bar
            aspectColorBar.color = GetAspectColor(currentIngredient.IngredientAspect);
            
            // Update description
            descriptionText.text = $"<b>Description:</b>\n{currentIngredient.ItemDescription}";
            descriptionText.fontStyle = FontStyles.Normal;
            
            // Update properties
            string properties = $"<b>Aspect:</b> {currentIngredient.IngredientAspect}\n" +
                              $"<b>Archetype:</b> {currentIngredient.IngredientArchetype}\n" +
                              $"<b>Rarity:</b> {currentIngredient.ItemRarity}\n" +
                              $"<b>Potency:</b> {currentIngredient.Potency}/5\n" +
                              $"<b>Grid Size:</b> {currentIngredient.GridWidth}×{currentIngredient.GridHeight}\n" +
                              $"<b>Stability:</b> {currentIngredient.StabilityRating:P0}\n" +
                              $"<b>Corrupted:</b> {(currentIngredient.IsCorrupted ? "Yes" : "No")}";
            
            if (currentIngredient.UnlocksAdditionalSpace)
            {
                properties += $"\n<b>Unlocks:</b> {currentIngredient.AdditionalSpaceCount} additional spaces";
            }
            
            propertiesText.text = properties;
        }
        
        public void ShowPanel()
        {
            if (!isPanelVisible)
            {
                rightPanel.SetActive(true); // instantly show
                isPanelVisible = true;
            }
        }

        public void HidePanel(bool animated = true) // ignore animated flag
        {
            if (isPanelVisible)
            {
                isPanelVisible = false;
                rightPanel.SetActive(false); // instantly hide
                currentIngredient = null;
            }
        }

        // For event subscriptions
        public void HidePanel()
        {
            HidePanel(false);
        }
        
        // Legacy compatibility methods for GridDemoUIManager
        public void ShowIngredientInfo(Ingredient ingredient, Vector2 screenPosition)
        {
            // Convert to grid position (approximate)
            Vector2Int gridPos = new Vector2Int(0, 0); // Default position
            ShowIngredientDetails(ingredient, gridPos);
        }
        
        public void HideIngredientInfo()
        {
            HidePanel();
        }

        
        private void HandlePanelAnimation()
        {
            if (isAnimating && slideAnimation)
            {
                panelRect.anchoredPosition = Vector2.Lerp(
                    panelRect.anchoredPosition, 
                    targetPosition, 
                    animationSpeed * Time.deltaTime
                );
                
                if (Vector2.Distance(panelRect.anchoredPosition, targetPosition) < 1f)
                {
                    panelRect.anchoredPosition = targetPosition;
                    isAnimating = false;
                }
            }
        }
        
        private void RemoveCurrentIngredient()
        {
            if (currentIngredient != null && gridManager != null)
            {
                // Find the ingredient interaction component and remove it
                // This will trigger the existing removal logic
                Vector3 worldPos = gridManager.GridToWorldPosition(currentGridPosition);
                
                Collider[] colliders = Physics.OverlapSphere(worldPos, 0.5f);
                foreach (var collider in colliders)
                {
                    IngredientInteraction interaction = collider.GetComponent<IngredientInteraction>();
                    if (interaction != null && interaction.ingredient == currentIngredient)
                    {
                        // Use the existing removal logic
                        interaction.SendMessage("RemoveIngredient", SendMessageOptions.DontRequireReceiver);
                        break;
                    }
                }
                
                // Hide panel after removal
                HidePanel();
            }
        }
        
        private Color GetAspectColor(FourFatesStudios.ProjectWarden.Enums.Aspect aspect)
        {
            switch (aspect)
            {
                case FourFatesStudios.ProjectWarden.Enums.Aspect.Scorch: 
                    return new Color(1f, 0.3f, 0.3f, 1f);
                case FourFatesStudios.ProjectWarden.Enums.Aspect.Frigid: 
                    return new Color(0.3f, 0.8f, 1f, 1f);
                case FourFatesStudios.ProjectWarden.Enums.Aspect.Arc: 
                    return new Color(1f, 1f, 0.3f, 1f);
                case FourFatesStudios.ProjectWarden.Enums.Aspect.Caustic: 
                    return new Color(0.8f, 0.5f, 0.2f, 1f);
                case FourFatesStudios.ProjectWarden.Enums.Aspect.Corporeal: 
                    return new Color(0.7f, 0.7f, 0.7f, 1f);
                case FourFatesStudios.ProjectWarden.Enums.Aspect.Divine: 
                    return new Color(1f, 1f, 1f, 1f);
                default: 
                    return Color.gray;
            }
        }
        
        private Canvas FindMainCanvas()
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    return canvas;
                }
            }
            return canvases.Length > 0 ? canvases[0] : null;
        }
        
        private void OnDestroy()
        {
            if (clickDetector != null)
            {
                clickDetector.OnIngredientClicked -= ShowIngredientDetails;
                clickDetector.OnEmptySpaceClicked -= HidePanel;
            }
        }
    }
}