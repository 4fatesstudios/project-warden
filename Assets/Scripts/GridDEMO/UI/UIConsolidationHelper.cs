using UnityEngine;
using FourFatesStudios.ProjectWarden.GridDemo.UI;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Helper script to safely migrate from multiple UI components to the consolidated GridDemoUIController
    /// </summary>
    public class UIConsolidationHelper : MonoBehaviour
    {
        [Header("Migration Settings")]
        [SerializeField] private bool autoMigrateOnStart = false;
        [SerializeField] private bool debugMode = true;
        
        [ContextMenu("🔄 Migrate to Consolidated UI")]
        public void MigrateToConsolidatedUI()
        {
            if (debugMode)
            {
                Debug.Log("🔄 Starting UI consolidation migration...");
            }
            
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            if (gridDemoUI == null)
            {
                Debug.LogError("❌ GridDemo UI object not found!");
                return;
            }
            
            // Step 1: Remove conflicting components
            RemoveConflictingComponents(gridDemoUI);
            
            // Step 2: Add the new consolidated controller
            AddConsolidatedController(gridDemoUI);
            
            // Step 3: Clean up any duplicate UI elements
            CleanupDuplicateUI();
            
            if (debugMode)
            {
                Debug.Log("✅ UI consolidation migration completed!");
            }
        }
        
        private void RemoveConflictingComponents(GameObject gridDemoUI)
        {
            if (debugMode)
            {
                Debug.Log("🗑️ Removing conflicting UI components...");
            }
            
            // Remove GridDemoUI (legacy component)
            var gridDemoUIComponent = gridDemoUI.GetComponent<GridDemoUI>();
            if (gridDemoUIComponent != null)
            {
                if (debugMode)
                {
                    Debug.Log("   - Removing GridDemoUI component");
                }
                DestroyImmediate(gridDemoUIComponent);
            }
            
            // Remove GridDemoUIManager (redundant coordinator)
            var gridDemoUIManager = gridDemoUI.GetComponent<GridDemoUIManager>();
            if (gridDemoUIManager != null)
            {
                if (debugMode)
                {
                    Debug.Log("   - Removing GridDemoUIManager component");
                }
                DestroyImmediate(gridDemoUIManager);
            }
            
            // Remove CompactUIDesigner (will be replaced by consolidated controller)
            var compactUIDesigner = gridDemoUI.GetComponent<CompactUIDesigner>();
            if (compactUIDesigner != null)
            {
                if (debugMode)
                {
                    Debug.Log("   - Removing CompactUIDesigner component");
                }
                DestroyImmediate(compactUIDesigner);
            }
            
            // Remove any CompactSidebarFixer if present
            var sidebarFixer = gridDemoUI.GetComponent<CompactSidebarFixer>();
            if (sidebarFixer != null)
            {
                if (debugMode)
                {
                    Debug.Log("   - Removing CompactSidebarFixer component");
                }
                DestroyImmediate(sidebarFixer);
            }
        }
        
        private void AddConsolidatedController(GameObject gridDemoUI)
        {
            // Check if consolidated controller already exists
            var existingController = gridDemoUI.GetComponent<GridDemoUIController>();
            if (existingController != null)
            {
                if (debugMode)
                {
                    Debug.Log("✅ GridDemoUIController already exists, skipping addition");
                }
                return;
            }
            
            // Add the new consolidated controller
            var newController = gridDemoUI.AddComponent<GridDemoUIController>();
            
            if (debugMode)
            {
                Debug.Log("✅ Added GridDemoUIController component");
            }
            
            // The component will auto-setup on its next Start() call
        }
        
        private void CleanupDuplicateUI()
        {
            if (debugMode)
            {
                Debug.Log("🧹 Cleaning up duplicate UI elements...");
            }
            
            // Find all existing compact sidebars
            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            var duplicateSidebars = System.Array.FindAll(allObjects, go => go.name.Contains("Compact Sidebar"));
            
            if (duplicateSidebars.Length > 0)
            {
                foreach (var sidebar in duplicateSidebars)
                {
                    if (debugMode)
                    {
                        Debug.Log($"   - Removing duplicate sidebar: {sidebar.name}");
                    }
                    DestroyImmediate(sidebar);
                }
            }
            
            // Hide any old UI panels
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            if (gridDemoUI != null)
            {
                Transform ingredientPanel = gridDemoUI.transform.Find("Ingredient Panel");
                Transform infoPanel = gridDemoUI.transform.Find("Info Panel");
                
                if (ingredientPanel != null)
                {
                    ingredientPanel.gameObject.SetActive(false);
                    if (debugMode)
                    {
                        Debug.Log("   - Disabled old Ingredient Panel");
                    }
                }
                
                if (infoPanel != null)
                {
                    infoPanel.gameObject.SetActive(false);
                    if (debugMode)
                    {
                        Debug.Log("   - Disabled old Info Panel");
                    }
                }
            }
        }
        
        [ContextMenu("📊 Debug Current UI Components")]
        public void DebugCurrentComponents()
        {
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            if (gridDemoUI == null)
            {
                Debug.LogError("❌ GridDemo UI object not found!");
                return;
            }
            
            Debug.Log("📊 === Current UI Components on GridDemo UI ===");
            
            var components = gridDemoUI.GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                if (component != null)
                {
                    string componentType = component.GetType().Name;
                    bool isUIRelated = componentType.Contains("UI") || componentType.Contains("Compact") || componentType.Contains("GridDemo");
                    
                    if (isUIRelated)
                    {
                        Debug.Log($"   🎨 {componentType}");
                    }
                    else
                    {
                        Debug.Log($"   ⚙️ {componentType}");
                    }
                }
            }
            
            Debug.Log("📊 === End Component List ===");
        }
        
        [ContextMenu("🔍 Find All Compact Sidebars in Scene")]
        public void FindAllCompactSidebars()
        {
            Debug.Log("🔍 === Searching for Compact Sidebars ===");
            
            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            var sidebars = System.Array.FindAll(allObjects, go => go.name.Contains("Compact Sidebar"));
            
            Debug.Log($"Found {sidebars.Length} objects with 'Compact Sidebar' in name:");
            
            for (int i = 0; i < sidebars.Length; i++)
            {
                var sidebar = sidebars[i];
                Debug.Log($"   {i + 1}. {sidebar.name} (Parent: {sidebar.transform.parent?.name ?? "None"})");
                
                // Count children
                int childCount = sidebar.transform.childCount;
                Debug.Log($"      - Children: {childCount}");
                
                // Check if it has buttons
                var buttons = sidebar.GetComponentsInChildren<UnityEngine.UI.Button>();
                Debug.Log($"      - Buttons: {buttons.Length}");
            }
            
            Debug.Log("🔍 === End Search ===");
        }
        
        private void Start()
        {
            if (autoMigrateOnStart)
            {
                // Add a small delay to ensure other components have initialized
                Invoke(nameof(MigrateToConsolidatedUI), 0.1f);
            }
        }
    }
}