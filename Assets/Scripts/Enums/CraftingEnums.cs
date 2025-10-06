using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Enums
{
    public enum BrewMethod
    {
        QuickBrew,      // Fast but lower quality
        StandardBrew,   // Balanced time and quality
        SlowBrew,       // Longer time, higher quality
        ColdBrew,       // Special method for certain potions
        Fermentation,   // Extended process for unique effects
        Distillation,   // Purification method
        Sublimation     // Advanced technique for rare ingredients
    }
    
    public enum PotionTone
    {
        Dark,           // Evil/corrupted effects
        Light,          // Good/holy effects
        Neutral,        // Balanced effects
        Floral,         // Herbal/natural effects
        Tainted,        // Jibrael-influenced
        Divine          // Blessed/sacred effects
    }
    
    public enum PotionTurbidity
    {
        Clear,          // Transparent, high concentration
        Translucent,    // Semi-transparent, medium concentration
        Opaque          // Cloudy, low concentration or unstable
    }
    
    public enum PotionRarity
    {
        Common,         // Basic effects, common ingredients
        Uncommon,       // Enhanced effects, some rare ingredients
        Rare,           // Powerful effects, rare ingredients required
        Epic,           // Very powerful, very rare ingredients
        Legendary,      // Unique effects, extremely rare ingredients
        Artifact        // Game-changing effects, legendary ingredients
    }
    
    public enum CraftingResult
    {
        Success,        // Perfect potion created
        PartialSuccess, // Potion created but with reduced effects
        Failed,         // Created synthetic ingredient instead
        CriticalFail,   // Lost ingredients entirely
        CriticalSuccess // Enhanced potion with bonus effects
    }
    
    public enum BottleType
    {
        BasicVial,      // Small capacity, single use
        CombatFlask,    // Designed for combat use, quick access
        TravelBottle,   // Exploration use, enhanced durability
        CeremonialChalice, // Special occasions, enhanced effects
        ThrowingVial,   // Projectile use in combat
        InfusionCrystal, // Magical storage, can be recharged
        MasterworkVessel // Ultimate container, preserves potency
    }
    
    public enum AbilityInfusionTag
    {
        None,
        Offensive,      // Damage dealing abilities
        Supportive,     // Healing, buffs, defensive abilities
        Corporeal,      // Physical aspect abilities
        AspectInfusion  // Elemental aspect enhancement
    }
    
    public enum RecipeMasteryLevel
    {
        Unknown,        // Recipe not discovered
        Discovered,     // Can attempt crafting
        Familiar,       // 1-2 successful crafts
        Competent,      // 3-5 successful crafts
        Mastered,       // 6+ crafts, can delegate to assistant
        Perfected       // Achieved perfect results multiple times
    }
    
    public enum IngredientSourcing
    {
        Unknown,
        QuestReward,
        Merchant,
        Exploration,
        BaseGrowth,
        MonsterDrop,
        Synthesis,
        Refining,
        Gift
    }
}