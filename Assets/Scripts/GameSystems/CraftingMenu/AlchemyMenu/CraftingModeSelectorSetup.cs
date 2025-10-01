using UnityEngine;
using UnityEngine.UIElements;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu
{
    /// <summary>
    /// Setup script to configure the CraftingModeSelector with proper UIDocument component
    /// </summary>
    public class CraftingModeSelectorSetup : MonoBehaviour
    {
        [Header("UI Assets")]
        [SerializeField] private VisualTreeAsset craftingModeUXML;
        [SerializeField] private StyleSheet craftingModeUSS;
        
        [ContextMenu("Setup CraftingModeSelector UI")]
        public void SetupCraftingModeSelectorUI()
        {
            GameObject craftingModeSelector = GameObject.Find("CraftingModeSelector");
            
            if (craftingModeSelector == null)
            {
                Debug.LogError("🔍 CraftingModeSelector GameObject not found in scene!");
                return;
            }
            
            // Check if UIDocument already exists
            UIDocument uiDocument = craftingModeSelector.GetComponent<UIDocument>();
            
            if (uiDocument == null)
            {
                Debug.Log("🔧 Adding UIDocument component to CraftingModeSelector");
                uiDocument = craftingModeSelector.AddComponent<UIDocument>();
            }
            
            // Load the UXML asset
            if (craftingModeUXML != null)
            {
                uiDocument.visualTreeAsset = craftingModeUXML;
                Debug.Log("✅ CraftingModeSelector UXML asset assigned successfully");
            }
            else
            {
                Debug.LogError("❌ CraftingModeSelector UXML asset not assigned in inspector!");
                Debug.LogError("   Please assign the CraftingModeSelector.uxml file to this component");
            }
            
            // Load the USS style sheet
            if (craftingModeUSS != null)
            {
                // In Unity 6, StyleSheets are typically set in the UXML or via Inspector
                Debug.Log("✅ CraftingModeSelector USS style sheet found (styles managed via UXML)");
            }
            else
            {
                Debug.LogWarning("⚠️ CraftingModeSelector USS asset not assigned - UI may not be styled correctly");
            }
            
            // Ensure the CraftingModeSelector component exists
            var selector = craftingModeSelector.GetComponent<CraftingModeSelector>();
            if (selector == null)
            {
                Debug.LogError("❌ CraftingModeSelector component not found on GameObject!");
            }
            else
            {
                Debug.Log("✅ CraftingModeSelector setup completed successfully");
                
                // Trigger re-initialization
                selector.enabled = false;
                selector.enabled = true;
                
                Debug.Log("🔄 CraftingModeSelector re-initialized");
            }
        }
        
        [ContextMenu("Auto-Setup CraftingModeSelector UI")]
        public void AutoSetupCraftingModeSelectorUI()
        {
            // Try to find the assets automatically
            var assets = Resources.FindObjectsOfTypeAll<VisualTreeAsset>();
            foreach (var asset in assets)
            {
                if (asset.name == "CraftingModeSelector")
                {
                    craftingModeUXML = asset;
                    Debug.Log("✅ Found CraftingModeSelector UXML asset automatically");
                    break;
                }
            }
            
            var styleSheets = Resources.FindObjectsOfTypeAll<StyleSheet>();
            foreach (var styleSheet in styleSheets)
            {
                if (styleSheet.name == "CraftingModeSelector")
                {
                    craftingModeUSS = styleSheet;
                    Debug.Log("✅ Found CraftingModeSelector USS asset automatically");
                    break;
                }
            }
            
            // Now setup the UI
            SetupCraftingModeSelectorUI();
        }
    }
}