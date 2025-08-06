using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Characters.Components;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.Structs;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Effects
{
    [System.Serializable]
    public class DebuffDOTEffect : IEffect
    {
        [SerializeField, Tooltip("Base Damage"), Range(0, 9999)] private int baseDamage;
        [SerializeField, Tooltip("Stagger Damage"), Range(0, 999)] private int stagger;
        [SerializeField, Tooltip("Damage Type")] private DamageType damageType;
        [SerializeField, Tooltip("Aspect")] private Aspect aspect;
        [SerializeField, Tooltip("Effect Timing")] private EffectTimingInfo effectTimingInfo;
        
        public int BaseDamage { get => baseDamage; set => baseDamage = value; }
        public int Stagger { get => stagger; set => stagger = value; }
        public DamageType DamageType { get => damageType; set => damageType = value; }
        public Aspect Aspect { get => aspect; set => aspect = value; }
        public EffectTimingInfo EffectTimingInfo { get => effectTimingInfo; set => effectTimingInfo = value; }

        public void Apply(CombatController source, List<CombatController> targets, float scale = 1) {
            throw new System.NotImplementedException();
        }
    }
}