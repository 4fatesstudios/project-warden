using System.Collections.Generic;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Enums
{
    public enum Rarity {
        None,
        Common,
        Uncommon,
        Rare,
        Epic,
        Mythic,
        Unique
    }

    public static class RarityColors {
        public static readonly Dictionary<Rarity, Color> Colors = new() {
            { Rarity.Common, Color.grey },
            { Rarity.Rare, Color.green },
            { Rarity.Epic, Color.blue },
            { Rarity.Mythic, new Color(1f, 0.5f, 0f) },
            { Rarity.Unique, new Color(0.55f, 0.17f, 0.9f) }
        };
    }
}