using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Setup utility for adding CraftingModeSelector to a scene
    /// </summary>
    public class CraftingModeSelectorSetup : MonoBehaviour
    {
        [Header("Setup Configuration")]
        [SerializeField] private bool autoSetupOnStart = true;
        
        void Start()
        {
            if (autoSetupOnStart)
            {
                SetupCraftingModeSelector();
            }
        }
        
        [ContextMenu("Setup CraftingModeSelector")]
        public void SetupCraftingModeSelector()
        {
            // Check if CraftingModeSelector already exists
            var existing = FindFirstObjectByType<CraftingModeSelector>();
            if (existing != null)
            {
                Debug.Log("✅ CraftingModeSelector already exists in scene");
                return;
            }
            
            // Create the GameObject
            var craftingModeSelectorGO = new GameObject("CraftingModeSelector");
            
            // Add UIDocument component
            var uiDocument = craftingModeSelectorGO.AddComponent<UIDocument>();
            
            // Try to find and assign the UXML asset
            var assets = Resources.FindObjectsOfTypeAll<VisualTreeAsset>();
            foreach (var asset in assets)
            {
                if (asset.name == "CraftingModeSelector")
                {
                    uiDocument.visualTreeAsset = asset;
                    Debug.Log("✅ Auto-assigned CraftingModeSelector UXML asset");
                    break;
                }
            }
            
            // Add the CraftingModeSelector component
            var modeSelectorComponent = craftingModeSelectorGO.AddComponent<CraftingModeSelector>();
            
            // Initially hide it - it will show itself when needed
            craftingModeSelectorGO.SetActive(false);
            
            Debug.Log("🎮 CraftingModeSelector setup completed successfully!");
            Debug.Log("   Use CraftingNavigationController.ShowCraftingModeSelector() to show it");
        }
        
        [ContextMenu("Test Show Mode Selector")]
        public void TestShowModeSelector()
        {
            var modeSelector = FindFirstObjectByType<CraftingModeSelector>();
            if (modeSelector != null)
            {
                modeSelector.ShowModeSelector();
                Debug.Log("🎮 Mode selector shown for testing");
            }
            else
            {
                Debug.LogWarning("❌ No CraftingModeSelector found - run Setup first");
            }
        }
    }
}