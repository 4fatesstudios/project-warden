using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.UI;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu;
using System.Collections.Generic;
using System.Linq;

namespace FourFatesStudios.ProjectWarden.Editor
{
    /// <summary>
    /// Tool to help set up modular UI architecture
    /// </summary>
    public class UIArchitectureHelper : EditorWindow
    {
        [MenuItem("Tools/Setup UI Architecture")]
        public static void ShowWindow()
        {
            GetWindow<UIArchitectureHelper>("UI Architecture Helper");
        }

        private void OnGUI()
        {
            GUILayout.Label("UI Architecture Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox("🏗️ This tool helps you set up a modular UI architecture using the hybrid approach.", MessageType.Info);

            // Check current setup
            var uiManager = Object.FindFirstObjectByType<UIManager>();
            
            GUILayout.Label("Current Status:", EditorStyles.boldLabel);
            
            string uiManagerStatus = uiManager != null ? $"✅ Found: {uiManager.gameObject.name}" : "❌ Not found";
            EditorGUILayout.LabelField($"UIManager: {uiManagerStatus}");

            GUILayout.Space(10);

            // Create UIManager if needed
            if (uiManager == null)
            {
                EditorGUILayout.HelpBox("❌ No UIManager found. You need one to manage your UI panels.", MessageType.Warning);
                
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("🏗️ Create UIManager", GUILayout.Height(40)))
                {
                    CreateUIManager();
                    uiManager = Object.FindFirstObjectByType<UIManager>();
                }
                GUI.backgroundColor = Color.white;
                GUILayout.Space(10);
            }

            // Show recommended architecture
            GUILayout.Label("Recommended Architecture:", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox(
                "✅ One Main Scene (PotionCraftingMenu.unity)\n" +
                "✅ UIManager (manages panel switching)\n" +
                "✅ Core Systems (inventory, skill system)\n" +
                "✅ UI Panels as Prefabs (load/unload dynamically)\n" +
                "✅ Shared resources in scene", MessageType.None);

            GUILayout.Space(10);

            // Panel creation tools
            GUILayout.Label("Panel Management:", EditorStyles.boldLabel);
            
            if (GUILayout.Button("📋 Create Panel Prefabs from Current Objects"))
            {
                CreatePanelPrefabs();
            }
            
            if (GUILayout.Button("🔗 Auto-Configure UIManager"))
            {
                AutoConfigureUIManager();
            }

            GUILayout.Space(10);

            // Architecture benefits
            EditorGUILayout.HelpBox(
                "💡 Benefits of this architecture:\n" +
                "• Better performance (only active UI uses resources)\n" +
                "• Team-friendly (work on different panels separately)\n" +
                "• Maintainable (easy to debug individual features)\n" +
                "• Scalable (add new panels easily)\n" +
                "• Memory efficient (load/unload as needed)", MessageType.Info);

            GUILayout.Space(10);

            // Quick setup
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("⚡ Quick Setup Complete Architecture", GUILayout.Height(40)))
            {
                QuickSetupArchitecture();
            }
            GUI.backgroundColor = Color.white;
        }

        private void CreateUIManager()
        {
            var go = new GameObject("UIManager");
            go.layer = LayerMask.NameToLayer("UI");
            
            var uiManager = go.AddComponent<UIManager>();
            
            // Try to find and assign main UIDocument
            var mainUIDoc = Object.FindFirstObjectByType<UnityEngine.UIElements.UIDocument>();
            if (mainUIDoc != null)
            {
                var field = typeof(UIManager).GetField("mainUIDocument", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(uiManager, mainUIDoc);
            }

            Debug.Log("✅ Created UIManager");
            Selection.activeGameObject = go;
        }

        private void CreatePanelPrefabs()
        {
            // Create Prefabs directory if it doesn't exist
            string prefabDir = "Assets/Prefabs/UI";
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
            }

            var existingControllers = new Dictionary<string, MonoBehaviour>
            {
                {"BulkCrafting", Object.FindFirstObjectByType<BulkCraftingController>()}
            };

            int created = 0;
            foreach (var kvp in existingControllers)
            {
                if (kvp.Value != null)
                {
                    string prefabName = $"{kvp.Key}Panel";
                    string prefabPath = $"{prefabDir}/{prefabName}.prefab";
                    
                    // Create prefab from the controller's GameObject
                    var prefab = PrefabUtility.SaveAsPrefabAsset(kvp.Value.gameObject, prefabPath);
                    if (prefab != null)
                    {
                        Debug.Log($"✅ Created prefab: {prefabName}");
                        created++;
                    }
                }
            }

            if (created > 0)
            {
                AssetDatabase.Refresh();
                Debug.Log($"✅ Created {created} panel prefabs in {prefabDir}");
            }
            else
            {
                Debug.LogWarning("⚠️ No controllers found to convert to prefabs");
            }
        }

