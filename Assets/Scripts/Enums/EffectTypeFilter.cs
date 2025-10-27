namespace FourFatesStudios.ProjectWarden.Enums
{
    /// <summary>
    /// Effect type filter options for the infusion editor
    /// </summary>
    public enum EffectTypeFilter
    {
        All,
        Heal,           // HealEffect
        BuffHeal,       // BuffHealEffect  
        Damage,         // DamageEffect
        Shield,         // ShieldEffect
        BuffShield,     // BuffShieldEffect
        BuffStat,       // BuffStatEffect
        DebuffStat,     // DebuffStatEffect
        DebuffDOT,      // DebuffDOTEffect (Damage Over Time)
        None            // Infusions with no effects
    }
}