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
            throw new System.NotImplementedException();
        }
    }
}