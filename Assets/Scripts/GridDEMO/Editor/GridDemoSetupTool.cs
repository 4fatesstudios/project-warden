using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.GridDemo;
using UnityCamera = UnityEngine.Camera;

namespace FourFatesStudios.ProjectWarden.GridDemo.Editor
{
    public class GridDemoSetupTool : EditorWindow
    {
        [MenuItem("Tools/Grid Demo/Setup Tool")]
        public static void ShowWindow()
        {
            GridDemoSetupTool window = GetWindow<GridDemoSetupTool>();
            window.titleContent = new GUIContent("Grid Demo Setup");
            window.Show();
        }
        
        private Vector2 scrollPosition;
        private int gridWidth = 8;
        private int gridHeight = 8;
        private float cellSize = 1f;
        private List<Ingredient> selectedIngredients = new List<Ingredient>();
        private bool createUI = true;
        private bool setupCamera = true;
        private bool setupLighting = true;
        
        private void OnGUI()
        {
            GUILayout.Label("Grid Game Demo Setup Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawGridConfiguration();
            GUILayout.Space(10);
            
            DrawIngredientSelection();
            GUILayout.Space(10);
            
            DrawSetupOptions();
            GUILayout.Space(10);
            
            DrawActionButtons();
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawGridConfiguration()
        {
            EditorGUILayout.LabelField("Grid Configuration", EditorStyles.boldLabel);
            
            gridWidth = EditorGUILayout.IntSlider("Grid Width", gridWidth, 4, 16);
            gridHeight = EditorGUILayout.IntSlider("Grid Height", gridHeight, 4, 16);
            cellSize = EditorGUILayout.Slider("Cell Size", cellSize, 0.5f, 2f);
            
            EditorGUILayout.HelpBox($"Grid will be {gridWidth}x{gridHeight} with {cellSize}m cell size", MessageType.Info);
        }
        
        private void DrawIngredientSelection()
        {
            EditorGUILayout.LabelField("Ingredient Selection", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Auto-Find All Ingredients"))
            {
                selectedIngredients = FindAllIngredients();
            }
            
            if (GUILayout.Button("Clear Ingredient List"))
            {
                selectedIngredients.Clear();
            }
            
            GUILayout.Space(5);
            
            if (selectedIngredients.Count > 0)
            {
                EditorGUILayout.LabelField($"Selected Ingredients ({selectedIngredients.Count}):", EditorStyles.boldLabel);
                
                for (int i = 0; i < selectedIngredients.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    selectedIngredients[i] = (Ingredient)EditorGUILayout.ObjectField(
                        selectedIngredients[i], typeof(Ingredient), false);
                    
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        selectedIngredients.RemoveAt(i);
                        i--;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No ingredients selected. Click 'Auto-Find All Ingredients' to populate the list.", MessageType.Warning);
            }
            
            if (GUILayout.Button("Add Ingredient Slot"))
            {
                selectedIngredients.Add(null);
            }
        }
        
        private void DrawSetupOptions()
        {
            EditorGUILayout.LabelField("Setup Options", EditorStyles.boldLabel);
            
            createUI = EditorGUILayout.Toggle("Create UI Panel", createUI);
            setupCamera = EditorGUILayout.Toggle("Setup Camera", setupCamera);
            setupLighting = EditorGUILayout.Toggle("Setup Lighting", setupLighting);
        }
        
        private void DrawActionButtons()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Create Grid Demo Scene", GUILayout.Height(30)))
            {
                CreateGridDemoScene();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("Add Grid to Current Scene", GUILayout.Height(25)))
            {
                AddGridToCurrentScene();
            }
            
            if (GUILayout.Button("Create Test Ingredients", GUILayout.Height(25)))
            {
                CreateTestIngredients();
            }
        }
        
        private List<Ingredient> FindAllIngredients()
        {
            List<Ingredient> ingredients = new List<Ingredient>();
            string[] guids = AssetDatabase.FindAssets("t:Ingredient");
            
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Ingredient ingredient = AssetDatabase.LoadAssetAtPath<Ingredient>(assetPath);
                if (ingredient != null)
                {
                    ingredients.Add(ingredient);
                }
            }
            
            Debug.Log($"Found {ingredients.Count} ingredients in project");
            return ingredients;
        }
        
        private void CreateGridDemoScene()
        {
            // Create new scene
            var newScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, 
                UnityEditor.SceneManagement.NewSceneMode.Single);
            
            // Add grid to the new scene
            AddGridToCurrentScene();
            
            // Save the scene
            string scenePath = "Assets/Scenes/GridDemo.unity";
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(newScene, scenePath);
            
            Debug.Log($"Created Grid Demo scene at {scenePath}");
        }
        
        private void AddGridToCurrentScene()
        {
            // Create grid manager object
            GameObject gridObj = new GameObject("GridGameManager");
            GridGameManager gridManager = gridObj.AddComponent<GridGameManager>();
            
            // Configure grid
            gridManager.gridWidth = gridWidth;
            gridManager.gridHeight = gridHeight;
            gridManager.cellSize = cellSize;
            gridManager.gridStartPosition = Vector3.zero;
            
            // Add components
            gridObj.AddComponent<GridVisualizer>();
            gridObj.AddComponent<IngredientPlacer>();
            gridObj.AddComponent<InputActions>();
            
            // Set available ingredients
            gridManager.availableIngredients = new List<Ingredient>(selectedIngredients);
            
            // Setup camera
            if (setupCamera)
            {
                SetupCamera();
            }
            
            // Setup lighting
            if (setupLighting)
            {
                SetupLighting();
            }
            
            // Create UI
            if (createUI)
            {
                CreateDemoUI();
            }
            
            // Select the created object
            Selection.activeGameObject = gridObj;
            
            Debug.Log("Grid Demo added to current scene");
        }
        
        private void SetupCamera()
        {
            // Fixed: Use Unity's Camera.main explicitly to avoid namespace conflicts with ProjectWarden.Camera
            // namespace by using an alias UnityCamera = UnityEngine.Camera
            UnityCamera cam = UnityCamera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<UnityCamera>();
                camObj.tag = "MainCamera";
            }
            
            // Position camera to view the grid
            float gridSize = Mathf.Max(gridWidth, gridHeight) * cellSize;
            Vector3 gridCenter = new Vector3(gridWidth * cellSize * 0.5f, 0, gridHeight * cellSize * 0.5f);
            
            cam.transform.position = gridCenter + new Vector3(0, gridSize * 1.5f, -gridSize * 0.8f);
            cam.transform.LookAt(gridCenter);
            
            // Configure camera for grid viewing
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.2f, 0.2f, 0.3f);
            cam.orthographic = false;
            cam.fieldOfView = 60f;
        }
        
