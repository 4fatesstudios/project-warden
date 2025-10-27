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
            foreach (var target in targets)
            {
                if (target != null)
                {
                    Debug.Log($"Applying stat debuffs to {target.gameObject.name} with scale {scale:F2}");
                    
                    if (statModifierList != null)
                    {
                        // Apply negative stat modifications with timing
                        Debug.Log($"Applying {statModifierList} stat debuff modifiers");
                        // You can extend this to integrate with your stat system:
                        // target.GetComponent<StatComponent>()?.ApplyDebuffModifiers(statModifierList, effectTimingInfo, scale);
                    }
                }
            }
        }
    }
}