        private void AutoConfigureUIManager()
        {
            var uiManager = Object.FindFirstObjectByType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("❌ UIManager not found. Create one first.");
                return;
            }

            // Find all UI panel prefabs
            var prefabPaths = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/Prefabs/UI" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.Contains("Panel"))
                .ToArray();

            var prefabs = new List<GameObject>();
            foreach (var path in prefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    prefabs.Add(prefab);
                }
            }

            if (prefabs.Count > 0)
            {
                // Set prefabs via reflection
                var field = typeof(UIManager).GetField("panelPrefabs", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(uiManager, prefabs.ToArray());

                EditorUtility.SetDirty(uiManager);
                Debug.Log($"✅ Configured UIManager with {prefabs.Count} panel prefabs");
            }
            else
            {
                Debug.LogWarning("⚠️ No panel prefabs found in Assets/Prefabs/UI");
            }
        }

        private void QuickSetupArchitecture()
        {
            Debug.Log("🚀 Setting up complete UI architecture...");

            // Step 1: Create UIManager if needed
            var uiManager = Object.FindFirstObjectByType<UIManager>();
            if (uiManager == null)
            {
                CreateUIManager();
                uiManager = Object.FindFirstObjectByType<UIManager>();
            }

            // Step 2: Create prefab directory
            string prefabDir = "Assets/Prefabs/UI";
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder(prefabDir))
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

            // Step 3: Create panel prefabs
            CreatePanelPrefabs();

            // Step 4: Configure UIManager
            AutoConfigureUIManager();

            // Step 5: Create example navigation script
            CreateNavigationExample();

            AssetDatabase.Refresh();
            Debug.Log("✅ Complete UI architecture setup finished!");
            
            EditorUtility.DisplayDialog("Architecture Setup Complete!", 
                "✅ UIManager created\n✅ Panel prefabs generated\n✅ Navigation example added\n\nYour UI is now modular and scalable!", 
                "Awesome!");
        }

        private void CreateNavigationExample()
        {
            string scriptPath = "Assets/Scripts/UI/UINavigationExample.cs";
            
            string navigationScript = @"using UnityEngine;
using FourFatesStudios.ProjectWarden.UI;

/// <summary>
/// Example script showing how to use the UIManager for navigation
/// </summary>
public class UINavigationExample : MonoBehaviour
{
    void Update()
    {
        // Example navigation controls
        if (Input.GetKeyDown(KeyCode.Alpha1))
            UIManager.Instance?.SwitchToPanel(""PotionCrafting"");
            
        if (Input.GetKeyDown(KeyCode.Alpha2))
            UIManager.Instance?.SwitchToPanel(""Refinement"");
            
        if (Input.GetKeyDown(KeyCode.Alpha3))
            UIManager.Instance?.SwitchToPanel(""BulkCrafting"");
            
        if (Input.GetKeyDown(KeyCode.Alpha4))
            UIManager.Instance?.SwitchToPanel(""AlchemyBook"");
            
        if (Input.GetKeyDown(KeyCode.Escape))
            UIManager.Instance?.GoBack();
    }
    
    // Example methods for UI buttons
    public void OpenPotionCrafting() => UIManager.Instance?.SwitchToPanel(""PotionCrafting"");
    public void OpenBulkCrafting() => UIManager.Instance?.SwitchToPanel(""BulkCrafting"");
    public void OpenAlchemyBook() => UIManager.Instance?.SwitchToPanel(""AlchemyBook"");
    public void GoBack() => UIManager.Instance?.GoBack();
}";

            System.IO.File.WriteAllText(scriptPath, navigationScript);
            AssetDatabase.Refresh();
            Debug.Log("✅ Created navigation example script");
        }

        [MenuItem("Tools/Quick UI Architecture Setup", false, 170)]
        public static void QuickSetup()
        {
            var window = CreateInstance<UIArchitectureHelper>();
            window.QuickSetupArchitecture();
        }
    }
}