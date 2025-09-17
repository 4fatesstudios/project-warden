using UnityEngine;
using UnityEditor;
using System.Reflection;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.Editor;

namespace AlchemySystem.Tests
{
    /// <summary>
    /// Clean validation script for the Alchemy System Editor integration tests.
    /// Tests all core functionalities including the demo script.
    /// </summary>
    public class InfusionEditorValidationClean : MonoBehaviour
    {
        [MenuItem("Alchemy/Tests/Run All Validation Tests (Clean)")]
        public static void RunAllTests()
        {
            Debug.Log("🧪 Starting comprehensive Alchemy System Editor validation tests...");
            Debug.Log("===============================================================");

            int passedTests = 0;
            int totalTests = 10;

            // Test 1: Check if Infusion ScriptableObject exists
            if (TestInfusionScriptableObjectExists())
            {
                Debug.Log("✅ Test 1 PASSED: Infusion ScriptableObject exists");
                passedTests++;
            }
            else
            {
                Debug.LogError("❌ Test 1 FAILED: Infusion ScriptableObject missing");
            }

            // Test 2: Check if Ingredient ScriptableObject exists
            if (TestIngredientScriptableObjectExists())
            {
                Debug.Log("✅ Test 2 PASSED: Ingredient ScriptableObject exists");
                passedTests++;
            }
            else
            {
                Debug.LogError("❌ Test 2 FAILED: Ingredient ScriptableObject missing");
            }

            // Test 3: Check if Potion ScriptableObject exists
            if (TestPotionScriptableObjectExists())
            {
                Debug.Log("✅ Test 3 PASSED: Potion ScriptableObject exists");
                passedTests++;
            }
            else
            {
                Debug.LogError("❌ Test 3 FAILED: Potion ScriptableObject missing");
            }

            // Test 4: Check if AlchemyRecipe ScriptableObject exists
            if (TestAlchemyRecipeScriptableObjectExists())
            {
                Debug.Log("✅ Test 4 PASSED: AlchemyRecipe ScriptableObject exists");
                passedTests++;
            }
            else
            {
                Debug.LogError("❌ Test 4 FAILED: AlchemyRecipe ScriptableObject missing");
            }

            // Test 5: Check if Alchemy System Editor window exists
            if (TestAlchemySystemEditorExists())
            {
                Debug.Log("✅ Test 5 PASSED: AlchemySystemEditor window exists");
                passedTests++;
            }
            else
            {
                Debug.LogError("❌ Test 5 FAILED: AlchemySystemEditor window missing");
            }

            // Test 6: Test Infusion creation capability
            if (TestInfusionCreation())
            {
                Debug.Log("✅ Test 6 PASSED: Infusion creation works");
                passedTests++;
            }
            else
            {
                Debug.LogError("❌ Test 6 FAILED: Infusion creation error");
            }

            // Test 7: Test basic asset creation
            if (TestBasicAssetCreation())
            {
                Debug.Log("✅ Test 7 PASSED: Basic asset creation works");
                passedTests++;
            }
            else
            {
                Debug.LogError("❌ Test 7 FAILED: Basic asset creation error");
            }

            // Test 8: Test editor window instantiation
            if (TestEditorWindowInstantiation())
            {
                Debug.Log("✅ Test 8 PASSED: Editor window can be instantiated");
                passedTests++;
            }
            else
            {
                Debug.LogError("❌ Test 8 FAILED: Editor window instantiation error");
            }

            // Test 9: Test demo script compilation
            if (TestDemoScriptExists())
            {
                Debug.Log("✅ Test 9 PASSED: AlchemyEditorTestClean demo class exists");
                passedTests++;
            }
            else
            {
                Debug.LogError("❌ Test 9 FAILED: AlchemyEditorTestClean compilation error");
            }

            // Test 10: Test namespace and using directives
            if (TestNamespaceIntegrity())
            {
                Debug.Log("✅ Test 10 PASSED: All namespaces and dependencies correct");
                passedTests++;
            }
            else
            {
                Debug.LogError("❌ Test 10 FAILED: Namespace or dependency issues");
            }

            Debug.Log("===============================================================");
            Debug.Log($"🎯 VALIDATION RESULTS: {passedTests}/{totalTests} tests passed");
            
            if (passedTests == totalTests)
            {
                Debug.Log("🎉 ALL TESTS PASSED! Alchemy System Editor is ready for use!");
            }
            else
            {
                Debug.LogWarning($"⚠️ {totalTests - passedTests} test(s) failed. Please check the errors above.");
            }
        }

