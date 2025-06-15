using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.Stats;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Structs
{
    [System.Serializable]
    public struct DamageInstance {
        [SerializeField, Tooltip("Base Damage"), Range(1, 9999)] private int baseDamage;
        [SerializeField, Tooltip("Stagger Damage"), Range(1, 999)] private int stagger;
        [SerializeField, Tooltip("Exposure Damage"), Range(1, 999)] private int exposure;
        [SerializeField, Tooltip("Damage Type")] private DamageType damageType;
        [SerializeField, Tooltip("Aspect")] private Aspect aspect;
    }

    public struct HealInstance {
        [SerializeField, Tooltip("Base Heal"), Range(1, 9999)] private int baseHeal;
    }
    
    [System.Serializable]
    public struct BuffStatInstance {
        [SerializeField, Tooltip("Stat Buff")] private StatModifier statModifier; // turn into Passive
        [SerializeField, Tooltip("Effect Timing")] private EffectTimingInfo effectTiming;
    }
    
    [System.Serializable]
    public struct BuffHealInstance {
        [SerializeField, Tooltip("Heal Instance")] private HealInstance[] healInstances;
        [SerializeField, Tooltip("Effect Timing")] private EffectTimingInfo effectTiming;
    }
    
    [System.Serializable]
    public struct DebuffStatInstance {
        [SerializeField, Tooltip("Stat Buff")] private StatModifier statModifier; // turn into Passive
        [SerializeField, Tooltip("Effect Timing")] private EffectTimingInfo effectTimingInfo;
    }
    
    [System.Serializable]
    public struct DebuffDOTInstance {
        [SerializeField, Tooltip("Damage Instances")] private DamageInstance[] damageInstances;
        [SerializeField, Tooltip("Effect Timing")] private EffectTimingInfo effectTimingInfo;
    }

    [System.Serializable]
    public struct EffectTimingInfo {
        [SerializeField, Tooltip("Effect Timing")] private EffectTiming effectTiming;
        [SerializeField, Tooltip("Turn Duration")] private int turnDuration;
        
        public bool RequiresTurnDuration =>
            effectTiming == EffectTiming.Delay || 
            effectTiming == EffectTiming.OverTime || 
            effectTiming == EffectTiming.OnTurn;
    }
}