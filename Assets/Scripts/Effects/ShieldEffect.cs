using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Characters.Components;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Effects
{
    [System.Serializable]
    public class ShieldEffect : IEffect {
        [SerializeField, Tooltip("Base Shield"), Range(0, 9999)] private int baseShield;
        
        public int BaseShield { get => baseShield; set => baseShield = value; }

        public void Apply(CombatController source, List<CombatController> targets, float scale = 1) {
            throw new System.NotImplementedException();
        }
    }
}