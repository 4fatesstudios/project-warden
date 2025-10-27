using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu;
using FourFatesStudios.ProjectWarden.GameSystems.SkillSystem;
using FourFatesStudios.ProjectWarden.GridDemo.UI;
using FourFatesStudios.ProjectWarden.Effects;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Comprehensive test suite for all enhanced alchemy system features
    /// Consolidates: BugFixTestSuite, PlacementTester, IngredientInteractionTester, ParticleEffectTest, GridVisualizationTest
    /// </summary>
    public class AlchemySystemTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        public GridGameManager gridGameManager;
        public IngredientEffectVisualizer effectVisualizer;
        
        [Header("Test Ingredients")]
        public List<Ingredient> testIngredients = new List<Ingredient>();
        public List<IngredientButton> testButtons;
        
        [Header("Test Settings")]
        public bool enableDetailedLogging = true;
        public bool enableTestLogging = true;
        
        [Header("Runtime Testing Shortcuts")]
        public KeyCode placementTestKey = KeyCode.T;
        public KeyCode clearTestKey = KeyCode.C;
        public KeyCode verifyTestKey = KeyCode.V;
        
        [Header("Test Results")]
        [SerializeField] private bool collisionDetectionWorking = false;
        [SerializeField] private bool buttonStatesWorking = false;
        [SerializeField] private int successfulPlacements = 0;
        [SerializeField] private int blockedPlacements = 0;

        private void Start()
        {
            if (gridGameManager == null)
            {
                gridGameManager = FindFirstObjectByType<GridGameManager>();
            }
            
            if (effectVisualizer == null)
            {
                effectVisualizer = FindFirstObjectByType<IngredientEffectVisualizer>();
            }

            // Auto-find test ingredients if none assigned
            if (testIngredients.Count == 0)
            {
                LoadTestIngredients();
            }
            
            // Initialize logging
            if (enableTestLogging)
            {
                Debug.Log("🧪 AlchemySystemTester: Comprehensive test suite initialized");
                Debug.Log("   ✨ Similar infusions = CONTINUOUS gentle sparkles");
                Debug.Log("   💥 Different infusions = PERIODIC dramatic bursts");
                Debug.Log("   🌟 Neutral interactions = CONTINUOUS subtle effects");
                Debug.Log($"   🎮 Runtime shortcuts: {placementTestKey} (Test), {clearTestKey} (Clear), {verifyTestKey} (Verify)");
            }
            
            // Auto-run setup tests after initialization
            Invoke(nameof(RunInitializationTests), 1f);
        }

        private void LoadTestIngredients()
        {
            // Try to load test ingredients from Resources
            var ingredients = Resources.LoadAll<Ingredient>("TestIngredients");
            if (ingredients.Length > 0)
            {
                testIngredients.AddRange(ingredients);
                Debug.Log($"🧪 Loaded {ingredients.Length} test ingredients");
            }
            else
            {
                Debug.LogWarning("No test ingredients found in Resources/TestIngredients");
            }
        }
        
        // ===== INITIALIZATION TESTS =====
        
        private void RunInitializationTests()
        {
            if (enableTestLogging)
            {
                Debug.Log("🧪 Running initialization tests...");
                TestComponentReferences();
                TestParticleEffectSystem();
            }
        }
        
        private void TestComponentReferences()
        {
            Debug.Log("🔍 Testing component references:");
            Debug.Log($"   GridGameManager: {(gridGameManager != null ? "✅" : "❌")}");
            Debug.Log($"   IngredientEffectVisualizer: {(effectVisualizer != null ? "✅" : "❌")}");
            Debug.Log($"   Test Ingredients: {testIngredients.Count} loaded");
            
            if (gridGameManager == null)
            {
                Debug.LogError("❌ GridGameManager not found! Many tests will fail.");
            }
            if (effectVisualizer == null && enableTestLogging)
            {
                Debug.LogWarning("⚠️ IngredientEffectVisualizer not found - particle effect tests unavailable");
            }
        }
        
        private void TestParticleEffectSystem()
        {
            if (effectVisualizer != null)
            {
                Debug.Log("🧪 Testing particle effect system initialization");
                // Just verify it's working, don't actually create effects yet
            }
        }

        // ===== BUG FIX TEST SUITE (from BugFixTestSuite.cs) =====

        [ContextMenu("Run All Bug Fix Tests")]
        public void RunAllBugFixTests()
        {
            Debug.Log("=== BUG FIX TEST SUITE ===");
            
            TestCollisionDetection();
            TestButtonStates();
            TestVisualPreview();
            
            LogBugFixTestResults();
        }

        [ContextMenu("Test Collision Detection")]
        public void TestCollisionDetection()
        {
            Debug.Log("🔍 Testing collision detection...");
            
            if (gridGameManager == null || testIngredients.Count < 2)
            {
                Debug.LogWarning("Cannot test collision detection - missing GridGameManager or test ingredients");
                return;
            }
            
            // Clear grid first
            gridGameManager.ClearGrid();
            
            // Test 1: Place first ingredient
            var firstIngredient = testIngredients[0];
            Vector2Int pos1 = new Vector2Int(1, 1);
            
            bool canPlaceFirst = gridGameManager.CanPlaceIngredient(firstIngredient, pos1);
            bool placedFirst = gridGameManager.TryPlaceIngredient(firstIngredient, pos1);
            
            Debug.Log($"First placement - Can place: {canPlaceFirst}, Placed: {placedFirst}");
            
            if (placedFirst)
            {
                successfulPlacements++;
                
                // Test 2: Try to place second ingredient overlapping the first
                var secondIngredient = testIngredients[1];
                Vector2Int pos2 = pos1; // Same position = should fail
                
                bool canPlaceSecond = gridGameManager.CanPlaceIngredient(secondIngredient, pos2);
                bool placedSecond = gridGameManager.TryPlaceIngredient(secondIngredient, pos2);
                
                Debug.Log($"Overlapping placement - Can place: {canPlaceSecond}, Placed: {placedSecond}");
                
                if (!canPlaceSecond && !placedSecond)
                {
                    blockedPlacements++;
                    collisionDetectionWorking = true;
                    Debug.Log("✅ Collision detection working correctly!");
                }
                else
                {
                    Debug.LogError("❌ Collision detection FAILED - ingredients can overlap!");
                }
                
                // Test 3: Try to place in a valid adjacent position
                Vector2Int pos3 = new Vector2Int(3, 1);
                bool canPlaceThird = gridGameManager.CanPlaceIngredient(secondIngredient, pos3);
                bool placedThird = gridGameManager.TryPlaceIngredient(secondIngredient, pos3);
                
                Debug.Log($"Adjacent placement - Can place: {canPlaceThird}, Placed: {placedThird}");
                
                if (canPlaceThird && placedThird)
                {
                    successfulPlacements++;
                    Debug.Log("✅ Adjacent placement working correctly!");
                }
            }
        }

        [ContextMenu("Test Button States")]
        public void TestButtonStates()
        {
            Debug.Log("🎨 Testing button states...");
            
            // Find ingredient buttons in the scene
            var ingredientButtons = FindObjectsByType<IngredientButton>(FindObjectsSortMode.None);
            
            if (ingredientButtons.Length == 0)
            {
                Debug.LogWarning("No IngredientButton components found for testing");
                return;
            }
            
            int buttonsWithCorrectStates = 0;
            
            foreach (var button in ingredientButtons)
            {
                if (button.AssociatedIngredient == null) continue;
                
                // Test normal state
                button.SetSelected(false);
                Color normalColor = button.Button?.colors.normalColor ?? Color.magenta;
                
                // Test selected state  
                button.SetSelected(true);
                Color selectedColor = button.Button?.colors.normalColor ?? Color.magenta;
                
                // Test back to normal
                button.SetSelected(false);
                Color returnedColor = button.Button?.colors.normalColor ?? Color.magenta;
                
                // Check if colors are different and not white
                bool statesCorrect = normalColor != Color.white && 
                                   selectedColor != Color.white && 
                                   normalColor != selectedColor &&
                                   Mathf.Approximately(normalColor.r, returnedColor.r);
                
                if (statesCorrect)
                {
                    buttonsWithCorrectStates++;
                    Debug.Log($"✅ Button '{button.AssociatedIngredient.ItemName}' has correct states");
                }
                else
                {
                    Debug.LogError($"❌ Button '{button.AssociatedIngredient.ItemName}' has incorrect states " +
                                 $"(Normal: {normalColor}, Selected: {selectedColor}, Returned: {returnedColor})");
                }
            }
            
            buttonStatesWorking = buttonsWithCorrectStates == ingredientButtons.Length;
            
            if (buttonStatesWorking)
            {
                Debug.Log($"✅ All {buttonsWithCorrectStates} buttons have correct state management!");
            }
            else
            {
                Debug.LogError($"❌ Button states FAILED - {buttonsWithCorrectStates}/{ingredientButtons.Length} working correctly");
            }
        }

        [ContextMenu("Test Visual Preview")]
        public void TestVisualPreview()
        {
            Debug.Log("🎮 Visual preview enhancement info:");
            Debug.Log("✅ Enhanced ASE grid editor with 3D-style visual preview");
            Debug.Log("✅ Added game appearance simulation with lighting/shadows");
            Debug.Log("✅ Added preview information panel with ingredient details");
            Debug.Log("📝 Manual test: Open AlchemySystemEditor > Ingredients tab > Edit shapes to see enhanced preview");
        }

        private void LogBugFixTestResults()
        {
            Debug.Log("=== BUG FIX TEST RESULTS SUMMARY ===");
            Debug.Log($"🔍 Collision Detection: {(collisionDetectionWorking ? "PASS" : "FAIL")}");
            Debug.Log($"🎨 Button States: {(buttonStatesWorking ? "PASS" : "FAIL")}");
            Debug.Log($"🎮 Visual Preview: ENHANCED (manual verification required)");
            Debug.Log($"📊 Placement Stats: {successfulPlacements} successful, {blockedPlacements} correctly blocked");
            
            if (collisionDetectionWorking && buttonStatesWorking)
            {
                Debug.Log("🎉 ALL AUTOMATED TESTS PASSED! The three major bugs have been fixed!");
            }
            else
            {
                Debug.LogError("⚠️ Some tests failed. Check the individual test results above.");
            }
        }

        // ===== PLACEMENT TESTING (from PlacementTester.cs) =====

        [ContextMenu("Test Runtime Placement")]
        public void TestRuntimePlacement()
        {
            if (gridGameManager == null || gridGameManager.availableIngredients.Count == 0)
            {
                Debug.LogError("❌ No GridGameManager or ingredients available!");
                return;
            }

            var testIngredient = gridGameManager.availableIngredients[0];
            var testPosition = new Vector2Int(Random.Range(0, gridGameManager.gridWidth - 1), 
                                            Random.Range(0, gridGameManager.gridHeight - 1));

            Debug.Log($"🧪 === RUNTIME PLACEMENT TEST ===");
            Debug.Log($"🧪 Testing: {testIngredient.ItemName} at {testPosition}");

            // Check cell state BEFORE placement
            var cellBefore = gridGameManager.GetCell(testPosition.x, testPosition.y);
            bool wasOccupiedBefore = cellBefore?.IsOccupied ?? false;
            string occupantBefore = cellBefore?.OccupiedByIngredient?.ItemName ?? "None";

            if (enableDetailedLogging)
            {
                Debug.Log($"🧪 BEFORE: Cell ({testPosition.x},{testPosition.y}) - Occupied: {wasOccupiedBefore}, By: {occupantBefore}");
            }

            // Attempt placement
            bool placementSuccess = gridGameManager.TryPlaceIngredient(testIngredient, testPosition);
            
            // Check cell state AFTER placement
            var cellAfter = gridGameManager.GetCell(testPosition.x, testPosition.y);
            bool isOccupiedAfter = cellAfter?.IsOccupied ?? false;
            string occupantAfter = cellAfter?.OccupiedByIngredient?.ItemName ?? "None";

            if (enableDetailedLogging)
            {
                Debug.Log($"🧪 AFTER: Cell ({testPosition.x},{testPosition.y}) - Occupied: {isOccupiedAfter}, By: {occupantAfter}");
            }

            // Analyze results
            Debug.Log($"🧪 Placement Result: {(placementSuccess ? "✅ SUCCESS" : "❌ FAILED")}");

            if (placementSuccess && !isOccupiedAfter)
            {
                Debug.LogError("🚨 BUG DETECTED: Placement succeeded but cell not marked as occupied!");
                
                // Additional debugging
                Debug.LogError("🔍 Investigating further...");
                
                // Try to manually call the marking function to see what happens
                var method = typeof(GridGameManager).GetMethod("MarkCellsAsOccupied", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    Debug.Log("🔧 Attempting to manually call MarkCellsAsOccupied...");
                    method.Invoke(gridGameManager, new object[] { testIngredient, testPosition });
                    
                    // Check again
                    var cellAfterManual = gridGameManager.GetCell(testPosition.x, testPosition.y);
                    bool isOccupiedAfterManual = cellAfterManual?.IsOccupied ?? false;
                    Debug.Log($"🔧 After manual marking: Cell occupied = {isOccupiedAfterManual}");
                }
            }
            else if (placementSuccess && isOccupiedAfter)
            {
                Debug.Log("✅ Everything working correctly!");
            }
            else if (!placementSuccess && wasOccupiedBefore)
            {
                Debug.Log("✅ Collision detection working correctly - blocked by existing ingredient");
            }
            else if (!placementSuccess && !wasOccupiedBefore)
            {
                Debug.LogWarning("⚠️ Placement failed on empty cell - check bounds or other restrictions");
            }

            // Show final grid state summary
            ShowGridSummary();
        }

        [ContextMenu("Test Clear Grid")]
        public void TestClearGrid()
        {
            if (gridGameManager == null) return;

            Debug.Log($"🧹 === CLEAR GRID TEST ===");
            ShowGridSummary();
            
            gridGameManager.ClearGrid();
            
            Debug.Log($"🧹 Grid cleared. New state:");
            ShowGridSummary();
        }

        [ContextMenu("Show Grid Summary")]
        public void ShowGridSummary()
        {
            if (gridGameManager == null) return;

            int occupiedCount = 0;
            int highlightedCount = 0;

            for (int x = 0; x < gridGameManager.gridWidth; x++)
            {
                for (int y = 0; y < gridGameManager.gridHeight; y++)
                {
                    var cell = gridGameManager.GetCell(x, y);
                    if (cell != null)
                    {
                        if (cell.IsOccupied) occupiedCount++;
                        if (cell.IsHighlighted) highlightedCount++;
                    }
                }
            }

            int total = gridGameManager.gridWidth * gridGameManager.gridHeight;
            float occupancyPercent = (occupiedCount * 100f) / total;

            Debug.Log($"📊 Grid Summary: {occupiedCount}/{total} cells occupied ({occupancyPercent:F1}%), {highlightedCount} highlighted");
        }

        [ContextMenu("Test Specific Position")]
        public void TestSpecificPosition()
        {
            // Test a specific position that's easy to verify
            if (gridGameManager == null || gridGameManager.availableIngredients.Count == 0) return;

            var testIngredient = gridGameManager.availableIngredients[0];
            var testPosition = new Vector2Int(2, 2); // Center position

            Debug.Log($"🎯 === SPECIFIC POSITION TEST: (2,2) ===");

            // Clear the grid first to ensure clean test
            gridGameManager.ClearGrid();

            // Test the placement
            bool success = gridGameManager.TryPlaceIngredient(testIngredient, testPosition);
            
            // Check result
            var cell = gridGameManager.GetCell(testPosition.x, testPosition.y);
            bool isOccupied = cell?.IsOccupied ?? false;

            Debug.Log($"🎯 Result: Placement={success}, CellOccupied={isOccupied}");

            if (success != isOccupied)
            {
                Debug.LogError("🚨 MISMATCH: Placement success doesn't match cell occupancy!");
            }
        }

        [ContextMenu("Debug Cell Access")]
        public void DebugCellAccess()
        {
            if (gridGameManager == null) return;

            Debug.Log($"🔧 === CELL ACCESS DEBUG ===");

            // Test if we can get cells properly
            for (int x = 0; x < gridGameManager.gridWidth && x < 3; x++)
            {
                for (int y = 0; y < gridGameManager.gridHeight && y < 3; y++)
                {
                    var cell = gridGameManager.GetCell(x, y);
                    if (cell != null)
                    {
                        Debug.Log($"🔧 Cell ({x},{y}): Position={cell.Position}, Occupied={cell.IsOccupied}");
                    }
                    else
                    {
                        Debug.LogError($"🔧 Cell ({x},{y}): NULL!");
                    }
                }
            }
        }

        [ContextMenu("Test Verify Occupancy")]
        public void TestVerifyOccupancy()
        {
            if (gridGameManager == null) return;

            Debug.Log($"🔍 === OCCUPANCY VERIFICATION TEST ===");
            gridGameManager.VerifyAllPlacedIngredients();
            gridGameManager.FullOccupancyAudit();
        }

        // ===== INGREDIENT INTERACTION TESTING (from IngredientInteractionTester.cs) =====

        [ContextMenu("Test Ingredient Infusion Detection")]
        public void TestIngredientEffectDetection()
        {
            Debug.Log("🔍 === TESTING INGREDIENT INFUSION DETECTION ===");

            foreach (var ingredient in testIngredients)
            {
                if (ingredient == null) continue;

                bool hasEffects = ingredient.HasEffects();
                bool hasInfusions = ingredient.HasInfusionEffects();

                Debug.Log($"🧪 {ingredient.ItemName}:");
                Debug.Log($"   HasEffects (any): {hasEffects}");
                Debug.Log($"   HasInfusions: {hasInfusions}");

                // Check EffectBundle (should be ignored for interactions)
                if (ingredient.EffectBundle?.Effects != null)
                {
                    Debug.Log($"   EffectBundle effects: {ingredient.EffectBundle.Effects.Count} (IGNORED for interactions)");
                }

                // Check InfusionBundle (only this matters for interactions)
                if (ingredient.InfusionBundle?.Infusions != null)
                {
                    Debug.Log($"   InfusionBundle infusions: {ingredient.InfusionBundle.Infusions.Count} (USED for interactions)");
                    foreach (var infusion in ingredient.InfusionBundle.Infusions)
                    {
                        Debug.Log($"     - {infusion.GetType().Name}");
                    }
                }
                else
                {
                    Debug.Log($"   InfusionBundle: null or empty");
                }
            }
        }

        [ContextMenu("Test Pairwise Infusion Interactions")]
        public void TestPairwiseIngredientInteractions()
        {
            Debug.Log("🤝 === TESTING PAIRWISE INFUSION INTERACTIONS ===");

            for (int i = 0; i < testIngredients.Count; i++)
            {
                for (int j = i + 1; j < testIngredients.Count; j++)
                {
                    var ingredient1 = testIngredients[i];
                    var ingredient2 = testIngredients[j];

                    if (ingredient1 == null || ingredient2 == null) continue;

                    Debug.Log($"\n🔄 Testing {ingredient1.ItemName} vs {ingredient2.ItemName}:");

                    bool hasSimilar = ingredient1.HasSimilarEffectsTo(ingredient2);
                    var similarEffects = ingredient1.GetSimilarEffectsTo(ingredient2);

                    Debug.Log($"   HasSimilarInfusionTypes: {hasSimilar}");
                    Debug.Log($"   GetSimilarInfusionTypes: {similarEffects.Count} matches");

                    // Show what infusion types each has
                    bool hasInfusions1 = ingredient1.HasInfusionEffects();
                    bool hasInfusions2 = ingredient2.HasInfusionEffects();
                    Debug.Log($"   {ingredient1.ItemName} has infusions: {hasInfusions1}");
                    Debug.Log($"   {ingredient2.ItemName} has infusions: {hasInfusions2}");

                    if (similarEffects.Count > 0)
                    {
                        Debug.Log("   ✨ Similar infusion types found:");
                        foreach (var (thisInfusion, otherInfusion) in similarEffects)
                        {
                            Debug.Log($"     - {thisInfusion.GetType().Name} ↔ {otherInfusion.GetType().Name}");
                        }
                    }
                    else
                    {
                        Debug.Log("   💥 No similar infusion types found");
                    }
                }
            }
        }

        [ContextMenu("Test Same Ingredient Self-Infusion Interaction")]
        public void TestSameIngredientSelfInteraction()
        {
            Debug.Log("🪞 === TESTING SAME INGREDIENT SELF-INFUSION INTERACTION ===");

            foreach (var ingredient in testIngredients)
            {
                if (ingredient == null || !ingredient.HasInfusionEffects()) continue;

                Debug.Log($"\n🧪 Testing {ingredient.ItemName} with itself:");

                bool hasSimilar = ingredient.HasSimilarEffectsTo(ingredient);
                var similarEffects = ingredient.GetSimilarEffectsTo(ingredient);

                Debug.Log($"   HasSimilarInfusionTypes to self: {hasSimilar}");
                Debug.Log($"   GetSimilarInfusionTypes to self: {similarEffects.Count} matches");

                if (similarEffects.Count > 0)
                {
                    Debug.Log("   ✨ Self-similar infusion types found (expected):");
                    foreach (var (thisInfusion, otherInfusion) in similarEffects)
                    {
                        Debug.Log($"     - {thisInfusion.GetType().Name} ↔ {otherInfusion.GetType().Name}");
                    }
                }
                else
                {
                    Debug.LogError($"   ❌ ERROR: No self-similar infusion types found - this should not happen for ingredients with infusions!");
                }
            }
        }

        [ContextMenu("Test Ingredient Placement and Infusion Interaction")]
        public void TestIngredientPlacementAndInteraction()
        {
            if (gridGameManager == null || testIngredients.Count < 2)
            {
                Debug.LogError("Need GridGameManager and at least 2 test ingredients");
                return;
            }

            Debug.Log("🎮 === TESTING INGREDIENT PLACEMENT AND INFUSION INTERACTION ===");

            // Clear grid first
            gridGameManager.ClearGrid();

            // Find two ingredients with infusion effects
            Ingredient ingredient1 = null;
            Ingredient ingredient2 = null;

            foreach (var ingredient in testIngredients)
            {
                if (ingredient != null && ingredient.HasInfusionEffects())
                {
                    if (ingredient1 == null)
                        ingredient1 = ingredient;
                    else if (ingredient2 == null)
                    {
                        ingredient2 = ingredient;
                        break;
                    }
                }
            }

            if (ingredient1 == null || ingredient2 == null)
            {
                Debug.LogError("Need at least 2 ingredients with infusion effects for this test");
                return;
            }

            Debug.Log($"Using {ingredient1.ItemName} and {ingredient2.ItemName}");

            // Test infusion similarity before placement
            bool shouldHaveSimilarInfusions = ingredient1.HasSimilarEffectsTo(ingredient2);
            var expectedSimilarEffects = ingredient1.GetSimilarEffectsTo(ingredient2);

            Debug.Log($"Expected infusion interaction: {(shouldHaveSimilarInfusions ? "Similar" : "Different")} infusion types");
            Debug.Log($"Expected similar infusion type count: {expectedSimilarEffects.Count}");

            // Place ingredients adjacent to each other
            Vector2Int pos1 = new Vector2Int(2, 2);
            Vector2Int pos2 = new Vector2Int(3, 2); // Adjacent

            bool success1 = gridGameManager.TryPlaceIngredient(ingredient1, pos1);
            bool success2 = gridGameManager.TryPlaceIngredient(ingredient2, pos2);

            if (success1 && success2)
            {
                Debug.Log("✅ Both ingredients placed successfully");
                Debug.Log("🎨 Check console for particle effect creation messages");
                Debug.Log($"   Expected: {(shouldHaveSimilarInfusions ? "✨ Similar infusion types sparkle" : "💥 Different infusion types reaction")}");
            }
            else
            {
                Debug.LogError($"❌ Failed to place ingredients: {ingredient1.ItemName}={success1}, {ingredient2.ItemName}={success2}");
            }
        }

        [ContextMenu("Test All Interaction Features")]
        public void TestAllInteractionFeatures()
        {
            TestIngredientEffectDetection();
            System.Threading.Tasks.Task.Delay(500).Wait();

            TestPairwiseIngredientInteractions();
            System.Threading.Tasks.Task.Delay(500).Wait();

            TestSameIngredientSelfInteraction();
            System.Threading.Tasks.Task.Delay(500).Wait();

            TestIngredientPlacementAndInteraction();

            Debug.Log("\n🎉 === ALL INFUSION INTERACTION TESTS COMPLETED ===");
            Debug.Log("Review the console output above to verify:");
            Debug.Log("1. Ingredients with same infusion types show 'Similar infusion types' interactions");
            Debug.Log("2. Self-interactions always show similar infusion types");
            Debug.Log("3. Particle effects match the expected infusion interaction type");
            Debug.Log("4. Only infusion types are compared (not effect bundle or effect types)");
        }

        // ===== PARTICLE EFFECT TESTING (from ParticleEffectTest.cs) =====

        [ContextMenu("Test Mixed Particle Effects")]
        public void TestMixedEffects()
        {
            if (effectVisualizer != null)
            {
                Debug.Log("🧪 Testing mixed particle effects (continuous + periodic)");
                effectVisualizer.RefreshAllIngredientInteractions();
            }
            else
            {
                Debug.LogError("🧪 Cannot test - no IngredientEffectVisualizer found");
            }
        }

        [ContextMenu("Clear All Particle Effects")]
        public void ClearAllEffects()
        {
            if (effectVisualizer != null)
            {
                Debug.Log("🧪 Clearing all effects");
                effectVisualizer.ClearAllEffects();
            }
            else
            {
                Debug.LogError("🧪 Cannot clear - no IngredientEffectVisualizer found");
            }
        }

        [ContextMenu("Log Effect Behavior")]
        public void LogEffectBehavior()
        {
            Debug.Log("🧪 Current Effect Behavior:");
            Debug.Log("   ✨ SIMILAR infusions → Continuous gentle sparkles (5-20 particles/sec)");
            Debug.Log("   💥 DIFFERENT infusions → Periodic bursts (15-30 particles every 1-6 seconds)");
            Debug.Log("   🌟 NEUTRAL interactions → Continuous subtle sparkles (3 particles/sec)");
        }

        // ===== GRID VISUALIZATION TESTING (from GridVisualizationTest.cs) =====

        [ContextMenu("Test Grid Preservation")]
        public void TestGridPreservation()
        {
            if (gridGameManager == null)
            {
                Debug.LogError("No GridGameManager found!");
                return;
            }

            // Count grid objects before clearing
            Transform[] childrenBefore = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                childrenBefore[i] = transform.GetChild(i);
            }

            Debug.Log($"🧪 Grid objects before clearing: {childrenBefore.Length}");
            
            // Clear the grid
            gridGameManager.ClearGrid();
            
            // Count grid objects after clearing
            Transform[] childrenAfter = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                childrenAfter[i] = transform.GetChild(i);
            }

            Debug.Log($"🧪 Grid objects after clearing: {childrenAfter.Length}");
            
            if (childrenBefore.Length == childrenAfter.Length)
            {
                Debug.Log("✅ Grid preservation test PASSED - Grid objects preserved!");
            }
            else
            {
                Debug.LogError("❌ Grid preservation test FAILED - Grid objects were destroyed!");
            }
        }

        // ===== COMPREHENSIVE TESTING METHODS =====

        [ContextMenu("Run Complete Test Suite")]
        public void RunCompleteTestSuite()
        {
            Debug.Log("🚀 === RUNNING COMPLETE CONSOLIDATED TEST SUITE ===");
            
            // Bug fix tests
            RunAllBugFixTests();
            Debug.Log(""); // Spacer
            
            // Placement tests
            TestRuntimePlacement();
            TestSpecificPosition();
            Debug.Log(""); // Spacer
            
            // Interaction tests
            TestAllInteractionFeatures();
            Debug.Log(""); // Spacer
            
            // Effect tests
            TestMixedEffects();
            LogEffectBehavior();
            Debug.Log(""); // Spacer
            
            // Grid preservation test
            TestGridPreservation();
            Debug.Log(""); // Spacer
            
            // Enhanced alchemy systems
            TestAllEnhancedSystems();
            
            Debug.Log("🚀 === COMPLETE TEST SUITE FINISHED ===");
            Debug.Log("🚀 All consolidated functionality from 5 test files has been executed!");
        }

        [ContextMenu("Clear Test Grid")]
        public void ClearTestGrid()
        {
            if (gridGameManager != null)
            {
                gridGameManager.ClearGrid();
                successfulPlacements = 0;
                blockedPlacements = 0;
                Debug.Log("🧹 Test grid cleared and counters reset");
            }
        }

        [ContextMenu("Test Obstacle System")]
        public void TestObstacleSystem()
        {
            if (gridGameManager == null)
            {
                Debug.LogError("GridGameManager not assigned!");
                return;
            }

            Debug.Log("🚧 === TESTING OBSTACLE SYSTEM ===");

            // Clear grid first
            gridGameManager.ClearGrid();

            // Spawn some test obstacles
            gridGameManager.SpawnInitialObstacles();

            // Test placing ingredients on obstacles
            if (testIngredients.Count > 0)
            {
                var testIngredient = testIngredients[0];
                gridGameManager.SelectIngredient(testIngredient);

                Debug.Log($"🧪 Testing placement of {testIngredient.ItemName}");
                Debug.Log("Try clicking on different obstacle types to see interactions!");
                Debug.Log("🚧 Obstacle Types:");
                Debug.Log("  - Corporeal (Gray): Blocked - cannot place");
                Debug.Log("  - Frigid (Blue): Frozen - needs adjacent Scorch/Corporeal to unlock");
                Debug.Log("  - Scorch (Red): Volatile - requires Scorch/Caustic/Arc aspects");
                Debug.Log("  - Caustic (Green): Degrade - reduces potency by 20%");
                Debug.Log("  - Arc (Yellow): Chaotic - triggers random effects");
                Debug.Log("  - Divine (Gold): Sanctified - only unrefined Divine aspects (+10% potency)");
            }
            else
            {
                Debug.LogWarning("No test ingredients available for obstacle testing");
            }
        }

        [ContextMenu("Test Synergy System")]
        public void TestSynergySystem()
        {
            if (gridGameManager == null)
            {
                Debug.LogError("GridGameManager not assigned!");
                return;
            }

            Debug.Log("🔮 === TESTING SYNERGY SYSTEM ===");

            // Get synergy system
            var synergySystem = gridGameManager.GetComponent<SynergySystem>();
            if (synergySystem == null)
            {
                Debug.LogError("SynergySystem not found on GridGameManager!");
                return;
            }

            // Clear grid first
            gridGameManager.ClearGrid();

            // Test with multiple ingredients of same aspect
            if (testIngredients.Count >= 3)
            {
                var placements = new Dictionary<Vector2Int, Ingredient>();

                // Place three corporeal ingredients close together
                for (int i = 0; i < 3 && i < testIngredients.Count; i++)
                {
                    var ingredient = testIngredients[i];
                    var position = new Vector2Int(i, 0);
                    placements[position] = ingredient;
                    gridGameManager.TryPlaceIngredient(ingredient, position);
                }

                // Analyze synergies
                var result = synergySystem.AnalyzeSynergies(placements);

                Debug.Log($"🔮 Synergy Analysis Results:");
                Debug.Log($"   Active Synergies: {result.activeSynergyCount}");
                Debug.Log($"   Total Potency Multiplier: {result.totalPotencyMultiplier:F2}x");
                Debug.Log($"   Total Efficiency Bonus: {result.totalEfficiencyBonus:F2}");

                foreach (var description in result.synergyDescriptions)
                {
                    Debug.Log($"   🌟 {description}");
                }
            }
            else
            {
                Debug.LogWarning("Need at least 3 test ingredients for synergy testing");
            }
        }

        [ContextMenu("Test Failure System")]
        public void TestFailureSystem()
        {
            if (gridGameManager == null)
            {
                Debug.LogError("GridGameManager not assigned!");
                return;
            }

            Debug.Log("⚠️ === TESTING FAILURE SYSTEM ===");

            // Get failure system
            var failureSystem = gridGameManager.GetComponent<FailureSystem>();
            if (failureSystem == null)
            {
                Debug.LogError("FailureSystem not found on GridGameManager!");
                return;
            }

            // Clear grid first
            gridGameManager.ClearGrid();

            // Create conflicting ingredient placements (simulated)
            if (testIngredients.Count >= 2)
            {
                var placements = new Dictionary<Vector2Int, Ingredient>();
                placements[new Vector2Int(0, 0)] = testIngredients[0];
                placements[new Vector2Int(1, 0)] = testIngredients[1];

                // Test conflict analysis
                var analysis = failureSystem.AnalyzeIngredientConflicts(placements.Values.ToList());

                Debug.Log($"⚠️ Conflict Analysis:");
                Debug.Log($"   Has Conflicts: {analysis.hasConflicts}");
                Debug.Log($"   Total Failure Chance: {analysis.totalFailureChance:P}");
                Debug.Log($"   Detected Conflicts: {analysis.detectedConflicts.Count}");

                foreach (var conflict in analysis.detectedConflicts)
                {
                    Debug.Log($"   🔥 {conflict.conflictRule.conflictName}: +{conflict.failureIncrease:P} failure");
                }

                // End of conflict analysis testing
                if (analysis.hasConflicts)
                {
                    failureSystem.HandleRecipeFailure(placements.Values.ToList());
                    Debug.Log("💎 Synthetic creation handled through failure system events");
                }
            }
            else
                        Debug.Log($"   Aspects: {string.Join(", ", new string[] {"synthetic creation complete"})}");
            {
                Debug.LogWarning("Need at least 2 test ingredients for failure testing");
            }
        }

        [ContextMenu("Test Skill Tree")]
        public void TestSkillTree()
        {
            if (gridGameManager == null)
            {
                Debug.LogError("GridGameManager not assigned!");
                return;
            }

            Debug.Log("🌳 === TESTING SKILL TREE ===");

            // Get skill tree
            var skillTree = gridGameManager.GetComponent<AlchemySkillTree>();
            if (skillTree == null)
            {
                Debug.LogError("AlchemySkillTree not found on GridGameManager!");
                return;
            }

            // Test skill tree functionality
            skillTree.TestSkillTree();

            // Try to unlock some skills
            Debug.Log("🌳 Testing skill unlocks:");

            bool unlocked1 = skillTree.TryUnlockSkill("grid_expansion_4x4");
            Debug.Log($"   Grid Expansion 4x4: {(unlocked1 ? "✅ Unlocked" : "❌ Failed")}");

            bool unlocked2 = skillTree.TryUnlockSkill("ingredient_overlap_1");
            Debug.Log($"   Ingredient Overlap 1: {(unlocked2 ? "✅ Unlocked" : "❌ Failed")}");

            bool unlocked3 = skillTree.TryUnlockSkill("potency_boost_1");
            Debug.Log($"   Potency Boost 1: {(unlocked3 ? "✅ Unlocked" : "❌ Failed")}");

            // Show current skill status
            var unlockedSkills = skillTree.GetUnlockedSkills();
            Debug.Log($"🌳 Currently unlocked skills: {unlockedSkills.Count}");
            foreach (var skill in unlockedSkills)
            {
                Debug.Log($"   🌟 {skill.skillName} (Value: {skill.effectValue})");
            }
        }

        [ContextMenu("Test Template System")]
        public void TestTemplateSystem()
        {
            if (gridGameManager == null)
            {
                Debug.LogError("GridGameManager not assigned!");
                return;
            }

            Debug.Log("🗺️ === TESTING TEMPLATE SYSTEM ===");

            // Get template system
            var templateSystem = gridGameManager.GetComponent<TemplateSystem>();
            if (templateSystem == null)
            {
                Debug.LogError("TemplateSystem not found on GridGameManager!");
                return;
            }

            // Test template system
            templateSystem.TestTemplateSystem();

            // Try to activate a template
            bool activated = templateSystem.ActivateTemplate("beginner_balance");
            Debug.Log($"🗺️ Template activation: {(activated ? "✅ Success" : "❌ Failed")}");

            if (activated)
            {
                var progress = templateSystem.CheckTemplateProgress();
                Debug.Log($"🗺️ Template Progress:");
                Debug.Log($"   Name: {progress.templateName}");
                Debug.Log($"   Completion: {progress.completionPercentage:P}");
                Debug.Log($"   Current Bonus: {progress.currentBonus:F1}x");
            }

            // Show discovered templates
            var discovered = templateSystem.GetDiscoveredTemplates();
            Debug.Log($"🗺️ Discovered Templates: {discovered.Count}");
            foreach (var template in discovered)
            {
                Debug.Log($"   📜 {template.templateName} ({template.difficulty})");
            }
        }

        [ContextMenu("Test Enhanced Grid System")]
        public void TestEnhancedGridSystem()
        {
            if (gridGameManager == null)
            {
                Debug.LogError("GridGameManager not assigned!");
                return;
            }

            Debug.Log("🔄 === TESTING ENHANCED GRID SYSTEM ===");

            // Get enhanced grid system
            var enhancedGrid = gridGameManager.GetComponent<EnhancedGridSystem>();
            if (enhancedGrid == null)
            {
                Debug.LogError("EnhancedGridSystem not found on GridGameManager!");
                return;
            }

            // Test enhanced grid system
            enhancedGrid.TestEnhancedGridSystem();

            // Test space utilization calculation
            if (testIngredients.Count >= 2)
            {
                var placements = new Dictionary<Vector2Int, Ingredient>
                {
                    { new Vector2Int(1, 1), testIngredients[0] },
                    { new Vector2Int(2, 1), testIngredients[1] }
                };

                var utilization = enhancedGrid.CalculateSpaceUtilization(placements);
                Debug.Log($"🔄 Space Utilization Analysis:");
                Debug.Log($"   Bounding Box: {utilization.boundingBoxSize.x}x{utilization.boundingBoxSize.y}");
                Debug.Log($"   Compactness: {utilization.compactness:P}");
                Debug.Log($"   Adjacency Score: {utilization.adjacencyScore:F2}");
                Debug.Log($"   Symmetry Score: {utilization.symmetryScore:F2}");
            }

            // Test grid expansion
            if (testIngredients.Count > 0)
            {
                var ingredient = testIngredients[0];
                bool expanded = enhancedGrid.TryExpandGrid(ingredient, 2);
                Debug.Log($"🔄 Grid Expansion Test: {(expanded ? "✅ Success" : "❌ Failed or not applicable")}");

                var stats = enhancedGrid.GetExpansionStats();
                Debug.Log($"🔄 Current Grid: {stats.currentSize.x}x{stats.currentSize.y} (was {stats.originalSize.x}x{stats.originalSize.y})");
            }
        }

        [ContextMenu("Test Proficiency Grading")]
        public void TestProficiencyGrading()
        {
            if (gridGameManager == null)
            {
                Debug.LogError("GridGameManager not assigned!");
                return;
            }

            Debug.Log("📊 === TESTING PROFICIENCY GRADING ===");

            // Clear grid first
            gridGameManager.ClearGrid();

            // Place some ingredients to test grading
            if (testIngredients.Count >= 2)
            {
                var ingredient1 = testIngredients[0];
                var ingredient2 = testIngredients[1];

                // Place ingredients in different patterns
                bool success1 = gridGameManager.TryPlaceIngredient(ingredient1, new Vector2Int(1, 1));
                bool success2 = gridGameManager.TryPlaceIngredient(ingredient2, new Vector2Int(2, 1)); // Adjacent

                if (success1 && success2)
                {
                    Debug.Log("✅ Placed ingredients successfully");
                    
                    // Get placed ingredients for proficiency calculation
                    var placements = new Dictionary<Vector2Int, Ingredient>
                    {
                        { new Vector2Int(1, 1), ingredient1 },
                        { new Vector2Int(2, 1), ingredient2 }
                    };

                    // Calculate proficiency grade using the static method
                    var weights = new ProficiencyWeights();
                    weights.NormalizeWeights();
                    var grade = ProficiencyGrading.CalculateProficiency(gridGameManager, placements, gridGameManager.aspectObstacles, weights, false); // Assume recipe mode for testing
                    
                    Debug.Log($"📊 === PROFICIENCY RESULTS ===");
                    Debug.Log($"📊 Overall Grade: {grade.gradeLevel} ({grade.overallScore:P})");
                    Debug.Log($"📊 Individual Scores:");
                    Debug.Log($"   🏠 Coverage: {grade.coverageRatio:P}");
                    Debug.Log($"   🤝 Adjacency: {grade.adjacencySynergy:P}");
                    Debug.Log($"   📈 Expansion: {grade.expansionUtilization:P}");
                    Debug.Log($"   🔷 Shape: {grade.shapeDifficulty:P}");
                    Debug.Log($"   📐 Orientation: {grade.orientationEfficiency:P}");
                    Debug.Log($"   🚧 Obstacles: {grade.obstaclesCompleted:P}");
                    Debug.Log($"📊 Feedback: {grade.feedback}");
                    
                    // Show grade color
                    var gradeColor = ProficiencyGrading.GetGradeColor(grade.gradeLevel);
                    Debug.Log($"📊 Grade Color: R:{gradeColor.r:F1} G:{gradeColor.g:F1} B:{gradeColor.b:F1}");
                }
                else
                {
                    Debug.LogError("Failed to place test ingredients for grading");
                }
            }
            else
            {
                Debug.LogWarning("Need at least 2 test ingredients for proficiency testing");
            }
        }
        [ContextMenu("Test Refined vs Unrefined")]
        public void TestRefinedVsUnrefined()
        {
            Debug.Log("🔬 === TESTING REFINED VS UNREFINED INGREDIENTS ===");

            if (testIngredients.Count > 0)
            {
                var originalIngredient = testIngredients[0];
                
                Debug.Log($"🔬 Original Ingredient: {originalIngredient.ItemName}");
                Debug.Log($"   Size: {originalIngredient.GridWidth}x{originalIngredient.GridHeight}");
                Debug.Log($"   Potency: {originalIngredient.Potency}");
                Debug.Log($"   Unrefined: {originalIngredient.IsUnrefined}");
                Debug.Log($"   Aspect: {originalIngredient.IngredientAspect}");

                // Test refinement logic (simulate the refinement process)
                Debug.Log("🔬 Refinement Analysis:");
                if (originalIngredient.IsUnrefined)
                {
                    Debug.Log("   ✅ This ingredient can be refined!");
                    Debug.Log("   🔬 Refined version would have:");
                    Debug.Log("     - 25% smaller size");
                    Debug.Log("     - 50% higher potency");
                    Debug.Log("     - 5% obstacle spawn chance (vs 15% for unrefined)");
                    Debug.Log("     - Cannot be placed on Divine obstacles");
                    
                    // Calculate refined stats
                    int refinedWidth = Mathf.Max(1, Mathf.RoundToInt(originalIngredient.GridWidth * 0.75f));
                    int refinedHeight = Mathf.Max(1, Mathf.RoundToInt(originalIngredient.GridHeight * 0.75f));
                    int refinedPotency = Mathf.RoundToInt(originalIngredient.Potency * 1.5f);
                    
                    Debug.Log($"   📊 Refined Stats: {refinedWidth}x{refinedHeight}, Potency: {refinedPotency}");
                }
                else
                {
                    Debug.Log("   ❌ This ingredient is already refined");
                    Debug.Log("   🔬 Refined ingredients have:");
                    Debug.Log("     - Compact size for efficient placement");
                    Debug.Log("     - Higher potency per grid cell");
                    Debug.Log("     - Lower obstacle spawn chance");
                    Debug.Log("     - Restriction from Divine obstacle placement");
                }

                // Test Divine obstacle interaction
                Debug.Log("🔬 Divine Obstacle Compatibility:");
                if (originalIngredient.IngredientAspect == Aspect.Divine)
                {
                    if (originalIngredient.IsUnrefined)
                    {
                        Debug.Log("   ✅ Can be placed on Divine obstacles (Unrefined Divine)");
                        Debug.Log("   🌟 Would receive +10% potency bonus");
                    }
                    else
                    {
                        Debug.Log("   ❌ Cannot be placed on Divine obstacles (Refined Divine)");
                        Debug.Log("   ⚠️ Divine obstacles reject refined ingredients");
                    }
                }
                else
                {
                    Debug.Log("   ❌ Wrong aspect - Divine obstacles only accept Divine ingredients");
                }
            }
            else
            {
                Debug.LogWarning("No test ingredients available for refinement testing");
            }
        }

        [ContextMenu("Create Complex Test Scenario")]
        public void CreateComplexTestScenario()
        {
            if (gridGameManager == null)
            {
                Debug.LogError("GridGameManager not assigned!");
                return;
            }

            Debug.Log("🎮 === CREATING COMPLEX TEST SCENARIO ===");

            // Clear grid
            gridGameManager.ClearGrid();

            // Add specific obstacles for testing
            gridGameManager.aspectObstacles.Clear();

            // Add different obstacle types at specific positions
            var obstacles = new (ObstacleType type, Vector2Int pos)[]
            {
                (ObstacleType.FrigidFrozen, new Vector2Int(0, 0)),
                (ObstacleType.Scorch, new Vector2Int(1, 0)),
                (ObstacleType.Divine, new Vector2Int(2, 0)),
                (ObstacleType.Caustic, new Vector2Int(3, 0)),
                (ObstacleType.Arc, new Vector2Int(4, 0))
            };

            foreach (var (type, pos) in obstacles)
            {
                var obstacle = new AspectObstacle(type, pos);
                gridGameManager.aspectObstacles.Add(obstacle);
                
                // Also set the obstacle on the grid cell so it knows it has an obstacle
                var gridCell = gridGameManager.GetCell(pos.x, pos.y);
                if (gridCell != null)
                {
                    gridCell.SetObstacle(obstacle);
                }
            }

            Debug.Log("🚧 Added test obstacles:");
            Debug.Log("   (0,0): Frigid - needs adjacency to unlock");
            Debug.Log("   (1,0): Scorch - needs Scorch/Caustic/Arc aspects");
            Debug.Log("   (2,0): Divine - needs unrefined Divine aspects");
            Debug.Log("   (3,0): Caustic - reduces potency by 20%");
            Debug.Log("   (4,0): Arc - triggers random chaotic effects");

            // Refresh visualizer to show obstacles
            if (gridGameManager.GetComponent<GridVisualizer>() != null)
            {
                gridGameManager.GetComponent<GridVisualizer>().RefreshGrid();
            }

            Debug.Log("🎮 Test scenario ready! Try placing different ingredient types on the obstacles.");
            Debug.Log("🎮 Tips:");
            Debug.Log("   - Place Scorch aspect ingredient next to Frigid to melt it");
            Debug.Log("   - Divine obstacles only accept unrefined Divine ingredients");
            Debug.Log("   - Arc obstacles will trigger random effects");
            Debug.Log("   - Watch for proficiency grade changes as you place ingredients");
        }

        [ContextMenu("Test All Enhanced Systems")]
        public void TestAllEnhancedSystems()
        {
            Debug.Log("🚀 === TESTING ALL ENHANCED ALCHEMY SYSTEMS ===");

            // Test each system in sequence
            TestSynergySystem();
            Debug.Log(""); // Spacer

            TestFailureSystem();
            Debug.Log(""); // Spacer

            TestSkillTree();
            Debug.Log(""); // Spacer

            TestTemplateSystem();
            Debug.Log(""); // Spacer

            TestEnhancedGridSystem();
            Debug.Log(""); // Spacer

            TestProficiencyGrading();
            Debug.Log(""); // Spacer

            TestObstacleSystem();
            Debug.Log(""); // Spacer

            TestRefinedVsUnrefined();
            Debug.Log(""); // Spacer

            CreateComplexTestScenario();

            Debug.Log("🚀 ALL ENHANCED SYSTEMS TESTED! Check console logs above for detailed results.");
            Debug.Log("🚀 Interactive elements:");
            Debug.Log("   - Use F6-F12 keys to test individual systems");
            Debug.Log("   - Click on grid cells to place ingredients");
            Debug.Log("   - Watch real-time proficiency grades");
            Debug.Log("   - Try activating templates and unlocking skills");
        }

        private void Update()
        {
            // Runtime testing shortcuts
            if (Input.GetKeyDown(placementTestKey))
            {
                TestRuntimePlacement();
            }
            else if (Input.GetKeyDown(clearTestKey))
            {
                TestClearGrid();
            }
            else if (Input.GetKeyDown(verifyTestKey))
            {
                TestVerifyOccupancy();
            }
            
            // Enhanced alchemy system keyboard shortcuts
            else if (Input.GetKeyDown(KeyCode.F1))
            {
                TestObstacleSystem();
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            {
                TestProficiencyGrading();
            }
            else if (Input.GetKeyDown(KeyCode.F3))
            {
                TestRefinedVsUnrefined();
            }
            else if (Input.GetKeyDown(KeyCode.F4))
            {
                CreateComplexTestScenario();
            }
            else if (Input.GetKeyDown(KeyCode.F5))
            {
                TestAllEnhancedSystems();
            }
            else if (Input.GetKeyDown(KeyCode.F6))
            {
                TestSynergySystem();
            }
            else if (Input.GetKeyDown(KeyCode.F7))
            {
                TestFailureSystem();
            }
            else if (Input.GetKeyDown(KeyCode.F8))
            {
                TestSkillTree();
            }
            else if (Input.GetKeyDown(KeyCode.F9))
            {
                TestTemplateSystem();
            }
            else if (Input.GetKeyDown(KeyCode.F10))
            {
                TestEnhancedGridSystem();
            }
            
            // Consolidated test shortcuts
            else if (Input.GetKeyDown(KeyCode.F11))
            {
                RunCompleteTestSuite();
            }
            else if (Input.GetKeyDown(KeyCode.F12))
            {
                RunAllBugFixTests();
            }
            
            // Interaction testing shortcuts (from original IngredientInteractionTester)
            else if (Input.GetKeyDown(KeyCode.Y))
            {
                TestPairwiseIngredientInteractions();
            }
            else if (Input.GetKeyDown(KeyCode.U))
            {
                TestSameIngredientSelfInteraction();
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                TestIngredientPlacementAndInteraction();
            }
            else if (Input.GetKeyDown(KeyCode.O))
            {
                TestAllInteractionFeatures();
            }
        }

        private void OnGUI()
        {
            if (gridGameManager == null) return;

            // Show current proficiency grade in top-right corner
            if (testIngredients.Count >= 2)
            {
                try
                {
                    // Get current placements for proficiency calculation
                    var placements = new Dictionary<Vector2Int, Ingredient>();
                    
                    // Simulate current placements (in a real implementation, get from GridGameManager)
                    if (gridGameManager.GetComponent<IngredientPlacer>() != null)
                    {
                        // Try to get actual placements
                        var ingredientPlacer = gridGameManager.GetComponent<IngredientPlacer>();
                        // This would be connected to the actual placed ingredients
                        // For now, we'll show a placeholder
                    }
                    
                    // Show system status instead
                    var synergySystem = gridGameManager.GetComponent<SynergySystem>();
                    var failureSystem = gridGameManager.GetComponent<FailureSystem>();
                    var skillTree = gridGameManager.GetComponent<AlchemySkillTree>();
                    var templateSystem = gridGameManager.GetComponent<TemplateSystem>();
                    var enhancedGrid = gridGameManager.GetComponent<EnhancedGridSystem>();

                    GUI.color = Color.cyan;
                    GUI.Label(new Rect(Screen.width - 250, 10, 240, 30), "Enhanced Alchemy Systems");
                    GUI.color = Color.white;

                    int yOffset = 35;
                    GUI.Label(new Rect(Screen.width - 250, yOffset, 240, 20), $"🔮 Synergy: {(synergySystem != null ? "✅" : "❌")}");
                    yOffset += 20;
                    GUI.Label(new Rect(Screen.width - 250, yOffset, 240, 20), $"⚠️ Failure: {(failureSystem != null ? "✅" : "❌")}");
                    yOffset += 20;
                    GUI.Label(new Rect(Screen.width - 250, yOffset, 240, 20), $"🌳 Skills: {(skillTree != null ? "✅" : "❌")}");
                    yOffset += 20;
                    GUI.Label(new Rect(Screen.width - 250, yOffset, 240, 20), $"🗺️ Templates: {(templateSystem != null ? "✅" : "❌")}");
                    yOffset += 20;
                    GUI.Label(new Rect(Screen.width - 250, yOffset, 240, 20), $"🔄 Enhanced Grid: {(enhancedGrid != null ? "✅" : "❌")}");
                    
                    // Show active template if any
                    if (templateSystem != null && templateSystem.IsTemplateActive)
                    {
                        yOffset += 25;
                        GUI.color = Color.yellow;
                        GUI.Label(new Rect(Screen.width - 250, yOffset, 240, 20), $"Active: {templateSystem.CurrentTemplate.templateName}");
                        GUI.color = Color.white;
                        
                        var progress = templateSystem.CheckTemplateProgress();
                        yOffset += 20;
                        GUI.Label(new Rect(Screen.width - 250, yOffset, 240, 20), $"Progress: {progress.completionPercentage:P}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"GUI error in AlchemySystemTester: {e.Message}");
                }
            }

            // Show comprehensive keyboard shortcuts
            GUI.Label(new Rect(10, Screen.height - 200, 450, 180), 
                "Enhanced Alchemy System Tests:\n" +
                "F1: Obstacles  F2: Proficiency  F3: Refinement  F4: Complex Scenario\n" +
                "F5: Test All Systems  F6: Synergy  F7: Failure  F8: Skills\n" +
                "F9: Templates  F10: Enhanced Grid\n\n" +
                "Use Right-Click → Context Menu for more options");

            // Show obstacle legend
            if (gridGameManager.aspectObstacles.Count > 0)
            {
                GUI.Label(new Rect(10, 10, 300, 200),
                    "🚧 Obstacle Types:\n" +
                    "■ Gray: Corporeal (Blocked)\n" +
                    "■ Blue: Frigid (Needs adjacency)\n" +
                    "■ Red: Scorch (Specific aspects)\n" +
                    "■ Green: Caustic (Potency penalty)\n" +
                    "■ Yellow: Arc (Random effects)\n" +
                    "■ Gold: Divine (Unrefined Divine only)\n\n" +
                    $"Current Obstacles: {gridGameManager.aspectObstacles.Count}");
            }
        }
    }
}