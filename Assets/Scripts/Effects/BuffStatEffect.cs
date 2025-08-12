using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Characters.Components;
using FourFatesStudios.ProjectWarden.Stats;
using FourFatesStudios.ProjectWarden.Structs;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Effects
{
    [System.Serializable]
    public class BuffStatEffect : IEffect {
        [SerializeField, Tooltip("Stat Buff Modifiers")] private StatModifierList statModifierList;
        [SerializeField, Tooltip("Effect Timing")] private EffectTimingInfo effectTiming;
        
        public StatModifierList StatModifierList { get => statModifierList; set => statModifierList = value; }
        public EffectTimingInfo EffectTiming { get => effectTiming; set => effectTiming = value; }
        
        public void Apply(CombatController source, List<CombatController> targets, float scale = 1) {
            throw new System.NotImplementedException();
        }
    }
}