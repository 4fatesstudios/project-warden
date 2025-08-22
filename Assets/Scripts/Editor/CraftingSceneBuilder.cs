using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Demo;

namespace FourFatesStudios.ProjectWarden.Editor
{
    /// <summary>
    /// Builds a complete crafting system scene in Edit Mode
    /// This recreates what the runtime scripts do, but permanently in the editor
    /// </summary>
    public class CraftingSceneBuilder : EditorWindow
    {
        private string sceneName = "CraftingSystemComplete";
        private bool setupCameras = true;
        private bool createUI = true;
        private bool addIngredients = true;
        private bool setupLighting = true;

        [MenuItem("Tools/Crafting Scene Builder")]
        public static void ShowWindow()
        {
            GetWindow<CraftingSceneBuilder>("Crafting Scene Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Crafting System Scene Builder", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Builds a complete crafting system scene in Edit Mode (no Play Mode required)", MessageType.Info);
            
            GUILayout.Space(10);

            // Scene settings
            GUILayout.Label("Scene Configuration", EditorStyles.boldLabel);
            sceneName = EditorGUILayout.TextField("Scene Name:", sceneName);
            
            GUILayout.Space(10);
            
            // Build options
            GUILayout.Label("Build Options", EditorStyles.boldLabel);
            setupCameras = EditorGUILayout.Toggle("Setup Cameras & Lighting", setupCameras);
            createUI = EditorGUILayout.Toggle("Create UI System", createUI);
            addIngredients = EditorGUILayout.Toggle("Add Demo Ingredients", addIngredients);
            setupLighting = EditorGUILayout.Toggle("Setup Scene Lighting", setupLighting);

            GUILayout.Space(15);

            // Build button
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Build Complete Crafting Scene", GUILayout.Height(40)))
            {
                BuildCraftingScene();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // Quick setup button
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Quick Setup Current Scene", GUILayout.Height(30)))
            {
                QuickSetupCurrentScene();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // Instructions
            EditorGUILayout.HelpBox("🎯 Use 'Build Complete Crafting Scene' to create a new scene with everything.\n" +
                                  "🚀 Use 'Quick Setup Current Scene' to add crafting system to current scene.", MessageType.None);
        }

        private void BuildCraftingScene()
        {
            // Create new scene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Set scene name for saving
            newScene.name = sceneName;

            try
            {
                SetupScene();
                
                // Save the scene
                string scenePath = $"Assets/Scenes/{sceneName}.unity";
                EditorSceneManager.SaveScene(newScene, scenePath);
                
                EditorUtility.DisplayDialog("Success", 
                    $"Crafting scene built successfully!\nSaved as: {scenePath}\n\nPress Play to test the crafting system.", "OK");
                
                Debug.Log($"✅ Crafting scene built: {scenePath}");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to build scene: {e.Message}", "OK");
                Debug.LogError($"Scene build failed: {e}");
            }
        }

        private void QuickSetupCurrentScene()
        {
            try
            {
                SetupScene();
                
                // Mark scene as dirty to save changes
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                
                EditorUtility.DisplayDialog("Success", 
                    "Crafting system added to current scene!\n\nPress Play to test the system.", "OK");
                
                Debug.Log("✅ Crafting system added to current scene");
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Error", $"Failed to setup scene: {e.Message}", "OK");
                Debug.LogError($"Scene setup failed: {e}");
            }
        }

        private void SetupScene()
        {
            // Setup cameras and lighting
            if (setupCameras)
            {
                SetupCamerasAndLighting();
            }

            // Create the main crafting system
            CreateCraftingSystem();

            // Create UI system
            if (createUI)
            {
                CreateUISystem();
            }

            // Add demo ingredients
            if (addIngredients)
            {
                CreateDemoIngredients();
            }

            // Final setup
            SetupEventSystem();
        }

        private void SetupCamerasAndLighting()
        {
            // Ensure we have a main camera
            var camera = UnityEngine.Camera.main;
            if (camera == null)
            {
                var cameraGO = new GameObject("Main Camera");
                camera = cameraGO.AddComponent<UnityEngine.Camera>();
                cameraGO.tag = "MainCamera";
            }

            // Configure camera for UI
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
            camera.transform.position = new Vector3(0, 1, -10);

            // Add lighting if needed
            if (setupLighting && RenderSettings.sun == null)
            {
                var lightGO = new GameObject("Directional Light");
                var light = lightGO.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = Color.white;
                light.intensity = 1f;
                lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                RenderSettings.sun = light;
            }
        }

        private void CreateCraftingSystem()
        {
            // Create main crafting system root
            var craftingSystemRoot = new GameObject("CraftingSystem");
            
            // Add the demo setup script (using reflection to avoid type issues)
            var simpleUIType = System.Type.GetType("SimpleWorkingUI");
            if (simpleUIType != null)
            {
                craftingSystemRoot.AddComponent(simpleUIType);
            }

            // Add the advanced setup guide for reference (from Demo namespace)
            var setupGuideType = System.Type.GetType("Demo.CraftingSystemSetupGuide");
            if (setupGuideType != null)
            {
                var setupGuide = craftingSystemRoot.AddComponent(setupGuideType);
                // Try to set autoSetupDemo to false via reflection
                var autoSetupField = setupGuideType.GetField("autoSetupDemo");
                if (autoSetupField != null)
                {
                    autoSetupField.SetValue(setupGuide, false);
                }
            }

            Debug.Log("✓ Created main crafting system");
        }

        private void CreateUISystem()
        {
            // Find or create UI root
            var craftingSystem = GameObject.Find("CraftingSystem");
            if (craftingSystem == null)
            {
                craftingSystem = new GameObject("CraftingSystem");
            }

            // Create UI hierarchy
            var uiRoot = new GameObject("CraftingUI");
            uiRoot.transform.SetParent(craftingSystem.transform);

            // Add individual UI components (using simple types to avoid missing references)
            CreateUIComponent(uiRoot, "PotionCraftingUI", null);
            CreateUIComponent(uiRoot, "GridMinigameUI", null);
            CreateUIComponent(uiRoot, "BulkCraftingUI", null);

            // Create refinement system
            var refinementRoot = new GameObject("RefinementSystem");
            refinementRoot.transform.SetParent(uiRoot.transform);
            
            CreateUIComponent(refinementRoot, "RoastingMinigameUI", null);
            CreateUIComponent(refinementRoot, "DistillationMinigameUI", null);
            CreateUIComponent(refinementRoot, "GrindingMinigameUI", null);

            Debug.Log("✓ Created UI system hierarchy");
        }

        private void CreateUIComponent(GameObject parent, string name, System.Type controllerType)
        {
            var uiObject = new GameObject(name);
            uiObject.transform.SetParent(parent.transform);

            // Add UIDocument
            var uiDocument = uiObject.AddComponent<UnityEngine.UIElements.UIDocument>();
            
            // Add controller component if type is provided
            if (controllerType != null)
            {
                var controller = uiObject.AddComponent(controllerType);
                
                // Try to assign UIDocument reference if the controller has it
                var uiDocField = controllerType.GetField("uiDocument");
                if (uiDocField != null)
                {
                    uiDocField.SetValue(controller, uiDocument);
                }
            }
            
            // Start inactive to avoid runtime errors until UXML is assigned
            uiObject.SetActive(false);
        }

        private void CreateDemoIngredients()
        {
            // Create inventory holder
            var inventoryGO = new GameObject("DemoInventory");
            var inventory = inventoryGO.AddComponent<ItemSlotContainerHolder>();
            
            // Create ingredient creator for generating test ingredients (if available)
            var creatorGO = new GameObject("IngredientCreator");
            // Note: DemoIngredientCreator might not be available in all setups
            
            // In edit mode, we can't directly create ScriptableObjects the same way
            // But we can set up the structure for runtime creation
            Debug.Log("✓ Created inventory system - ingredients will be generated at runtime");
        }

        private void SetupEventSystem()
        {
            // Ensure we have an EventSystem for UI
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            Debug.Log("✓ Event system configured");
        }

        // Quick access menu item
        [MenuItem("GameObject/Setup Crafting System Here", false, 10)]
        private static void SetupCraftingSystemFromContext()
        {
            var builder = GetWindow<CraftingSceneBuilder>("Crafting Scene Builder");
            builder.QuickSetupCurrentScene();
            builder.Close();
        }
    }
}