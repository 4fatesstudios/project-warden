using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Characters.Components;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Effects
{
    [System.Serializable]
    public class HealEffect : IEffect {
        [SerializeField, Tooltip("Base Heal"), Range(0, 9999)] private int baseHeal;
        
        public int BaseHeal { get => baseHeal; set => baseHeal = value; }
        
        public void Apply(CombatController source, List<CombatController> targets, float scale = 1) {
            throw new System.NotImplementedException();
        }
    }
}