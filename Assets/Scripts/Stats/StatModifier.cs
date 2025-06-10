using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Stats
{
    [System.Serializable]
    public class StatModifier{
        public Stat stat = Stat.Vitality;
        public StatModifierType type = StatModifierType.Additive;
        public int modifier = 0;

        public StatModifier(Stat stat, StatModifierType type, int modifier) {
            this.stat = stat;
            this.type = type;
            this.modifier = modifier;
        }
    }
}