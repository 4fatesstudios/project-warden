using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Crafting;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu;

namespace FourFatesStudios.ProjectWarden.GameSystems.SkillSystem
{
    [Serializable]
    public class AlchemySkill
    {
        public string skillId;
        public string skillName;
        public string description;
        public int tier;
        public int requiredLevel;
        public List<string> prerequisites = new List<string>();
        public bool isUnlocked = false;
        public SkillType skillType;
        public float bonusValue;
        public Sprite skillIcon;
        
        public enum SkillType
        {
            BrewingSpeed,        // Reduces brewing time
            SuccessRate,         // Increases success chance
            QualityBonus,        // Improves potion quality
            IngredientSaver,     // Chance to save ingredients
            CriticalChance,      // Increases critical success rate
            RecipeDiscovery,     // Unlocks recipe discovery methods
            MassCrafting,        // Enables batch/mass production
            AdvancedTechniques,  // Unlocks new brewing methods
            SpecialIngredients,  // Access to rare ingredient types
            EquipmentMastery,    // Better equipment efficiency
            AssistantTraining,   // More/better assistants
            StationUpgrade       // Better brewing stations
        }
    }
    
    public class AlchemySkillTree : MonoBehaviour
    {
        [Header("Skill Configuration")]
        [SerializeField] private List<AlchemySkill> allSkills = new List<AlchemySkill>();
        [SerializeField] private int currentAlchemyLevel = 1;
        [SerializeField] private int currentExperience = 0;
        [SerializeField] private int experienceToNextLevel = 100;
        
        [Header("Experience Rates")]
        [SerializeField] private int successfulBrewExp = 10;
        [SerializeField] private int criticalSuccessExp = 25;
        [SerializeField] private int recipeDiscoveryExp = 50;
        [SerializeField] private int recipeMasteryExp = 100;
        [SerializeField] private int failedBrewExp = 5;
        
        [Header("Skill Points")]
        [SerializeField] private int availableSkillPoints = 0;
        [SerializeField] private int skillPointsPerLevel = 1;
        [SerializeField] private Dictionary<string, bool> unlockedSkills = new Dictionary<string, bool>();
        
        // Events
        public static event Action<int, int> OnLevelChanged; // level, experience
        public static event Action<int> OnSkillPointsChanged;
        public static event Action<AlchemySkill> OnSkillUnlocked;
        public static event Action<int> OnExperienceGained;
        
        private void Start()
        {
            InitializeSkillTree();
            LoadPlayerProgress();
        }
        
        private void InitializeSkillTree()
        {
            if (allSkills.Count == 0)
            {
                CreateDefaultSkillTree();
            }
            
            // Initialize unlocked skills dictionary
            foreach (var skill in allSkills)
            {
                if (!unlockedSkills.ContainsKey(skill.skillId))
                {
                    unlockedSkills[skill.skillId] = skill.isUnlocked;
                }
            }
            
            Debug.Log($"🌳 Alchemy skill tree initialized with {allSkills.Count} skills");
        }
        
