using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.Effects;
using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewInfusion", menuName = "Alchemy/Infusion")]
    public class Infusion : ScriptableObject
    {
        [Header("Infusion Identity")]
        [SerializeField, Tooltip("The name of this infusion")]
        private string infusionName = "New Infusion";
        
        [SerializeField, Tooltip("Descriptive text about what this infusion does")]
        [TextArea(3, 5)]
        private string description = "";
        
        [SerializeField, Tooltip("Category this infusion belongs to")]
        private InfusionCategory category = InfusionCategory.Elemental;
        
        [SerializeField, Tooltip("Rarity of this infusion")]
        private Rarity rarity = Rarity.Common;
        
        [Header("Visual Properties")]
        [SerializeField, Tooltip("Color associated with this infusion for visual effects")]
        private Color infusionColor = Color.white;
        
        [SerializeField, Tooltip("Icon representing this infusion")]
        private Sprite infusionIcon;
        
        [Header("Effects")]
        [SerializeField, Tooltip("Bundle of effects this infusion provides")]
        private EffectBundle effectBundle = new EffectBundle();
        
        [Header("Game Balance")]
        [SerializeField, Tooltip("Power level of this infusion (1-10)"), Range(1, 10)]
        private int powerLevel = 1;
        
        [SerializeField, Tooltip("Can this infusion stack with others of the same type?")]
        private bool canStack = false;
        
        [SerializeField, Tooltip("Maximum stacks allowed (if stacking is enabled)"), Range(1, 10)]
        private int maxStacks = 1;
        
        // Public Properties
        public string InfusionName => infusionName;
        public string Description => description;
        public InfusionCategory Category => category;
        public Rarity Rarity => rarity;
        public Color InfusionColor => infusionColor;
        public Sprite InfusionIcon => infusionIcon;
        public EffectBundle EffectBundle => effectBundle;
        public int PowerLevel => powerLevel;
        public bool CanStack => canStack;
        public int MaxStacks => maxStacks;
        
        /// <summary>
        /// Get all effect types in this infusion's effect bundle
        /// </summary>
        public List<System.Type> GetEffectTypes()
        {
            var types = new List<System.Type>();
            if (effectBundle?.Effects != null)
            {
                foreach (var effect in effectBundle.Effects)
                {
                    if (effect != null)
                    {
                        types.Add(effect.GetType());
                    }
                }
            }
            return types;
        }
        
        /// <summary>
        /// Check if this infusion contains effects of a specific type
        /// </summary>
        public bool HasEffectOfType<T>() where T : IEffect
        {
            return GetEffectTypes().Any(type => typeof(T).IsAssignableFrom(type));
        }
        
        /// <summary>
        /// Get a string representation of effect categories for filtering
        /// </summary>
        public List<string> GetEffectCategories()
        {
            var categories = new List<string>();
            if (effectBundle?.Effects != null)
            {
                foreach (var effect in effectBundle.Effects)
                {
                    if (effect != null)
                    {
                        string category = GetEffectCategory(effect.GetType());
                        if (!categories.Contains(category))
                        {
                            categories.Add(category);
                        }
                    }
                }
            }
            return categories;
        }
        
        /// <summary>
        /// Get the category name for an effect type
        /// </summary>
        public static string GetEffectCategory(System.Type effectType)
        {
            if (effectType == null) return "Unknown";
            
            string typeName = effectType.Name;
            
            // Categorize based on naming patterns
            if (typeName.Contains("Damage")) return "Damage";
            if (typeName.Contains("Heal")) return "Healing";
            if (typeName.Contains("Shield")) return "Defense";
            if (typeName.Contains("Buff")) return "Buff";
            if (typeName.Contains("Debuff")) return "Debuff";
            if (typeName.Contains("DOT")) return "Damage Over Time";
            if (typeName.Contains("Stat")) return "Stat Modification";
            
            return "Utility";
        }
        
        /// <summary>
        /// Validate this infusion for completeness
        /// </summary>
        public bool IsValid(out string validationMessage)
        {
            if (string.IsNullOrEmpty(infusionName))
            {
                validationMessage = "Infusion name cannot be empty";
                return false;
            }
            
            if (effectBundle == null || effectBundle.Effects == null || effectBundle.Effects.Count == 0)
            {
                validationMessage = "Infusion must have at least one effect";
                return false;
            }
            
            if (effectBundle.Effects.Any(e => e == null))
            {
                validationMessage = "Infusion contains null effects";
                return false;
            }
            
            validationMessage = "Valid";
            return true;
        }
        
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(infusionName))
                infusionName = name;
                
            if (effectBundle == null)
                effectBundle = new EffectBundle();
                
            if (canStack && maxStacks < 1)
                maxStacks = 1;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only method to initialize infusion fields directly
        /// </summary>
        public void InitializeInfusion(string newName, string newDescription, Color newColor, InfusionCategory newCategory = InfusionCategory.Elemental)
        {
            infusionName = newName;
            description = newDescription;
            infusionColor = newColor;
            category = newCategory;
            if (effectBundle == null)
                effectBundle = new EffectBundle();
            
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
    
    [System.Serializable]
    public enum InfusionCategory
    {
        Elemental,      // Fire, Ice, Lightning, etc.
        Physical,       // Strength, Speed, etc.
        Mental,         // Intelligence, Wisdom, etc.
        Magical,        // Mana, Spell effects, etc.
        Defensive,      // Shields, Armor, etc.
        Offensive,      // Damage boosters, etc.
        Utility,        // Special effects, misc
        Alchemical,     // Alchemy-specific effects
        Corrupted,      // Negative or dark effects
        Divine         // Holy or sacred effects
    }
}