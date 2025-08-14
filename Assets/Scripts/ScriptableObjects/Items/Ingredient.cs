using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Structs;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    [CreateAssetMenu(fileName = "NewIngredient", menuName = "Items/Ingredient")]
    public class Ingredient : Item {
        [SerializeField, Tooltip("Descriptive noun for custom potion naming.")] 
        private string noun;
        
        [SerializeField, Tooltip("Descriptive adjective for custom potion naming.")]
        private string adjective;
        
        [SerializeField, Tooltip("Alchemical ingredient type.")]
        private IngredientArchetype ingredientArchetype;

        [SerializeField, Tooltip("Alchemical ingredient aspect.")]
        private Aspect ingredientAspect;

        [SerializeField, Tooltip("Is alchemical ingredient corrupted or not.")]
        private bool isCorrupted;

        [Header("Crafting Properties")]
        [SerializeField, Tooltip("Potency level for alchemy crafting (1-5, higher = stronger effects).")]
        [Range(1, 5)]
        private int potency = 1;

        [SerializeField, Tooltip("Grid width for tetris-style crafting.")]
        [Range(1, 4)]
        private int gridWidth = 1;

        [SerializeField, Tooltip("Grid height for tetris-style crafting.")]
        [Range(1, 4)]
        private int gridHeight = 1;

        [SerializeField, Tooltip("Can this ingredient unlock additional grid space when placed?")]
        private bool unlocksAdditionalSpace = false;

        [SerializeField, Tooltip("Additional grid spaces unlocked (if applicable).")]
        [Range(0, 8)]
        private int additionalSpaceCount = 0;

        [Header("Ingredient Effect")] 
        [SerializeField, Tooltip("The infusion(s) given from the ingredient")] 
        private List<Infusion> infusions;

        [Header("Refinement")] 
        [SerializeField] private bool canGrind;
        [SerializeField] private Ingredient grindingResult;
        
        [SerializeField] private bool canDistill;
        [SerializeField] private Ingredient distillingResult;
        
        [SerializeField] private bool canRoast;
        [SerializeField] private Ingredient roastingResult;
        
        // DEPRECATED, REMOVE ALL USES, to delete
        private List<PotionEffect> potionEffects;
        public IReadOnlyList<PotionEffect> PotionEffects => potionEffects;
        // end to delete

        public string Noun => noun;
        public string Adjective => adjective;
        public IngredientArchetype IngredientArchetype => ingredientArchetype;
        public Aspect IngredientAspect => ingredientAspect;
        public bool IsCorrupted => isCorrupted;
        public virtual int Potency => potency;
        public virtual int GridWidth => gridWidth;
        public virtual int GridHeight => gridHeight;
        public virtual bool UnlocksAdditionalSpace => unlocksAdditionalSpace;
        public virtual int AdditionalSpaceCount => additionalSpaceCount;
        public List<Infusion> Infusions => infusions;
        public bool CanGrind => canGrind;
        public Ingredient GrindingResult => grindingResult;
        public bool CanDistill => canDistill;
        public Ingredient DistillingResult => distillingResult;
        public bool CanRoast => canRoast;
        public Ingredient RoastingResult => roastingResult;

#if UNITY_EDITOR
        private new void OnValidate()
        {
            base.OnValidate();
            if (potionEffects == null)
                return;

            if (potionEffects.Contains(null))
            {
                Debug.LogWarning($"[{name}] contains null entries in its PotionEffects list.", this);
            }
            
            if (!canGrind) grindingResult = null;
            if (!canDistill) distillingResult = null;
            if (!canRoast) roastingResult = null;
        }
#endif
        public override string ToString()
        {
            return $"{ItemName} ({IngredientArchetype}) - {ItemDescription}";
        }
    }
}