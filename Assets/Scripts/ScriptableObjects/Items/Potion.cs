using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.Effects;
using FourFatesStudios.ProjectWarden.Structs;
using FourFatesStudios.ProjectWarden.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    [CreateAssetMenu(fileName = "NewPotion", menuName = "Items/Potion")]
    public class Potion : Item
    {
        [SerializeField, Tooltip("Potion item type.")]
        private ItemPotionType itemPotionType;

        [Header("Potion Potency")]
        [SerializeField] private int potency = 100;
        
        [Header("Infusion System")]
        [SerializeField, Tooltip("Infusions added to this potion")]
        private InfusionBundle infusionBundle = new InfusionBundle();
        
        [Header("Crafting System Properties")]
        [SerializeField] private PotionRarity rarity = PotionRarity.Common;
        [SerializeField] private PotionTone tone = PotionTone.Neutral;
        [SerializeField] private BottleType bottleType = BottleType.BasicVial;
        [SerializeField] private bool isCustomPotion = false;
        
        [Header("Visual Appearance")]
        [SerializeField] private Color primaryColor = Color.blue;
        [SerializeField] private Color secondaryColor = Color.white;
        [SerializeField] private PotionTurbidity turbidity = PotionTurbidity.Translucent;
        [SerializeField] private bool hasGlow = false;
        [SerializeField] private bool hasBubbles = false;
        [SerializeField] private bool hasParticles = false;
        
        [Header("Combat Integration")]
        [SerializeField] private List<AbilityInfusionTag> compatibleAbilities = new List<AbilityInfusionTag>();
        [SerializeField] private bool canBeUsedInCombat = true;
        [SerializeField] private bool canBeUsedOutOfCombat = true;
        [SerializeField] private bool canBeUsedInExploration = true;
        
        [Header("Usage Properties")]
        [SerializeField] private int maxStackSize = 10;
        [SerializeField] private float shelfLife = 7200f; // 2 hours default
        [SerializeField] private bool degradesOverTime = true;
        [SerializeField] private float consumeTime = 2f;
        
        [Header("Special Properties")]
        [SerializeField] private bool isKeyItem = false;
        [SerializeField] private bool isThrowable = false;
        [SerializeField] private float throwRange = 10f;
        [SerializeField] private bool affectsArea = false;
        [SerializeField] private float areaRadius = 3f;
        
        // Crafting metadata
        [SerializeField] private float craftQuality = 1.0f;
        [SerializeField] private string crafterName = "";
        [SerializeField] private List<Ingredient> sourceIngredients = new List<Ingredient>();

        private bool upgraded = false;

        public event EventHandler OnUpgradeStatusChanged;

        public ItemPotionType ItemPotionType => itemPotionType;
        public InfusionBundle InfusionBundle => infusionBundle;
        public int Potency => potency;
        
        // New crafting system properties
        public PotionRarity Rarity => rarity;
        public PotionTone Tone => tone;
        public BottleType BottleType => bottleType;
        public bool IsCustomPotion => isCustomPotion;
        public Color PrimaryColor => primaryColor;
        public Color SecondaryColor => secondaryColor;
        public PotionTurbidity Turbidity => turbidity;
        public bool HasGlow => hasGlow;
        public bool HasBubbles => hasBubbles;
        public bool HasParticles => hasParticles;
        public IReadOnlyList<AbilityInfusionTag> CompatibleAbilities => compatibleAbilities;
        public bool CanBeUsedInCombat => canBeUsedInCombat;
        public bool CanBeUsedOutOfCombat => canBeUsedOutOfCombat;
        public bool CanBeUsedInExploration => canBeUsedInExploration;
        public int MaxStackSize => maxStackSize;
        public float ShelfLife => shelfLife;
        public bool DegradesOverTime => degradesOverTime;
        public float ConsumeTime => consumeTime;
        public bool IsKeyItem => isKeyItem;
        public bool IsThrowable => isThrowable;
        public float ThrowRange => throwRange;
        public bool AffectsArea => affectsArea;
        public float AreaRadius => areaRadius;
        public float CraftQuality => craftQuality;
        public string CrafterName => crafterName;
        public IReadOnlyList<Ingredient> SourceIngredients => sourceIngredients;

        public bool Upgraded
        {
            get => upgraded;
            set
            {
                if (value == upgraded) return;
                upgraded = value;
                OnUpgradeStatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void InitializeFromCrafting(PotionRarity potionRarity, PotionTone potionTone, BottleType bottle, 
            InfusionBundle infusions, float quality, string crafter, List<Ingredient> ingredients, int basePotency = 100)
        {
            rarity = potionRarity;
            tone = potionTone;
            bottleType = bottle;
            infusionBundle = infusions ?? new InfusionBundle();
            craftQuality = quality;
            crafterName = crafter;
            potency = basePotency;
            if (ingredients != null) sourceIngredients = new List<Ingredient>(ingredients);
            
            UpdateAppearanceFromIngredients(ingredients);
            UpdatePropertiesFromInfusions();
        }
        
        private void UpdateAppearanceFromIngredients(List<Ingredient> ingredients)
        {
            if (ingredients == null || ingredients.Count == 0) return;
            
            // Calculate dominant aspect for color
            var aspectCounts = new Dictionary<Aspect, int>();
            var toneInfluences = new Dictionary<PotionTone, int>();
            
            foreach (var ingredient in ingredients)
            {
                // Count aspects
                if (aspectCounts.ContainsKey(ingredient.IngredientAspect))
                    aspectCounts[ingredient.IngredientAspect]++;
                else
                    aspectCounts[ingredient.IngredientAspect] = 1;
                
                // Determine tone influence
                var ingredientTone = DetermineIngredientTone(ingredient);
                if (toneInfluences.ContainsKey(ingredientTone))
                    toneInfluences[ingredientTone]++;
                else
                    toneInfluences[ingredientTone] = 1;
            }
            
            // Set colors based on dominant aspect
            var dominantAspect = GetDominantAspect(aspectCounts);
            primaryColor = GetAspectColor(dominantAspect);
            
            // Set tone based on dominant influence
            var dominantTone = GetDominantTone(toneInfluences);
            tone = dominantTone;
            
            // Adjust turbidity based on ingredient compatibility
            turbidity = CalculateTurbidity(ingredients);
        }
        
        private PotionTone DetermineIngredientTone(Ingredient ingredient)
        {
            if (ingredient.IsCorrupted) return PotionTone.Tainted;
            if (ingredient.IngredientAspect == Aspect.Divine) return PotionTone.Divine;
            if (ingredient.IngredientArchetype == IngredientArchetype.Herb) return PotionTone.Floral;
            
            return PotionTone.Neutral;
        }
        
        private Aspect GetDominantAspect(Dictionary<Aspect, int> aspectCounts)
        {
            Aspect dominant = Aspect.Corporeal;
            int maxCount = 0;
            
            foreach (var kvp in aspectCounts)
            {
                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    dominant = kvp.Key;
                }
            }
            
            return dominant;
        }
        
        private PotionTone GetDominantTone(Dictionary<PotionTone, int> toneInfluences)
        {
            PotionTone dominant = PotionTone.Neutral;
            int maxCount = 0;
            
            foreach (var kvp in toneInfluences)
            {
                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    dominant = kvp.Key;
                }
            }
            
            return dominant;
        }
        
        private Color GetAspectColor(Aspect aspect)
        {
            return aspect switch
            {
                Aspect.Scorch => new Color(0.9f, 0.3f, 0.1f), // Red-orange
                Aspect.Frigid => new Color(0.2f, 0.6f, 0.9f), // Ice blue
                Aspect.Arc => new Color(0.9f, 0.9f, 0.2f),    // Electric yellow
                Aspect.Divine => new Color(0.9f, 0.8f, 0.2f), // Golden
                Aspect.Caustic => new Color(0.6f, 1.0f, 0.2f, 1.0f), // Acid Green
                Aspect.Corporeal => new Color(0.4f, 0.7f, 0.3f), // Green
                _ => new Color(0.5f, 0.5f, 0.8f) // Default blue
            };
        }
        
        private PotionTurbidity CalculateTurbidity(List<Ingredient> ingredients)
        {
            float stabilitySum = 0f;
            foreach (var ingredient in ingredients)
            {
                stabilitySum += ingredient.StabilityRating;
            }
            
            float averageStability = stabilitySum / ingredients.Count;
            
            if (averageStability >= 0.8f) return PotionTurbidity.Clear;
            if (averageStability >= 0.5f) return PotionTurbidity.Translucent;
            return PotionTurbidity.Opaque;
        }
        
        private void UpdatePropertiesFromInfusions()
        {
            if (infusionBundle?.Infusions == null || infusionBundle.Infusions.Count == 0) return;
            
            var allEffects = infusionBundle.GetAllEffects();
            
            // Determine usage contexts based on effect types from infusions
            canBeUsedInCombat = HasEffectOfType<DamageEffect>() || HasEffectOfType<HealEffect>() || HasEffectOfType<BuffStatEffect>();
            canBeUsedOutOfCombat = HasEffectOfType<HealEffect>() || HasEffectOfType<BuffHealEffect>() || HasEffectOfType<BuffStatEffect>();
            canBeUsedInExploration = HasEffectOfType<BuffStatEffect>(); // Could add more specific exploration effects
            
            // Set visual effects based on infusion count and types
            hasGlow = infusionBundle.Infusions.Count > 3 || HasEffectOfType<BuffStatEffect>();
            hasBubbles = infusionBundle.Infusions.Count > 2;
            hasParticles = rarity >= PotionRarity.Rare || HasEffectOfType<DamageEffect>();
        }
        
        private bool HasEffectOfType<T>() where T : IEffect
        {
            // Check infusion effects only
            if (infusionBundle?.Infusions != null)
            {
                var infusionEffects = infusionBundle.GetAllEffects();
                return infusionEffects.Any(effect => effect is T);
            }
            
            return false;
        }
        
        /// <summary>
        /// Get all effects from infusions
        /// </summary>
        public List<IEffect> GetAllEffects()
        {
            var allEffects = new List<IEffect>();
            
            // Add infusion effects
            if (infusionBundle?.Infusions != null)
            {
                allEffects.AddRange(infusionBundle.GetAllEffects());
            }
            
            return allEffects;
        }
        
        /// <summary>
        /// Get total power level from all infusions
        /// </summary>
        public int GetInfusionPowerLevel()
        {
            return infusionBundle?.GetTotalPowerLevel() ?? 0;
        }
        
        /// <summary>
        /// Check if potion has any infusions
        /// </summary>
        public bool HasInfusions()
        {
            return infusionBundle?.Infusions?.Count > 0;
        }
        
        /// <summary>
        /// Add an infusion to this potion
        /// </summary>
        public void AddInfusion(Infusion infusion)
        {
            if (infusion != null)
            {
                infusionBundle.AddInfusion(infusion);
                UpdatePropertiesFromInfusions(); // Refresh properties based on new effects
            }
        }
        
        /// <summary>
        /// Remove an infusion from this potion
        /// </summary>
        public bool RemoveInfusion(Infusion infusion)
        {
            if (infusion != null && infusionBundle.RemoveInfusion(infusion))
            {
                UpdatePropertiesFromInfusions(); // Refresh properties after removal
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Get a description that includes infusion information
        /// </summary>
        public string GetEnhancedDescription()
        {
            string baseDesc = GetVisualDescription();
            
            if (HasInfusions())
            {
                var infusionNames = infusionBundle.Infusions.Select(i => i.InfusionName).Distinct();
                baseDesc += $"\\n\\nInfused with: {string.Join(", ", infusionNames)}";
                baseDesc += $"\\nInfusion Power: {GetInfusionPowerLevel()}";
            }
            
            return baseDesc;
        }
        
        public bool CanCombineWith(AbilityInfusionTag abilityTag)
        {
            return compatibleAbilities.Contains(abilityTag) || 
                   compatibleAbilities.Contains(AbilityInfusionTag.None);
        }
        
        public string GetVisualDescription()
        {
            string description = $"A {turbidity.ToString().ToLower()} {tone.ToString().ToLower()} potion";
            
            if (primaryColor != Color.white)
            {
                description += $" with a {GetColorName(primaryColor)} hue";
            }
            
            if (hasGlow) description += " that glows softly";
            if (hasBubbles) description += " with gentle bubbles";
            if (hasParticles) description += " containing swirling particles";
            
            description += $" contained in a {bottleType.ToString().ToLower().Replace("_", " ")}.";
            
            return description;
        }
        
        private string GetColorName(Color color)
        {
            if (color.r > 0.7f && color.g < 0.4f && color.b < 0.4f) return "crimson";
            if (color.g > 0.7f && color.r < 0.4f && color.b < 0.4f) return "emerald";
            if (color.b > 0.7f && color.r < 0.4f && color.g < 0.4f) return "sapphire";
            if (color.r > 0.7f && color.g > 0.7f && color.b < 0.4f) return "golden";
            if (color.r > 0.5f && color.g < 0.3f && color.b > 0.5f) return "violet";
            if (color.r > 0.6f && color.g > 0.3f && color.b < 0.3f) return "amber";
            
            return "mysterious";
        }

        /// <summary>
        /// Add an infusion containing the specified effect
        /// </summary>
        public void AddEffectViaInfusion(IEffect effect, string infusionName = "Custom Infusion")
        {
            if (effect == null)
                throw new ArgumentNullException(nameof(effect));
            
            // Create a temporary infusion to hold this effect
            var tempInfusion = ScriptableObject.CreateInstance<Infusion>();
            #if UNITY_EDITOR
            tempInfusion.InitializeInfusion(infusionName, $"Contains {effect.GetType().Name}", Color.white);
            // Note: This would require extending Infusion to support runtime effect addition
            #endif
            
            AddInfusion(tempInfusion);
        }
        
        
        public bool IsStackableWith(Potion other)
        {
            if (other == null) return false;
            if (ItemPotionType != other.ItemPotionType) return false;
            if (Upgraded != other.Upgraded) return false;
            
            // Compare infusion bundles
            if (infusionBundle?.Infusions?.Count != other.infusionBundle?.Infusions?.Count) return false;
            
            if (infusionBundle?.Infusions != null && other.infusionBundle?.Infusions != null)
            {
                // Check if both have the same infusions
                for (int i = 0; i < infusionBundle.Infusions.Count; i++)
                {
                    bool foundMatch = false;
                    for (int j = 0; j < other.infusionBundle.Infusions.Count; j++)
                    {
                        if (infusionBundle.Infusions[i] == other.infusionBundle.Infusions[j])
                        {
                            foundMatch = true;
                            break;
                        }
                    }
                    if (!foundMatch) return false;
                }
            }

            return true;
        }

        public string GetEffectSignature()
        {
            StringBuilder builder = new();

            builder.Append(ItemPotionType.ToString());
            builder.Append("_");
            builder.Append(Upgraded ? "U" : "N");

            // Include InfusionBundle effects if they exist
            if (infusionBundle?.Infusions != null)
            {
                var allEffects = infusionBundle.GetAllEffects();
                foreach (var effect in allEffects)
                {
                    builder.Append("|");
                    builder.Append(effect.GetType().Name);
                    // Add specific effect properties based on type
                    builder.Append(":");
                    builder.Append(GetEffectSpecificSignature(effect));
                }
            }

            return ComputeHash(builder.ToString());
        }
        
        private string GetEffectSpecificSignature(IEffect effect)
        {
            return effect switch
            {
                HealEffect healEffect => $"H:{healEffect.BaseHeal}",
                DamageEffect damageEffect => $"D:{damageEffect.BaseDamage}:{damageEffect.DamageType}:{damageEffect.Aspect}",
                BuffStatEffect buffEffect => $"BS:{buffEffect.GetType().Name}",
                DebuffStatEffect debuffEffect => $"DS:{debuffEffect.GetType().Name}",
                ShieldEffect shieldEffect => $"S:{shieldEffect.GetType().Name}",
                _ => effect.GetType().Name
            };
        }

        private string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return System.Convert.ToBase64String(hash);
        }

#if UNITY_EDITOR
        private new void OnValidate()
        {
            // Initialize infusion bundle if it doesn't exist
            if (infusionBundle == null)
            {
                infusionBundle = new InfusionBundle();
            }
            
            // Update properties based on current infusions
            UpdatePropertiesFromInfusions();
        }
#endif
    }
}