        private void CreateDefaultSkillTree()
        {
            allSkills = new List<AlchemySkill>
            {
                // Tier 1 - Basic Skills
                new AlchemySkill
                {
                    skillId = "efficient_brewing",
                    skillName = "Efficient Brewing",
                    description = "Reduce brewing time by 15%",
                    tier = 1,
                    requiredLevel = 5,
                    skillType = AlchemySkill.SkillType.BrewingSpeed,
                    bonusValue = 0.15f
                },
                new AlchemySkill
                {
                    skillId = "steady_hands",
                    skillName = "Steady Hands",
                    description = "Increase success rate by 10%",
                    tier = 1,
                    requiredLevel = 3,
                    skillType = AlchemySkill.SkillType.SuccessRate,
                    bonusValue = 0.1f
                },
                new AlchemySkill
                {
                    skillId = "ingredient_conservation",
                    skillName = "Ingredient Conservation",
                    description = "5% chance to retain ingredients after brewing",
                    tier = 1,
                    requiredLevel = 7,
                    skillType = AlchemySkill.SkillType.IngredientSaver,
                    bonusValue = 0.05f
                },
                
                // Tier 2 - Intermediate Skills
                new AlchemySkill
                {
                    skillId = "advanced_brewing",
                    skillName = "Advanced Brewing",
                    description = "Unlock Distillation and Sublimation brewing methods",
                    tier = 2,
                    requiredLevel = 15,
                    prerequisites = new List<string> { "efficient_brewing" },
                    skillType = AlchemySkill.SkillType.AdvancedTechniques,
                    bonusValue = 1f
                },
                new AlchemySkill
                {
                    skillId = "quality_control",
                    skillName = "Quality Control",
                    description = "Increase potion quality by 20%",
                    tier = 2,
                    requiredLevel = 12,
                    prerequisites = new List<string> { "steady_hands" },
                    skillType = AlchemySkill.SkillType.QualityBonus,
                    bonusValue = 0.2f
                },
                new AlchemySkill
                {
                    skillId = "recipe_intuition",
                    skillName = "Recipe Intuition",
                    description = "Discover recipes from fewer trigger ingredients",
                    tier = 2,
                    requiredLevel = 10,
                    skillType = AlchemySkill.SkillType.RecipeDiscovery,
                    bonusValue = 1f
                },
                new AlchemySkill
                {
                    skillId = "critical_brewing",
                    skillName = "Critical Brewing",
                    description = "Double critical success chance",
                    tier = 2,
                    requiredLevel = 18,
                    prerequisites = new List<string> { "quality_control" },
                    skillType = AlchemySkill.SkillType.CriticalChance,
                    bonusValue = 2f
                },
                
                // Tier 3 - Expert Skills
                new AlchemySkill
                {
                    skillId = "mass_production",
                    skillName = "Mass Production",
                    description = "Enable batch brewing of mastered recipes",
                    tier = 3,
                    requiredLevel = 25,
                    prerequisites = new List<string> { "advanced_brewing", "ingredient_conservation" },
                    skillType = AlchemySkill.SkillType.MassCrafting,
                    bonusValue = 1f
                },
                new AlchemySkill
                {
                    skillId = "exotic_ingredients",
                    skillName = "Exotic Ingredients",
                    description = "Access to Tainted and Divine ingredient types",
                    tier = 3,
                    requiredLevel = 30,
                    prerequisites = new List<string> { "recipe_intuition" },
                    skillType = AlchemySkill.SkillType.SpecialIngredients,
                    bonusValue = 1f
                },
                new AlchemySkill
                {
                    skillId = "equipment_mastery",
                    skillName = "Equipment Mastery",
                    description = "50% better equipment bonuses",
                    tier = 3,
                    requiredLevel = 28,
                    prerequisites = new List<string> { "critical_brewing" },
                    skillType = AlchemySkill.SkillType.EquipmentMastery,
                    bonusValue = 0.5f
                },
                
                // Tier 4 - Master Skills
                new AlchemySkill
                {
                    skillId = "apprentice_training",
                    skillName = "Apprentice Training",
                    description = "Train assistants to handle complex recipes",
                    tier = 4,
                    requiredLevel = 40,
                    prerequisites = new List<string> { "mass_production" },
                    skillType = AlchemySkill.SkillType.AssistantTraining,
                    bonusValue = 1f
                },
                new AlchemySkill
                {
                    skillId = "legendary_techniques",
                    skillName = "Legendary Techniques",
                    description = "Unlock Artifact-tier potion creation",
                    tier = 4,
                    requiredLevel = 45,
                    prerequisites = new List<string> { "exotic_ingredients", "equipment_mastery" },
                    skillType = AlchemySkill.SkillType.AdvancedTechniques,
                    bonusValue = 1f
                },
                new AlchemySkill
                {
                    skillId = "grand_laboratory",
                    skillName = "Grand Laboratory",
                    description = "Double brewing station capacity and efficiency",
                    tier = 4,
                    requiredLevel = 50,
                    prerequisites = new List<string> { "apprentice_training", "legendary_techniques" },
                    skillType = AlchemySkill.SkillType.StationUpgrade,
                    bonusValue = 2f
                }
            };
            
            Debug.Log($"🔧 Created default skill tree with {allSkills.Count} skills");
        }
        
        public void GainExperience(int amount, string source = "")
        {
            currentExperience += amount;
            OnExperienceGained?.Invoke(amount);
            
            if (!string.IsNullOrEmpty(source))
            {
                Debug.Log($"📈 Gained {amount} alchemy experience from {source}");
            }
            
            CheckLevelUp();
        }
        
        private void CheckLevelUp()
        {
            while (currentExperience >= experienceToNextLevel)
            {
                currentExperience -= experienceToNextLevel;
                currentAlchemyLevel++;
                availableSkillPoints += skillPointsPerLevel;
                
                // Calculate next level experience requirement
                experienceToNextLevel = CalculateExperienceRequirement(currentAlchemyLevel);
                
                Debug.Log($"🎉 Level up! Alchemy level is now {currentAlchemyLevel}");
                OnLevelChanged?.Invoke(currentAlchemyLevel, currentExperience);
                OnSkillPointsChanged?.Invoke(availableSkillPoints);
                
                // Check for newly available skills
                CheckAvailableSkills();
            }
        }
        
        private int CalculateExperienceRequirement(int level)
        {
            // Exponential growth: base * level^1.5
            return Mathf.RoundToInt(100 * Mathf.Pow(level, 1.5f));
        }
        
