using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu
{
    /// <summary>
    /// Helper script to quickly set up and test the Tetris-like alchemy system
    /// </summary>
    [System.Serializable]
    public class TetrisAlchemySetupHelper : MonoBehaviour
    {
        [Header("Auto Setup Configuration")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool createTestData = false;
        [SerializeField] private GridMinigameController gridController;
        [SerializeField] private UIDocument uiDocument;
        
        [Header("Sample Data")]
        [SerializeField] private List<Ingredient> sampleIngredients = new List<Ingredient>();
        [SerializeField] private List<AlchemyRecipe> sampleRecipes = new List<AlchemyRecipe>();
        [SerializeField] private StyleSheet tetrisGridStyles;
        
        [Header("Quick Test Settings")]
        [SerializeField] private bool enableQuickTests = true;
        [SerializeField] private KeyCode testBasicPlacement = KeyCode.F1;
        [SerializeField] private KeyCode testInteractions = KeyCode.F2;
        [SerializeField] private KeyCode testExpansion = KeyCode.F3;
        [SerializeField] private KeyCode testSkills = KeyCode.F4;
        [SerializeField] private KeyCode resetSystem = KeyCode.F5;

        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupTetrisAlchemySystem();
            }
        }
        
        private void Update()
        {
            if (enableQuickTests)
            {
                HandleQuickTestInputs();
            }
        }
        
        [ContextMenu("Setup Tetris Alchemy System")]
        public void SetupTetrisAlchemySystem()
        {
            Debug.Log("🔧 Setting up Tetris Alchemy System...");
            
            // 1. Find or create grid controller
            if (gridController == null)
            {
                gridController = FindFirstObjectByType<GridMinigameController>();
                if (gridController == null)
                {
                    Debug.LogWarning("No GridMinigameController found! Please add one to the scene.");
                    return;
                }
            }
            
            // 2. Setup UI Document
            if (uiDocument == null)
            {
                uiDocument = gridController.GetComponent<UIDocument>();
                if (uiDocument == null)
                {
                    uiDocument = gridController.gameObject.AddComponent<UIDocument>();
                }
            }
            
            // 3. Apply styles if available
            if (tetrisGridStyles != null)
            {
                var rootElement = uiDocument.rootVisualElement;
                if (rootElement.styleSheets.count == 0)
                {
                    rootElement.styleSheets.Add(tetrisGridStyles);
                    Debug.Log("  ✅ Applied Tetris grid styles");
                }
            }
            
            // 4. Setup sample data
            if (sampleIngredients.Count > 0)
            {
                gridController.SetAvailableIngredients(sampleIngredients);
                Debug.Log($"  ✅ Loaded {sampleIngredients.Count} sample ingredients");
            }
            
            // 5. Setup interactions
            SetupSampleInteractions();
            
            // 6. Create test data if requested
            if (createTestData)
            {
                CreateTestData();
            }
            
            Debug.Log("🎉 Tetris Alchemy System setup complete!");
        }
        
        private void SetupSampleInteractions()
        {
            if (sampleIngredients.Count < 2) return;
            
            // Add some basic interactions based on aspects
            var interactions = 0;
            
            for (int i = 0; i < sampleIngredients.Count; i++)
            {
                for (int j = i + 1; j < sampleIngredients.Count; j++)
                {
                    var ingredient1 = sampleIngredients[i];
                    var ingredient2 = sampleIngredients[j];
                    
                    // Create interactions for complementary aspects
                    if (AreAspectsComplementary(ingredient1.IngredientAspect, ingredient2.IngredientAspect))
                    {
                        gridController.AddIngredientInteraction(
                            ingredient1,
                            ingredient2,
                            1.5f,
                            $"{ingredient1.IngredientAspect} + {ingredient2.IngredientAspect} synergy"
                        );
                        interactions++;
                    }
                }
            }
            
            if (interactions > 0)
            {
                Debug.Log($"  ✅ Created {interactions} sample ingredient interactions");
            }
        }
        
        private bool AreAspectsComplementary(Aspect aspect1, Aspect aspect2)
        {
            // Define complementary aspects that create positive interactions
            var complementaryPairs = new Dictionary<Aspect, List<Aspect>>
            {
                { Aspect.Scorch, new List<Aspect> { Aspect.Arc, Aspect.Corporeal } },
                { Aspect.Frigid, new List<Aspect> { Aspect.Divine, Aspect.Corporeal } },
                { Aspect.Arc, new List<Aspect> { Aspect.Scorch, Aspect.Divine } },
                { Aspect.Divine, new List<Aspect> { Aspect.Frigid, Aspect.Arc } },
                { Aspect.Corporeal, new List<Aspect> { Aspect.Scorch, Aspect.Frigid } },
                { Aspect.Caustic, new List<Aspect> { } } // Caustic is generally destructive
            };
            
            return complementaryPairs.ContainsKey(aspect1) && 
                   complementaryPairs[aspect1].Contains(aspect2);
        }
        
        private void HandleQuickTestInputs()
        {
            if (Input.GetKeyDown(testBasicPlacement))
            {
                TestBasicPlacement();
            }
            else if (Input.GetKeyDown(testInteractions))
            {
                TestInteractions();
            }
            else if (Input.GetKeyDown(testExpansion))
            {
                TestExpansion();
            }
            else if (Input.GetKeyDown(testSkills))
            {
                TestSkills();
            }
            else if (Input.GetKeyDown(resetSystem))
            {
                ResetSystem();
            }
        }
        
        [ContextMenu("Test Basic Placement")]
        public void TestBasicPlacement()
        {
            Debug.Log("🧩 Testing basic placement...");
            
            if (sampleIngredients.Count == 0)
            {
                Debug.LogWarning("No sample ingredients available for testing!");
                return;
            }
            
            // Set up basic ingredients for placement testing
            var testIngredients = sampleIngredients.GetRange(0, Mathf.Min(3, sampleIngredients.Count));
            gridController.SetAvailableIngredients(testIngredients);
            
            Debug.Log($"  📋 Loaded {testIngredients.Count} ingredients for placement testing");
            Debug.Log("  💡 Click ingredients in the palette, then click grid cells to place them");
        }
        
        [ContextMenu("Test Interactions")]
        public void TestInteractions()
        {
            Debug.Log("🔬 Testing ingredient interactions...");
            
            // Enable interaction display
            var showInteractionsField = typeof(GridMinigameController)
                .GetField("showIngredientInteractions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            showInteractionsField?.SetValue(gridController, true);
            
            SetupSampleInteractions();
            
            Debug.Log("  ✨ Interactions enabled - place adjacent ingredients to see effects");
        }
        
        [ContextMenu("Test Expansion")]
        public void TestExpansion()
        {
            Debug.Log("🔧 Testing grid expansion...");
            
            // Find or create an expansion ingredient
            var expansionIngredient = sampleIngredients.Find(i => i.UnlocksAdditionalSpace);
            
            if (expansionIngredient == null)
            {
                Debug.LogWarning("No expansion ingredients found! Create an ingredient with 'UnlocksAdditionalSpace' enabled.");
                return;
            }
            
            var testIngredients = new List<Ingredient> { expansionIngredient };
            if (sampleIngredients.Count > 1)
            {
                testIngredients.AddRange(sampleIngredients.FindAll(i => i != expansionIngredient).GetRange(0, Mathf.Min(2, sampleIngredients.Count - 1)));
            }
            
            gridController.SetAvailableIngredients(testIngredients);
            
            Debug.Log($"  🎯 Place {expansionIngredient.ItemName} to see grid expansion");
        }
        
        [ContextMenu("Test Skills")]
        public void TestSkills()
        {
            Debug.Log("🎓 Testing skill integration...");
            
            var skillSystem = AlchemySkillSystem.Instance;
            if (skillSystem == null)
            {
                Debug.LogWarning("No AlchemySkillSystem found! Skills cannot be tested.");
                return;
            }
            
            // Unlock test skills
            skillSystem.UnlockSkill("enhanced_grid");
            skillSystem.UnlockSkill("overlap_placement");
            
            // Apply to grid
            gridController.ApplySkillBonuses();
            
            Debug.Log("  ✅ Test skills unlocked and applied");
        }
        
        [ContextMenu("Reset System")]
        public void ResetSystem()
        {
            Debug.Log("🔄 Resetting Tetris Alchemy System...");
            
            // Reset grid controller
            if (gridController != null)
            {
                gridController.SetRecipeAsKeyRecipe(false);
                // Reset would go here if available
            }
            
            // Reset skills
            var skillSystem = AlchemySkillSystem.Instance;
            if (skillSystem != null)
            {
                skillSystem.ResetSkillData();
            }
            
            Debug.Log("  ✅ System reset complete");
        }
        
        [ContextMenu("Create Test Data")]
        public void CreateTestData()
        {
            Debug.Log("🛠️ Creating test data...");
            
#if UNITY_EDITOR
            CreateTestIngredients();
            CreateTestRecipes();
#else
            Debug.LogWarning("Test data creation is only available in the Unity Editor");
#endif
        }
        
#if UNITY_EDITOR
        private void CreateTestIngredients()
        {
            var testIngredients = new List<Ingredient>();
            
            // Create basic test ingredients with different shapes and aspects
            var ingredientConfigs = new[]
            {
                new { name = "Fire Essence", aspect = Aspect.Scorch, width = 1, height = 1, expansion = false },
                new { name = "Ice Crystal", aspect = Aspect.Frigid, width = 2, height = 1, expansion = false },
                new { name = "Lightning Core", aspect = Aspect.Arc, width = 1, height = 2, expansion = false },
                new { name = "Sacred Herb", aspect = Aspect.Divine, width = 2, height = 2, expansion = false },
                new { name = "Void Stone", aspect = Aspect.Caustic, width = 1, height = 1, expansion = true },
                new { name = "Living Root", aspect = Aspect.Corporeal, width = 3, height = 1, expansion = false }
            };
            
            foreach (var config in ingredientConfigs)
            {
                var ingredient = ScriptableObject.CreateInstance<Ingredient>();
                
                // Set properties using reflection (as fields are private)
                var itemNameField = typeof(Item).GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var aspectField = typeof(Ingredient).GetField("ingredientAspect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var widthField = typeof(Ingredient).GetField("gridWidth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var heightField = typeof(Ingredient).GetField("gridHeight", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var expansionField = typeof(Ingredient).GetField("unlocksAdditionalSpace", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var expansionCountField = typeof(Ingredient).GetField("additionalSpaceCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                itemNameField?.SetValue(ingredient, config.name);
                aspectField?.SetValue(ingredient, config.aspect);
                widthField?.SetValue(ingredient, config.width);
                heightField?.SetValue(ingredient, config.height);
                expansionField?.SetValue(ingredient, config.expansion);
                expansionCountField?.SetValue(ingredient, config.expansion ? 2 : 0);
                
                testIngredients.Add(ingredient);
            }
            
            sampleIngredients = testIngredients;
            Debug.Log($"  ✅ Created {testIngredients.Count} test ingredients");
        }
        
        private void CreateTestRecipes()
        {
            var testRecipes = new List<AlchemyRecipe>();
            
            if (sampleIngredients.Count >= 3)
            {
                var recipe = ScriptableObject.CreateInstance<AlchemyRecipe>();
                
                // Set basic recipe properties
                var nameField = typeof(Item).GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var input1Field = typeof(AlchemyRecipe).GetField("inputIngredient1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var input2Field = typeof(AlchemyRecipe).GetField("inputIngredient2", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var input3Field = typeof(AlchemyRecipe).GetField("inputIngredient3", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var isKeyField = typeof(AlchemyRecipe).GetField("isKeyRecipe", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                nameField?.SetValue(recipe, "Test Healing Potion");
                input1Field?.SetValue(recipe, sampleIngredients[0]);
                input2Field?.SetValue(recipe, sampleIngredients[1]);
                input3Field?.SetValue(recipe, sampleIngredients[2]);
                isKeyField?.SetValue(recipe, false);
                
                testRecipes.Add(recipe);
            }
            
            sampleRecipes = testRecipes;
            Debug.Log($"  ✅ Created {testRecipes.Count} test recipes");
        }
#endif
        
        private void OnValidate()
        {
            // Ensure we have the required references
            if (gridController == null)
            {
                gridController = GetComponent<GridMinigameController>();
            }
            
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }
        }
        
        void OnGUI()
        {
            if (!enableQuickTests) return;
            
            // Display quick test instructions in game view
            var oldColor = GUI.color;
            GUI.color = Color.white;
            
            var rect = new Rect(10, 10, 300, 120);
            GUI.Box(rect, "");
            
            var style = new GUIStyle(GUI.skin.label);
            style.fontSize = 10;
            style.normal.textColor = Color.white;
            
            GUI.Label(new Rect(15, 15, 290, 110), 
                $"Tetris Alchemy Quick Tests:\n" +
                $"{testBasicPlacement} - Basic Placement\n" +
                $"{testInteractions} - Test Interactions\n" +
                $"{testExpansion} - Test Expansion\n" +
                $"{testSkills} - Test Skills\n" +
                $"{resetSystem} - Reset System", style);
            
            GUI.color = oldColor;
        }
    }
}