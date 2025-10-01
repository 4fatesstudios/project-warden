using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu;

/// <summary>
/// Debug window to test Grid Minigame functionality
/// </summary>
public class GridDebugWindow : EditorWindow
{
    [MenuItem("Tools/Grid Debug Window")]
    public static void ShowWindow()
    {
        GetWindow<GridDebugWindow>("Grid Debug");
    }

    private void OnGUI()
    {
        GUILayout.Label("Grid Minigame Debug Tools", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Find Grid Controller"))
        {
            var gridController = FindFirstObjectByType<GridMinigameController>();
            if (gridController != null)
            {
                Debug.Log($"✅ Found GridMinigameController on {gridController.gameObject.name}");
                Debug.Log($"   • GameObject active: {gridController.gameObject.activeInHierarchy}");
                Debug.Log($"   • Component enabled: {gridController.enabled}");
                Selection.activeGameObject = gridController.gameObject;
            }
            else
            {
                Debug.LogWarning("❌ GridMinigameController not found in scene!");
            }
        }
        
        if (GUILayout.Button("Show Grid Minigame"))
        {
            var navController = FindFirstObjectByType<CraftingNavigationController>();
            if (navController != null)
            {
                navController.ShowGridMinigame();
                Debug.Log("🎮 Called ShowGridMinigame()");
            }
            else
            {
                Debug.LogWarning("❌ CraftingNavigationController not found!");
            }
        }
        
        if (GUILayout.Button("Test Grid Display"))
        {
            var gridController = FindFirstObjectByType<GridMinigameController>();
            if (gridController != null)
            {
                // Use reflection to call the test method
                var method = gridController.GetType().GetMethod("TestGridDisplay");
                if (method != null)
                {
                    method.Invoke(gridController, null);
                }
                else
                {
                    Debug.LogWarning("TestGridDisplay method not found");
                }
            }
            else
            {
                Debug.LogWarning("❌ GridMinigameController not found!");
            }
        }
        
        if (GUILayout.Button("List All UI GameObjects"))
        {
            var allUIs = new string[] 
            {
                "CraftingMenuSystem", "AlchemyMenuUI", "AlchemyBookUI", 
                "GridMinigameUI", "BulkCraftingUI", "RefinementUI"
            };
            
            Debug.Log("🔍 UI GameObject Status:");
            foreach (var uiName in allUIs)
            {
                var obj = GameObject.Find(uiName);
                string status = obj != null 
                    ? $"✅ Found (Active: {obj.activeInHierarchy})" 
                    : "❌ Missing";
                Debug.Log($"   • {uiName}: {status}");
            }
        }
        
        if (GUILayout.Button("Show Alchemy Book"))
        {
            var navController = FindFirstObjectByType<CraftingNavigationController>();
            if (navController != null)
            {
                navController.ShowAlchemyBook();
                Debug.Log("📖 Called ShowAlchemyBook()");
            }
            else
            {
                Debug.LogWarning("❌ CraftingNavigationController not found!");
            }
        }
        
        if (GUILayout.Button("Test Alchemy Book"))
        {
            var alchemyBook = FindFirstObjectByType<GameSystems.CraftingMenu.AlchemyBookMenu.AlchemyBook>();
            if (alchemyBook != null)
            {
                Debug.Log($"📖 Found AlchemyBook on {alchemyBook.gameObject.name}");
                Debug.Log($"   • GameObject active: {alchemyBook.gameObject.activeInHierarchy}");
                Debug.Log($"   • Component enabled: {alchemyBook.enabled}");
                Selection.activeGameObject = alchemyBook.gameObject;
            }
            else
            {
                Debug.LogWarning("❌ AlchemyBook component not found in scene!");
            }
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Use these buttons to debug alchemy system issues:\n" +
                                "1. Find Grid Controller - locates the grid component\n" +
                                "2. Show Grid Minigame - activates the grid panel\n" +
                                "3. Test Grid Display - runs grid diagnostics\n" +
                                "4. List All UI GameObjects - checks scene setup\n" +
                                "5. Show Alchemy Book - activates the book panel\n" +
                                "6. Test Alchemy Book - runs book diagnostics", 
                                MessageType.Info);
    }
}