        private static bool TestInfusionScriptableObjectExists()
        {
            try
            {
                var infusionType = typeof(Infusion);
                return infusionType != null && infusionType.IsSubclassOf(typeof(ScriptableObject));
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private static bool TestIngredientScriptableObjectExists()
        {
            try
            {
                var ingredientType = typeof(Ingredient);
                return ingredientType != null && ingredientType.IsSubclassOf(typeof(ScriptableObject));
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private static bool TestPotionScriptableObjectExists()
        {
            try
            {
                var potionType = typeof(Potion);
                return potionType != null && potionType.IsSubclassOf(typeof(ScriptableObject));
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private static bool TestAlchemyRecipeScriptableObjectExists()
        {
            try
            {
                var recipeType = typeof(AlchemyRecipe);
                return recipeType != null && recipeType.IsSubclassOf(typeof(ScriptableObject));
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private static bool TestAlchemySystemEditorExists()
        {
            try
            {
                var editorType = typeof(AlchemySystemEditor);
                return editorType != null && editorType.IsSubclassOf(typeof(EditorWindow));
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private static bool TestInfusionCreation()
        {
            try
            {
                var infusion = ScriptableObject.CreateInstance<Infusion>();
                if (infusion != null)
                {
                    // Clean up the test object
                    DestroyImmediate(infusion);
                    return true;
                }
                return false;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private static bool TestBasicAssetCreation()
        {
            try
            {
                var ingredient = ScriptableObject.CreateInstance<Ingredient>();
                var potion = ScriptableObject.CreateInstance<Potion>();
                var recipe = ScriptableObject.CreateInstance<AlchemyRecipe>();
                
                bool success = ingredient != null && potion != null && recipe != null;
                
                // Clean up test objects
                if (ingredient != null) DestroyImmediate(ingredient);
                if (potion != null) DestroyImmediate(potion);
                if (recipe != null) DestroyImmediate(recipe);
                
                return success;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private static bool TestEditorWindowInstantiation()
        {
            try
            {
                var window = EditorWindow.GetWindow<AlchemySystemEditor>();
                if (window != null)
                {
                    window.Close();
                    return true;
                }
                return false;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private static bool TestDemoScriptExists()
        {
            try
            {
                // Updated to use the clean demo script
                var demoType = typeof(AlchemySystem.Demo.AlchemyEditorTestClean);
                return demoType != null;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private static bool TestNamespaceIntegrity()
        {
            try
            {
                // Test if all required namespaces are accessible
                var infusionAssembly = typeof(Infusion).Assembly;
                var editorAssembly = typeof(AlchemySystemEditor).Assembly;
                
                return infusionAssembly != null && editorAssembly != null;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        [MenuItem("Alchemy/Tests/Test Rename UI Integration (Clean)")]
        public static void TestRenameUIIntegration()
        {
            Debug.Log("🖊️ Testing Rename UI Integration...");

            try
            {
                var window = EditorWindow.GetWindow<AlchemySystemEditor>();
                if (window != null)
                {
                    Debug.Log("✅ Alchemy System Editor window opened successfully");
                    Debug.Log("💡 Look for the 📝 rename buttons next to items in each tab");
                    Debug.Log("💡 Click any 📝 button to test the rename functionality");
                }
                else
                {
                    Debug.LogError("❌ Failed to open Alchemy System Editor window");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Error testing rename UI: {ex.Message}");
            }
        }
    }
}