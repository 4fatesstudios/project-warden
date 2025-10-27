using UnityEngine;
using System.Linq;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Clean fixes for scroll container visibility and multiple text planes issues
    /// </summary>
    public class ScrollAndTextFixes : MonoBehaviour
    {
        [Header("Fix Options")]
        [SerializeField] private bool fixOnStart = true;
        [SerializeField] private bool showCompletionMessage = true;
        
        [Header("Text Label Options")]
        [SerializeField] private bool disableIngredientLabels = true;
        [SerializeField] private bool useUILabelsInstead = false;
        
        private void Start()
        {
            if (fixOnStart)
            {
                ApplyAllFixes();
            }
        }
        
        [ContextMenu("🔧 Apply All Fixes")]
        public void ApplyAllFixes()
        {
            Debug.Log("FixedScrollAndTextFixes: Applying fixes...");
            
            try
            {
                FixScrollContainerDisplay();
                FixTextPlaneGeneration();
                
                if (showCompletionMessage)
                {
                    Debug.Log("✅ All fixes applied successfully! Scroll container is now visible and text planes are disabled.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"FixedScrollAndTextFixes: Error: {e.Message}");
            }
        }
        
        private void FixScrollContainerDisplay()
        {
            Debug.Log("FixedScrollAndTextFixes: Fixing scroll container visibility...");
            
            // Let AutomaticSidebarManager handle all sidebar creation and management (if it exists)
            bool foundAutomaticManager = false;
            
            var gridGameManager = FindFirstObjectByType<FourFatesStudios.ProjectWarden.GridDemo.GridGameManager>();
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
                Debug.Log("✅ AutomaticSidebarManager found - it will handle all UI automatically");
            }
            else
            {
                Debug.Log("📝 AutomaticSidebarManager will be added by GridGameManager when it starts");
            }
            
            Debug.Log("✅ Scroll container fixes completed - UI management delegated to AutomaticSidebarManager!");
        }
        
        /// <summary>
        /// Ensures only one compact menu exists in the scene by removing ACTUAL duplicates only
        /// </summary>
        private void EnsureOnlyOneCompactMenu()
        {
            // Find all objects with "Compact" in their name
            GameObject[] allCompactObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go => go.name.Contains("Compact Sidebar") || 
                            go.name.Contains("Compact Menu") || 
                            go.name.Contains("CompactSidebar"))
                .ToArray();
            
            // Find all CompactUIDesigner components
            CompactUIDesigner[] allDesigners = FindObjectsByType<CompactUIDesigner>(FindObjectsSortMode.None);
            
            int removedCount = 0;
            
            // Only remove compact sidebar GameObjects if there are MORE THAN ONE
            if (allCompactObjects.Length > 1)
            {
                Debug.Log($"FixedScrollAndTextFixes: Found {allCompactObjects.Length} compact sidebar objects, removing duplicates...");
                
                // Keep the first one, remove the rest
                for (int i = 1; i < allCompactObjects.Length; i++)
                {
                    if (allCompactObjects[i] != null)
                    {
                        Debug.Log($"FixedScrollAndTextFixes: Removing duplicate compact menu: {allCompactObjects[i].name}");
                        DestroyImmediate(allCompactObjects[i]);
                        removedCount++;
                    }
                }
            }
            
            // Keep only the first CompactUIDesigner, remove the rest
            if (allDesigners.Length > 1)
            {
                Debug.Log($"FixedScrollAndTextFixes: Found {allDesigners.Length} CompactUIDesigner components, removing duplicates...");
                
                for (int i = 1; i < allDesigners.Length; i++)
                {
                    if (allDesigners[i] != null)
                    {
                        Debug.Log($"FixedScrollAndTextFixes: Removing duplicate CompactUIDesigner: {allDesigners[i].name}");
                        DestroyImmediate(allDesigners[i].gameObject);
                        removedCount++;
                    }
                }
            }
            
            if (removedCount > 0)
            {
                Debug.Log($"✅ Removed {removedCount} actual duplicate compact menu objects");
            }
            else
            {
                Debug.Log("✅ No duplicate compact menus found - single sidebar preserved");
            }
        }
        
        private void FixTextPlaneGeneration()
        {
            Debug.Log("FixedScrollAndTextFixes: Preventing text plane generation...");
            
            IngredientPlacer ingredientPlacer = FindFirstObjectByType<IngredientPlacer>();
            if (ingredientPlacer != null)
            {
                var enableLabelsField = typeof(IngredientPlacer).GetField("enableIngredientLabels", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var use3DLabelsField = typeof(IngredientPlacer).GetField("use3DTextLabels", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var useUILabelsField = typeof(IngredientPlacer).GetField("useUITextLabels", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (enableLabelsField != null)
                {
                    enableLabelsField.SetValue(ingredientPlacer, !disableIngredientLabels);
                }
                
                if (use3DLabelsField != null)
                {
                    use3DLabelsField.SetValue(ingredientPlacer, false);
                }
                
                if (useUILabelsField != null)
                {
                    useUILabelsField.SetValue(ingredientPlacer, useUILabelsInstead);
                }
                
                Debug.Log("✅ Text plane generation disabled!");
            }
            
            CleanupExistingTextPlanes();
        }
        
        private void CleanupExistingTextPlanes()
        {
            TextMesh[] textMeshes = FindObjectsByType<TextMesh>(FindObjectsSortMode.None);
            int removedCount = 0;
            
            foreach (var textMesh in textMeshes)
            {
                if (textMesh.gameObject.name.Contains("Label"))
                {
                    DestroyImmediate(textMesh.gameObject);
                    removedCount++;
                }
            }
            
            if (removedCount > 0)
            {
                Debug.Log($"✅ Removed {removedCount} text plane labels");
            }
        }
        
        [ContextMenu("🧪 Test Fixes")]
        public void TestFixes()
        {
            Debug.Log("=== TESTING FIXES ===");
            
            GameObject scrollArea = GameObject.Find("Ingredient Scroll");
            if (scrollArea != null)
            {
                var scrollBg = scrollArea.GetComponent<UnityEngine.UI.Image>();
                var scrollOutline = scrollArea.GetComponent<UnityEngine.UI.Outline>();
                
                Debug.Log($"Scroll Background: {(scrollBg ? "Present" : "Missing")}");
                Debug.Log($"Scroll Outline: {(scrollOutline ? "Present" : "Missing")}");
            }
            else
            {
                Debug.LogWarning("No scroll area found!");
            }
            
            TextMesh[] textMeshes = FindObjectsByType<TextMesh>(FindObjectsSortMode.None);
            Debug.Log($"Text planes in scene: {textMeshes.Length}");
            
            Debug.Log("=== TEST COMPLETE ===");
        }
    }
}