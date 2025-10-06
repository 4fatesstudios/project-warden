using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Characters.Components;
using FourFatesStudios.ProjectWarden.Structs;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Effects
{
    [System.Serializable]
    public class BuffHealEffect : IEffect {
        [SerializeField, Tooltip("Base Heal"), Range(0, 9999)] private int baseHeal;
        [SerializeField, Tooltip("Effect Timing")] private EffectTimingInfo effectTiming;
        
        public int BaseHeal { get => baseHeal; set => baseHeal = value; }
        public EffectTimingInfo EffectTiming { get => effectTiming; set => effectTiming = value; }
        
        public void Apply(CombatController source, List<CombatController> targets, float scale = 1) {
            foreach (var target in targets)
            {
                if (target != null)
                {
                    int finalHealAmount = Mathf.RoundToInt(baseHeal * scale);
                    Debug.Log($"Applying heal over time buff to {target.gameObject.name}: {finalHealAmount} HP");
                    Debug.Log($"Effect timing: {effectTiming}");
                    
                    // You can extend this to integrate with your buff/heal over time system:
                    // target.GetComponent<BuffComponent>()?.ApplyHealOverTime(finalHealAmount, effectTiming);
                }
            }
        }
    }
}