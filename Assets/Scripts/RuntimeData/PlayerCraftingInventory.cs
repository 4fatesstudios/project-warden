using System.Collections.Generic;

namespace FourFatesStudios.ProjectWarden.RuntimeData
{
    using FourFatesStudios.ProjectWarden.GameSystems.Crafting;

    public static class PlayerCraftingInventory
    {
        public static List<CraftedPotion> CraftedPotions = new();
        public static List<CraftedAlchemyComponent> CraftedComponents = new();
    }
}
