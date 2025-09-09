using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Demo script to showcase the new shape-based ingredient placement system
    /// </summary>
    public class ShapeBasedPlacementDemo : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [SerializeField] private GridGameManager gridManager;
        [SerializeField] private List<Ingredient> demoIngredients = new List<Ingredient>();
        [SerializeField] private Canvas demoUI;
        
        [Header("UI Controls")]
        [SerializeField] private Button[] ingredientButtons;
        [SerializeField] private Button clearGridButton;
        [SerializeField] private Button randomPlacementButton;
        [SerializeField] private Text instructionText;
        
        [Header("Shape Template Demo")]
        [SerializeField] private Button[] templateButtons;
        [SerializeField] private Ingredient templateDemoIngredient;
        
        private int currentIngredientIndex = 0;
        
        private void Start()
        {
            SetupDemo();
        }
        
        private void SetupDemo()
        {
            // Auto-find GridGameManager if not assigned
            if (gridManager == null)
            {
                gridManager = FindFirstObjectByType<GridGameManager>();
            }
            
            if (gridManager == null)
            {
                Debug.LogError("ShapeBasedPlacementDemo: No GridGameManager found!");
                return;
            }
            
            SetupUI();
            SetupInstructions();
            
            // Select first ingredient by default
            if (demoIngredients.Count > 0)
            {
                SelectIngredient(0);
            }
            
            Debug.Log("Shape-based placement demo initialized!");
        }
        
        private void SetupUI()
        {
            // Setup ingredient selection buttons
            for (int i = 0; i < ingredientButtons.Length && i < demoIngredients.Count; i++)
            {
                int index = i; // Capture for closure
                if (ingredientButtons[i] != null)
                {
                    ingredientButtons[i].onClick.AddListener(() => SelectIngredient(index));
                    
                    // Update button text with ingredient name
                    var text = ingredientButtons[i].GetComponentInChildren<Text>();
                    if (text != null && i < demoIngredients.Count)
                    {
                        text.text = demoIngredients[i].ItemName;
                    }
                }
            }
            
            // Setup utility buttons
            if (clearGridButton != null)
            {
                clearGridButton.onClick.AddListener(ClearGrid);
            }
            
            if (randomPlacementButton != null)
            {
                randomPlacementButton.onClick.AddListener(RandomPlacement);
            }
            
            // Setup template demo buttons
            var templates = System.Enum.GetValues(typeof(ShapeTemplate));
            for (int i = 0; i < templateButtons.Length && i < templates.Length; i++)
            {
                var template = (ShapeTemplate)templates.GetValue(i);
                if (templateButtons[i] != null)
                {
                    templateButtons[i].onClick.AddListener(() => ApplyTemplate(template));
                    
                    // Update button text
                    var text = templateButtons[i].GetComponentInChildren<Text>();
                    if (text != null)
                    {
                        text.text = template.ToString();
                    }
                }
            }
        }
        
        private void SetupInstructions()
        {
            if (instructionText != null)
            {
                instructionText.text = 
                    "SHAPE-BASED INGREDIENT PLACEMENT DEMO\n\n" +
                    "• Select an ingredient using the buttons\n" +
                    "• Hover over the grid to see shape preview\n" +
                    "• Click to place the ingredient\n" +
                    "• Green = valid placement, Red = invalid\n" +
                    "• Use template buttons to change shapes\n" +
                    "• Notice how different shapes affect placement!";
            }
        }
        
        private void SelectIngredient(int index)
        {
            if (index >= 0 && index < demoIngredients.Count)
            {
                currentIngredientIndex = index;
                var ingredient = demoIngredients[index];
                gridManager.SelectIngredient(ingredient);
                
                Debug.Log($"Selected ingredient: {ingredient.ItemName} " +
                         $"(Shape: {ingredient.ShapeData?.Template ?? ShapeTemplate.Rectangle}, " +
                         $"Size: {ingredient.GridWidth}x{ingredient.GridHeight})");
                
                UpdateIngredientInfo(ingredient);
            }
        }
        
        private void UpdateIngredientInfo(Ingredient ingredient)
        {
            if (instructionText != null)
            {
                string shapeInfo = ingredient.ShapeData != null 
                    ? $"Shape: {ingredient.ShapeData.Template} ({ingredient.ShapeData.ActiveCells.Count} cells)"
                    : $"Rectangle: {ingredient.GridWidth}x{ingredient.GridHeight}";
                    
                instructionText.text = 
                    $"SELECTED: {ingredient.ItemName}\n" +
                    $"{shapeInfo}\n" +
                    $"Aspect: {ingredient.IngredientAspect}\n" +
                    $"Potency: {ingredient.Potency}\n\n" +
                    "Hover over grid to preview placement!";
            }
        }
        
        private void ClearGrid()
        {
            gridManager.ClearGrid();
            Debug.Log("Grid cleared!");
        }
        
        private void RandomPlacement()
        {
            if (demoIngredients.Count == 0) return;
            
            // Try to place 3-5 random ingredients
            int placementCount = Random.Range(3, 6);
            
            for (int i = 0; i < placementCount; i++)
            {
                var randomIngredient = demoIngredients[Random.Range(0, demoIngredients.Count)];
                var randomPosition = new Vector2Int(
                    Random.Range(0, gridManager.gridWidth - 1),
                    Random.Range(0, gridManager.gridHeight - 1)
                );
                
                if (gridManager.CanPlaceIngredient(randomIngredient, randomPosition))
                {
                    gridManager.TryPlaceIngredient(randomIngredient, randomPosition);
                }
            }
            
            Debug.Log($"Random placement completed!");
        }
        
        private void ApplyTemplate(ShapeTemplate template)
        {
            if (templateDemoIngredient != null)
            {
                templateDemoIngredient.ApplyShapeTemplate(template);
                Debug.Log($"Applied {template} template to demo ingredient");
                
                // If this ingredient is currently selected, update the selection
                if (currentIngredientIndex < demoIngredients.Count && 
                    demoIngredients[currentIngredientIndex] == templateDemoIngredient)
                {
                    UpdateIngredientInfo(templateDemoIngredient);
                }
            }
        }
        
        private void Update()
        {
            // Quick keyboard shortcuts for demo
            if (Input.GetKeyDown(KeyCode.Alpha1) && demoIngredients.Count > 0) SelectIngredient(0);
            if (Input.GetKeyDown(KeyCode.Alpha2) && demoIngredients.Count > 1) SelectIngredient(1);
            if (Input.GetKeyDown(KeyCode.Alpha3) && demoIngredients.Count > 2) SelectIngredient(2);
            if (Input.GetKeyDown(KeyCode.Alpha4) && demoIngredients.Count > 3) SelectIngredient(3);
            if (Input.GetKeyDown(KeyCode.C)) ClearGrid();
            if (Input.GetKeyDown(KeyCode.R)) RandomPlacement();
        }
        
        private void OnValidate()
        {
            // Ensure we have at least one demo ingredient
            if (demoIngredients.Count == 0)
            {
                Debug.LogWarning("ShapeBasedPlacementDemo: Please assign demo ingredients!");
            }
        }
    }
}