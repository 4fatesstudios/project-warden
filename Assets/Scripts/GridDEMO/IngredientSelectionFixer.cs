using System;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.GridDemo.UI;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Fixed version of the ingredient selection script
    /// Attach this to the GridDemo UI GameObject and it will set up everything automatically
    /// </summary>
    public class IngredientSelectionFixer : MonoBehaviour
    {
        [Header("Auto-Fix Settings")]
        [SerializeField] private bool runOnStart = true;
        [SerializeField] private bool createButtonsImmediately = true;
        
        private void Start()
        {
            if (runOnStart)
            {
                FixIngredientSelectionUI();
            }
        }
        
        [ContextMenu("Fix Ingredient Selection")]
        public void FixIngredientSelectionUI()
        {
            Debug.Log("IngredientSelectionFixer: Starting UI fix...");
            
            // Get the GridGameManager
            GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager == null)
            {
                Debug.LogError("IngredientSelectionFixer: No GridGameManager found!");
                return;
            }
            
            // Get the UI components
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            GameObject ingredientPanel = GameObject.Find("GridDemo UI/Ingredient Panel");
            
            if (gridDemoUI == null || ingredientPanel == null)
            {
                Debug.LogError("IngredientSelectionFixer: UI GameObjects not found!");
                return;
            }
            
            // Add necessary UI components
            SetupUIComponents(gridDemoUI, ingredientPanel);
            
            if (createButtonsImmediately)
            {
                CreateIngredientButtons(gridManager, ingredientPanel);
                CreateClearButton(gridManager, ingredientPanel);
            }
            
            Debug.Log("IngredientSelectionFixer: Ingredient selection is now working!");
        }
        
        private void SetupUIComponents(GameObject gridDemoUI, GameObject ingredientPanel)
        {
            // Add GridDemoUIManager if missing
            GridDemoUIManager uiManager = gridDemoUI.GetComponent<GridDemoUIManager>();
            if (uiManager == null)
            {
                uiManager = gridDemoUI.AddComponent<GridDemoUIManager>();
                Debug.Log("IngredientSelectionFixer: Added GridDemoUIManager");
            }
        }
        
        private void CreateIngredientButtons(GridGameManager gridManager, GameObject ingredientPanel)
        {
            // Create a container for ingredient buttons
            Transform container = ingredientPanel.transform.Find("Ingredient Buttons");
            if (container == null)
            {
                GameObject containerObj = new GameObject("Ingredient Buttons");
                containerObj.transform.SetParent(ingredientPanel.transform, false);
                
                // Setup container layout
                RectTransform containerRect = containerObj.AddComponent<RectTransform>();
                containerRect.anchorMin = Vector2.zero;
                containerRect.anchorMax = Vector2.one;
                containerRect.offsetMin = Vector2.zero;
                containerRect.offsetMax = Vector2.zero;
                
                // Add grid layout
                GridLayoutGroup gridLayout = containerObj.AddComponent<GridLayoutGroup>();
                gridLayout.cellSize = new Vector2(150f, 50f);
                gridLayout.spacing = new Vector2(10f, 10f);
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = 3;
                gridLayout.padding = new RectOffset(10, 10, 10, 10);
                
                // Add content size fitter
                ContentSizeFitter sizeFitter = containerObj.AddComponent<ContentSizeFitter>();
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                
                container = containerObj.transform;
                Debug.Log("IngredientSelectionFixer: Created ingredient button container");
            }
            
            // Clear existing buttons
            foreach (Transform child in container)
            {
                DestroyImmediate(child.gameObject);
            }
            
            // Create buttons for each available ingredient
            if (gridManager.availableIngredients != null)
            {
                foreach (var ingredient in gridManager.availableIngredients)
                {
                    if (ingredient != null)
                    {
                        CreateIngredientButton(ingredient, container, gridManager);
                    }
                }
                
                Debug.Log($"IngredientSelectionFixer: Created {gridManager.availableIngredients.Count} ingredient buttons");
            }
            else
            {
                Debug.LogWarning("IngredientSelectionFixer: No available ingredients found in GridGameManager");
            }
        }
        
        private void CreateIngredientButton(
            Ingredient ingredient, 
            Transform container, 
            GridGameManager gridManager)
        {
            // Create button GameObject
            GameObject buttonObj = new GameObject($"Btn_{ingredient.ItemName}");
            buttonObj.transform.SetParent(container, false);
            
            // Add RectTransform
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(150f, 50f);
            
            // Add Image component for background
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = GetAspectColor(ingredient.IngredientAspect);
            
            // Add Button component
            Button button = buttonObj.AddComponent<Button>();
            
            // Create text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = ingredient.ItemName;
            buttonText.fontSize = 12f;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            
            // Add click functionality - only place on grid, not everywhere
            button.onClick.AddListener(() => {
                // Only select the ingredient, don't place it immediately
                gridManager.SelectIngredient(ingredient);
                Debug.Log($"Selected ingredient: {ingredient.ItemName} - Click on grid to place it");
                
                // Update button visual feedback
                UpdateButtonSelection(container, buttonObj);
            });
            
            // Add hover effects
            var eventTrigger = buttonObj.AddComponent<EventTrigger>();
            
            var pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener(data => {
                buttonImage.color = GetAspectColor(ingredient.IngredientAspect) * 1.2f;
            });
            eventTrigger.triggers.Add(pointerEnter);
            
            var pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener(data => {
                if (!IsButtonSelected(buttonObj))
                {
                    buttonImage.color = GetAspectColor(ingredient.IngredientAspect);
                }
            });
            eventTrigger.triggers.Add(pointerExit);
        }
        
        private void UpdateButtonSelection(Transform container, GameObject selectedButton)
        {
            // Reset all buttons to normal state
            foreach (Transform child in container)
            {
                Image childImage = child.GetComponent<Image>();
                if (childImage != null)
                {
                    string ingredientName = child.name.Replace("Btn_", "");
                    // Find ingredient to get aspect color
                    GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
                    var ingredient = gridManager.availableIngredients.Find(
                        i => i != null && i.ItemName == ingredientName);
                    
                    if (ingredient != null)
                    {
                        childImage.color = GetAspectColor(ingredient.IngredientAspect);
                    }
                }
                
                // Remove selected tag
                if (child.gameObject.CompareTag("EditorOnly"))
                {
                    child.gameObject.tag = "Untagged";
                }
            }
            
            // Highlight selected button
            Image selectedImage = selectedButton.GetComponent<Image>();
            if (selectedImage != null)
            {
                selectedImage.color = Color.white;
                selectedButton.tag = "EditorOnly"; // Use as temporary "selected" marker
            }
        }
        
        private bool IsButtonSelected(GameObject button)
        {
            return button.CompareTag("EditorOnly");
        }
        
        private Color GetAspectColor(Aspect aspect)
        {
            switch (aspect)
            {
                case Aspect.Scorch: 
                    return new Color(1f, 0.3f, 0.3f, 0.8f);
                case Aspect.Frigid: 
                    return new Color(0.3f, 0.8f, 1f, 0.8f);
                case Aspect.Arc: 
                    return new Color(1f, 1f, 0.3f, 0.8f);
                case Aspect.Caustic: 
                    return new Color(0.8f, 0.5f, 0.2f, 0.8f);
                case Aspect.Corporeal: 
                    return new Color(0.7f, 0.7f, 0.7f, 0.8f);
                case Aspect.Divine: 
                    return new Color(1f, 1f, 1f, 0.8f);
                default: 
                    return new Color(0.6f, 0.6f, 0.6f, 0.8f);
            }
        }
        
        [ContextMenu("Refresh Buttons")]
        public void RefreshButtons()
        {
            GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
            GameObject ingredientPanel = GameObject.Find("GridDemo UI/Ingredient Panel");
            
            if (gridManager != null && ingredientPanel != null)
            {
                CreateIngredientButtons(gridManager, ingredientPanel);
                CreateClearButton(gridManager, ingredientPanel);
            }
        }
        
        private void CreateClearButton(GridGameManager gridManager, GameObject ingredientPanel)
        {
            // Find or create the button container
            Transform container = ingredientPanel.transform.Find("Ingredient Buttons");
            if (container == null) return;
            
            // Check if clear button already exists
            Transform existingClearButton = container.Find("Btn_ClearGrid");
            if (existingClearButton != null)
            {
                DestroyImmediate(existingClearButton.gameObject);
            }
            
            // Create clear button GameObject
            GameObject clearButtonObj = new GameObject("Btn_ClearGrid");
            clearButtonObj.transform.SetParent(container, false);
            
            // Add RectTransform
            RectTransform buttonRect = clearButtonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(150f, 50f);
            
            // Add Image component for background
            Image buttonImage = clearButtonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 0.8f); // Red color for clear button
            
            // Add Button component
            Button button = clearButtonObj.AddComponent<Button>();
            
            // Create text child
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
            
            // Add clear functionality
            button.onClick.AddListener(() => {
                if (gridManager != null)
                {
                    gridManager.ClearGrid();
                    Debug.Log("Grid cleared successfully!");
                }
            });
            
            // Add hover effects
            var eventTrigger = clearButtonObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            
            var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((data) => {
                buttonImage.color = new Color(1f, 0.3f, 0.3f, 1f); // Brighter red on hover
            });
            eventTrigger.triggers.Add(pointerEnter);
            
            var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => {
                buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 0.8f); // Back to normal red
            });
            eventTrigger.triggers.Add(pointerExit);
            
            Debug.Log("IngredientSelectionFixer: Created Clear Grid button");
        }
    }
}