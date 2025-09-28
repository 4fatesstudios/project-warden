using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.GridDemo.UI;
using FourFatesStudios.ProjectWarden.GridDemo;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu;
using FourFatesStudios.ProjectWarden.GameSystems.SkillSystem;

public class GridDemoEnhancedIntegration : EditorWindow
{
    [MenuItem("Project Warden/Grid Demo/Integrate Enhanced Systems with Current Scene")]
    static void IntegrateEnhancedSystems()
    {
        Debug.Log("🚀 === INTEGRATING ENHANCED SYSTEMS WITH GRIDDEMO SCENE ===");
        
        try
        {
            // Step 1: Verify scene components
            var gridManager = FindFirstObjectByType<FourFatesStudios.ProjectWarden.GridDemo.GridGameManager>();
            var canvas = FindFirstObjectByType<Canvas>();
            
            if (gridManager == null)
            {
                Debug.LogError("❌ GridGameManager not found in scene!");
                return;
            }
            
            if (canvas == null)
            {
                Debug.LogError("❌ Canvas not found in scene!");
                return;
            }
            
            Debug.Log($"✅ Found GridGameManager: {gridManager.name}");
            Debug.Log($"✅ Found Canvas: {canvas.name}");
            
            // Step 2: Create Enhanced Systems Root
            var enhancedSystemsRoot = new GameObject("🧪 Enhanced Alchemy Systems");
            Undo.RegisterCreatedObjectUndo(enhancedSystemsRoot, "Create Enhanced Systems Root");
            
            // Step 3: Add Core Systems
            CreateSystemComponent<FourFatesStudios.ProjectWarden.GameSystems.SkillSystem.AlchemySkillTree>(enhancedSystemsRoot, "Alchemy Skill Tree");
            CreateSystemComponent<FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.SynergySystem>(enhancedSystemsRoot, "Synergy System");
            CreateSystemComponent<FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.TemplateSystem>(enhancedSystemsRoot, "Template System");
            CreateSystemComponent<FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.FailureSystem>(enhancedSystemsRoot, "Failure System");
            CreateSystemComponent<FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.EnhancedGridSystem>(enhancedSystemsRoot, "Enhanced Grid System");
            CreateSystemComponent<FourFatesStudios.ProjectWarden.GridDemo.ExtractionMinigame>(enhancedSystemsRoot, "Extraction Minigame");
            CreateSystemComponent<FourFatesStudios.ProjectWarden.GridDemo.UI.ImprovedClickDetector>(enhancedSystemsRoot, "Improved Click Detector");
            
            Debug.Log("✅ Added 7 core enhanced systems");
            
            // Step 4: Add UI Systems
            var enhancedUIRoot = new GameObject("🎨 Enhanced UI Systems");
            enhancedUIRoot.transform.SetParent(canvas.transform, false);
            Undo.RegisterCreatedObjectUndo(enhancedUIRoot, "Create Enhanced UI Root");
            
            var rectTransform = enhancedUIRoot.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            CreateUIComponent<EnhancedAlchemyUIManager>(enhancedUIRoot, "Enhanced Alchemy UI Manager");
            CreateUIComponent<UISystemBridge>(enhancedUIRoot, "UI System Bridge");
            CreateUIComponent<EnhancedUISetup>(enhancedUIRoot, "Enhanced UI Setup");
            
            Debug.Log("✅ Added 3 UI enhancement systems");
            
            // Step 5: Mark scene as dirty
            EditorUtility.SetDirty(enhancedSystemsRoot);
            EditorUtility.SetDirty(enhancedUIRoot);
            
            // Step 6: Log completion
            Debug.Log("🎉 === INTEGRATION COMPLETE ===");
            Debug.Log("📋 Added to your GridDemo scene:");
            Debug.Log("• 🧪 Enhanced Alchemy Systems (7 core systems)");
            Debug.Log("• 🎨 Enhanced UI Systems (3 UI systems under Canvas)");
            Debug.Log("");
            Debug.Log("🔧 Next steps:");
            Debug.Log("1. Select the 'Enhanced UI Setup' component under Canvas → Enhanced UI Systems");
            Debug.Log("2. In the Inspector, right-click the component header");
            Debug.Log("3. Choose '🚀 Setup Enhanced UI Integration'");
            Debug.Log("4. This will create the user interface for all enhanced features");
            Debug.Log("");
            Debug.Log("🧪 Testing:");
            Debug.Log("• Right-click any enhanced system component for testing options");
            Debug.Log("• All systems work with your existing GridDemo setup");
            Debug.Log("• Enhanced features are now available!");
            
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Integration failed: {e.Message}");
            Debug.LogException(e);
        }
    }
    
