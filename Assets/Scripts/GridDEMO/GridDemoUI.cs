using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.GridDemo.UI;
using TMPro;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Legacy GridDemoUI - now acts as a bridge to the new UI system
    /// </summary>
    public class GridDemoUI : MonoBehaviour
    {
        [Header("Legacy UI References")]
        public Transform ingredientButtonContainer;
        public Button ingredientButtonPrefab;
        public Button clearGridButton;
        public Button randomizeButton;
        public TextMeshProUGUI selectedIngredientText;
        public TextMeshProUGUI gridInfoText;
        public Slider gridSizeSlider;
        public TextMeshProUGUI gridSizeText;
        
        [Header("Info Panel")]
        public GameObject infoPanel;
        public TextMeshProUGUI ingredientNameText;
        public TextMeshProUGUI ingredientDescriptionText;
        public TextMeshProUGUI ingredientStatsText;
        public Image ingredientIcon;
        
        [Header("Auto-Migration")]
        [SerializeField] private bool migrateToNewSystem = true;
        
        private GridGameManager gridManager;
        private GridDemoUIManager uiManager;
        private List<Button> ingredientButtons = new List<Button>();
        private Ingredient currentSelectedIngredient;
        
        private void Awake()
        {
            gridManager = FindFirstObjectByType<GridGameManager>();
            
            if (migrateToNewSystem)
            {
                SetupNewUISystem();
            }
        }
        
        private void Start()
        {
            if (!migrateToNewSystem)
            {
                SetupLegacyUI();
                CreateIngredientButtons();
                UpdateGridInfo();
            }
        }
        
        private void SetupNewUISystem()
        {
            // Check if UI Manager already exists
            uiManager = GetComponent<GridDemoUIManager>();
            if (uiManager == null)
            {
                uiManager = gameObject.AddComponent<GridDemoUIManager>();
            }
            
            // Add drag and drop demo
            DragDropDemo dragDropDemo = GetComponent<DragDropDemo>();
            if (dragDropDemo == null)
            {
                dragDropDemo = gameObject.AddComponent<DragDropDemo>();
                Debug.Log("GridDemoUI: Added DragDropDemo component");
            }
            
            Debug.Log("GridDemoUI: Migrated to new UI system with SmartClickDetector and Drag & Drop. Consider removing this legacy component.");
        }
        
        private void SetupLegacyUI()
        {
            // Setup clear grid button
            if (clearGridButton != null)
            {
                clearGridButton.onClick.AddListener(() => {
                    gridManager.ClearGrid();
                    UpdateGridInfo();
                });
            }
            
            // Setup randomize button
            if (randomizeButton != null)
            {
                randomizeButton.onClick.AddListener(RandomizeGrid);
            }
            
            // Setup grid size slider
            if (gridSizeSlider != null)
            {
                gridSizeSlider.minValue = 4;
                gridSizeSlider.maxValue = 12;
                gridSizeSlider.value = gridManager.gridWidth;
                gridSizeSlider.onValueChanged.AddListener(OnGridSizeChanged);
                UpdateGridSizeText();
            }
            
            // Hide info panel initially
            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }
        }
        
        private void CreateIngredientButtons()
        {
            if (ingredientButtonContainer == null || ingredientButtonPrefab == null)
                return;
            
            // Clear existing buttons
            foreach (Transform child in ingredientButtonContainer)
            {
                Destroy(child.gameObject);
            }
            ingredientButtons.Clear();
            
            // Create buttons for each available ingredient
            foreach (Ingredient ingredient in gridManager.availableIngredients)
            {
                if (ingredient != null)
                {
                    CreateIngredientButton(ingredient);
                }
            }
        }
        
        private void CreateIngredientButton(Ingredient ingredient)
        {
            Button button = Instantiate(ingredientButtonPrefab, ingredientButtonContainer);
            button.name = $"Btn_{ingredient.ItemName}";
            
            // Setup button text
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = ingredient.ItemName;
                buttonText.color = GetAspectColor(ingredient.IngredientAspect);
            }
            
            // Setup button image
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                Color aspectColor = GetAspectColor(ingredient.IngredientAspect);
                aspectColor.a = 0.3f;
                buttonImage.color = aspectColor;
            }
            
            // Add click listener
            button.onClick.AddListener(() => SelectIngredient(ingredient));
            
            // Add hover events
            var eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            
            var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((data) => ShowIngredientInfo(ingredient));
            eventTrigger.triggers.Add(pointerEnter);
            
            var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => HideIngredientInfo());
            eventTrigger.triggers.Add(pointerExit);
            
            ingredientButtons.Add(button);
        }
        
        private void SelectIngredient(Ingredient ingredient)
        {
            currentSelectedIngredient = ingredient;
            gridManager.SelectIngredient(ingredient);
            
            if (selectedIngredientText != null)
            {
                selectedIngredientText.text = $"Selected: {ingredient.ItemName}";
                selectedIngredientText.color = GetAspectColor(ingredient.IngredientAspect);
            }
            
            // Update button visuals
            UpdateButtonSelection(ingredient);
        }
        
        private void UpdateButtonSelection(Ingredient selectedIngredient)
        {
            foreach (Button button in ingredientButtons)
            {
                ColorBlock colors = button.colors;
                
                // Check if this button represents the selected ingredient
                bool isSelected = button.name.Contains(selectedIngredient.ItemName);
                
                if (isSelected)
                {
                    colors.normalColor = Color.white;
                    colors.highlightedColor = Color.yellow;
                }
                else
                {
                    Color aspectColor = GetAspectColor(selectedIngredient.IngredientAspect);
                    aspectColor.a = 0.3f;
                    colors.normalColor = aspectColor;
                    colors.highlightedColor = aspectColor * 1.2f;
                }
                
                button.colors = colors;
            }
        }
        
        private void ShowIngredientInfo(Ingredient ingredient)
        {
            if (infoPanel != null)
            {
                infoPanel.SetActive(true);
                
                if (ingredientNameText != null)
                    ingredientNameText.text = ingredient.ItemName;
                
                if (ingredientDescriptionText != null)
                    ingredientDescriptionText.text = ingredient.ItemDescription;
                
                if (ingredientStatsText != null)
                {
                    string stats = $"Aspect: {ingredient.IngredientAspect}\n" +
                                   $"Potency: {ingredient.Potency}/5\n" +
                                   $"Size: {ingredient.GridWidth}x{ingredient.GridHeight}\n" +
                                   $"Archetype: {ingredient.IngredientArchetype}\n" +
                                   $"Stability: {ingredient.StabilityRating:P0}";
                    
                    if (ingredient.IsCorrupted)
                        stats += "\n<color=red>CORRUPTED</color>";
                    
                    ingredientStatsText.text = stats;
                }
                
                // Set icon if available
                if (ingredientIcon != null && ingredient.ItemIcon != null)
                {
                    ingredientIcon.sprite = ingredient.ItemIcon;
                    ingredientIcon.color = Color.white;
                }
                else if (ingredientIcon != null)
                {
                    ingredientIcon.color = GetAspectColor(ingredient.IngredientAspect);
                }
            }
        }
        
        private void HideIngredientInfo()
        {
            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }
        }
        
        private void OnGridSizeChanged(float value)
        {
            int newSize = Mathf.RoundToInt(value);
            gridManager.gridWidth = newSize;
            gridManager.gridHeight = newSize;
            UpdateGridSizeText();
            
            // Note: In a full implementation, you'd need to rebuild the grid
            Debug.Log($"Grid size changed to {newSize}x{newSize} (requires restart to take effect)");
        }
        
        private void UpdateGridSizeText()
        {
            if (gridSizeText != null && gridSizeSlider != null)
            {
                int size = Mathf.RoundToInt(gridSizeSlider.value);
                gridSizeText.text = $"{size}x{size}";
            }
        }
        
        private void RandomizeGrid()
        {
            gridManager.ClearGrid();
            
            if (gridManager.availableIngredients.Count == 0)
                return;
            
            // Place random ingredients
            int attempts = 20;
            while (attempts > 0)
            {
                Ingredient randomIngredient = gridManager.availableIngredients[Random.Range(0, gridManager.availableIngredients.Count)];
                Vector2Int randomPosition = new Vector2Int(
                    Random.Range(0, gridManager.gridWidth - randomIngredient.GridWidth + 1),
                    Random.Range(0, gridManager.gridHeight - randomIngredient.GridHeight + 1)
                );
                
                if (gridManager.TryPlaceIngredient(randomIngredient, randomPosition))
                {
                    attempts -= 5; // Successful placement
                }
                
                attempts--;
            }
            
            UpdateGridInfo();
        }
        
        public void UpdateGridInfo()
        {
            if (gridInfoText != null)
            {
                IngredientPlacer placer = gridManager.GetComponent<IngredientPlacer>();
                int placedCount = placer.GetAllPlacedIngredients().Count;
                int totalCells = gridManager.gridWidth * gridManager.gridHeight;
                int occupiedCells = 0;
                
                GridCell[,] cells = gridManager.GetAllCells();
                for (int x = 0; x < gridManager.gridWidth; x++)
                {
                    for (int y = 0; y < gridManager.gridHeight; y++)
                    {
                        if (cells[x, y].IsOccupied)
                            occupiedCells++;
                    }
                }
                
                float occupancyRate = (float)occupiedCells / totalCells * 100f;
                
                gridInfoText.text = $"Grid: {gridManager.gridWidth}x{gridManager.gridHeight}\n" +
                                   $"Ingredients: {placedCount}\n" +
                                   $"Occupancy: {occupancyRate:F1}%";
            }
        }
        
        private Color GetAspectColor(Aspect aspect)
        {
            switch (aspect)
            {
                case Aspect.Scorch: return Color.red;
                case Aspect.Frigid: return Color.cyan;
                case Aspect.Arc: return Color.yellow;
                case Aspect.Caustic: return new Color(0.5f, 0.3f, 0.1f); // Brown
                case Aspect.Corporeal: return Color.gray;
                case Aspect.Divine: return Color.white;
                default: return Color.gray;
            }
        }
        
        private void Update()
        {
            // Update grid info periodically
            if (Time.frameCount % 60 == 0) // Every 60 frames
            {
                UpdateGridInfo();
            }
        }
        
        // Public methods for backward compatibility
        public void RefreshIngredientButtons()
        {
            if (uiManager != null)
            {
                uiManager.RefreshUI();
            }
            else
            {
                CreateIngredientButtons();
            }
        }
    }
}