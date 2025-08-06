using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Stats;
using FourFatesStudios.ProjectWarden.Stats;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Structs
{
    [System.Serializable]
    public struct DamageInstance {
        [SerializeField, Tooltip("Base Damage"), Range(0, 9999)] private int baseDamage;
        [SerializeField, Tooltip("Stagger Damage"), Range(0, 999)] private int stagger;
        [SerializeField, Tooltip("Damage Type")] private DamageType damageType;
        [SerializeField, Tooltip("Aspect")] private Aspect aspect;
        
        public int BaseDamage { get => baseDamage; set => baseDamage = value; }
        public int Stagger { get => stagger; set => stagger = value; }
        public DamageType DamageType { get => damageType; set => damageType = value; }
        public Aspect Aspect { get => aspect; set => aspect = value; }
    }

    [System.Serializable]
    public struct HealInstance {
        [SerializeField, Tooltip("Base Heal"), Range(0, 9999)] private int baseHeal;
        
        public int BaseHeal { get => baseHeal; set => baseHeal = value; }
    }
    
    [System.Serializable]
    public struct ShieldInstance {
        [SerializeField, Tooltip("Base Shield"), Range(0, 9999)] private int baseShield;
        
        public int BaseHeal { get => baseShield; set => baseShield = value; }
    }
    
    [System.Serializable]
    public struct BuffStatInstance {
        [SerializeField, Tooltip("Stat Buff Modifiers")] private StatModifierList statModifierList;
        [SerializeField, Tooltip("Effect Timing")] private EffectTimingInfo effectTiming;
        
        public StatModifierList StatModifierList { get => statModifierList; set => statModifierList = value; }
        public EffectTimingInfo EffectTiming { get => effectTiming; set => effectTiming = value; }
    }
    
    [System.Serializable]
    public struct BuffHealInstance {
        [SerializeField, Tooltip("Heal Instance")] private HealInstance[] healInstances;
        [SerializeField, Tooltip("Effect Timing")] private EffectTimingInfo effectTiming;
        
        public HealInstance[] HealInstances { get => healInstances; set => healInstances = value; }
        public EffectTimingInfo EffectTiming { get => effectTiming; set => effectTiming = value; }
    }
    
    [System.Serializable]
    public struct BuffShieldInstance {
        [SerializeField, Tooltip("Shield Instance")] private ShieldInstance[] shieldInstances;
        [SerializeField, Tooltip("Effect Timing")] private EffectTimingInfo effectTiming;
        
        public ShieldInstance[] ShieldInstances { get => shieldInstances; set => shieldInstances = value; }
        public EffectTimingInfo EffectTiming { get => effectTiming; set => effectTiming = value; }
    }
    
    [System.Serializable]
    public struct DebuffStatInstance {
        [SerializeField, Tooltip("Stat Debuff Modifiers")] private StatModifierList statModifierList;
        [SerializeField, Tooltip("Effect Timing")] private EffectTimingInfo effectTimingInfo;
        
        public StatModifierList StatModifierList { get => statModifierList; set => statModifierList = value; }
        public EffectTimingInfo EffectTimingInfo { get => effectTimingInfo; set => effectTimingInfo = value; }
    }
    
    [System.Serializable]
    public struct DebuffDOTInstance {
        [SerializeField, Tooltip("Damage Instances")] private DamageInstance[] damageInstances;
        [SerializeField, Tooltip("Effect Timing")] private EffectTimingInfo effectTimingInfo;
        
        public DamageInstance[] DamageInstances { get => damageInstances; set => damageInstances = value; }
        public EffectTimingInfo EffectTimingInfo { get => effectTimingInfo; set => effectTimingInfo = value; }
    }

    [System.Serializable]
    public struct EffectTimingInfo {
        [SerializeField, Tooltip("Effect Timing")] private EffectTiming effectTiming;
        [SerializeField, Tooltip("Turn Duration")] private int turnDuration;
        
        public bool RequiresTurnDuration =>
            effectTiming == EffectTiming.Delay || 
            effectTiming == EffectTiming.OverTime || 
            effectTiming == EffectTiming.OnTurn;
        
        public EffectTiming EffectTiming { get => effectTiming; set => effectTiming = value; }
        public int TurnDuration { get => turnDuration; set => turnDuration = value; }
    }
}