using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Effects
{
    [System.Serializable]
    public class EffectBundle {
        [SerializeReference] public List<IEffect> Effects = new();
    }
}