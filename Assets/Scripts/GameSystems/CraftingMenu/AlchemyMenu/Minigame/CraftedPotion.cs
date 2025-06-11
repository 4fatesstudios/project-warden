using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.PotionEffects;

namespace FourFatesStudios.ProjectWarden.GameSystems.Crafting
{
    public class CraftedPotion
    {
        public string Name;
        public List<PotionEffect> Effects;
        public bool Upgraded;
    }
}
