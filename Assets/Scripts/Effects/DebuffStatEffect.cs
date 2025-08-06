using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Characters.Components;
using FourFatesStudios.ProjectWarden.Stats;
using FourFatesStudios.ProjectWarden.Structs;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Effects
{
    [System.Serializable]
    public class DebuffStatEffect : IEffect {
        [SerializeField, Tooltip("Stat Debuff Modifiers")] private StatModifierList statModifierList;
        [SerializeField, Tooltip("Effect Timing")] private EffectTimingInfo effectTimingInfo;
        
        public StatModifierList StatModifierList { get => statModifierList; set => statModifierList = value; }
        public EffectTimingInfo EffectTimingInfo { get => effectTimingInfo; set => effectTimingInfo = value; }
        
        public void Apply(CombatController source, List<CombatController> targets, float scale = 1) {
            throw new System.NotImplementedException();
        }
    }
}