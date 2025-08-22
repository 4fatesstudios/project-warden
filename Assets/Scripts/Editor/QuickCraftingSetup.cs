using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.UI;
using UnityEngine.SceneManagement;

namespace FourFatesStudios.ProjectWarden.Editor
{
    /// <summary>
    /// Simple tool to quickly set up the crafting system hybrid architecture
    /// </summary>
    public class QuickCraftingSetup : EditorWindow
    {
        [MenuItem("Tools/Quick Convert to Hybrid")]
        public static void QuickConvert()
        {
            var currentScene = SceneManager.GetActiveScene();
            if (currentScene.name != "Crafting System")
            {
                EditorUtility.DisplayDialog("Wrong Scene", 
                    "Please open the 'Crafting System' scene first.\n\nCurrent scene: " + currentScene.name, 
                    "OK");
                return;
            }

            Debug.Log("🚀 Starting quick conversion to hybrid architecture...");

            // Step 1: Create CraftingUIManager
            CreateCraftingUIManager();

            // Step 2: Create Inventory System
            CreateInventorySystem();

            // Step 3: Create Navigation Controller
            CreateNavigationController();

            // Step 4: Mark scene as dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(currentScene);

            Debug.Log("✅ Quick conversion complete!");
            
            EditorUtility.DisplayDialog("Conversion Complete!", 
                "🎉 Your Crafting System scene has been converted to hybrid architecture!\n\n" +
                "✅ CraftingUIManager created\n" +
                "✅ Inventory system added\n" +
                "✅ Navigation controls setup\n\n" +
                "Press Play and use keys 1-4 to test!", 
                "Awesome!");
        }

        [MenuItem("Tools/Setup Crafting System")]
        public static void ShowWindow()
        {
            GetWindow<QuickCraftingSetup>("Quick Crafting Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("Quick Crafting System Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            var currentScene = SceneManager.GetActiveScene();
            EditorGUILayout.LabelField($"Current Scene: {currentScene.name}");

            if (currentScene.name != "Crafting System")
            {
                EditorGUILayout.HelpBox("⚠️ Please open the 'Crafting System' scene first.", MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox("🔄 This will convert your scene to use hybrid modular architecture.", MessageType.Info);

            GUILayout.Space(10);

            // Check current components
            var craftingUIManager = Object.FindFirstObjectByType<CraftingUIManager>();
            var inventoryHolder = Object.FindFirstObjectByType<ItemSlotContainerHolder>();
            var navigationController = Object.FindFirstObjectByType<CraftingNavigationController>();

            GUILayout.Label("Current Components:", EditorStyles.boldLabel);
            
            string managerStatus = craftingUIManager != null ? "✅ Found" : "❌ Missing";
            EditorGUILayout.LabelField($"CraftingUIManager: {managerStatus}");
            
            string inventoryStatus = inventoryHolder != null ? "✅ Found" : "❌ Missing";
            EditorGUILayout.LabelField($"Inventory: {inventoryStatus}");
            
            string navStatus = navigationController != null ? "✅ Found" : "❌ Missing";
            EditorGUILayout.LabelField($"Navigation: {navStatus}");

            GUILayout.Space(10);

            // Convert button
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🚀 Convert to Hybrid Architecture", GUILayout.Height(50)))
            {
                QuickConvert();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // Individual components
            if (craftingUIManager == null && GUILayout.Button("Create CraftingUIManager"))
            {
                CreateCraftingUIManager();
            }

            if (inventoryHolder == null && GUILayout.Button("Create Inventory System"))
            {
                CreateInventorySystem();
            }

            if (navigationController == null && GUILayout.Button("Create Navigation Controller"))
            {
                CreateNavigationController();
            }

            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "After conversion:\n" +
                "• Press 1-4 to switch between main panels\n" +
                "• Press F1-F4 to access minigames\n" +
                "• Press ESC to go back", MessageType.Info);
        }

        private static void CreateCraftingUIManager()
        {
            var existing = Object.FindFirstObjectByType<CraftingUIManager>();
            if (existing != null)
            {
                Debug.Log("✅ CraftingUIManager already exists");
                return;
            }

            var managerGO = new GameObject("CraftingUIManager");
            var manager = managerGO.AddComponent<CraftingUIManager>();

            // Auto-assign UI panels using reflection
            AutoAssignUIPanels(manager);

            Debug.Log("✅ Created CraftingUIManager");
            Selection.activeGameObject = managerGO;
        }

        private static void AutoAssignUIPanels(CraftingUIManager manager)
        {
            var managerType = typeof(CraftingUIManager);

            // Find and assign UI panels
            var panelMappings = new System.Collections.Generic.Dictionary<string, string>
            {
                {"craftingMenuSystem", "CraftingMenuSystem"},
                {"potionCraftingUI", "PotionCraftingUI"},
                {"gridMinigameUI", "GridMinigameUI"},
                {"bulkCraftingUI", "BulkCraftingUI"},
                {"refinementUI", "RefinementUI"},
                {"roastingMinigameUI", "RoastingMinigameUI"},
                {"distillingMinigameUI", "DistillingMinigameUI"},
                {"grindingMinigameUI", "GrindingMinigameUI"}
            };

            foreach (var mapping in panelMappings)
            {
                var field = managerType.GetField(mapping.Key, 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var gameObject = GameObject.Find(mapping.Value);
                
                if (field != null && gameObject != null)
                {
                    field.SetValue(manager, gameObject);
                    Debug.Log($"🔗 Connected {mapping.Value} to CraftingUIManager");
                }
            }

            EditorUtility.SetDirty(manager);
        }

        private static void CreateInventorySystem()
        {
            var existing = Object.FindFirstObjectByType<ItemSlotContainerHolder>();
            if (existing != null)
            {
                Debug.Log("✅ Inventory system already exists");
                return;
            }

            var inventoryGO = new GameObject("Inventory");
            var holder = inventoryGO.AddComponent<ItemSlotContainerHolder>();

            Debug.Log("✅ Created inventory system");
            EditorUtility.SetDirty(holder);
        }

        private static void CreateNavigationController()
        {
            var existing = Object.FindFirstObjectByType<CraftingNavigationController>();
            if (existing != null)
            {
                Debug.Log("✅ Navigation controller already exists");
                return;
            }

            var navGO = new GameObject("CraftingNavigationController");
            navGO.AddComponent<CraftingNavigationController>();

            Debug.Log("✅ Created navigation controller");
        }
    }
}