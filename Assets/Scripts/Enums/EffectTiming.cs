using FourFatesStudios.ProjectWarden.Attributes;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Enums
{
    public enum EffectTiming {
        [EnumDisplayName("On Activation")] OnActivation,
        Delay,
        [EnumDisplayName("Over Time")] OverTime,
        [EnumDisplayName("On Turn")] OnTurn,
    }
}