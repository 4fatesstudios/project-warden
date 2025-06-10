using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects
{
    [System.Serializable]
    public class StatModifier{
        public Stat stat = Stat.Vitality;
        public StatModifierType type = StatModifierType.Additive;
        public int modifier = 0;
        public string source = "Unknown Source";
    }
}