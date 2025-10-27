using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.GridDemo.UI;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Quick fix script to resolve ingredient scroll display issues
    /// Add this to the GridGameManager GameObject in the scene
    /// </summary>
    [System.Serializable]
    public class IngredientScrollFix : MonoBehaviour
    {
        [Header("Auto-Fix Settings")]
        [SerializeField] private bool fixOnStart = true;
        [SerializeField] private bool forceLoadIngredients = true;
        
        [Header("Debug Info")]
        [SerializeField] private int loadedIngredientsCount = 0;
        [SerializeField] private bool compactUIExists = false;
        [SerializeField] private bool sidebarExists = false;
        
        private GridGameManager gridManager;
        
        private void Start()
        {
            if (fixOnStart)
            {
                Invoke(nameof(PerformFix), 1f); // Wait a bit longer for all components to initialize
            }
        }
        
        [ContextMenu("Perform Fix")]
        public void PerformFix()
        {
            Debug.Log("🔧 === INGREDIENT SCROLL FIX STARTED ===");
            
            // Step 1: Get GridGameManager
            gridManager = GetComponent<GridGameManager>();
            if (gridManager == null)
            {
                gridManager = FindFirstObjectByType<GridGameManager>();
            }
            
            if (gridManager == null)
            {
                Debug.LogError("❌ GridGameManager not found!");
                return;
            }
            
            // Step 2: Load ingredients if needed
            if (forceLoadIngredients || gridManager.availableIngredients == null || gridManager.availableIngredients.Count == 0)
            {
                LoadAllIngredients();
            }
            
            // Step 3: Force create CompactUIDesigner if needed
            EnsureCompactUIExists();
            
            // Step 4: Update debug info
            UpdateDebugInfo();
            
            Debug.Log("✅ === INGREDIENT SCROLL FIX COMPLETED ===");
        }
        
        private void LoadAllIngredients()
        {
            Debug.Log("📦 Loading ingredients from Resources...");
            
            // Initialize list if null
            if (gridManager.availableIngredients == null)
            {
                gridManager.availableIngredients = new System.Collections.Generic.List<Ingredient>();
            }
            
            // Clear existing
            gridManager.availableIngredients.Clear();
            
            // Load from Items/Ingredients folder
            var ingredients = Resources.LoadAll<Ingredient>("Items/Ingredients");
            
            if (ingredients.Length == 0)
            {
                // Fallback: Load all ingredients from anywhere
                ingredients = Resources.LoadAll<Ingredient>("");
            }
            
            // Add ingredients to manager
            foreach (var ingredient in ingredients)
            {
                if (ingredient != null)
                {
                    gridManager.availableIngredients.Add(ingredient);
                    Debug.Log($"   ✅ Loaded: {ingredient.ItemName}");
                }
            }
            
            loadedIngredientsCount = gridManager.availableIngredients.Count;
            Debug.Log($"📊 Total ingredients loaded: {loadedIngredientsCount}");
        }
        
        private void EnsureCompactUIExists()
        {
            // Find GridDemo UI object
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            if (gridDemoUI == null)
            {
                Debug.LogError("❌ GridDemo UI object not found!");
                return;
            }
            
            // Check if CompactUIDesigner exists
            CompactUIDesigner compactUI = gridDemoUI.GetComponent<CompactUIDesigner>();
            compactUIExists = compactUI != null;
            
            if (!compactUIExists)
            {
                Debug.Log("🔧 Adding CompactUIDesigner component...");
                compactUI = gridDemoUI.AddComponent<CompactUIDesigner>();
                compactUIExists = true;
            }
            
            // Force refresh the UI
            if (compactUI != null)
            {
                Debug.Log("🎨 Refreshing Compact UI...");
                
                // Clear any existing sidebar first
                GameObject existingSidebar = GameObject.Find("Compact Sidebar");
                if (existingSidebar != null)
                {
                    DestroyImmediate(existingSidebar);
                    Debug.Log("🗑️ Removed existing sidebar");
                }
                
                // Create new UI
                compactUI.DesignCompactUI();
                
                // Verify sidebar was created
                sidebarExists = GameObject.Find("Compact Sidebar") != null;
                Debug.Log($"📋 Sidebar created: {sidebarExists}");
            }
        }
        
        private void UpdateDebugInfo()
        {
            compactUIExists = FindFirstObjectByType<CompactUIDesigner>() != null;
            sidebarExists = GameObject.Find("Compact Sidebar") != null;
            
            if (gridManager != null && gridManager.availableIngredients != null)
            {
                loadedIngredientsCount = gridManager.availableIngredients.Count;
            }
            
            Debug.Log($"🔍 Debug Info - Ingredients: {loadedIngredientsCount}, CompactUI: {compactUIExists}, Sidebar: {sidebarExists}");
            
            // Check ingredient buttons
            if (sidebarExists)
            {
                GameObject buttonContainer = GameObject.Find("Compact Sidebar/Ingredient Scroll/Viewport/Content/Button Container");
                if (buttonContainer != null)
                {
                    int buttonCount = buttonContainer.transform.childCount;
                    Debug.Log($"🔘 Ingredient buttons found: {buttonCount}");
                    
                    if (buttonCount == 0 && loadedIngredientsCount > 0)
                    {
                        Debug.LogWarning("⚠️ Ingredients loaded but no buttons created! There may be an issue with button generation.");
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ Button container not found in sidebar structure!");
                }
            }
        }
        
        private void Update()
        {
            // Quick fix hotkey
            if (Input.GetKeyDown(KeyCode.F6))
            {
                PerformFix();
            }
        }
        
        private void OnValidate()
        {
            // Update debug info when inspector values change
            if (Application.isPlaying)
            {
                UpdateDebugInfo();
            }
        }
    }
}