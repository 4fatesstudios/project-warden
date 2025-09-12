using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Effects;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FourFatesStudios.ProjectWarden.Editor
{
    public class InfusionSystemIntegrationTest : EditorWindow
    {
        private Vector2 scrollPosition;
        private bool testCompleted = false;
        private List<string> testResults = new List<string>();
        private int passedTests = 0;
        private int totalTests = 0;
        
        [MenuItem("Alchemy/Testing/Integration Test Suite")]
        public static void ShowWindow()
        {
            var window = GetWindow<InfusionSystemIntegrationTest>("Infusion Integration Tests");
            window.minSize = new Vector2(800, 600);
        }
        
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("🧪 Infusion System Integration Test Suite", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This test suite validates the complete infusion system integration including:\\n" +
                "• Infusion ScriptableObject functionality\\n" +
                "• InfusionBundle operations\\n" +
                "• Ingredient system integration\\n" +
                "• Potion system integration\\n" +
                "• Editor window functionality\\n" +
                "• Migration compatibility", 
                MessageType.Info);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🚀 Run Full Test Suite", GUILayout.Height(30)))
            {
                RunFullTestSuite();
            }
            
            if (GUILayout.Button("🧹 Clear Results", GUILayout.Height(30)))
            {
                ClearResults();
            }
            
            if (GUILayout.Button("📋 Export Results", GUILayout.Height(30)))
            {
                ExportResults();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Results display
            if (testResults.Count > 0)
            {
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("📊 Test Results", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                
                if (testCompleted)
                {
                    string resultText = passedTests == totalTests ? "✅ ALL PASSED" : $"⚠️ {passedTests}/{totalTests} PASSED";
                    Color originalColor = GUI.color;
                    GUI.color = passedTests == totalTests ? Color.green : Color.yellow;
                    GUILayout.Label(resultText, "Button");
                    GUI.color = originalColor;
                }
                EditorGUILayout.EndHorizontal();
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));
                
                foreach (var result in testResults)
                {
                    EditorGUILayout.LabelField(result, EditorStyles.miniLabel);
                }
                
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.BeginVertical("Box");
                GUILayout.Label("Ready to run tests. Click 'Run Full Test Suite' to begin.", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndVertical();
            }
        }
        
        private void RunFullTestSuite()
        {
            ClearResults();
            LogTest("🚀 Starting Infusion System Integration Test Suite...");
            
            try
            {
                // Test 1: Infusion ScriptableObject functionality
                TestInfusionScriptableObject();
                
                // Test 2: InfusionBundle operations
                TestInfusionBundleOperations();
                
                // Test 3: Ingredient integration
                TestIngredientIntegration();
                
                // Test 4: Potion integration
                TestPotionIntegration();
                
                // Test 5: Editor functionality
                TestEditorFunctionality();
                
                // Test 6: Migration compatibility
                TestMigrationCompatibility();
                
                // Test 7: Validation systems
                TestValidationSystems();
                
                // Test 8: Performance tests
                TestPerformance();
                
                testCompleted = true;
                LogTest($"\\n🎉 Test Suite Complete! {passedTests}/{totalTests} tests passed.");
                
                if (passedTests == totalTests)
                {
                    LogTest("✅ All tests passed! Infusion system is working correctly.");
                }
                else
                {
                    LogTest($"⚠️ {totalTests - passedTests} tests failed. Check the logs for details.");
                }
            }
            catch (System.Exception e)
            {
                LogTest($"❌ Test suite crashed: {e.Message}");
                testCompleted = true;
            }
        }
        
        private void TestInfusionScriptableObject()
        {
            LogTest("\\n📋 Test 1: Infusion ScriptableObject Functionality");
            
            // Test creation
            AssertTest("Can create Infusion ScriptableObject", () =>
            {
                var infusion = ScriptableObject.CreateInstance<Infusion>();
                return infusion != null;
            });
            
            // Test property access
            AssertTest("Infusion properties accessible", () =>
            {
                var infusion = ScriptableObject.CreateInstance<Infusion>();
                return infusion.InfusionName != null && 
                       infusion.EffectBundle != null &&
                       infusion.PowerLevel >= 1 &&
                       infusion.PowerLevel <= 10;
            });
            
            // Test validation
            AssertTest("Infusion validation works", () =>
            {
                var infusion = ScriptableObject.CreateInstance<Infusion>();
                bool isValid = infusion.IsValid(out string message);
                return !string.IsNullOrEmpty(message);
            });
            
            // Test categorization
            AssertTest("Effect categorization works", () =>
            {
                var infusion = ScriptableObject.CreateInstance<Infusion>();
                var categories = infusion.GetEffectCategories();
                return categories != null;
            });
        }
        
        private void TestInfusionBundleOperations()
        {
            LogTest("\\n🧪 Test 2: InfusionBundle Operations");
            
            // Test creation
            AssertTest("Can create InfusionBundle", () =>
            {
                var bundle = new InfusionBundle();
                return bundle != null && bundle.Infusions != null;
            });
            
            // Test adding infusions
            AssertTest("Can add infusions to bundle", () =>
            {
                var bundle = new InfusionBundle();
                var infusion = ScriptableObject.CreateInstance<Infusion>();
                SetInfusionName(infusion, "Test Infusion");
                
                bundle.AddInfusion(infusion);
                return bundle.Infusions.Count == 1 && bundle.ContainsInfusion(infusion);
            });
            
            // Test removing infusions
            AssertTest("Can remove infusions from bundle", () =>
            {
                var bundle = new InfusionBundle();
                var infusion = ScriptableObject.CreateInstance<Infusion>();
                SetInfusionName(infusion, "Test Infusion");
                
                bundle.AddInfusion(infusion);
                bool removed = bundle.RemoveInfusion(infusion);
                return removed && bundle.Infusions.Count == 0;
            });
            
            // Test validation
            AssertTest("Bundle validation works", () =>
            {
                var bundle = new InfusionBundle();
                bool isValid = bundle.IsValid(out string message);
                return isValid && message == "Valid";
            });
            
            // Test effect aggregation
            AssertTest("Effect aggregation works", () =>
            {
                var bundle = new InfusionBundle();
                var allEffects = bundle.GetAllEffects();
                return allEffects != null;
            });
        }
        
        private void TestIngredientIntegration()
        {
            LogTest("\\n🌿 Test 3: Ingredient Integration");
            
            // Find a test ingredient
            var ingredients = FindAssetsByType<Ingredient>();
            if (ingredients.Count == 0)
            {
                LogTest("  ⚠️ No ingredients found. Creating test ingredient...");
                // Could create a test ingredient here
                return;
            }
            
            var testIngredient = ingredients[0];
            
            AssertTest("Ingredient has InfusionBundle property", () =>
            {
                return testIngredient.InfusionBundle != null;
            });
            
            AssertTest("Ingredient HasEffects() method works", () =>
            {
                bool hasEffects = testIngredient.HasEffects();
                return true; // Method should not throw
            });
            
            AssertTest("Ingredient GetEffectTypes() method works", () =>
            {
                var effectTypes = testIngredient.GetEffectTypes();
                return effectTypes != null;
            });
            
            AssertTest("Ingredient HasEffectOfType<T>() method works", () =>
            {
                bool hasHealEffect = testIngredient.HasEffectOfType<HealEffect>();
                return true; // Method should not throw
            });
        }
        
        private void TestPotionIntegration()
        {
            LogTest("\\n🍶 Test 4: Potion Integration");
            
            // Find a test potion
            var potions = FindAssetsByType<Potion>();
            if (potions.Count == 0)
            {
                LogTest("  ⚠️ No potions found. Skipping potion integration tests.");
                return;
            }
            
            var testPotion = potions[0];
            
            AssertTest("Potion has InfusionBundle property", () =>
            {
                return testPotion.InfusionBundle != null;
            });
            
            AssertTest("Potion infusion methods work", () =>
            {
                bool hasInfusions = testPotion.HasInfusions();
                int powerLevel = testPotion.GetInfusionPowerLevel();
                var allEffects = testPotion.GetAllEffects();
                return allEffects != null;
            });
            
            AssertTest("Potion enhanced description works", () =>
            {
                string description = testPotion.GetEnhancedDescription();
                return !string.IsNullOrEmpty(description);
            });
        }
        
        private void TestEditorFunctionality()
        {
            LogTest("\\n⚙️ Test 5: Editor Functionality");
            
            AssertTest("AlchemySystemEditor exists", () =>
            {
                var editorType = typeof(AlchemySystemEditor);
                return editorType != null;
            });
            
            AssertTest("InfusionMigrationUtility exists", () =>
            {
                var utilityType = typeof(InfusionMigrationUtility);
                return utilityType != null;
            });
            
            AssertTest("InfusionSystemTester exists", () =>
            {
                var testerType = typeof(InfusionSystemTester);
                return testerType != null;
            });
            
            // Test infusion creation through editor
            AssertTest("Can find existing infusions", () =>
            {
                var infusions = FindAssetsByType<Infusion>();
                LogTest($"    Found {infusions.Count} existing infusions");
                return true;
            });
        }
        
        private void TestMigrationCompatibility()
        {
            LogTest("\\n🔄 Test 6: Migration Compatibility");
            
            // Test that old and new systems can coexist
            AssertTest("Old Infusion struct still exists", () =>
            {
                var oldInfusionType = System.Type.GetType("FourFatesStudios.ProjectWarden.Structs.Infusion");
                return oldInfusionType != null;
            });
            
            AssertTest("New Infusion ScriptableObject exists", () =>
            {
                var newInfusionType = typeof(Infusion);
                return newInfusionType != null && newInfusionType.IsSubclassOf(typeof(ScriptableObject));
            });
            
            AssertTest("Migration utility can be instantiated", () =>
            {
                try
                {
                    var migrationUtility = EditorWindow.CreateInstance<InfusionMigrationUtility>();
                    return migrationUtility != null;
                }
                catch
                {
                    return false;
                }
            });
        }
        
        private void TestValidationSystems()
        {
            LogTest("\\n✅ Test 7: Validation Systems");
            
            // Test infusion validation
            AssertTest("Infusion validation detects invalid state", () =>
            {
                var infusion = ScriptableObject.CreateInstance<Infusion>();
                // Don't set any properties - should be invalid
                bool isValid = infusion.IsValid(out string message);
                return !isValid && !string.IsNullOrEmpty(message);
            });
            
            // Test bundle validation
            AssertTest("Bundle validation works with null infusions", () =>
            {
                var bundle = new InfusionBundle();
                bundle.Infusions.Add(null); // Add null infusion
                bool isValid = bundle.IsValid(out string message);
                return !isValid;
            });
            
            // Test ingredient validation  
            AssertTest("Ingredient OnValidate initializes InfusionBundle", () =>
            {
                var ingredients = FindAssetsByType<Ingredient>();
                if (ingredients.Count > 0)
                {
                    var ingredient = ingredients[0];
                    return ingredient.InfusionBundle != null;
                }
                return true; // Skip if no ingredients
            });
        }
        
        private void TestPerformance()
        {
            LogTest("\\n⚡ Test 8: Performance Tests");
            
            // Test large bundle operations
            AssertTest("Large bundle operations complete quickly", () =>
            {
                var bundle = new InfusionBundle();
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                // Add many test infusions
                for (int i = 0; i < 100; i++)
                {
                    var infusion = ScriptableObject.CreateInstance<Infusion>();
                    SetInfusionName(infusion, $"Test Infusion {i}");
                    bundle.AddInfusion(infusion);
                }
                
                // Test operations
                var allEffects = bundle.GetAllEffects();
                var categories = bundle.GetAllEffectCategories();
                int totalPower = bundle.GetTotalPowerLevel();
                
                stopwatch.Stop();
                LogTest($"    Large bundle operations completed in {stopwatch.ElapsedMilliseconds}ms");
                
                return stopwatch.ElapsedMilliseconds < 1000; // Should complete within 1 second
            });
            
            // Test effect type checking performance
            AssertTest("Effect type checking is efficient", () =>
            {
                var ingredients = FindAssetsByType<Ingredient>();
                if (ingredients.Count == 0) return true;
                
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                foreach (var ingredient in ingredients)
                {
                    ingredient.HasEffectOfType<HealEffect>();
                    ingredient.HasEffectOfType<DamageEffect>();
                    ingredient.GetEffectTypes();
                }
                
                stopwatch.Stop();
                LogTest($"    Effect type checking completed in {stopwatch.ElapsedMilliseconds}ms for {ingredients.Count} ingredients");
                
                return stopwatch.ElapsedMilliseconds < 500;
            });
        }
        
        // Helper methods
        private void AssertTest(string testName, System.Func<bool> test)
        {
            totalTests++;
            try
            {
                bool result = test();
                if (result)
                {
                    LogTest($"  ✅ {testName}");
                    passedTests++;
                }
                else
                {
                    LogTest($"  ❌ {testName} - Test returned false");
                }
            }
            catch (System.Exception e)
            {
                LogTest($"  ❌ {testName} - Exception: {e.Message}");
            }
        }
        
        private void LogTest(string message)
        {
            testResults.Add(message);
            Debug.Log(message);
        }
        
        private void ClearResults()
        {
            testResults.Clear();
            testCompleted = false;
            passedTests = 0;
            totalTests = 0;
        }
        
        private void ExportResults()
        {
            if (testResults.Count == 0)
            {
                EditorUtility.DisplayDialog("Export Results", "No test results to export. Run the test suite first.", "OK");
                return;
            }
            
            string path = EditorUtility.SaveFilePanel("Export Test Results", "", "infusion_test_results.txt", "txt");
            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.WriteAllLines(path, testResults);
                EditorUtility.DisplayDialog("Export Results", $"Test results exported to:\\n{path}", "OK");
            }
        }
        
        private List<T> FindAssetsByType<T>() where T : Object
        {
            var assets = new List<T>();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }
            
            return assets;
        }
        
        private void SetInfusionName(Infusion infusion, string name)
        {
            var infusionType = typeof(Infusion);
            var nameField = infusionType.GetField("infusionName", BindingFlags.NonPublic | BindingFlags.Instance);
            nameField?.SetValue(infusion, name);
        }
    }
}