        private void CheckAvailableSkills()
        {
            foreach (var skill in allSkills)
            {
                if (!skill.isUnlocked && CanUnlockSkill(skill))
                {
                    // Skill becomes available but not automatically unlocked
                    Debug.Log($"💡 Skill available: {skill.skillName} (Level {skill.requiredLevel})");
                }
            }
        }
        
        public bool CanUnlockSkill(AlchemySkill skill)
        {
            // Check level requirement
            if (currentAlchemyLevel < skill.requiredLevel)
                return false;
                
            // Check if already unlocked
            if (unlockedSkills.TryGetValue(skill.skillId, out bool isUnlocked) && isUnlocked)
                return false;
                
            // Check prerequisites
            foreach (var prerequisiteId in skill.prerequisites)
            {
                if (!unlockedSkills.TryGetValue(prerequisiteId, out bool prereqUnlocked) || !prereqUnlocked)
                    return false;
            }
            
            return true;
        }
        
        public bool UnlockSkill(string skillId)
        {
            var skill = allSkills.Find(s => s.skillId == skillId);
            if (skill == null)
            {
                Debug.LogWarning($"Skill not found: {skillId}");
                return false;
            }
            
            if (!CanUnlockSkill(skill))
            {
                Debug.LogWarning($"Cannot unlock skill: {skill.skillName}");
                return false;
            }
            
            if (availableSkillPoints <= 0)
            {
                Debug.LogWarning("No skill points available!");
                return false;
            }
            
            // Unlock the skill
            skill.isUnlocked = true;
            unlockedSkills[skill.skillId] = true;
            availableSkillPoints--;
            
            Debug.Log($"🌟 Unlocked skill: {skill.skillName}");
            OnSkillUnlocked?.Invoke(skill);
            OnSkillPointsChanged?.Invoke(availableSkillPoints);
            
            // Apply skill effects immediately
            ApplySkillEffects(skill);
            
            return true;
        }
        
        private void ApplySkillEffects(AlchemySkill skill)
        {
            // Notify other systems about skill unlock
            var craftingController = FindFirstObjectByType<ComprehensiveCraftingController>();
            
            switch (skill.skillType)
            {
                case AlchemySkill.SkillType.AssistantTraining:
                    craftingController?.SetAssistantCount(GetAssistantCount());
                    break;
                    
                case AlchemySkill.SkillType.StationUpgrade:
                    craftingController?.SetBrewingStationCount(GetBrewingStationCount());
                    break;
                    
                case AlchemySkill.SkillType.EquipmentMastery:
                    craftingController?.SetEquipmentStatus(true);
                    break;
            }
        }
        
        public void OnBrewingCompleted(CraftingResult result, PotionRecipe recipe)
        {
            int expGained = result switch
            {
                CraftingResult.CriticalSuccess => criticalSuccessExp,
                CraftingResult.Success => successfulBrewExp,
                CraftingResult.PartialSuccess => successfulBrewExp / 2,
                CraftingResult.Failed => failedBrewExp,
                CraftingResult.CriticalFail => 0,
                _ => 0
            };
            
            // Bonus experience for higher tier recipes
            expGained += (int)recipe.Rarity * 5;
            
            GainExperience(expGained, $"brewing {recipe.RecipeName}");
        }
        
        public void OnRecipeDiscovered(PotionRecipe recipe)
        {
            int expGained = recipeDiscoveryExp + (int)recipe.Rarity * 10;
            GainExperience(expGained, $"discovering {recipe.RecipeName}");
        }
        
        public void OnRecipeMastered(PotionRecipe recipe)
        {
            int expGained = recipeMasteryExp + (int)recipe.Rarity * 20;
            GainExperience(expGained, $"mastering {recipe.RecipeName}");
        }
        
        #region Skill Bonus Calculations
        
        public float GetBrewingSpeedBonus()
        {
            float bonus = 0f;
            foreach (var skill in GetUnlockedSkills())
            {
                if (skill.skillType == AlchemySkill.SkillType.BrewingSpeed)
                    bonus += skill.bonusValue;
            }
            return bonus;
        }
        
        public float GetSuccessRateBonus()
        {
            float bonus = 0f;
            foreach (var skill in GetUnlockedSkills())
            {
                if (skill.skillType == AlchemySkill.SkillType.SuccessRate)
                    bonus += skill.bonusValue;
            }
            return bonus;
        }
        
        public float GetQualityBonus()
        {
            float bonus = 0f;
            foreach (var skill in GetUnlockedSkills())
            {
                if (skill.skillType == AlchemySkill.SkillType.QualityBonus)
                    bonus += skill.bonusValue;
            }
            return bonus;
        }
        
