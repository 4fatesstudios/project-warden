using UnityEngine;
using UnityEditor;

namespace FourFatesStudios.ProjectWarden.Editor
{
    /// <summary>
    /// Tool to show available menu items for debugging
    /// </summary>
    public class MenuItemLister
    {
        [MenuItem("Tools/Show Available Tools", false, 250)]
        public static void ShowAvailableTools()
        {
            Debug.Log("📋 Available Crafting System Tools:");
            Debug.Log("• Tools → Convert Crafting Scene (SimpleCraftingConverter)");
            Debug.Log("• Tools → Setup Hybrid UI (SimpleCraftingConverter)");
            Debug.Log("• Tools → Quick Convert to Hybrid (QuickCraftingSetup)");
            Debug.Log("• Tools → Setup Crafting System (QuickCraftingSetup)");
            Debug.Log("• Tools → Setup Crafting System Components");
            Debug.Log("• Tools → Connect Crafting System Components");
            Debug.Log("• Tools → Quick Add Test Ingredients");
            Debug.Log("• Tools → Check Compilation Status");
            Debug.Log("• Tools → Force Refresh Assets");
            
            EditorUtility.DisplayDialog("Available Tools", 
                "Available Crafting System Tools:\n\n" +
                "• Convert Crafting Scene\n" +
                "• Setup Hybrid UI\n" +
                "• Quick Convert to Hybrid\n" +
                "• Setup Crafting System\n" +
                "• Quick Add Test Ingredients\n" +
                "• Check Compilation Status\n\n" +
                "Check the console for the complete list!", 
                "OK");
        }
    }
}