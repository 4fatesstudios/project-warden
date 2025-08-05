using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Effects
{
    [System.Serializable]
    public class EffectBundle {
        [SerializeField] private List<IEffect> effects = new();
        public List<IEffect> Effects => effects;
    }
}