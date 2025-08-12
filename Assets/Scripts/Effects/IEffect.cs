using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Characters.Components;
using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Effects
{
    public interface IEffect {
        void Apply(CombatController source, List<CombatController> targets, float scale=1.0f);
    }
}