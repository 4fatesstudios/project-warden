using System.Collections.Generic;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Enums.Tooltips.Providers
{
    // implement by tagging the following above any Targeting enum declarations
    // [EnumTooltip(typeof(EffectTiming), typeof(EffectTimingTooltips))]
    public class EffectTimingTooltips {
        public static readonly Dictionary<EffectTiming, string> Tooltips = new() {
            { EffectTiming.OnActivation, "Causes effect on skill use when hitting target." },
            { EffectTiming.Delay, "Causes effect after specified duration turns." },
            { EffectTiming.OverTime, "Causes effect at start of the affected's turn." },
            { EffectTiming.OnTurn, "Causes effect at start of the source's turn." },
        };
    }
}