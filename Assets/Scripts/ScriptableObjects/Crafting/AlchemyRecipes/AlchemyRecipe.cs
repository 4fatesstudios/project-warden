using System;
using System.Collections.Generic;
using UnityEngine;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes
{
    [CreateAssetMenu(fileName = "NewRecipe", menuName = "AlchemyRecipes/Generic Recipe")]
    public class AlchemyRecipe : ScriptableObject
    {
        [SerializeField, Tooltip("Input ingredient 1 is added.")]
        private Ingredient inputIngredient1;
        [SerializeField, Tooltip("Input ingredient 2 is added.")]
        private Ingredient inputIngredient2;
        [SerializeField, Tooltip("Input ingredient 3 is added.")]
        private Ingredient inputIngredient3;

        [SerializeField, Tooltip("Number of successful hits required.")]
        private int requiredHits;

        [SerializeField, Tooltip("Maximum allowed attempts.")]
        private int maxAttempts;

        [SerializeField, Tooltip("Output potion to be created.")]
        private Potion outputPotion;

        [SerializeField, Tooltip("How long the process must stay in the correct state (seconds).")]
        private float requiredTemperature = 5f;

        [SerializeField, Tooltip("Maximum time allowed to complete the distillation (seconds).")]
        private float totalDuration = 10f;
        public int RequiredHits => requiredHits;
        public int MaxAttempts => maxAttempts;

        public Ingredient InputIngredient1 => inputIngredient1;
        public Ingredient InputIngredient2 => inputIngredient2;
        public Ingredient InputIngredient3 => inputIngredient3;

        public Potion OutputPotion => outputPotion;

        public float RequiredDuration => requiredTemperature;
        public float TotalDuration => totalDuration;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (requiredHits == 0 || requiredHits > maxAttempts)
            {
                Debug.LogWarning($"Recipe [{name}] is an Invalid amount compared to Max Attempt.");
            }
            if (maxAttempts == 0 || requiredHits > maxAttempts)
            {
                Debug.LogWarning($"Recipe [{name}] is an Invalid amount compared to Required Hits.");
            }

            var ingredients = new[] { inputIngredient1, inputIngredient2, inputIngredient3 };

            // Check for missing input ingredients
            if (ingredients[0] == null)
                Debug.LogWarning($"[{name}] Input ingredient 1 is not assigned.");
            if (ingredients[1] == null)
                Debug.LogWarning($"[{name}] Input ingredient 2 is not assigned.");
            if (ingredients[2] == null)
                Debug.LogWarning($"[{name}] Input ingredient 3 is not assigned.");

            if (outputPotion == null)
                Debug.LogWarning($"[{name}] Output Potion is not assigned.");

            if (totalDuration < 0 || requiredTemperature < 0)
                Debug.LogWarning($"[{name}] Thresholds cannot be negative.");

            // Sort and preview recipe key for debugging
            if (ingredients[0] != null && ingredients[1] != null && ingredients[2] != null)
            {
                System.Array.Sort(ingredients, (a, b) => a.name.CompareTo(b.name));
                string keyPreview = $"[{name}] Recipe Key: {ingredients[0].name}, {ingredients[1].name}, {ingredients[2].name}";
                Debug.Log(keyPreview);
            }
        }
#endif
    }
}