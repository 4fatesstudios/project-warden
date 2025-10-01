using UnityEngine;
using FourFatesStudios.ProjectWarden.GridDemo.UI;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Debugging script to manually setup the UI system when it's not working
    /// </summary>
    public class DebugUISetup : MonoBehaviour
    {
        [Header("Debug Controls")]
        [SerializeField] private bool setupOnStart = true;
        [SerializeField] private bool forceRecreate = false;
        
        private void Start()
        {
            if (setupOnStart)
            {
                SetupUISystem();
            }
        }
        
        [ContextMenu("Setup UI System")]
        public void SetupUISystem()
        {
            Debug.Log("🔧 DebugUISetup: Starting manual UI setup...");
            
            // Find the GridDemo UI object
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            if (gridDemoUI == null)
            {
                Debug.LogError("❌ GridDemo UI object not found!");
                return;
            }
            
            // Check if GridDemoUIManager exists
            GridDemoUIManager uiManager = gridDemoUI.GetComponent<GridDemoUIManager>();
            if (uiManager == null)
            {
                Debug.Log("🔧 Adding GridDemoUIManager to GridDemo UI...");
                uiManager = gridDemoUI.AddComponent<GridDemoUIManager>();
            }
            else
            {
                Debug.Log("✅ GridDemoUIManager already exists");
            }
            
            // Check if CompactUIDesigner exists
            CompactUIDesigner compactDesigner = gridDemoUI.GetComponent<CompactUIDesigner>();
            if (compactDesigner == null)
            {
                Debug.Log("🔧 Adding CompactUIDesigner to GridDemo UI...");
                compactDesigner = gridDemoUI.AddComponent<CompactUIDesigner>();
            }
            else
            {
                Debug.Log("✅ CompactUIDesigner already exists");
            }
            
            // Force UI creation
            if (compactDesigner != null)
            {
                if (forceRecreate)
                {
                    Debug.Log("🔄 Force recreating UI...");
                    compactDesigner.ForceRecreateUI();
                }
                else
                {
                    Debug.Log("🎨 Creating compact UI...");
                    compactDesigner.DesignCompactUI();
                }
            }
            
            Debug.Log("✅ DebugUISetup: Manual UI setup completed!");
        }
        
        [ContextMenu("Force Recreate UI")]
        public void ForceRecreateUI()
        {
            forceRecreate = true;
            SetupUISystem();
            forceRecreate = false;
        }
        
        [ContextMenu("Check Current UI State")]
        public void CheckCurrentUIState()
        {
            Debug.Log("📊 === CURRENT UI STATE ===");
            
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            Debug.Log($"GridDemo UI exists: {gridDemoUI != null}");
            
            if (gridDemoUI != null)
            {
                GridDemoUIManager uiManager = gridDemoUI.GetComponent<GridDemoUIManager>();
                CompactUIDesigner compactDesigner = gridDemoUI.GetComponent<CompactUIDesigner>();
                
                Debug.Log($"GridDemoUIManager attached: {uiManager != null}");
                Debug.Log($"CompactUIDesigner attached: {compactDesigner != null}");
                
                GameObject sidebar = GameObject.Find("Compact Sidebar");
                Debug.Log($"Compact Sidebar exists: {sidebar != null}");
                
                if (sidebar != null)
                {
                    GameObject buttonContainer = GameObject.Find("Compact Sidebar/Ingredient Scroll/Viewport/Content/Button Container");
                    if (buttonContainer != null)
                    {
                        int buttonCount = buttonContainer.transform.childCount;
                        Debug.Log($"Ingredient buttons count: {buttonCount}");
                    }
                    else
                    {
                        Debug.Log("Button container not found in expected path");
                    }
                }
            }
            
            // Check GridGameManager
            GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
            Debug.Log($"GridGameManager found: {gridManager != null}");
            
            if (gridManager != null)
            {
                Debug.Log($"Available ingredients: {gridManager.availableIngredients?.Count ?? 0}");
            }
            
            Debug.Log("📊 === END UI STATE ===");
        }
    }
}