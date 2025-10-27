using System;
using UnityEngine;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.Structs
{
    [Serializable]
    public struct RefiningData
    {
        [Header("Refining Configuration")]
        [Tooltip("Type of refining process required")]
        public RefiningType refiningType;
        
        [Tooltip("Difficulty of the refining process")]
        public RefiningDifficulty difficulty;
        
        [Header("Success Rates")]
        [Tooltip("Base success rate (0.0 to 1.0)")]
        [Range(0f, 1f)]
        public float baseSuccessRate;
        
        [Tooltip("Chance of critical success (enhanced result)")]
        [Range(0f, 1f)]
        public float criticalSuccessRate;
        
        [Tooltip("Chance of ingredient being lost on failure")]
        [Range(0f, 1f)]
        public float lossRate;
        
        [Header("Results")]
        [Tooltip("Result on successful refining")]
        public Ingredient successResult;
        
        [Tooltip("Result on critical success (optional)")]
        public Ingredient criticalResult;
        
        [Header("Requirements")]
        [Tooltip("Minimum skill level required")]
        [Range(1, 100)]
        public int minimumSkillLevel;
        
        [Tooltip("Processing time in seconds")]
        [Range(1f, 300f)]
        public float processingTime;
        
        [Tooltip("Special conditions for this refining")]
        public string specialRequirements;
        
        public bool IsValidForArchetype(IngredientArchetype archetype)
        {
            return refiningType switch
            {
                RefiningType.Grinding => archetype == IngredientArchetype.Ore,
                RefiningType.Distilling => archetype == IngredientArchetype.Herb || 
                                         archetype == IngredientArchetype.Organic || 
                                         archetype == IngredientArchetype.Solvent,
                RefiningType.Roasting => archetype == IngredientArchetype.Herb || 
                                       archetype == IngredientArchetype.Organic,
                _ => false
            };
        }
        
        public float CalculateSuccessRate(int playerSkillLevel, float equipmentBonus = 0f)
        {
            if (playerSkillLevel < minimumSkillLevel)
                return 0f;
                
            float skillMultiplier = 1f + ((playerSkillLevel - minimumSkillLevel) * 0.05f);
            return Mathf.Clamp01(baseSuccessRate * skillMultiplier + equipmentBonus);
        }
        
        public static RefiningData CreateDefault(RefiningType type)
        {
            return new RefiningData
            {
                refiningType = type,
                difficulty = RefiningDifficulty.Easy,
                baseSuccessRate = 0.7f,
                criticalSuccessRate = 0.1f,
                lossRate = 0.2f,
                minimumSkillLevel = 1,
                processingTime = 30f,
                specialRequirements = ""
            };
        }
    }
}