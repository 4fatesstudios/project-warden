using UnityEngine;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Ensures the compact sidebar exists with ingredients, without creating duplicates
    /// </summary>
    public class IngredientSidebarEnsurer : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool ensureOnStart = true;
        [SerializeField] private float delaySeconds = 1.0f;
        [SerializeField] private bool debugMode = true;

        private bool hasEnsuredSidebar = false;

        private void Start()
        {
            if (ensureOnStart)
            {
                Invoke(nameof(EnsureCompactSidebar), delaySeconds);
            }
        }

        [ContextMenu("Ensure Compact Sidebar Exists")]
        public void EnsureCompactSidebar()
        {
            if (hasEnsuredSidebar)
            {
                if (debugMode)
                    Debug.Log("🔧 IngredientSidebarEnsurer: Sidebar already ensured, skipping");
                return;
            }

            if (debugMode)
                Debug.Log("🔧 IngredientSidebarEnsurer: Ensuring compact sidebar exists...");

            // First, check if a compact sidebar already exists
            GameObject existingSidebar = GameObject.Find("Compact Sidebar");
            if (existingSidebar != null)
            {
                if (debugMode)
                    Debug.Log("✅ Compact Sidebar already exists, checking for ingredients...");
                
                CheckAndRefreshIngredients(existingSidebar);
                hasEnsuredSidebar = true;
                return;
            }

            // If no sidebar exists, we need to create one
            CreateMinimalCompactSidebar();
            hasEnsuredSidebar = true;
        }

        private void CheckAndRefreshIngredients(GameObject sidebar)
        {
            // Find the ingredient scroll area and check if it's populated
            Transform ingredientScroll = FindChildRecursive(sidebar.transform, "Ingredient Scroll");
            if (ingredientScroll == null)
            {
                if (debugMode)
                    Debug.LogWarning("⚠️ Ingredient Scroll not found in existing sidebar");
                return;
            }

            // Check if ingredients are present
            Transform viewport = ingredientScroll.Find("Viewport");
            Transform content = viewport?.Find("Content");
            
            if (content != null && content.childCount == 0)
            {
                if (debugMode)
                    Debug.Log("🔄 Ingredients missing, trying to refresh...");
                
                // Try to find and use existing CompactUIDesigner
                CompactUIDesigner designer = FindFirstObjectByType<CompactUIDesigner>();
                if (designer != null)
                {
                    designer.RefreshIngredientButtons();
                    if (debugMode)
                        Debug.Log("✅ Refreshed ingredients using existing CompactUIDesigner");
                }
                else
                {
                    if (debugMode)
                        Debug.Log("📝 No CompactUIDesigner found to refresh ingredients");
                }
            }
            else
            {
                if (debugMode)
                    Debug.Log($"✅ Ingredients already present: {content?.childCount ?? 0} items");
            }
        }

        private void CreateMinimalCompactSidebar()
        {
            if (debugMode)
                Debug.Log("🏗️ Creating minimal compact sidebar...");

            // Only create if absolutely necessary and only once
            CompactUIDesigner designer = FindFirstObjectByType<CompactUIDesigner>();
            if (designer == null)
            {
                // Find GridDemo UI to attach the designer to
                GameObject gridDemoUI = GameObject.Find("GridDemo UI");
                if (gridDemoUI != null)
                {
                    designer = gridDemoUI.AddComponent<CompactUIDesigner>();
                    if (debugMode)
                        Debug.Log("🔧 Added CompactUIDesigner to GridDemo UI");
                }
                else
                {
                    if (debugMode)
                        Debug.LogError("❌ GridDemo UI not found - cannot create sidebar");
                    return;
                }
            }

            // Create the sidebar
            designer.DesignCompactUI();
            
            if (debugMode)
                Debug.Log("✅ Compact sidebar created successfully!");

            // Add back button after a short delay to ensure sidebar is fully created
            Invoke(nameof(AddBackButtonToSidebar), 0.5f);
        }

        private void AddBackButtonToSidebar()
        {
            GameObject compactSidebar = GameObject.Find("Compact Sidebar");
            if (compactSidebar == null)
            {
                if (debugMode)
                    Debug.LogWarning("⚠️ Cannot add back button - Compact Sidebar not found");
                return;
            }

            // Add the SimpleBackButtonAdder to handle back button creation
            SimpleBackButtonAdder backButtonAdder = gameObject.GetComponent<SimpleBackButtonAdder>();
            if (backButtonAdder == null)
            {
                backButtonAdder = gameObject.AddComponent<SimpleBackButtonAdder>();
            }
            
            // Trigger the back button creation
            backButtonAdder.AddBackButtonToExistingSidebar();
            
            if (debugMode)
                Debug.Log("✅ Back button added to compact sidebar!");
        }

        private Transform FindChildRecursive(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                    return child;

                Transform found = FindChildRecursive(child, childName);
                if (found != null)
                    return found;
            }
            return null;
        }

        private void Update()
        {
            // Manual trigger with F10
            if (Input.GetKeyDown(KeyCode.F10))
            {
                Debug.Log("🔧 F10 pressed - Ensuring compact sidebar exists...");
                hasEnsuredSidebar = false; // Reset to allow re-ensuring
                EnsureCompactSidebar();
            }
        }
    }
}