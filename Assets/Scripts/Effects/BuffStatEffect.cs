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
            foreach (var target in targets)
            {
                if (target != null)
                {
                    Debug.Log($"Applying stat buffs to {target.gameObject.name} with scale {scale:F2}");
                    
                    if (statModifierList != null)
                    {
                        // Apply stat modifications with timing
                        Debug.Log($"Applying {statModifierList} stat modifiers");
                        // You can extend this to integrate with your stat system:
                        // target.GetComponent<StatComponent>()?.ApplyModifiers(statModifierList, effectTiming, scale);
                    }
                }
            }
        }
    }
}