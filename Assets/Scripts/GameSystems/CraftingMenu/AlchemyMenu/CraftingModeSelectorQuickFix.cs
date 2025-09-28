using UnityEngine;
using UnityEngine.UIElements;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu
{
    /// <summary>
    /// Quick fix script to handle missing UIDocument setup
    /// </summary>
    [System.Serializable]
    public class CraftingModeSelectorQuickFix : MonoBehaviour
    {
        private void Start()
        {
            // Find the CraftingModeSelector GameObject
            GameObject craftingModeSelector = GameObject.Find("CraftingModeSelector");
            if (craftingModeSelector == null)
            {
                Debug.LogWarning("🔍 CraftingModeSelector GameObject not found in scene!");
                return;
            }
            
            // Check if it has a UIDocument component
            UIDocument uiDocument = craftingModeSelector.GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.Log("🔧 Adding missing UIDocument component to CraftingModeSelector");
                uiDocument = craftingModeSelector.AddComponent<UIDocument>();
                
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
                
                // Try to find and assign the USS style sheet
                var styleSheets = Resources.FindObjectsOfTypeAll<StyleSheet>();
                foreach (var styleSheet in styleSheets)
                {
                    if (styleSheet.name == "CraftingModeSelector")
                    {
                        // In Unity 6, StyleSheets are typically set in the UXML or via Inspector
                        Debug.Log("✅ Found CraftingModeSelector USS style sheet (styles managed via UXML)");
                        break;
                    }
                }
                
                // Trigger re-initialization of the CraftingModeSelector
                var selector = craftingModeSelector.GetComponent<CraftingModeSelector>();
                if (selector != null)
                {
                    selector.enabled = false;
                    selector.enabled = true;
                    Debug.Log("🔄 CraftingModeSelector re-initialized with UIDocument");
                }
            }
            
            // Self-destruct after doing the fix
            Destroy(this);
        }
    }
}