    static T CreateSystemComponent<T>(GameObject parent, string name) where T : MonoBehaviour
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        var component = obj.AddComponent<T>();
        
        Undo.RegisterCreatedObjectUndo(obj, $"Create {name}");
        Debug.Log($"  ✅ Created {typeof(T).Name}");
        return component;
    }
    
    static T CreateUIComponent<T>(GameObject parent, string name) where T : MonoBehaviour
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        var component = obj.AddComponent<T>();
        
        Undo.RegisterCreatedObjectUndo(obj, $"Create {name}");
        Debug.Log($"  ✅ Created {typeof(T).Name}");
        return component;
    }
    
    [MenuItem("Project Warden/Grid Demo/Remove Enhanced Systems")]
    static void RemoveEnhancedSystems()
    {
        var enhancedSystems = GameObject.Find("🧪 Enhanced Alchemy Systems");
        var enhancedUI = GameObject.Find("🎨 Enhanced UI Systems");
        
        if (enhancedSystems != null)
        {
            Undo.DestroyObjectImmediate(enhancedSystems);
            Debug.Log("🗑️ Removed Enhanced Alchemy Systems");
        }
        
        if (enhancedUI != null)
        {
            Undo.DestroyObjectImmediate(enhancedUI);
            Debug.Log("🗑️ Removed Enhanced UI Systems");
        }
        
        if (enhancedSystems == null && enhancedUI == null)
        {
            Debug.LogWarning("⚠️ No enhanced systems found to remove");
        }
        else
        {
            Debug.Log("✅ Enhanced systems removal complete");
        }
    }
    
    [MenuItem("Project Warden/Grid Demo/Check Integration Status")]
    static void CheckIntegrationStatus()
    {
        Debug.Log("📋 === INTEGRATION STATUS CHECK ===");
        
        var gridManager = FindFirstObjectByType<FourFatesStudios.ProjectWarden.GridDemo.GridGameManager>();
        var canvas = FindFirstObjectByType<Canvas>();
        var enhancedSystems = GameObject.Find("🧪 Enhanced Alchemy Systems");
        var enhancedUI = GameObject.Find("🎨 Enhanced UI Systems");
        
        Debug.Log($"✅ GridGameManager: {gridManager != null}");
        Debug.Log($"✅ Canvas: {canvas != null}");
        Debug.Log($"✅ Enhanced Systems: {enhancedSystems != null}");
        Debug.Log($"✅ Enhanced UI: {enhancedUI != null}");
        
        if (enhancedSystems != null)
        {
            int systemCount = enhancedSystems.transform.childCount;
            Debug.Log($"⚙️ Core systems count: {systemCount}/7");
        }
        
        if (enhancedUI != null)
        {
            int uiCount = enhancedUI.transform.childCount;
            Debug.Log($"🎨 UI systems count: {uiCount}/3");
        }
        
        // Check specific components
        var skillTree = FindFirstObjectByType<FourFatesStudios.ProjectWarden.GameSystems.SkillSystem.AlchemySkillTree>();
        var synergySystem = FindFirstObjectByType<FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.SynergySystem>();
        var templateSystem = FindFirstObjectByType<FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.TemplateSystem>();
        var enhancedUIManager = FindFirstObjectByType<EnhancedAlchemyUIManager>();
        var uiBridge = FindFirstObjectByType<UISystemBridge>();
        var enhancedUISetup = FindFirstObjectByType<EnhancedUISetup>();
        
        Debug.Log($"🧪 Skill Tree: {skillTree != null}");
        Debug.Log($"✨ Synergy System: {synergySystem != null}");
        Debug.Log($"🗺️ Template System: {templateSystem != null}");
        Debug.Log($"🎨 Enhanced UI Manager: {enhancedUIManager != null}");
        Debug.Log($"🔗 UI Bridge: {uiBridge != null}");
        Debug.Log($"🔧 Enhanced UI Setup: {enhancedUISetup != null}");
        
        bool fullyIntegrated = enhancedSystems != null && enhancedUI != null && 
                              skillTree != null && enhancedUIManager != null;
        
        if (fullyIntegrated)
        {
            Debug.Log("🎉 Integration complete and verified!");
            Debug.Log("🔧 Next: Use Enhanced UI Setup to configure the interface");
        }
        else
        {
            Debug.Log("⚠️ Integration incomplete. Run integration menu item.");
        }
    }
}