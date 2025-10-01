using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.Effects;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects
{
    [System.Serializable]
    public class InfusionBundle
    {
        [SerializeField, Tooltip("List of infusions in this bundle")]
        private List<Infusion> infusions = new List<Infusion>();
        
        public List<Infusion> Infusions => infusions;
        
        /// <summary>
        /// Add an infusion to this bundle
        /// </summary>
        public void AddInfusion(Infusion infusion)
        {
            if (infusion == null) return;
            
            // Check if we can stack this infusion
            if (infusion.CanStack)
            {
                var existingCount = infusions.Count(i => i == infusion);
                if (existingCount < infusion.MaxStacks)
                {
                    infusions.Add(infusion);
                }
            }
            else
            {
                // Don't add if already exists
                if (!infusions.Contains(infusion))
                {
                    infusions.Add(infusion);
                }
            }
        }
        
        /// <summary>
        /// Remove an infusion from this bundle
        /// </summary>
        public bool RemoveInfusion(Infusion infusion)
        {
            return infusions.Remove(infusion);
        }
        
        /// <summary>
        /// Remove all instances of an infusion
        /// </summary>
        public int RemoveAllInfusions(Infusion infusion)
        {
            return infusions.RemoveAll(i => i == infusion);
        }
        
        /// <summary>
        /// Clear all infusions
        /// </summary>
        public void Clear()
        {
            infusions.Clear();
        }
        
        /// <summary>
        /// Check if this bundle contains a specific infusion
        /// </summary>
        public bool ContainsInfusion(Infusion infusion)
        {
            return infusions.Contains(infusion);
        }
        
        /// <summary>
        /// Get the stack count of a specific infusion
        /// </summary>
        public int GetStackCount(Infusion infusion)
        {
            return infusions.Count(i => i == infusion);
        }
        
        /// <summary>
        /// Get all unique infusions (ignoring stacks)
        /// </summary>
        public List<Infusion> GetUniqueInfusions()
        {
            return infusions.Distinct().ToList();
        }
        
        /// <summary>
        /// Get all effects from all infusions combined
        /// </summary>
        public List<IEffect> GetAllEffects()
        {
            var allEffects = new List<IEffect>();
            
            foreach (var infusion in infusions)
            {
                if (infusion?.EffectBundle?.Effects != null)
                {
                    allEffects.AddRange(infusion.EffectBundle.Effects);
                }
            }
            
            return allEffects;
        }
        
        /// <summary>
        /// Get all effect categories from this bundle
        /// </summary>
        public List<string> GetAllEffectCategories()
        {
            var categories = new List<string>();
            
            foreach (var infusion in infusions)
            {
                if (infusion != null)
                {
                    var infusionCategories = infusion.GetEffectCategories();
                    foreach (var category in infusionCategories)
                    {
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
        /// Check if bundle has any effects
        /// </summary>
        public bool HasEffects()
        {
            return infusions.Any(i => i?.EffectBundle?.Effects != null && i.EffectBundle.Effects.Count > 0);
        }
        
        /// <summary>
        /// Get total power level of all infusions
        /// </summary>
        public int GetTotalPowerLevel()
        {
            return infusions.Where(i => i != null).Sum(i => i.PowerLevel);
        }
        
        /// <summary>
        /// Validate this bundle for completeness
        /// </summary>
        public bool IsValid(out string validationMessage)
        {
            if (infusions == null)
            {
                validationMessage = "Infusions list is null";
                return false;
            }
            
            if (infusions.Any(i => i == null))
            {
                validationMessage = "Bundle contains null infusions";
                return false;
            }
            
            foreach (var infusion in infusions)
            {
                if (!infusion.IsValid(out string infusionError))
                {
                    validationMessage = $"Invalid infusion '{infusion.InfusionName}': {infusionError}";
                    return false;
                }
            }
            
            validationMessage = "Valid";
            return true;
        }
    }
}