        public float GetIngredientSaveChance()
        {
            float chance = 0f;
            foreach (var skill in GetUnlockedSkills())
            {
                if (skill.skillType == AlchemySkill.SkillType.IngredientSaver)
                    chance += skill.bonusValue;
            }
            return Mathf.Clamp01(chance);
        }
        
        public float GetCriticalChanceMultiplier()
        {
            float multiplier = 1f;
            foreach (var skill in GetUnlockedSkills())
            {
                if (skill.skillType == AlchemySkill.SkillType.CriticalChance)
                    multiplier *= skill.bonusValue;
            }
            return multiplier;
        }
        
        public bool HasMassCrafting()
        {
            return GetUnlockedSkills().Any(s => s.skillType == AlchemySkill.SkillType.MassCrafting);
        }
        
        public bool HasExoticIngredients()
        {
            return GetUnlockedSkills().Any(s => s.skillType == AlchemySkill.SkillType.SpecialIngredients);
        }
        
        public bool HasEquipmentMastery()
        {
            return GetUnlockedSkills().Any(s => s.skillType == AlchemySkill.SkillType.EquipmentMastery);
        }
        
        public int GetAssistantCount()
        {
            int baseCount = 0;
            foreach (var skill in GetUnlockedSkills())
            {
                if (skill.skillType == AlchemySkill.SkillType.AssistantTraining)
                    baseCount += (int)skill.bonusValue;
            }
            return baseCount;
        }
        
        public int GetBrewingStationCount()
        {
            int baseCount = 1;
            foreach (var skill in GetUnlockedSkills())
            {
                if (skill.skillType == AlchemySkill.SkillType.StationUpgrade)
                    baseCount *= (int)skill.bonusValue;
            }
            return baseCount;
        }
        
        public List<BrewMethod> GetUnlockedBrewMethods()
        {
            var methods = new List<BrewMethod> { BrewMethod.QuickBrew, BrewMethod.StandardBrew, BrewMethod.SlowBrew };
            
            foreach (var skill in GetUnlockedSkills())
            {
                if (skill.skillType == AlchemySkill.SkillType.AdvancedTechniques)
                {
                    if (skill.skillId == "advanced_brewing")
                    {
                        methods.Add(BrewMethod.Distillation);
                        methods.Add(BrewMethod.Sublimation);
                    }
                    else if (skill.skillId == "legendary_techniques")
                    {
                        methods.Add(BrewMethod.ColdBrew);
                        methods.Add(BrewMethod.Fermentation);
                    }
                }
            }
            
            return methods;
        }
        
        #endregion
        
        #region Public API
        
        public List<AlchemySkill> GetAllSkills()
        {
            return new List<AlchemySkill>(allSkills);
        }
        
        public List<AlchemySkill> GetUnlockedSkills()
        {
            return allSkills.Where(s => unlockedSkills.TryGetValue(s.skillId, out bool unlocked) && unlocked).ToList();
        }
        
        public List<AlchemySkill> GetAvailableSkills()
        {
            return allSkills.Where(s => CanUnlockSkill(s)).ToList();
        }
        
        public List<AlchemySkill> GetSkillsByTier(int tier)
        {
            return allSkills.Where(s => s.tier == tier).ToList();
        }
        
        public AlchemySkill GetSkill(string skillId)
        {
            return allSkills.Find(s => s.skillId == skillId);
        }
        
        public bool IsSkillUnlocked(string skillId)
        {
            return unlockedSkills.TryGetValue(skillId, out bool unlocked) && unlocked;
        }
        
        public int GetCurrentLevel()
        {
            return currentAlchemyLevel;
        }
        
        public int GetCurrentExperience()
        {
            return currentExperience;
        }
        
        public int GetExperienceToNextLevel()
        {
            return experienceToNextLevel;
        }
        
        public int GetAvailableSkillPoints()
        {
            return availableSkillPoints;
        }
        
        public void AddSkillPoints(int amount)
        {
            availableSkillPoints += amount;
            OnSkillPointsChanged?.Invoke(availableSkillPoints);
        }
        
        #endregion
        
        #region Save/Load
        
        private void LoadPlayerProgress()
        {
            // This would load from your save system
            // For now, we'll start with default values
        }
        
        public void SaveProgress()
        {
            // This would save to your save system
            Debug.Log($"💾 Saved alchemy progress: Level {currentAlchemyLevel}, {availableSkillPoints} skill points");
        }
        
        public void ResetSkillData()
        {
            currentAlchemyLevel = 1;
            currentExperience = 0;
            experienceToNextLevel = 100;
            availableSkillPoints = 0;
            
            foreach (var skill in allSkills)
            {
                skill.isUnlocked = false;
                unlockedSkills[skill.skillId] = false;
            }
            
            Debug.Log("🔄 Reset all alchemy skill data");
        }
        
        #endregion
        
        private void OnDestroy()
        {
            SaveProgress();
        }
    }
}