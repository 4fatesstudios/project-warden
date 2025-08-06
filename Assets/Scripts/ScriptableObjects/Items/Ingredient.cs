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
        public List<Infusion> Infusions => infusions;
        public bool CanGrind => canGrind;
        public Ingredient GrindingResult => grindingResult;
        public bool CanDistill => canDistill;
        public Ingredient DistillingResult => distillingResult;
        public bool CanRoast => canRoast;
        public Ingredient RoastingResult => roastingResult;

#if UNITY_EDITOR
        private void OnValidate()
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