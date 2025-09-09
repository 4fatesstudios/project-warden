using System;
using System.Collections.Generic;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.Structs;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Crafting
{
    [CreateAssetMenu(fileName = "NewPotionRecipe", menuName = "Crafting/Potion Recipe")]
    public class PotionRecipe : ScriptableObject
    {
        [Header("Recipe Identity")]
        [SerializeField] private string recipeName;
        [SerializeField] private string recipeDescription;
        [SerializeField] private Sprite recipeIcon;
        [SerializeField] private PotionRarity rarity = PotionRarity.Common;
        [SerializeField] private bool isUniqueRecipe = false;
        
        [Header("Required Ingredients")]
        [SerializeField] private List<RecipeIngredient> requiredIngredients = new List<RecipeIngredient>();
        [SerializeField] private List<RecipeIngredient> optionalIngredients = new List<RecipeIngredient>();
        [SerializeField] private Ingredient accentIngredient; // Optional enhancement
        
        [Header("Brewing Configuration")]
        [SerializeField] private BrewMethod recommendedBrewMethod = BrewMethod.StandardBrew;
        [SerializeField] private List<BrewMethod> validBrewMethods = new List<BrewMethod>();
        [SerializeField] private float baseBrewingTime = 60f;
        [SerializeField] private bool requiresSpecificOrder = false;
        [SerializeField] private List<int> ingredientOrder = new List<int>(); // Index order if specific order required
        
        [Header("Results")]
        [SerializeField] private Potion resultPotion;
        [SerializeField] private int baseYield = 1;
        [SerializeField] private float qualityMultiplier = 1.0f;
        [SerializeField] private List<BottleType> compatibleBottles = new List<BottleType>();
        
        [Header("Discovery & Mastery")]
        [SerializeField] private bool isKnownFromStart = false;
        [SerializeField] private string discoveryHint;
        [SerializeField] private List<Ingredient> discoveryTriggerIngredients = new List<Ingredient>();
        [SerializeField] private int masteryThreshold = 3;
        [SerializeField] private bool canBeMastered = true;
        
        [Header("Special Requirements")]
        [SerializeField] private int minimumAlchemySkill = 1;
        [SerializeField] private List<string> specialRequirements = new List<string>();
        [SerializeField] private bool requiresAdvancedEquipment = false;
        
        [Header("Effects & Appearance")]
        [SerializeField] private PotionTone expectedTone = PotionTone.Neutral;
        [SerializeField] private Color primaryColor = Color.blue;
        [SerializeField] private Color secondaryColor = Color.white;
        [SerializeField] private PotionTurbidity expectedTurbidity = PotionTurbidity.Translucent;
        
        // Properties
        public string RecipeName => recipeName;
        public string RecipeDescription => recipeDescription;
        public Sprite RecipeIcon => recipeIcon;
        public PotionRarity Rarity => rarity;
        public bool IsUniqueRecipe => isUniqueRecipe;
        public IReadOnlyList<RecipeIngredient> RequiredIngredients => requiredIngredients;
        public IReadOnlyList<RecipeIngredient> OptionalIngredients => optionalIngredients;
        public Ingredient AccentIngredient => accentIngredient;
        public BrewMethod RecommendedBrewMethod => recommendedBrewMethod;
        public IReadOnlyList<BrewMethod> ValidBrewMethods => validBrewMethods;
        public float BaseBrewingTime => baseBrewingTime;
        public bool RequiresSpecificOrder => requiresSpecificOrder;
        public IReadOnlyList<int> IngredientOrder => ingredientOrder;
        public Potion ResultPotion => resultPotion;
        public int BaseYield => baseYield;
        public float QualityMultiplier => qualityMultiplier;
        public IReadOnlyList<BottleType> CompatibleBottles => compatibleBottles;
        public bool IsKnownFromStart => isKnownFromStart;
        public string DiscoveryHint => discoveryHint;
        public IReadOnlyList<Ingredient> DiscoveryTriggerIngredients => discoveryTriggerIngredients;
        public int MasteryThreshold => masteryThreshold;
        public bool CanBeMastered => canBeMastered;
        public int MinimumAlchemySkill => minimumAlchemySkill;
        public IReadOnlyList<string> SpecialRequirements => specialRequirements;
        public bool RequiresAdvancedEquipment => requiresAdvancedEquipment;
        public PotionTone ExpectedTone => expectedTone;
        public Color PrimaryColor => primaryColor;
        public Color SecondaryColor => secondaryColor;
        public PotionTurbidity ExpectedTurbidity => expectedTurbidity;
        
        public bool HasIngredient(Ingredient ingredient)
        {
            foreach (var reqIngredient in requiredIngredients)
            {
                if (reqIngredient.ingredient == ingredient)
                    return true;
            }
            
            foreach (var optIngredient in optionalIngredients)
            {
                if (optIngredient.ingredient == ingredient)
                    return true;
            }
            
            return accentIngredient == ingredient;
        }
        
        public bool CanDiscoverWith(List<Ingredient> availableIngredients)
        {
            if (isKnownFromStart || discoveryTriggerIngredients.Count == 0)
                return true;
                
            foreach (var triggerIngredient in discoveryTriggerIngredients)
            {
                if (availableIngredients.Contains(triggerIngredient))
                    return true;
            }
            
            return false;
        }
        
        public float CalculateBrewingTime(BrewMethod method, int playerSkill)
        {
            float timeMultiplier = method switch
            {
                BrewMethod.QuickBrew => 0.5f,
                BrewMethod.StandardBrew => 1.0f,
                BrewMethod.SlowBrew => 2.0f,
                BrewMethod.ColdBrew => 3.0f,
                BrewMethod.Fermentation => 5.0f,
                BrewMethod.Distillation => 1.5f,
                BrewMethod.Sublimation => 4.0f,
                _ => 1.0f
            };
            
            // Higher skill reduces brewing time
            float skillReduction = 1.0f - (playerSkill * 0.005f); // Max 50% reduction at skill 100
            skillReduction = Mathf.Clamp(skillReduction, 0.5f, 1.0f);
            
            return baseBrewingTime * timeMultiplier * skillReduction;
        }
        
        public float CalculateSuccessRate(BrewMethod method, int playerSkill, bool hasOptimalIngredients)
        {
            float baseRate = 0.7f;
            
            // Method affects success rate
            if (validBrewMethods.Contains(method))
            {
                baseRate += 0.2f;
                if (method == recommendedBrewMethod)
                    baseRate += 0.1f;
            }
            else
            {
                baseRate -= 0.3f; // Penalty for wrong method
            }
            
            // Skill bonus
            baseRate += (playerSkill - minimumAlchemySkill) * 0.01f;
            
            // Ingredient quality bonus
            if (hasOptimalIngredients)
                baseRate += 0.15f;
            
            return Mathf.Clamp01(baseRate);
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure we have at least one valid brew method
            if (validBrewMethods.Count == 0)
            {
                validBrewMethods.Add(recommendedBrewMethod);
            }
            
            // Validate ingredient order matches required ingredients
            if (requiresSpecificOrder && ingredientOrder.Count != requiredIngredients.Count)
            {
                Debug.LogWarning($"[{name}] Ingredient order count doesn't match required ingredients count!", this);
            }
            
            // Ensure compatible bottles list isn't empty
            if (compatibleBottles.Count == 0)
            {
                compatibleBottles.Add(BottleType.BasicVial);
            }
            
            // Validate discovery triggers
            if (!isKnownFromStart && discoveryTriggerIngredients.Count == 0)
            {
                Debug.LogWarning($"[{name}] Recipe is not known from start but has no discovery triggers!", this);
            }
        }
#endif
        
        public override string ToString()
        {
            return $"{recipeName} ({rarity}) - {requiredIngredients.Count} ingredients, {baseBrewingTime:F0}s brew time";
        }
    }
    
    [Serializable]
    public struct RecipeIngredient
    {
        public Ingredient ingredient;
        public int quantity;
        public bool acceptsSubstitutes;
        public List<Ingredient> substitutes;
        
        public RecipeIngredient(Ingredient ingredient, int quantity = 1, bool acceptsSubstitutes = false)
        {
            this.ingredient = ingredient;
            this.quantity = quantity;
            this.acceptsSubstitutes = acceptsSubstitutes;
            this.substitutes = new List<Ingredient>();
        }
    }
}