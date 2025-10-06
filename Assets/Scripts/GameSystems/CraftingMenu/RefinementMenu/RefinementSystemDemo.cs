using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.Structs;
using FourFatesStudios.ProjectWarden.GameSystems;
using FourFatesStudios.ProjectWarden.GameSystems.RefinementMenu;

namespace FourFatesStudios.ProjectWarden.GameSystems
{
    /// <summary>
    /// Demo script to test and showcase the enhanced refinement system
    /// </summary>
    public class RefinementSystemDemo : MonoBehaviour
    {
        [Header("Demo Settings")]
        [SerializeField] private bool createDemoIngredients = true;
        [SerializeField] private bool enableDebugLogging = true;
        
        [Header("Demo Ingredient Creation")]
        [SerializeField] private Sprite defaultIcon;
        
        private RefinementMenuManager refinementManager;
        
        void Start()
        {
            refinementManager = FindFirstObjectByType<RefinementMenuManager>();
            
            if (createDemoIngredients)
            {
                CreateDemoIngredients();
            }
            
            if (enableDebugLogging)
            {
                Debug.Log("🧪 RefinementSystemDemo initialized");
                LogSystemStatus();
            }
        }
        
        [ContextMenu("Create Demo Ingredients")]
        public void CreateDemoIngredients()
        {
            Debug.Log("🏗️ Creating demo ingredients for refinement system testing...");
            
            // Create demo ingredients as ScriptableObject instances (runtime only)
            var demoIngredients = new Ingredient[]
            {
                CreateDemoIngredient("Iron Ore", IngredientArchetype.Ore, canGrind: true),
                CreateDemoIngredient("Moonflower Petals", IngredientArchetype.Herb, canDistill: true, canRoast: true),
                CreateDemoIngredient("Silver Dust", IngredientArchetype.Ore, canGrind: false), // Already ground
                CreateDemoIngredient("Dragon Scale", IngredientArchetype.Organic, canRoast: true),
                CreateDemoIngredient("Crystal Water", IngredientArchetype.Solvent, canDistill: true),
                CreateDemoIngredient("Corrupted Moss", IngredientArchetype.Herb, canDistill: true, isCorrupted: true),
                CreateDemoIngredient("Fire Opal", IngredientArchetype.Ore, canGrind: true, potency: 4),
                CreateDemoIngredient("Healing Herb", IngredientArchetype.Herb, canDistill: true, canRoast: true, potency: 2)
            };
            
            // Add demo ingredients to refinement manager
            if (refinementManager != null)
            {
                foreach (var ingredient in demoIngredients)
                {
                    // This would typically be done through the inventory system
                    Debug.Log($"Created demo ingredient: {ingredient.ItemName} - {ingredient.IngredientArchetype}");
                }
                
                refinementManager.RefreshAvailableIngredients();
                Debug.Log($"✅ Created {demoIngredients.Length} demo ingredients");
            }
            else
            {
                Debug.LogWarning("⚠️ RefineMenuManager not found - cannot add demo ingredients");
            }
        }
        
