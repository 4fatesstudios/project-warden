using System;
using System.Collections.Generic;
using UnityEngine;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Distills
{
    [CreateAssetMenu(fileName = "NewDistill", menuName = "Distills/Generic Distill")]
    public class Distill : ScriptableObject
    {
        [SerializeField, Tooltip("Input ingredient to be distilled.")]
        private Ingredient inputIngredient;

        [SerializeField, Tooltip("Output ingredient after distillation.")]
        private Ingredient outputIngredient;

        [SerializeField, Tooltip("How long the process must stay in the correct state (seconds).")]
        private float requiredDuration = 5f;

        [SerializeField, Tooltip("Maximum time allowed to complete the distillation (seconds).")]
        private float totalDuration = 10f;

        [SerializeField, Tooltip("Ordered progression stages (e.g., None Å® Shiny Å® Sparkling Å® ShinyAndSparkling).")]
        private List<DistillVisualState> progressionStages = new();

        public Ingredient InputIngredient => inputIngredient;
        public Ingredient OutputIngredient => outputIngredient;

        public float RequiredDuration => requiredDuration;
        public float TotalDuration => totalDuration;

        public IReadOnlyList<DistillVisualState> ProgressionStages => progressionStages;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (inputIngredient == null)
                Debug.LogWarning($"[{name}] Input ingredient is not assigned.");
            if (outputIngredient == null)
                Debug.LogWarning($"[{name}] Output ingredient is not assigned.");

            if (progressionStages == null || progressionStages.Count == 0)
            {
                Debug.LogWarning($"[{name}] No progression stages set. Distillation may not function.");
            }
            else if (HasDuplicateStages())
            {
                Debug.LogWarning($"[{name}] Duplicate visual states detected in progression stages. Consider using distinct values.");
            }
            if (progressionStages.Count > 4)
            {
                Debug.LogWarning($"[{name}] More than 4 progression stages detected. Make sure your distillation logic supports this.");
            }
        }

        private bool HasDuplicateStages()
        {
            if (progressionStages == null) return false;

            var unique = new HashSet<DistillVisualState>(progressionStages);
            return unique.Count < progressionStages.Count;
        }
#endif
    }
}