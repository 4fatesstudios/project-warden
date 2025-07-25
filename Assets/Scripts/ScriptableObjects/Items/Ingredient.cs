using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Structs;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    [CreateAssetMenu(fileName = "NewIngredient", menuName = "Items/Ingredient")]
    public class Ingredient : Item
    {
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
        
        // to delete
        [Header("DEPRECATED, REMOVE ALL USES")]
        [SerializeField, Tooltip("Potion effects this ingredient can provide.")]
        private List<PotionEffect> potionEffects;
        public IReadOnlyList<PotionEffect> PotionEffects => potionEffects;
        // end to delete

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
            if (potionEffects == null)
                return;

            if (potionEffects.Contains(null))
            {
                Debug.LogWarning($"[{name}] contains null entries in its PotionEffects list.", this);
            }
        }
#endif
        public override string ToString()
        {
            return $"{ItemName} ({IngredientArchetype}) - {ItemDescription}";
        }
    }
}