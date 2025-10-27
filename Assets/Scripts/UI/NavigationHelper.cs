using UnityEngine;

namespace FourFatesStudios.ProjectWarden.UI
{
    public class NavigationHelper : MonoBehaviour
    {
        [ContextMenu("Go Back to Menu Selector")]
        public void GoBackToMenuSelector()
        {
            Debug.Log("🔄 NavigationHelper: Going back to MenuSelector...");
            
            // Hide GridDemo UI
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            if (gridDemoUI != null)
            {
                Debug.Log("🎯 Hiding GridDemo UI");
                gridDemoUI.SetActive(false);
            }
            
            // Show MenuSelector
            GameObject menuSelector = GameObject.Find("MenuSelector");
            if (menuSelector != null)
            {
                Debug.Log("✅ Showing MenuSelector");
                menuSelector.SetActive(true);
            }
            
            // Hide CraftingModeSelector
            GameObject craftingModeSelector = GameObject.Find("CraftingModeSelector");
            if (craftingModeSelector != null)
            {
                Debug.Log("✅ Hiding CraftingModeSelector");
                craftingModeSelector.SetActive(false);
            }
            
            Debug.Log("🎯 Navigation to MenuSelector completed");
        }
        
        [ContextMenu("Show Crafting Mode Selector")]
        public void ShowCraftingModeSelector()
        {
            Debug.Log("🔄 NavigationHelper: Showing CraftingModeSelector...");
            
            // Hide GridDemo UI
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            if (gridDemoUI != null)
            {
                Debug.Log("🎯 Hiding GridDemo UI");
                gridDemoUI.SetActive(false);
            }
            
            // Hide MenuSelector
            GameObject menuSelector = GameObject.Find("MenuSelector");
            if (menuSelector != null)
            {
                Debug.Log("✅ Hiding MenuSelector");
                menuSelector.SetActive(false);
            }
            
            // Show CraftingModeSelector using proper method
            GameObject craftingModeSelector = GameObject.Find("CraftingModeSelector");
            if (craftingModeSelector != null)
            {
                Debug.Log("✅ Re-enabling CraftingModeSelector");
                var modeSelectorComponent = craftingModeSelector.GetComponent<FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.CraftingModeSelector>();
                if (modeSelectorComponent != null)
                {
                    modeSelectorComponent.ShowModeSelector();
                }
                else
                {
                    // Fallback if component not found
                    craftingModeSelector.SetActive(true);
                }
            }
            
            Debug.Log("🎯 Navigation to CraftingModeSelector completed");
        }
    }
}