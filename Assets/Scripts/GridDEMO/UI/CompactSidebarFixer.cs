using UnityEngine;
using FourFatesStudios.ProjectWarden.GridDemo.UI;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Fixes issues with the compact sidebar not showing ingredients
    /// </summary>
    public class CompactSidebarFixer : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool debugMode = true;
        
        private void Start()
        {
            // Add a small delay to ensure all components are loaded
            Invoke(nameof(FixCompactSidebar), 0.5f);
        }
        
        private void Update()
        {
            // Quick fix shortcut for testing
            if (Input.GetKeyDown(KeyCode.F5))
            {
                FixCompactSidebar();
            }
        }
        
        [ContextMenu("Fix Compact Sidebar")]
        public void FixCompactSidebar()
        {
            if (debugMode)
            {
                Debug.Log("🔧 Starting Compact Sidebar Fix...");
            }
            
            // Step 1: Find GridGameManager and check ingredients
            GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager == null)
            {
                Debug.LogError("❌ GridGameManager not found! Cannot fix sidebar.");
                return;
            }
            
            // Step 2: Check if ingredients are available
            if (gridManager.availableIngredients == null || gridManager.availableIngredients.Count == 0)
            {
                Debug.LogWarning("⚠️ No ingredients found in GridGameManager! Loading default ingredients...");
                LoadDefaultIngredients(gridManager);
            }
            
            if (debugMode)
            {
                Debug.Log($"📦 Found {gridManager.availableIngredients.Count} ingredients in GridGameManager");
            }
            
            // Step 3: Find or create CompactUIDesigner
            CompactUIDesigner compactUI = FindFirstObjectByType<CompactUIDesigner>();
            if (compactUI == null)
            {
                Debug.Log("🔧 CompactUIDesigner not found. Creating one...");
                CreateCompactUIDesigner(gridManager);
            }
            else
            {
                Debug.Log("✅ CompactUIDesigner found. Refreshing ingredients...");
                compactUI.RefreshIngredientButtons();
            }
            
            // Step 4: Check if sidebar exists and is populated
            VerifySidebarSetup();
        }
        
        private void LoadDefaultIngredients(GridGameManager gridManager)
        {
            // Try to find ingredients in the project using different paths
            var ingredients = Resources.LoadAll<FourFatesStudios.ProjectWarden.ScriptableObjects.Items.Ingredient>("Items/Ingredients");
            
            if (ingredients.Length == 0)
            {
                // Try alternative paths
                ingredients = Resources.LoadAll<FourFatesStudios.ProjectWarden.ScriptableObjects.Items.Ingredient>("Ingredients");
            }
            
            if (ingredients.Length == 0)
            {
                // Try loading all ingredient assets
                ingredients = Resources.LoadAll<FourFatesStudios.ProjectWarden.ScriptableObjects.Items.Ingredient>("");
            }
            
            if (ingredients.Length > 0)
            {
                gridManager.availableIngredients.Clear();
                gridManager.availableIngredients.AddRange(ingredients);
                Debug.Log($"✅ Loaded {ingredients.Length} ingredients from Resources:");
                foreach (var ingredient in ingredients)
                {
                    Debug.Log($"   - {ingredient.ItemName} (Aspect: {ingredient.IngredientAspect})");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ No ingredients found in Resources. Trying manual asset loading...");
                TryLoadIngredientsManually(gridManager);
            }
        }
        
        private void TryLoadIngredientsManually(GridGameManager gridManager)
        {
            // List of known ingredient asset names
            string[] ingredientPaths = {
                "Items/Ingredients/EarthShard",
                "Items/Ingredients/FireClaw", 
                "Items/Ingredients/FireTalon",
                "Items/Ingredients/FrozenDew",
                "Items/Ingredients/ShadowHerb",
                "Items/Ingredients/TestIngredient1",
                "Items/Ingredients/TestIngredient2",
                "Items/Ingredients/WindEssence"
            };
            
            int loadedCount = 0;
            gridManager.availableIngredients.Clear();
            
            foreach (string path in ingredientPaths)
            {
                var ingredient = Resources.Load<FourFatesStudios.ProjectWarden.ScriptableObjects.Items.Ingredient>(path);
                if (ingredient != null)
                {
                    gridManager.availableIngredients.Add(ingredient);
                    loadedCount++;
                    Debug.Log($"   ✅ Loaded: {ingredient.ItemName}");
                }
            }
            
            if (loadedCount > 0)
            {
                Debug.Log($"✅ Manually loaded {loadedCount} ingredients");
            }
            else
            {
                Debug.LogError("❌ Failed to load any ingredients. Please assign them manually in GridGameManager!");
            }
        }
        
        private void CreateCompactUIDesigner(GridGameManager gridManager)
        {
            GameObject uiObject = GameObject.Find("GridDemo UI");
            if (uiObject == null)
            {
                Debug.LogError("❌ GridDemo UI object not found!");
                return;
            }
            
            // Add CompactUIDesigner to the UI object
            CompactUIDesigner compactUI = uiObject.GetComponent<CompactUIDesigner>();
            if (compactUI == null)
            {
                compactUI = uiObject.AddComponent<CompactUIDesigner>();
                Debug.Log("✅ Added CompactUIDesigner component");
            }
            
            // Trigger the design process
            compactUI.DesignCompactUI();
            Debug.Log("🎨 Compact UI design completed");
        }
        
        private void VerifySidebarSetup()
        {
            GameObject sidebar = GameObject.Find("Compact Sidebar");
            if (sidebar == null)
            {
                Debug.LogWarning("⚠️ Compact Sidebar not found in scene!");
                return;
            }
            
            // Check for ingredient scroll area
            var scrollArea = sidebar.transform.Find("Ingredient Scroll");
            if (scrollArea == null)
            {
                Debug.LogWarning("⚠️ Ingredient Scroll area not found!");
                return;
            }
            
            // Check for button container
            var viewport = scrollArea.Find("Viewport");
            if (viewport == null)
            {
                Debug.LogWarning("⚠️ Viewport not found in Ingredient Scroll!");
                return;
            }
            
            var content = viewport.Find("Content");
            if (content == null)
            {
                Debug.LogWarning("⚠️ Content not found in Viewport!");
                return;
            }
            
            var buttonContainer = content.Find("Button Container");
            if (buttonContainer == null)
            {
                Debug.LogWarning("⚠️ Button Container not found in Content!");
                return;
            }
            
            int buttonCount = buttonContainer.childCount;
            Debug.Log($"📊 Verification complete: Found {buttonCount} ingredient buttons in sidebar");
            
            if (buttonCount == 0)
            {
                Debug.LogWarning("⚠️ No ingredient buttons found! Ingredients may not be loaded or sidebar may need recreation.");
            }
            else
            {
                Debug.Log("✅ Sidebar verification successful!");
            }
        }
        
        [ContextMenu("Debug Scene Setup")]
        public void DebugSceneSetup()
        {
            Debug.Log("🔍 === SCENE DEBUG INFO ===");
            
            // GridGameManager info
            GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
            Debug.Log($"GridGameManager found: {gridManager != null}");
            if (gridManager != null)
            {
                Debug.Log($"Available ingredients: {gridManager.availableIngredients?.Count ?? 0}");
            }
            
            // UI Components info
            CompactUIDesigner compactUI = FindFirstObjectByType<CompactUIDesigner>();
            Debug.Log($"CompactUIDesigner found: {compactUI != null}");
            
            GridDemoUIManager uiManager = FindFirstObjectByType<GridDemoUIManager>();
            Debug.Log($"GridDemoUIManager found: {uiManager != null}");
            
            // Scene objects info
            GameObject sidebar = GameObject.Find("Compact Sidebar");
            Debug.Log($"Compact Sidebar in scene: {sidebar != null}");
            
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            Debug.Log($"GridDemo UI in scene: {gridDemoUI != null}");
            
            Debug.Log("🔍 === END DEBUG INFO ===");
        }
    }
}