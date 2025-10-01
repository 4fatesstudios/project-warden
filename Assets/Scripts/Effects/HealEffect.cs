using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Characters.Components;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Effects
{
    [System.Serializable]
    public class HealEffect : IEffect {
        [SerializeField, Tooltip("Base Heal"), Range(0, 9999)] private int baseHeal;
        
        public int BaseHeal { get => baseHeal; set => baseHeal = value; }
        
        public void Apply(CombatController source, List<CombatController> targets, float scale = 1) {
            foreach (var target in targets)
            {
                if (target != null)
                {
                    int finalHealAmount = Mathf.RoundToInt(baseHeal * scale);
                    // Apply healing to the target
                    // This would integrate with your health system
                    Debug.Log($"Healing {target.gameObject.name} for {finalHealAmount} HP");
                    
                    // You can extend this to actually apply healing:
                    // target.GetComponent<HealthComponent>()?.Heal(finalHealAmount);
                }
            }
        }
    }
}