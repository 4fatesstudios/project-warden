using System;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Roasts
{
    [CreateAssetMenu(fileName = "NewRoast", menuName = "Roasts/Generic Roast")]
    public class Roast : ScriptableObject
    {
        [SerializeField, Tooltip("Ingredient to be roasted.")]
        private Ingredient inputIngredient;

        [SerializeField, Tooltip("Ingredient produced after roasting.")]
        private Ingredient outputIngredient;

        [SerializeField, Tooltip("Total time allowed to roast (in seconds).")]
        private float totalCookTime = 10f;

        [SerializeField, Tooltip("Number of ingredients displayed during roasting.")]
        private int totalDisplayedIngredients = 3;

        [SerializeField, Tooltip("Amount of time required to be considered cooked (in seconds).")]
        private float requiredCookedTime = 6f;

        [Header("Roast State Thresholds")]
        [SerializeField, Tooltip("Minimum number of uncooked pieces allowed for success.")]
        private int uncookedThreshold = 0;

        [SerializeField, Tooltip("Minimum number of properly cooked pieces required.")]
        private int cookedThreshold = 2;

        [SerializeField, Tooltip("Maximum number of burnt pieces allowed.")]
        private int burntThreshold = 1;

        // Optional fields for corrupted cooking logic
        [SerializeField] private bool requiresCorruptedCooking = false;
        [SerializeField] private int requiredCookCount = 0;

        // Runtime-tracking cook progress (optional)
        [HideInInspector] public float cookedTime;

        // Properties
        public Ingredient InputIngredient => inputIngredient;
        public Ingredient OutputIngredient => outputIngredient;
        public float TotalCookTime => totalCookTime;
        public int TotalDisplayedIngredients => totalDisplayedIngredients;

        public float RequiredCookedTime => RequiresCorruptedCooking
            ? requiredCookedTime * 0.5f
            : requiredCookedTime;

        public int UncookedThreshold => uncookedThreshold;
        public int CookedThreshold => cookedThreshold;
        public int BurntThreshold => burntThreshold;

        public bool RequiresCorruptedCooking => requiresCorruptedCooking;
        public int RequiredCookCount => requiredCookCount;

        // Optional: for displaying current cook progress in the editor
        public float CookedTime => cookedTime;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (inputIngredient == null)
                Debug.LogWarning($"[{name}] Input ingredient is not assigned.");

            if (outputIngredient == null)
                Debug.LogWarning($"[{name}] Output ingredient is not assigned.");

            if (totalCookTime <= 0)
                Debug.LogWarning($"[{name}] Total cook time must be greater than 0.");

            if (requiredCookedTime <= 0)
                Debug.LogWarning($"[{name}] Required cooked time must be greater than 0.");

            if (requiredCookedTime > totalCookTime)
                Debug.LogWarning($"[{name}] Required cooked time exceeds total cook time.");

            if (totalDisplayedIngredients < 1)
                Debug.LogWarning($"[{name}] At least one displayed ingredient is required.");

            if (uncookedThreshold < 0 || cookedThreshold < 0 || burntThreshold < 0)
                Debug.LogWarning($"[{name}] Thresholds cannot be negative.");

            int totalThresholds = uncookedThreshold + cookedThreshold + burntThreshold;
            if (totalThresholds > totalDisplayedIngredients)
                Debug.LogWarning($"[{name}] Sum of thresholds ({totalThresholds}) exceeds total displayed ingredients ({totalDisplayedIngredients}).");

            if (RequiresCorruptedCooking && cookedThreshold > totalDisplayedIngredients)
                Debug.LogWarning($"[{name}] Corrupted item requires more cooked pieces than total available.");
        }
#endif
    }
}
