using System;
using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Grinds
{
    [CreateAssetMenu(fileName = "NewGrind", menuName = "Grinds/Generic Grind")]
    public class Grind : ScriptableObject
    {
        [SerializeField, Tooltip("Make sure you choose the right rarity")]
        private Ingredient inputIngredient = new();

        [SerializeField, Tooltip("Minimum Grind Speed"), Range(1, 9998)] private float minGrindSpeed = 1;
        [SerializeField, Tooltip("Maximum Grind Speed"), Range(1, 9999)] private float maxGrindSpeed = 2;
        [SerializeField, Tooltip("Minimum Pound Speed"), Range(1, 9998)] private float minPoundSpeed = 1;
        [SerializeField, Tooltip("Maximum Pound Speed"), Range(1, 9999)] private float maxPoundSpeed = 2;

        [SerializeField, Tooltip("Don't forget to make the result ingredient before setting the recipe.")]
        private Ingredient outputIngredient = new();

        public Ingredient InputIngredient => inputIngredient;
        public float MinGrindSpeed => minGrindSpeed;
        public float MaxGrindSpeed => maxGrindSpeed;
        public float MinPoundSpeed => minPoundSpeed;
        public float MaxPoundSpeed => maxPoundSpeed;
        public Ingredient OutputIngredient => outputIngredient;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure ingredients are set
            if (inputIngredient == null)
                Debug.LogWarning($"[{name}] Input ingredient is not assigned.");
            if (outputIngredient == null)
                Debug.LogWarning($"[{name}] Output ingredient is not assigned.");

            // Ensure grind and pound speed thresholds make sense
            if (minGrindSpeed >= maxGrindSpeed)
            {
                Debug.LogWarning($"[{name}] Min Grind Speed should be less than Max Grind Speed.");
                maxGrindSpeed = minGrindSpeed + 1;
            }

            if (minPoundSpeed >= maxPoundSpeed)
            {
                Debug.LogWarning($"[{name}] Min Pound Speed should be less than Max Pound Speed.");
                maxPoundSpeed = minPoundSpeed + 1;
            }

            // Clamp values in case inspector was bypassed
            minGrindSpeed = Mathf.Clamp(minGrindSpeed, 1, 9998);
            maxGrindSpeed = Mathf.Clamp(maxGrindSpeed, minGrindSpeed + 1, 9999);

            minPoundSpeed = Mathf.Clamp(minPoundSpeed, 1, 9998);
            maxPoundSpeed = Mathf.Clamp(maxPoundSpeed, minPoundSpeed + 1, 9999);
        }
#endif
    }
}