using UnityEngine;
using System;

namespace FourFatesStudios.ProjectWarden.Enums
{
    public enum Stat
    {
        Vitality,
        Strength,
        Wisdom,
        Agility,
        Luck,
        Health,
        Numo,
        Defense,
        [EnumDisplayName("Critical Chance")] Crit,
        Zeal,
        Potency,
        [EnumDisplayName("Melee Damage")] MeleeDMG,
        [EnumDisplayName("Ranged Damage")] RangedDMG,
        [EnumDisplayName("Corporeal Damage")] CorporealDMG,
        [EnumDisplayName("Frigid Damage")] FrigidDMG,
        [EnumDisplayName("Scorch Damage")] ScorchDMG,
        [EnumDisplayName("Caustic Damage")] CausticDMG,
        [EnumDisplayName("Arc Damage")] ArcDMG,
        [EnumDisplayName("Divine Damage")] DivineDMG,
        [EnumDisplayName("Corporeal Resistance")] CorporealRES,
        [EnumDisplayName("Frigid Resistance")] FrigidRES,
        [EnumDisplayName("Scorch Resistance")] ScorchRES,
        [EnumDisplayName("Caustic Resistance")] CausticRES,
        [EnumDisplayName("Arc Resistance")] ArcRES,
        [EnumDisplayName("Divine Resistance")] DivineRES,
        [EnumDisplayName("Special Attribute")] Special,
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumDisplayNameAttribute : Attribute
    {
        public string DisplayName { get; }
        public EnumDisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }

}