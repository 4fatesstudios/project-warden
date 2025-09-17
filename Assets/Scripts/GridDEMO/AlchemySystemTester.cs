using UnityEngine;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Test script to demonstrate the enhanced alchemy system features
    /// </summary>
    public class AlchemySystemTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        public GridGameManager gridGameManager;
        
        [Header("Test Ingredients")]
        public List<Ingredient> testIngredients = new List<Ingredient>();

        private void Start()
        {
            if (gridGameManager == null)
            {
                gridGameManager = FindFirstObjectByType<GridGameManager>();
            }

            // Auto-find test ingredients if none assigned
            if (testIngredients.Count == 0)
            {
                LoadTestIngredients();
            }
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

        [ContextMenu("Test Proficiency Grading")]
        public void TestProficiencyGrading()
        {
            if (gridGameManager == null)
            {
                Debug.LogError("GridGameManager not assigned!");
                return;
            }

            Debug.Log("📊 === TESTING PROFICIENCY GRADING ===");

            // Place some ingredients to test grading
            if (testIngredients.Count >= 2)
            {
                var ingredient1 = testIngredients[0];
                var ingredient2 = testIngredients[1];

                // Clear grid first
                gridGameManager.ClearGrid();

                // Place ingredients in different patterns
                bool success1 = gridGameManager.TryPlaceIngredient(ingredient1, new Vector2Int(1, 1));
                bool success2 = gridGameManager.TryPlaceIngredient(ingredient2, new Vector2Int(2, 1)); // Adjacent

                if (success1 && success2)
                {
                    Debug.Log("✅ Placed ingredients successfully");
                    
                    // Calculate proficiency grade
                    var grade = gridGameManager.CalculateCurrentProficiency();
                    
                    Debug.Log($"📊 === PROFICIENCY RESULTS ===");
                    Debug.Log($"📊 Overall Grade: {grade.gradeLevel} ({grade.overallScore:F1}%)");
                    Debug.Log($"📊 Individual Scores:");
                    Debug.Log($"   🏠 Coverage: {grade.coverageRatio:F1}%");
                    Debug.Log($"   🤝 Adjacency: {grade.adjacencySynergy:F1}%");
                    Debug.Log($"   📈 Expansion: {grade.expansionUtilization:F1}%");
                    Debug.Log($"   🔷 Shape: {grade.shapeDifficulty:F1}%");
                    Debug.Log($"   📐 Orientation: {grade.orientationEfficiency:F1}%");
                    Debug.Log($"   🚧 Obstacles: {grade.obstaclesCompleted:F1}%");
                    Debug.Log($"📊 Feedback: {grade.feedback}");
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

                if (gridGameManager.CanRefineIngredient(originalIngredient))
                {
                    Debug.Log("🔬 This ingredient can be refined!");
                    Debug.Log("🔬 Refined version would have:");
                    Debug.Log("   - 25% smaller size");
                    Debug.Log("   - 50% higher potency");
                    Debug.Log("   - 5% obstacle spawn chance (vs 15% for unrefined)");
                    Debug.Log("   - Cannot be placed on Divine obstacles");
                    
                    // Test refinement (placeholder)
                    var refinedIngredient = gridGameManager.CreateRefinedIngredient(originalIngredient);
                }
                else
                {
                    Debug.Log("🔬 This ingredient is already refined or cannot be refined");
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
            gridGameManager.aspectObstacles.Add(new AspectObstacle(ObstacleType.Frigid, new Vector2Int(0, 0)));
            gridGameManager.aspectObstacles.Add(new AspectObstacle(ObstacleType.Scorch, new Vector2Int(1, 0)));
            gridGameManager.aspectObstacles.Add(new AspectObstacle(ObstacleType.Divine, new Vector2Int(2, 0)));
            gridGameManager.aspectObstacles.Add(new AspectObstacle(ObstacleType.Caustic, new Vector2Int(3, 0)));
            gridGameManager.aspectObstacles.Add(new AspectObstacle(ObstacleType.Arc, new Vector2Int(4, 0)));

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

        [ContextMenu("Test All Features")]
        public void TestAllFeatures()
        {
            Debug.Log("🚀 === TESTING ALL ENHANCED ALCHEMY FEATURES ===");

            TestObstacleSystem();
            System.Threading.Tasks.Task.Delay(1000).Wait(); // Short delay

            TestProficiencyGrading();
            System.Threading.Tasks.Task.Delay(1000).Wait();

            TestRefinedVsUnrefined();
            System.Threading.Tasks.Task.Delay(1000).Wait();

            CreateComplexTestScenario();

            Debug.Log("🚀 All feature tests completed! Check the console logs above for detailed results.");
            Debug.Log("🚀 Interactive elements:");
            Debug.Log("   - Click on grid cells to place ingredients on obstacles");
            Debug.Log("   - Use context menu options to test individual features");
            Debug.Log("   - Watch proficiency grades update in real-time");
        }

        private void Update()
        {
            // Keyboard shortcuts for testing
            if (Input.GetKeyDown(KeyCode.F1))
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
                TestAllFeatures();
            }
        }

        private void OnGUI()
        {
            if (gridGameManager == null) return;

            // Show current proficiency grade in top-right corner
            if (gridGameManager.enableProficiencyGrading)
            {
                var grade = gridGameManager.CalculateCurrentProficiency();
                var gradeColor = gridGameManager.GetGradeColor(grade.gradeLevel);
                
                GUI.color = gradeColor;
                GUI.Label(new Rect(Screen.width - 200, 10, 190, 30), $"Grade: {grade.gradeLevel} ({grade.overallScore:F0}%)");
                GUI.color = Color.white;

                // Show detailed breakdown
                GUI.Label(new Rect(Screen.width - 200, 40, 190, 20), $"Coverage: {grade.coverageRatio:F0}%");
                GUI.Label(new Rect(Screen.width - 200, 60, 190, 20), $"Adjacency: {grade.adjacencySynergy:F0}%");
                GUI.Label(new Rect(Screen.width - 200, 80, 190, 20), $"Obstacles: {grade.obstaclesCompleted:F0}%");
            }

            // Show keyboard shortcuts
            GUI.Label(new Rect(10, Screen.height - 120, 300, 100), 
                "F1: Test Obstacles  F2: Test Grading  F3: Test Refinement\n" +
                "F4: Complex Scenario  F5: Test All Features");

            // Show obstacle legend
            if (gridGameManager.enableObstacles)
            {
                GUI.Label(new Rect(10, 10, 300, 200),
                    "🚧 Obstacle Types:\n" +
                    "■ Gray: Corporeal (Blocked)\n" +
                    "■ Blue: Frigid (Needs adjacency)\n" +
                    "■ Red: Scorch (Specific aspects)\n" +
                    "■ Green: Caustic (Potency penalty)\n" +
                    "■ Yellow: Arc (Random effects)\n" +
                    "■ Gold: Divine (Unrefined Divine only)");
            }
        }
    }
}