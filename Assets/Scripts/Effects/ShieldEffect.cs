using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Characters.Components;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Effects
{
    [System.Serializable]
    public class ShieldEffect : IEffect {
        [SerializeField, Tooltip("Base Shield"), Range(0, 9999)] private int baseShield;
        
        public int BaseShield { get => baseShield; set => baseShield = value; }

        public void Apply(CombatController source, List<CombatController> targets, float scale = 1) {
            foreach (var target in targets)
            {
                if (target != null)
                {
                    int finalShieldAmount = Mathf.RoundToInt(baseShield * scale);
                    Debug.Log($"Applying {finalShieldAmount} shield to {target.gameObject.name}");
                    
                    // You can extend this to integrate with your shield system:
                    // target.GetComponent<ShieldComponent>()?.AddShield(finalShieldAmount);
                }
            }
        }
    }
}