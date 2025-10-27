using FourFatesStudios.ProjectWarden.Effects;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Structs
{
    [System.Serializable]
    public struct Infusion {
        [SerializeField, Tooltip("The name of the infusion")]
        private string infusionName;
        
        [Header("Infusion Effects")]
        [SerializeField] private EffectBundle effects;

        public string InfusionName { get => infusionName; set => infusionName = value; }
        public EffectBundle Effects { get => effects; set => effects = value; }
    }
}