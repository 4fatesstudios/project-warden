using UnityEngine;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.Structs;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    [CreateAssetMenu(fileName = "NewRefinedIngredient", menuName = "Items/Refined Ingredient")]
    public class RefinedIngredient : Ingredient
    {
        [Header("Refined Properties")]
        [SerializeField, Tooltip("The original ingredient before refining")]
        private Ingredient baseIngredient;
        
        [SerializeField, Tooltip("Type of refining process used")]
        private RefiningType refiningMethod;
        
        [SerializeField, Tooltip("Was this a critical success result?")]
        private bool isCriticalResult = false;
        
        [SerializeField, Tooltip("Purity level of the refined ingredient (affects potency)")]
        [Range(0.1f, 2.0f)]
        private float purityMultiplier = 1.0f;
        
        [SerializeField, Tooltip("Stability of the refined ingredient (affects shelf life)")]
        [Range(10, 100)]
        private int stability = 100;
        
        [Header("Refined Effects")]
        [SerializeField, Tooltip("How much more potent than the base ingredient")]
        [Range(1.0f, 5.0f)]
        private float potencyMultiplier = 1.5f;
        
        [SerializeField, Tooltip("Additional effects gained through refining")]
        private string refinedEffectDescription;
        
        public Ingredient BaseIngredient => baseIngredient;
        public RefiningType RefiningMethod => refiningMethod;
        public bool IsCriticalResult => isCriticalResult;
        public float PurityMultiplier => purityMultiplier;
        public int Stability => stability;
        public float PotencyMultiplier => potencyMultiplier;
        public string RefinedEffectDescription => refinedEffectDescription;
        
        public override int Potency => Mathf.RoundToInt(base.Potency * potencyMultiplier * purityMultiplier);
        
        public void Initialize(Ingredient source, RefiningType method, bool critical = false, float purity = 1.0f)
        {
            baseIngredient = source;
            refiningMethod = method;
            isCriticalResult = critical;
            purityMultiplier = purity;
            
            // Generate refined name
            string prefix = method switch
            {
                RefiningType.Grinding => critical ? "Finely Ground" : "Ground",
                RefiningType.Distilling => critical ? "Pure Distilled" : "Distilled", 
                RefiningType.Roasting => critical ? "Perfectly Roasted" : "Roasted",
                _ => "Refined"
            };
            
            // Update item name
            var nameField = typeof(Item).GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            nameField?.SetValue(this, $"{prefix} {source.ItemName}");
            
            // Update description
            var descField = typeof(Item).GetField("itemDescription", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            string newDesc = $"A refined version of {source.ItemName} created through {method.ToString().ToLower()}.";
            if (critical) newDesc += " This exemplary refinement has enhanced properties.";
            descField?.SetValue(this, newDesc);
        }
        
        public bool IsExpired()
        {
            // Refined ingredients degrade over time based on stability
            // This would be implemented with a timestamp system
            return stability < 10;
        }
        
        public float GetStabilityDecayRate()
        {
            return refiningMethod switch
            {
                RefiningType.Grinding => 0.1f,     // Very stable
                RefiningType.Distilling => 0.5f,   // Moderate stability  
                RefiningType.Roasting => 0.3f,     // Good stability
                _ => 1.0f
            };
        }
        
#if UNITY_EDITOR
        private new void OnValidate()
        {
            base.OnValidate();
            
            if (baseIngredient != null)
            {
                // Validate that the refining method is appropriate for the base ingredient
                var refiningData = RefiningData.CreateDefault(refiningMethod);
                if (!refiningData.IsValidForArchetype(baseIngredient.IngredientArchetype))
                {
                    Debug.LogWarning($"[{name}] Refining method {refiningMethod} is not valid for {baseIngredient.IngredientArchetype} ingredients!", this);
                }
            }
            
            if (stability < 10)
            {
                Debug.LogWarning($"[{name}] Very low stability ({stability}). This ingredient may be expired!", this);
            }
        }
#endif
        
        public override string ToString()
        {
            string result = base.ToString();
            if (baseIngredient != null)
            {
                result += $" (Refined from {baseIngredient.ItemName} via {refiningMethod})";
            }
            if (isCriticalResult)
            {
                result += " [CRITICAL]";
            }
            return result;
        }
    }
}