        private Ingredient CreateDemoIngredient(string name, IngredientArchetype archetype, 
            bool canGrind = false, bool canDistill = false, bool canRoast = false, 
            int potency = 1, bool isCorrupted = false)
        {
            var ingredient = ScriptableObject.CreateInstance<Ingredient>();
            
            // Set basic properties using reflection since fields are private
            var itemNameField = typeof(Item).GetField("itemName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            itemNameField?.SetValue(ingredient, name);
            
            var itemDescriptionField = typeof(Item).GetField("itemDescription", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            itemDescriptionField?.SetValue(ingredient, $"Demo {archetype.ToString().ToLower()} for testing refinement");
            
            var itemIconField = typeof(Item).GetField("itemIcon", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            itemIconField?.SetValue(ingredient, defaultIcon);
            
            // Set ingredient-specific properties
            var archetypeField = typeof(Ingredient).GetField("ingredientArchetype", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            archetypeField?.SetValue(ingredient, archetype);
            
            var potencyField = typeof(Ingredient).GetField("potency", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            potencyField?.SetValue(ingredient, potency);
            
            var corruptedField = typeof(Ingredient).GetField("isCorrupted", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            corruptedField?.SetValue(ingredient, isCorrupted);
            
            // Set refinement capabilities
            var canGrindField = typeof(Ingredient).GetField("canGrind", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            canGrindField?.SetValue(ingredient, canGrind);
            
            var canDistillField = typeof(Ingredient).GetField("canDistill", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            canDistillField?.SetValue(ingredient, canDistill);
            
            var canRoastField = typeof(Ingredient).GetField("canRoast", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            canRoastField?.SetValue(ingredient, canRoast);
            
            // Set success rates
            var successRateField = typeof(Ingredient).GetField("baseRefiningSuccessRate", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            successRateField?.SetValue(ingredient, 0.7f + (potency * 0.05f));
            
            var stabilityField = typeof(Ingredient).GetField("stabilityRating", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            stabilityField?.SetValue(ingredient, isCorrupted ? 0.5f : 0.8f);
            
            var skillField = typeof(Ingredient).GetField("minimumRefiningSkill", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            skillField?.SetValue(ingredient, potency * 10);
            
            return ingredient;
        }
        
        [ContextMenu("Test Refinement System")]
        public void TestRefinementSystem()
        {
            Debug.Log("🔧 Testing refinement system functionality...");
            
            if (refinementManager == null)
            {
                refinementManager = FindFirstObjectByType<RefinementMenuManager>();
            }
            
            if (refinementManager != null)
            {
                Debug.Log("✅ RefineMenuManager found");
                Debug.Log($"• Hybrid architecture available: {refinementManager.IsHybridArchitectureAvailable()}");
                Debug.Log($"• Selected ingredient: {refinementManager.GetSelectedIngredient()?.ItemName ?? "None"}");
                
                refinementManager.RefreshAvailableIngredients();
                Debug.Log("✅ Ingredient list refreshed");
            }
            else
            {
                Debug.LogWarning("⚠️ RefineMenuManager not found in scene");
            }
            
            LogSystemStatus();
        }
        
        [ContextMenu("Test Minigame Controllers")]
        public void TestMinigameControllers()
        {
            Debug.Log("🎮 Testing minigame controllers...");
            
            var grindingController = FindFirstObjectByType<GrindingMinigameController>();
            var distillationController = FindFirstObjectByType<DistillationMinigameController>();
            var roastingController = FindFirstObjectByType<RoastingMinigameController>();
            
            Debug.Log($"• Grinding controller: {(grindingController != null ? "✅ Found" : "❌ Missing")}");
            Debug.Log($"• Distillation controller: {(distillationController != null ? "✅ Found" : "❌ Missing")}");
            Debug.Log($"• Roasting controller: {(roastingController != null ? "✅ Found" : "❌ Missing")}");
            
            // Test setting target ingredients
            if (grindingController != null && createDemoIngredients)
            {
                var testIngredient = CreateDemoIngredient("Test Ore", IngredientArchetype.Ore, canGrind: true);
                grindingController.SetTargetIngredient(testIngredient);
                Debug.Log("✅ Test ingredient set for grinding controller");
            }
        }
        
        private void LogSystemStatus()
        {
            Debug.Log("📊 Refinement System Status:");
            Debug.Log($"• Demo ingredients created: {createDemoIngredients}");
            Debug.Log($"• Debug logging enabled: {enableDebugLogging}");
            Debug.Log($"• Current scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            
            var allMinigameControllers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            int controllerCount = 0;
            foreach (var controller in allMinigameControllers)
            {
                if (controller.GetType().Name.Contains("MinigameController"))
                {
                    controllerCount++;
                }
            }
            Debug.Log($"• Minigame controllers found: {controllerCount}");
        }
        
        void Update()
        {
            // Test hotkeys for development
            if (enableDebugLogging)
            {
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    TestRefinementSystem();
                }
                
                if (Input.GetKeyDown(KeyCode.F2))
                {
                    CreateDemoIngredients();
                }
                
                if (Input.GetKeyDown(KeyCode.F3))
                {
                    TestMinigameControllers();
                }
            }
        }
        
        void OnGUI()
        {
            if (!enableDebugLogging) return;
            
            // Simple debug GUI
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Refinement System Demo", GUI.skin.box);
            
            if (GUILayout.Button("F1 - Test System"))
                TestRefinementSystem();
                
            if (GUILayout.Button("F2 - Create Demo Ingredients"))
                CreateDemoIngredients();
                
            if (GUILayout.Button("F3 - Test Controllers"))
                TestMinigameControllers();
                
            GUILayout.Label($"Selected: {refinementManager?.GetSelectedIngredient()?.ItemName ?? "None"}");
            
            GUILayout.EndArea();
        }
    }
}