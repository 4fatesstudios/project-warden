using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Characters.Components;
using FourFatesStudios.ProjectWarden.Structs;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Effects
{
    [System.Serializable]
    public class BuffShieldEffect : IEffect {
        [SerializeField, Tooltip("Base Shield"), Range(0, 9999)] private int baseShield;
        [SerializeField, Tooltip("Effect Timing")] private EffectTimingInfo effectTiming;
        
        public EffectTimingInfo EffectTiming { get => effectTiming; set => effectTiming = value; }
        public int BaseShield { get => baseShield; set => baseShield = value; }
        
        public void Apply(CombatController source, List<CombatController> targets, float scale = 1) {
            foreach (var target in targets)
            {
                if (target != null)
                {
                    int finalShieldAmount = Mathf.RoundToInt(baseShield * scale);
                    Debug.Log($"Applying shield buff to {target.gameObject.name}: {finalShieldAmount} shield");
                    Debug.Log($"Effect timing: {effectTiming}");
                    
                    // You can extend this to integrate with your shield buff system:
                    // target.GetComponent<BuffComponent>()?.ApplyShieldBuff(finalShieldAmount, effectTiming);
                }
            }
        }
    }
}