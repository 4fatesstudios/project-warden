using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Quick fix to add missing UIDocument to CraftingModeSelector
    /// </summary>
    [RequireComponent(typeof(CraftingModeSelector))]
    public class CraftingModeSelectorQuickFix : MonoBehaviour
    {
        [ContextMenu("Fix Missing UIDocument")]
        public void FixMissingUIDocument()
        {
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.Log("🔧 Adding missing UIDocument component...");
                uiDocument = gameObject.AddComponent<UIDocument>();
                
                // Find and assign the UXML asset
                var assets = Resources.FindObjectsOfTypeAll<VisualTreeAsset>();
                foreach (var asset in assets)
                {
                    if (asset.name == "CraftingModeSelector")
                    {
                        uiDocument.visualTreeAsset = asset;
                        Debug.Log("✅ CraftingModeSelector UXML assigned successfully!");
                        break;
                    }
                }
                
                // Re-enable the CraftingModeSelector to trigger initialization
                var modeSelector = GetComponent<CraftingModeSelector>();
                if (modeSelector != null)
                {
                    modeSelector.enabled = false;
                    modeSelector.enabled = true;
                    Debug.Log("✅ CraftingModeSelector reinitialized!");
                }
                
                Debug.Log("🎮 GUI is now ready! The mode selector should be visible.");
            }
            else
            {
                Debug.Log("✅ UIDocument already exists");
                if (uiDocument.visualTreeAsset == null)
                {
                    Debug.Log("🔧 UIDocument exists but UXML is missing - fixing...");
                    
                    var assets = Resources.FindObjectsOfTypeAll<VisualTreeAsset>();
                    foreach (var asset in assets)
                    {
                        if (asset.name == "CraftingModeSelector")
                        {
                            uiDocument.visualTreeAsset = asset;
                            Debug.Log("✅ CraftingModeSelector UXML assigned!");
                            break;
                        }
                    }
                }
            }
        }
        
        void Start()
        {
            // Auto-fix on start
            FixMissingUIDocument();
            
            // Remove this script after fixing
            Destroy(this);
        }
    }
}