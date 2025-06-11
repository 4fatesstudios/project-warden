using System.Collections.Generic;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Enums.Tooltips.Providers
{
    public static class TargetingTooltips {
        public static readonly Dictionary<Targeting, string> Tooltips = new() {
            { Targeting.Single, "Affects a single target." },
            { Targeting.Double, "Affects a single target and a random adjacent target." },
            { Targeting.Splash, "Affects a single target and all adjacent targets." },
            { Targeting.All, "Affects all targets of same team." },
            { Targeting.Random, "Affects a random target on target team." },
            { Targeting.Special, "Affects a target with special conditions." },
        };
    }
}