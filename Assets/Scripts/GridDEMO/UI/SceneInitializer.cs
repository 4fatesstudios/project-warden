using UnityEngine;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Manages the initial state of the GridDemo scene to ensure proper UI flow
    /// Ensures MenuSelector shows first, then transitions to other UIs as needed
    /// </summary>
    public class SceneInitializer : MonoBehaviour
    {
        [Header("Scene UI Management")]
        [SerializeField] private bool enableDebugLogging = true;
        
        void Start()
        {
            InitializeSceneState();
        }
        
        private void InitializeSceneState()
        {
            if (enableDebugLogging)
                Debug.Log("🎬 SceneInitializer: Setting up initial scene state...");
            
            // Find all the UI components in the scene
            GameObject menuSelector = GameObject.Find("MenuSelector");
            GameObject craftingModeSelector = GameObject.Find("CraftingModeSelector");
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            
            // Log what we found
            if (enableDebugLogging)
            {
                Debug.Log($"🔍 Found UI Elements - MenuSelector: {menuSelector != null}, CraftingModeSelector: {craftingModeSelector != null}, GridDemo UI: {gridDemoUI != null}");
            }
            
            // Set initial state: Only MenuSelector should be visible
            if (menuSelector != null)
            {
                menuSelector.SetActive(true);
                if (enableDebugLogging)
                    Debug.Log("✅ MenuSelector activated");
            }
            else
            {
                Debug.LogWarning("⚠️ MenuSelector not found in scene!");
            }
            
            // Hide CraftingModeSelector (it will be shown via navigation)
            if (craftingModeSelector != null)
            {
                craftingModeSelector.SetActive(false);
                if (enableDebugLogging)
                    Debug.Log("🚫 CraftingModeSelector deactivated (will be shown via navigation)");
            }
            
            if (enableDebugLogging)
                Debug.Log("🎯 SceneInitializer: Initial scene state configured - MenuSelector should now be visible");
        }
        
        /// <summary>
        /// Public method to show the grid UI when a recipe is selected
        /// This should be called by the recipe selection system
        /// </summary>
        public static void ShowGridUI()
        {
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            GameObject menuSelector = GameObject.Find("MenuSelector");
            GameObject craftingModeSelector = GameObject.Find("CraftingModeSelector");
            
            if (gridDemoUI != null)
            {
                gridDemoUI.SetActive(true);
                Debug.Log("✅ GridDemo UI activated for recipe crafting");
            }
            
            // Hide menu selectors when showing grid
            if (menuSelector != null) menuSelector.SetActive(false);
            if (craftingModeSelector != null) craftingModeSelector.SetActive(false);
        }
        
        /// <summary>
        /// Public method to return to the menu selector
        /// This should be called by the back button
        /// </summary>
        public static void ShowMenuSelector()
        {
            GameObject menuSelector = GameObject.Find("MenuSelector");
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            GameObject craftingModeSelector = GameObject.Find("CraftingModeSelector");
            
            if (menuSelector != null)
            {
                menuSelector.SetActive(true);
                Debug.Log("✅ MenuSelector activated");
            }
            
            // Hide other UIs when showing menu
            if (gridDemoUI != null) gridDemoUI.SetActive(true);
            if (craftingModeSelector != null) craftingModeSelector.SetActive(true);
        }
    }
}