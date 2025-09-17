using UnityEngine;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Effects;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Test script to verify ingredient interaction detection is working correctly
    /// </summary>
    public class IngredientInteractionTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        public GridGameManager gridGameManager;
        public IngredientEffectVisualizer effectVisualizer;
        
        [Header("Test Ingredients")]
        public List<Ingredient> testIngredients = new List<Ingredient>();

        private void Start()
        {
            if (gridGameManager == null)
                gridGameManager = FindFirstObjectByType<GridGameManager>();
                
            if (effectVisualizer == null)
                effectVisualizer = FindFirstObjectByType<IngredientEffectVisualizer>();

            // Load test ingredients if none assigned
            if (testIngredients.Count == 0)
            {
                LoadTestIngredients();
            }
        }

        private void LoadTestIngredients()
        {
            var ingredients = Resources.LoadAll<Ingredient>("TestIngredients");
            if (ingredients.Length > 0)
            {
                testIngredients.AddRange(ingredients);
                Debug.Log($"🧪 Loaded {ingredients.Length} test ingredients for interaction testing");
            }
        }

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

        private void Update()
        {
            // Keyboard shortcuts for testing
            if (Input.GetKeyDown(KeyCode.T))
            {
                TestIngredientEffectDetection();
            }
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
            GUI.Label(new Rect(10, Screen.height - 160, 400, 120),
                "Infusion Interaction Testing Shortcuts:\n" +
                "T: Test Infusion Detection\n" +
                "Y: Test Pairwise Infusion Interactions\n" +
                "U: Test Self-Infusion Interactions\n" +
                "I: Test Placement & Infusion Interaction\n" +
                "O: Test All Features");
        }
    }
}