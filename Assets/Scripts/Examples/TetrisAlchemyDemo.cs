using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.Examples
{
    /// <summary>
    /// Demonstrates the enhanced Tetris-like alchemy system features
    /// </summary>
    public class TetrisAlchemyDemo : MonoBehaviour
    {
        [Header("Demo Components")]
        [SerializeField] private GridMinigameController gridController;
        [SerializeField] private AlchemySkillSystem skillSystem;
        
        [Header("Sample Ingredients")]
        [SerializeField] private List<Ingredient> testIngredients = new List<Ingredient>();
        [SerializeField] private List<AlchemyRecipe> testRecipes = new List<AlchemyRecipe>();
        
        [Header("Demo Settings")]
        [SerializeField] private bool enableAutoDemo = false;
        [SerializeField] private float demoDuration = 30f;
        
        private void Start()
        {
            if (gridController == null)
                gridController = FindFirstObjectByType<GridMinigameController>();
                
            if (skillSystem == null)
                skillSystem = AlchemySkillSystem.Instance;
            
            if (enableAutoDemo)
            {
                StartAutomaticDemo();
            }
            
            LogInstructions();
        }
        
        private void Update()
        {
            HandleDemoInputs();
        }
        
        private void HandleDemoInputs()
        {
            // Demo controls
            if (Input.GetKeyDown(KeyCode.Alpha1))
                DemoBasicTetrisPlacement();
                
            if (Input.GetKeyDown(KeyCode.Alpha2))
                DemoIngredientInteractions();
                
            if (Input.GetKeyDown(KeyCode.Alpha3))
                DemoGridExpansion();
                
            if (Input.GetKeyDown(KeyCode.Alpha4))
                DemoOverlapping();
                
            if (Input.GetKeyDown(KeyCode.Alpha5))
                DemoKeyRecipe();
                
            if (Input.GetKeyDown(KeyCode.Alpha6))
                DemoSyntheticCreation();
                
            if (Input.GetKeyDown(KeyCode.Alpha7))
                DemoSkillProgression();
                
            if (Input.GetKeyDown(KeyCode.R))
                ResetDemo();
        }
        
        private void LogInstructions()
        {
            Debug.Log("🎮 Tetris Alchemy System Demo Controls:");
            Debug.Log("  1 - Basic Tetris Placement");
            Debug.Log("  2 - Ingredient Interactions");
            Debug.Log("  3 - Grid Expansion");
            Debug.Log("  4 - Overlapping (with skills)");
            Debug.Log("  5 - Key Recipe (story mode)");
            Debug.Log("  6 - Synthetic Creation (failures)");
            Debug.Log("  7 - Skill Progression");
            Debug.Log("  R - Reset Demo");
        }
        
        [ContextMenu("Demo Basic Tetris Placement")]
        public void DemoBasicTetrisPlacement()
        {
            Debug.Log("🧩 DEMO: Basic Tetris Placement");
            
            if (gridController == null || testIngredients.Count == 0)
            {
                Debug.LogWarning("Grid controller or test ingredients not assigned!");
                return;
            }
            
            // Set up basic ingredients with different shapes
            var ingredients = new List<Ingredient>();
            
            // Create sample ingredients with different grid sizes
            for (int i = 0; i < Mathf.Min(3, testIngredients.Count); i++)
            {
                ingredients.Add(testIngredients[i]);
                Debug.Log($"  Added {testIngredients[i].ItemName} ({testIngredients[i].GridWidth}x{testIngredients[i].GridHeight})");
            }
            
            gridController.SetAvailableIngredients(ingredients);
            
            Debug.Log("📋 Instructions:");
            Debug.Log("  - Click ingredients to select");
            Debug.Log("  - Click grid to place (like Tetris blocks)");
            Debug.Log("  - Ctrl+Click to remove");
            Debug.Log("  - Try to fill the grid efficiently!");
        }
        
        [ContextMenu("Demo Ingredient Interactions")]
        public void DemoIngredientInteractions()
        {
            Debug.Log("🔬 DEMO: Ingredient Interactions");
            
            if (gridController == null || testIngredients.Count < 2)
            {
                Debug.LogWarning("Need at least 2 test ingredients!");
                return;
            }
            
            // Add a known interaction
            gridController.AddIngredientInteraction(
                testIngredients[0], 
                testIngredients[1], 
                1.5f, 
                "Test interaction - creates enhanced effects"
            );
            
            gridController.SetAvailableIngredients(testIngredients.Take(3).ToList());
            
            Debug.Log($"  ✨ Added interaction: {testIngredients[0].ItemName} + {testIngredients[1].ItemName}");
            Debug.Log("📋 Instructions:");
            Debug.Log("  - Place the two interacting ingredients adjacent to each other");
            Debug.Log("  - Watch for the interaction highlight effect");
            Debug.Log("  - Interactions boost potion effects!");
        }
        
        [ContextMenu("Demo Grid Expansion")]
        public void DemoGridExpansion()
        {
            Debug.Log("🔧 DEMO: Grid Expansion");
            
            // Find or create an ingredient that unlocks additional space
            var expansionIngredient = testIngredients.FirstOrDefault(i => i.UnlocksAdditionalSpace);
            
            if (expansionIngredient == null)
            {
                Debug.LogWarning("No expansion ingredients found. Create an ingredient with 'UnlocksAdditionalSpace' checked.");
                return;
            }
            
            var ingredients = new List<Ingredient> { expansionIngredient };
            if (testIngredients.Count > 1)
                ingredients.AddRange(testIngredients.Where(i => i != expansionIngredient).Take(2));
            
            gridController.SetAvailableIngredients(ingredients);
            
            Debug.Log($"  🎯 Place {expansionIngredient.ItemName} to expand the grid");
            Debug.Log($"  📈 Will add {expansionIngredient.AdditionalSpaceCount} cells");
            Debug.Log("📋 Instructions:");
            Debug.Log("  - Place the expansion ingredient first");
            Debug.Log("  - Grid will automatically grow");
            Debug.Log("  - Now you have more space for other ingredients!");
        }
        
        [ContextMenu("Demo Overlapping")]
        public void DemoOverlapping()
        {
            Debug.Log("🎪 DEMO: Overlapping Placement");
            
            if (skillSystem == null)
            {
                Debug.LogWarning("Skill system not available!");
                return;
            }
            
            // Unlock overlapping skill
            skillSystem.UnlockSkill("overlap_placement");
            gridController.ApplySkillBonuses();
            
            gridController.SetAvailableIngredients(testIngredients.Take(4).ToList());
            
            Debug.Log("  🎓 Overlap placement skill unlocked!");
            Debug.Log("📋 Instructions:");
            Debug.Log("  - Now you can place ingredients on top of each other");
            Debug.Log("  - Limited to 3 overlapping tiles per ingredient");
            Debug.Log("  - Overlapping creates powerful combinations!");
        }
        
        [ContextMenu("Demo Key Recipe")]
        public void DemoKeyRecipe()
        {
            Debug.Log("🔑 DEMO: Key Recipe (Story Mode)");
            
            if (testRecipes.Count == 0)
            {
                Debug.LogWarning("No test recipes available!");
                return;
            }
            
            var keyRecipe = testRecipes.FirstOrDefault(r => r.IsKeyRecipe);
            if (keyRecipe == null)
            {
                Debug.LogWarning("No key recipes found. Set 'IsKeyRecipe' to true on a test recipe.");
                return;
            }
            
            gridController.SetRecipeAsKeyRecipe(true);
            gridController.SetAvailableIngredients(testIngredients.Take(3).ToList());
            
            Debug.Log($"  🎭 Selected key recipe: {keyRecipe.ItemName}");
            Debug.Log("📋 Key Recipe Features:");
            Debug.Log("  - Cannot fail (prevents frustration)");
            Debug.Log("  - Specific positioning requirements");
            Debug.Log("  - Will warn if arrangement 'feels off'");
            Debug.Log("  - No synthetic ingredients on failure");
        }
        
        [ContextMenu("Demo Synthetic Creation")]
        public void DemoSyntheticCreation()
        {
            Debug.Log("⚗️ DEMO: Synthetic Ingredient Creation");
            
            if (gridController == null)
            {
                Debug.LogWarning("Grid controller not available!");
                return;
            }
            
            // Set up ingredients that will conflict
            var conflictingIngredients = new List<Ingredient>();
            
            // Try to find fire and ice aspects
            var fireIngredient = testIngredients.FirstOrDefault(i => i.IngredientAspect == Aspect.Scorch);
            var iceIngredient = testIngredients.FirstOrDefault(i => i.IngredientAspect == Aspect.Frigid);
            
            if (fireIngredient != null) conflictingIngredients.Add(fireIngredient);
            if (iceIngredient != null) conflictingIngredients.Add(iceIngredient);
            
            // Add a third ingredient
            var otherIngredient = testIngredients.FirstOrDefault(i => i != fireIngredient && i != iceIngredient);
            if (otherIngredient != null) conflictingIngredients.Add(otherIngredient);
            
            gridController.SetAvailableIngredients(conflictingIngredients);
            
            Debug.Log("  💥 Set up conflicting ingredients (fire + ice)");
            Debug.Log("📋 Instructions:");
            Debug.Log("  - Place conflicting aspects together");
            Debug.Log("  - Attempt to craft (will 'fail')");
            Debug.Log("  - System creates synthetic ingredient instead");
            Debug.Log("  - Synthetics can be used in other recipes!");
        }
        
        [ContextMenu("Demo Skill Progression")]
        public void DemoSkillProgression()
        {
            Debug.Log("🎓 DEMO: Skill Progression System");
            
            if (skillSystem == null)
            {
                Debug.LogWarning("Skill system not available!");
                return;
            }
            
            // Unlock various skills
            Debug.Log("  Unlocking alchemy skills...");
            
            skillSystem.UnlockSkill("enhanced_grid");
            Debug.Log("    ✅ Enhanced Grid: Increases grid size");
            
            skillSystem.UnlockSkill("overlap_placement");
            Debug.Log("    ✅ Overlap Placement: Allow ingredients to overlap");
            
            skillSystem.UnlockSkill("ingredient_refund");
            Debug.Log("    ✅ Ingredient Refund: Random chance to retain ingredients");
            
            // Apply bonuses to grid
            gridController.ApplySkillBonuses();
            
            Debug.Log("📋 Skill Benefits:");
            Debug.Log("  - Enhanced Grid: 4x4 instead of 3x3");
            Debug.Log("  - Overlap: Stack up to 3 tiles deep");
            Debug.Log("  - Refund: Keep some ingredients after crafting");
            Debug.Log("  - Skills unlock through S-rank achievements!");
        }
        
        [ContextMenu("Reset Demo")]
        public void ResetDemo()
        {
            Debug.Log("🔄 Resetting demo...");
            
            if (skillSystem != null)
            {
                skillSystem.ResetSkillData();
            }
            
            if (gridController != null)
            {
                gridController.SetRecipeAsKeyRecipe(false);
                gridController.ApplySkillBonuses();
            }
            
            Debug.Log("✅ Demo reset complete");
        }
        
        private void StartAutomaticDemo()
        {
            Debug.Log("🤖 Starting automatic demo sequence...");
            
            InvokeRepeating(nameof(CycleDemoFeatures), 2f, demoDuration / 7f);
        }
        
        private int currentDemoStep = 0;
        private void CycleDemoFeatures()
        {
            switch (currentDemoStep % 7)
            {
                case 0: DemoBasicTetrisPlacement(); break;
                case 1: DemoIngredientInteractions(); break;
                case 2: DemoGridExpansion(); break;
                case 3: DemoOverlapping(); break;
                case 4: DemoKeyRecipe(); break;
                case 5: DemoSyntheticCreation(); break;
                case 6: DemoSkillProgression(); break;
            }
            
            currentDemoStep++;
            
            if (currentDemoStep >= 14) // Two full cycles
            {
                CancelInvoke(nameof(CycleDemoFeatures));
                Debug.Log("🎬 Automatic demo sequence complete!");
            }
        }
        
        private void OnDestroy()
        {
            CancelInvoke();
        }

#if UNITY_EDITOR
        [ContextMenu("Create Sample Test Data")]
        private void CreateSampleTestData()
        {
            Debug.Log("🛠️ Creating sample test data for Tetris Alchemy Demo...");
            
            // This would create sample ingredients and recipes with proper configurations
            // In a real implementation, you'd create ScriptableObject assets
            
            Debug.Log("💡 To set up demo properly:");
            Debug.Log("  1. Create test ingredients with different grid sizes");
            Debug.Log("  2. Set some ingredients to unlock additional space");
            Debug.Log("  3. Create recipes with different aspects");
            Debug.Log("  4. Mark some recipes as key recipes");
            Debug.Log("  5. Assign all to this demo script");
        }
#endif
    }
}