        private void SetupLighting()
        {
            // Create directional light
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1f;
            lightObj.transform.rotation = Quaternion.Euler(45f, -45f, 0f);
            
            // Set ambient lighting
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.6f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.3f);
        }
        
        private void CreateDemoUI()
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("GridDemo UI");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Add UI component
            GridDemoUI demoUI = canvasObj.AddComponent<GridDemoUI>();
            
            // Create basic UI structure
            CreateBasicUIStructure(canvasObj, demoUI);
            
            // Create EventSystem if it doesn't exist
            if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }
        
        private void CreateBasicUIStructure(GameObject canvasObj, GridDemoUI demoUI)
        {
            // This would create a basic UI structure
            // For brevity, I'll just create placeholder objects
            
            GameObject ingredientPanel = new GameObject("Ingredient Panel");
            ingredientPanel.transform.SetParent(canvasObj.transform, false);
            
            GameObject infoPanel = new GameObject("Info Panel");
            infoPanel.transform.SetParent(canvasObj.transform, false);
            
            Debug.Log("Basic UI structure created. You'll need to manually configure the UI elements.");
        }
        
        private void CreateTestIngredients()
        {
            // Create test ingredients directory
            string testDir = "Assets/Resources/TestIngredients";
            if (!AssetDatabase.IsValidFolder(testDir))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "TestIngredients");
            }
            
            // Create some basic test ingredients
            CreateTestIngredient("Fire Essence", "A concentrated essence of fire", Aspect.Scorch, IngredientArchetype.Synthetic, 3, 1, 1);
            CreateTestIngredient("Ice Crystal", "A perfectly formed ice crystal", Aspect.Frigid, IngredientArchetype.Ore, 2, 1, 1);
            CreateTestIngredient("Lightning Stone", "A stone charged with lightning", Aspect.Arc, IngredientArchetype.Ore, 4, 1, 2);
            CreateTestIngredient("Earth Root", "A root that draws power from the earth", Aspect.Corporeal, IngredientArchetype.Herb, 2, 2, 1);
            CreateTestIngredient("Life Bloom", "A flower that radiates life energy", Aspect.Divine, IngredientArchetype.Herb, 3, 1, 1);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("Created test ingredients in Assets/Resources/TestIngredients/");
        }
        
        private void CreateTestIngredient(string name, string description, Aspect aspect, IngredientArchetype archetype, int potency, int width, int height)
        {
            Ingredient ingredient = ScriptableObject.CreateInstance<Ingredient>();
            ingredient.name = name;
            
            // Use reflection to set private fields (this is for demo purposes)
            var itemNameField = typeof(Ingredient).BaseType.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var itemDescField = typeof(Ingredient).BaseType.GetField("itemDescription", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var aspectField = typeof(Ingredient).GetField("ingredientAspect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var archetypeField = typeof(Ingredient).GetField("ingredientArchetype", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var potencyField = typeof(Ingredient).GetField("potency", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var widthField = typeof(Ingredient).GetField("gridWidth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var heightField = typeof(Ingredient).GetField("gridHeight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            itemNameField?.SetValue(ingredient, name);
            itemDescField?.SetValue(ingredient, description);
            aspectField?.SetValue(ingredient, aspect);
            archetypeField?.SetValue(ingredient, archetype);
            potencyField?.SetValue(ingredient, potency);
            widthField?.SetValue(ingredient, width);
            heightField?.SetValue(ingredient, height);
            
            string path = $"Assets/Resources/TestIngredients/{name}.asset";
            AssetDatabase.CreateAsset(ingredient, path);
        }
    }
}