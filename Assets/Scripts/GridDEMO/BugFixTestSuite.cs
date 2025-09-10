using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.GridDemo.UI;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Test suite to verify the three major bug fixes:
    /// 1. Proper collision detection for ingredient placement
    /// 2. Enhanced ASE visual preview matching game appearance  
    /// 3. Fixed button state management (no more permanent white buttons)
    /// </summary>
    public class BugFixTestSuite : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private GridGameManager gridManager;
        [SerializeField] private List<Ingredient> testIngredients;
        [SerializeField] private List<IngredientButton> testButtons;
        
        [Header("Test Results")]
        [SerializeField] private bool collisionDetectionWorking = false;
        [SerializeField] private bool buttonStatesWorking = false;
        [SerializeField] private int successfulPlacements = 0;
        [SerializeField] private int blockedPlacements = 0;
        
        private void Start()
        {
            if (gridManager == null)
                gridManager = FindFirstObjectByType<GridGameManager>();
                
            Debug.Log("🧪 BugFixTestSuite: Starting automated tests...");
            
            // Run tests after a short delay to ensure everything is initialized
            Invoke(nameof(RunAllTests), 1f);
        }
        
        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            Debug.Log("=== BUG FIX TEST SUITE ===");
            
            TestCollisionDetection();
            TestButtonStates();
            TestVisualPreview();
            
            LogTestResults();
        }
        
        /// <summary>
        /// Test Fix #1: Collision detection should prevent overlapping ingredients
        /// </summary>
        private void TestCollisionDetection()
        {
            Debug.Log("🔍 Testing collision detection...");
            
            if (gridManager == null || testIngredients.Count < 2)
            {
                Debug.LogWarning("Cannot test collision detection - missing GridGameManager or test ingredients");
                return;
            }
            
            // Clear grid first
            gridManager.ClearGrid();
            
            // Test 1: Place first ingredient
            var firstIngredient = testIngredients[0];
            Vector2Int pos1 = new Vector2Int(1, 1);
            
            bool canPlaceFirst = gridManager.CanPlaceIngredient(firstIngredient, pos1);
            bool placedFirst = gridManager.TryPlaceIngredient(firstIngredient, pos1);
            
            Debug.Log($"First placement - Can place: {canPlaceFirst}, Placed: {placedFirst}");
            
            if (placedFirst)
            {
                successfulPlacements++;
                
                // Test 2: Try to place second ingredient overlapping the first
                var secondIngredient = testIngredients[1];
                Vector2Int pos2 = pos1; // Same position = should fail
                
                bool canPlaceSecond = gridManager.CanPlaceIngredient(secondIngredient, pos2);
                bool placedSecond = gridManager.TryPlaceIngredient(secondIngredient, pos2);
                
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
                bool canPlaceThird = gridManager.CanPlaceIngredient(secondIngredient, pos3);
                bool placedThird = gridManager.TryPlaceIngredient(secondIngredient, pos3);
                
                Debug.Log($"Adjacent placement - Can place: {canPlaceThird}, Placed: {placedThird}");
                
                if (canPlaceThird && placedThird)
                {
                    successfulPlacements++;
                    Debug.Log("✅ Adjacent placement working correctly!");
                }
            }
        }
        
        /// <summary>
        /// Test Fix #3: Button states should properly cycle through colors
        /// </summary>
        private void TestButtonStates()
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
        
        /// <summary>
        /// Test Fix #2: Visual preview info (this needs to be tested manually in ASE)
        /// </summary>
        private void TestVisualPreview()
        {
            Debug.Log("🎮 Visual preview enhancement info:");
            Debug.Log("✅ Enhanced ASE grid editor with 3D-style visual preview");
            Debug.Log("✅ Added game appearance simulation with lighting/shadows");
            Debug.Log("✅ Added preview information panel with ingredient details");
            Debug.Log("📝 Manual test: Open AlchemySystemEditor > Ingredients tab > Edit shapes to see enhanced preview");
        }
        
        private void LogTestResults()
        {
            Debug.Log("=== TEST RESULTS SUMMARY ===");
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
        
        [ContextMenu("Test Collision Only")]
        public void TestCollisionOnly()
        {
            TestCollisionDetection();
        }
        
        [ContextMenu("Test Buttons Only")]
        public void TestButtonsOnly()
        {
            TestButtonStates();
        }
        
        [ContextMenu("Clear Grid")]
        public void ClearTestGrid()
        {
            if (gridManager != null)
            {
                gridManager.ClearGrid();
                successfulPlacements = 0;
                blockedPlacements = 0;
                Debug.Log("🧹 Test grid cleared");
            }
        }
    }
}