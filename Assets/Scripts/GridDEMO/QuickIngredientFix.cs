using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.GridDemo.UI;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Quick fix for ingredient loading issues - run this once to fix the null ingredients problem
    /// </summary>
    public class QuickIngredientFix : MonoBehaviour
    {
        private void Start()
        {
            // Auto-fix on start with a longer delay to ensure everything is initialized
            Invoke(nameof(FixIngredientsNow), 1.5f);
        }
        
        [ContextMenu("Fix Ingredients Now")]
        public void FixIngredientsNow()
        {
            Debug.Log("🔧 === QUICK INGREDIENT FIX STARTING ===");
            
            // Find GridGameManager
            GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager == null)
            {
                Debug.LogError("❌ GridGameManager not found!");
                return;
            }
            
            // Try multiple methods to load ingredients
            List<Ingredient> allIngredients = new List<Ingredient>();
            
            // Method 1: Direct path
            Debug.Log("📦 Method 1: Loading from Resources/Items/Ingredients...");
            Ingredient[] ingredients1 = Resources.LoadAll<Ingredient>("Items/Ingredients");
            Debug.Log($"📦 Method 1 found: {ingredients1.Length} ingredients");
            
            // Method 2: Fallback search all Resources
            Debug.Log("📦 Method 2: Searching all Resources...");
            Ingredient[] ingredients2 = Resources.LoadAll<Ingredient>("");
            Debug.Log($"📦 Method 2 found: {ingredients2.Length} ingredients");
            
            // Method 3: Search via ScriptableObject base class
            Debug.Log("📦 Method 3: Searching via ScriptableObject...");
            var allScriptableObjects = Resources.LoadAll<ScriptableObject>("");
            var ingredientObjects = allScriptableObjects.OfType<Ingredient>().ToArray();
            Debug.Log($"📦 Method 3 found: {ingredientObjects.Length} ingredients");
            
            // Use the best method that found ingredients
            Ingredient[] bestIngredients = null;
            if (ingredients1.Length > 0)
            {
                bestIngredients = ingredients1;
                Debug.Log("✅ Using Method 1 results");
            }
            else if (ingredients2.Length > 0)
            {
                bestIngredients = ingredients2;
                Debug.Log("✅ Using Method 2 results");
            }
            else if (ingredientObjects.Length > 0)
            {
                bestIngredients = ingredientObjects;
                Debug.Log("✅ Using Method 3 results");
            }
            
            if (bestIngredients == null || bestIngredients.Length == 0)
            {
                Debug.LogError("❌ No ingredients found with any method!");
                Debug.LogWarning("💡 Make sure you have Ingredient ScriptableObjects in Assets/Resources/Items/Ingredients/");
                return;
            }
            
            // Filter out nulls and create clean list
            var validIngredients = bestIngredients.Where(i => i != null).Distinct().ToList();
            Debug.Log($"📦 {validIngredients.Count} valid ingredients after filtering");
            
            // Initialize or clear the list
            if (gridManager.availableIngredients == null)
            {
                gridManager.availableIngredients = new List<Ingredient>();
            }
            gridManager.availableIngredients.Clear();
            
            // Add all valid ingredients
            gridManager.availableIngredients.AddRange(validIngredients);
            
            Debug.Log($"✅ Fixed! GridGameManager now has {gridManager.availableIngredients.Count} ingredients:");
            foreach (var ingredient in gridManager.availableIngredients)
            {
                Debug.Log($"  • {ingredient.ItemName} (Type: {ingredient.GetType().Name})");
            }
            
            // Now refresh the UI
            Debug.Log("🔄 Refreshing UI...");
            RefreshCompactUI();
            
            Debug.Log("✅ Fix complete!");
        }
        
        private void RefreshCompactUI()
        {
            // Let AutomaticSidebarManager handle all UI management (if it exists)
            bool foundAutomaticManager = false;
            
            // Look for AutomaticSidebarManager on GridGameManager
            var gridGameManager = FindFirstObjectByType<GridGameManager>();
            if (gridGameManager != null)
            {
                var components = gridGameManager.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    if (component.GetType().Name == "AutomaticSidebarManager")
                    {
                        foundAutomaticManager = true;
                        break;
                    }
                }
            }
            
            if (foundAutomaticManager)
            {
                Debug.Log("✅ AutomaticSidebarManager found - it will handle UI refresh automatically");
            }
            else
            {
                Debug.Log("⚠️ AutomaticSidebarManager not found - trying fallback approach");
                
                // Fallback: try to refresh existing designer without recreating UI
                var compactDesigner = FindFirstObjectByType<CompactUIDesigner>();
                if (compactDesigner != null)
                {
                    compactDesigner.RefreshIngredientButtons();
                    Debug.Log("✅ Refreshed ingredients using existing CompactUIDesigner");
                }
                else
                {
                    Debug.Log("📝 No CompactUIDesigner found - UI will be handled automatically when needed");
                }
            }
        }
    }
}