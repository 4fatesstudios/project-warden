namespace FourFatesStudios.ProjectWarden.Enums
{
    public enum CraftingRank
    {
        F = 0,  // 40% potency
        E = 1,  // 50% potency  
        D = 2,  // 60% potency
        C = 3,  // 70% potency
        B = 4,  // 80% potency
        A = 5,  // 100% potency (baseline)
        S = 6   // 120% potency (perfect)
    }

    public static class CraftingRankExtensions
    {
        public static float GetPotencyMultiplier(this CraftingRank rank)
        {
            return rank switch
            {
                CraftingRank.F => 0.4f,
                CraftingRank.E => 0.5f,
                CraftingRank.D => 0.6f,
                CraftingRank.C => 0.7f,
                CraftingRank.B => 0.8f,
                CraftingRank.A => 1.0f,
                CraftingRank.S => 1.2f,
                _ => 1.0f
            };
        }

        public static string GetRankDisplayName(this CraftingRank rank)
        {
            return rank switch
            {
                CraftingRank.F => "F (Failed)",
                CraftingRank.E => "E (Poor)",
                CraftingRank.D => "D (Below Average)",
                CraftingRank.C => "C (Average)",
                CraftingRank.B => "B (Good)",
                CraftingRank.A => "A (Excellent)",
                CraftingRank.S => "S (Perfect)",
                _ => "Unknown"
            };
        }

        public static bool UnlocksAutoCrafting(this CraftingRank rank)
        {
            return rank == CraftingRank.S;
        }
    }
}