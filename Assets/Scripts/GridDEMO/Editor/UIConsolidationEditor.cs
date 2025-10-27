using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.GridDemo;
using FourFatesStudios.ProjectWarden.GridDemo.UI;

namespace FourFatesStudios.ProjectWarden.GridDemo.Editor
{
    /// <summary>
    /// Editor script to safely consolidate UI components
    /// </summary>
    public class UIConsolidationEditor : EditorWindow
    {
        [MenuItem("Tools/Grid Demo/Consolidate UI Components")]
        public static void ShowWindow()
        {
            GetWindow<UIConsolidationEditor>("UI Consolidation");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Grid Demo UI Consolidation", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            GUILayout.Label("This tool will:", EditorStyles.helpBox);
            GUILayout.Label("1. Remove conflicting UI components");
            GUILayout.Label("2. Add the new GridDemoUIController");
            GUILayout.Label("3. Clean up duplicate UI elements");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("🔄 Consolidate UI Components", GUILayout.Height(30)))
            {
                ConsolidateUIComponents();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("📊 Debug Current Components"))
            {
                DebugCurrentComponents();
            }
            
            if (GUILayout.Button("🔍 Find All Compact Sidebars"))
            {
                FindAllCompactSidebars();
            }
        }
        
        private void ConsolidateUIComponents()
        {
            Debug.Log("🔄 Starting UI consolidation from Editor...");
            
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            if (gridDemoUI == null)
            {
                EditorUtility.DisplayDialog("Error", "GridDemo UI object not found in scene!", "OK");
                return;
            }
            
            // Record for undo
            Undo.RegisterCompleteObjectUndo(gridDemoUI, "Consolidate UI Components");
            
            // Remove conflicting components
            RemoveConflictingComponents(gridDemoUI);
            
            // Add new consolidated controller
            AddConsolidatedController(gridDemoUI);
            
            // Clean up duplicates
            CleanupDuplicateUI();
            
            // Mark scene as dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            
            EditorUtility.DisplayDialog("Success", "UI consolidation completed successfully!", "OK");
            Debug.Log("✅ UI consolidation completed!");
        }
        
        private void RemoveConflictingComponents(GameObject gridDemoUI)
        {
            Debug.Log("🗑️ Removing conflicting UI components...");
            
            // Remove GridDemoUI
            var gridDemoUIComponent = gridDemoUI.GetComponent<GridDemoUI>();
            if (gridDemoUIComponent != null)
            {
                Debug.Log("   - Removing GridDemoUI component");
                Undo.DestroyObjectImmediate(gridDemoUIComponent);
            }
            
            // Remove GridDemoUIManager
            var gridDemoUIManager = gridDemoUI.GetComponent<GridDemoUIManager>();
            if (gridDemoUIManager != null)
            {
                Debug.Log("   - Removing GridDemoUIManager component");
                Undo.DestroyObjectImmediate(gridDemoUIManager);
            }
            
            // Remove CompactUIDesigner
            var compactUIDesigner = gridDemoUI.GetComponent<CompactUIDesigner>();
            if (compactUIDesigner != null)
            {
                Debug.Log("   - Removing CompactUIDesigner component");
                Undo.DestroyObjectImmediate(compactUIDesigner);
            }
            
            // Remove CompactSidebarFixer if present
            var sidebarFixer = gridDemoUI.GetComponent<CompactSidebarFixer>();
            if (sidebarFixer != null)
            {
                Debug.Log("   - Removing CompactSidebarFixer component");
                Undo.DestroyObjectImmediate(sidebarFixer);
            }
        }
        
        private void AddConsolidatedController(GameObject gridDemoUI)
        {
            var existingController = gridDemoUI.GetComponent<GridDemoUIController>();
            if (existingController != null)
            {
                Debug.Log("✅ GridDemoUIController already exists");
                return;
            }
            
            var newController = Undo.AddComponent<GridDemoUIController>(gridDemoUI);
            Debug.Log("✅ Added GridDemoUIController component");
        }
        
        private void CleanupDuplicateUI()
        {
            Debug.Log("🧹 Cleaning up duplicate UI elements...");
            
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            var duplicateSidebars = System.Array.FindAll(allObjects, go => go.name.Contains("Compact Sidebar"));
            
            foreach (var sidebar in duplicateSidebars)
            {
                Debug.Log($"   - Removing duplicate sidebar: {sidebar.name}");
                Undo.DestroyObjectImmediate(sidebar);
            }
            
            // Disable old panels
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            if (gridDemoUI != null)
            {
                Transform ingredientPanel = gridDemoUI.transform.Find("Ingredient Panel");
                Transform infoPanel = gridDemoUI.transform.Find("Info Panel");
                
                if (ingredientPanel != null)
                {
                    ingredientPanel.gameObject.SetActive(false);
                    Debug.Log("   - Disabled old Ingredient Panel");
                }
                
                if (infoPanel != null)
                {
                    infoPanel.gameObject.SetActive(false);
                    Debug.Log("   - Disabled old Info Panel");
                }
            }
        }
        
        private void DebugCurrentComponents()
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
                    Debug.Log($"   🎨 {componentType}");
                }
            }
            
            Debug.Log("📊 === End Component List ===");
        }
        
        private void FindAllCompactSidebars()
        {
            Debug.Log("🔍 === Searching for Compact Sidebars ===");
            
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            var sidebars = System.Array.FindAll(allObjects, go => go.name.Contains("Compact Sidebar"));
            
            Debug.Log($"Found {sidebars.Length} objects with 'Compact Sidebar' in name:");
            
            for (int i = 0; i < sidebars.Length; i++)
            {
                var sidebar = sidebars[i];
                Debug.Log($"   {i + 1}. {sidebar.name} (Parent: {sidebar.transform.parent?.name ?? "None"})");
                
                var buttons = sidebar.GetComponentsInChildren<UnityEngine.UI.Button>();
                Debug.Log($"      - Buttons: {buttons.Length}");
            }
            
            Debug.Log("🔍 === End Search ===");
        }
    }
}