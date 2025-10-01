/*
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu
{
    /// <summary>
    /// Handles the alchemy skill progression system from the design document
    /// Manages skill unlocks, overlap abilities, grid expansions, and other player progression
    /// </summary>
    public class AlchemySkillTree : MonoBehaviour
    {
        [Header("Skill Points")]
        [SerializeField] private int currentSkillPoints = 0;
        [SerializeField] private int totalSkillPointsEarned = 0;
        [SerializeField] private bool enableSkillPointGain = true;

        [Header("Skill Configuration")]
        [SerializeField] private List<AlchemySkill> availableSkills = new List<AlchemySkill>();
        [SerializeField] private List<string> unlockedSkills = new List<string>();

        [Header("Overlap System")]
        [SerializeField] private int maxOverlapTiles = 0;
        [SerializeField] private bool canOverlapOnObstacles = false;

        [Header("Grid Expansion")]
        [SerializeField] private Vector2Int baseGridSize = new Vector2Int(3, 3);
        [SerializeField] private Vector2Int maxGridSize = new Vector2Int(5, 5);
        [SerializeField] private Vector2Int currentGridSize = new Vector2Int(3, 3);

        [Header("Special Abilities")]
        [SerializeField] private int purifyCharges = 0;
        [SerializeField] private int maxPotionsHeld = 3;
        [SerializeField] private bool enableIngredientRefund = false;
        [SerializeField] private float refundChance = 0.0f;

        // Events
        public System.Action<AlchemySkill> OnSkillUnlocked;
        public System.Action<int> OnSkillPointsChanged;
        public System.Action<string> OnSkillUsed;

        // Skill tracking
        private Dictionary<string, float> skillValues = new Dictionary<string, float>();
        private Dictionary<string, int> skillUsageCounts = new Dictionary<string, int>();

        private void Start()
        {
            InitializeSkillTree();
        }

        /// <summary>
        /// Initialize the alchemy skill tree with default skills from the design document
        /// </summary>
        private void InitializeSkillTree()
        {
            availableSkills.Clear();

            // Ingredient Overlap Skills (1/2/3 ingredients can overlap 1 tile)
            availableSkills.Add(new AlchemySkill
            {
                skillId = "ingredient_overlap_1",
                skillName = "Basic Overlap",
                description = "Allow 1 ingredient to overlap 1 tile",
                category = SkillCategory.Overlap,
                cost = 2,
                prerequisites = new List<string>(),
                effectValue = 1f,
                effectType = SkillEffectType.OverlapTiles
            });

            availableSkills.Add(new AlchemySkill
            {
                skillId = "ingredient_overlap_2",
                skillName = "Advanced Overlap",
                description = "Allow 2 ingredients to overlap 1 tile",
                category = SkillCategory.Overlap,
                cost = 4,
                prerequisites = new List<string> { "ingredient_overlap_1" },
                effectValue = 2f,
                effectType = SkillEffectType.OverlapTiles
            });

            availableSkills.Add(new AlchemySkill
            {
                skillId = "ingredient_overlap_3",
                skillName = "Master Overlap",
                description = "Allow 3 ingredients to overlap 1 tile",
                category = SkillCategory.Overlap,
                cost = 6,
                prerequisites = new List<string> { "ingredient_overlap_2" },
                effectValue = 3f,
                effectType = SkillEffectType.OverlapTiles
            });

            // Grid Size Expansion
            availableSkills.Add(new AlchemySkill
            {
                skillId = "grid_expansion_4x4",
                skillName = "Expanded Grid",
                description = "Increase grid size to 4x4",
                category = SkillCategory.GridExpansion,
                cost = 3,
                prerequisites = new List<string>(),
                effectValue = 4f,
                effectType = SkillEffectType.GridSize
            });

            availableSkills.Add(new AlchemySkill
            {
                skillId = "grid_expansion_5x5",
                skillName = "Maximum Grid",
                description = "Increase grid size to 5x5",
                category = SkillCategory.GridExpansion,
                cost = 5,
                prerequisites = new List<string> { "grid_expansion_4x4" },
                effectValue = 5f,
                effectType = SkillEffectType.GridSize
            });

            // Ingredient Conservation
            availableSkills.Add(new AlchemySkill
            {
                skillId = "ingredient_conservation",
                skillName = "Ingredient Conservation",
                description = "Random chance to refund ingredients after crafting",
                category = SkillCategory.ResourceManagement,
                cost = 4,
                prerequisites = new List<string>(),
                effectValue = 0.15f,
                effectType = SkillEffectType.RefundChance
            });

            availableSkills.Add(new AlchemySkill
            {
                skillId = "improved_conservation",
                skillName = "Improved Conservation",
                description = "Increased chance for ingredient refund",
                category = SkillCategory.ResourceManagement,
                cost = 3,
                prerequisites = new List<string> { "ingredient_conservation" },
                effectValue = 0.25f,
                effectType = SkillEffectType.RefundChance
            });

            // Potion Storage
            availableSkills.Add(new AlchemySkill
            {
                skillId = "potion_storage_1",
                skillName = "Extra Potion Storage",
                description = "Hold 1 additional potion",
                category = SkillCategory.ResourceManagement,
                cost = 2,
                prerequisites = new List<string>(),
                effectValue = 1f,
                effectType = SkillEffectType.PotionStorage
            });

            availableSkills.Add(new AlchemySkill
            {
                skillId = "potion_storage_2",
                skillName = "Advanced Storage",
                description = "Hold 2 additional potions",
                category = SkillCategory.ResourceManagement,
                cost = 3,
                prerequisites = new List<string> { "potion_storage_1" },
                effectValue = 2f,
                effectType = SkillEffectType.PotionStorage
            });

            // Purify Ability
            availableSkills.Add(new AlchemySkill
            {
                skillId = "purify",
                skillName = "Purify",
                description = "Remove an obstacle completely (1 use)",
                category = SkillCategory.ObstacleManagement,
                cost = 5,
                prerequisites = new List<string>(),
                effectValue = 1f,
                effectType = SkillEffectType.PurifyCharges
            });

            availableSkills.Add(new AlchemySkill
            {
                skillId = "improved_purify",
                skillName = "Improved Purify",
                description = "Gain additional purify charge",
                category = SkillCategory.ObstacleManagement,
                cost = 4,
                prerequisites = new List<string> { "purify" },
                effectValue = 1f,
                effectType = SkillEffectType.PurifyCharges
            });

            // Combat Integration (NUMO continuity)
            availableSkills.Add(new AlchemySkill
            {
                skillId = "numo_continuity",
                skillName = "NUMO Continuity",
                description = "Continue using potion effects in combat with ramping cost",
                category = SkillCategory.Combat,
                cost = 6,
                prerequisites = new List<string>(),
                effectValue = 1f,
                effectType = SkillEffectType.CombatContinuity
            });

            // Alchemical Expertise
            availableSkills.Add(new AlchemySkill
            {
                skillId = "alchemical_expertise",
                skillName = "Alchemical Expertise",
                description = "Increase potency of all created potions by 15%",
                category = SkillCategory.Crafting,
                cost = 5,
                prerequisites = new List<string>(),
                effectValue = 0.15f,
                effectType = SkillEffectType.PotencyBonus
            });

            availableSkills.Add(new AlchemySkill
            {
                skillId = "master_alchemist",
                skillName = "Master Alchemist",
                description = "Further increase potion potency by 10%",
                category = SkillCategory.Crafting,
                cost = 4,
                prerequisites = new List<string> { "alchemical_expertise" },
                effectValue = 0.25f, // Total 25% with expertise
                effectType = SkillEffectType.PotencyBonus
            });

            // Synergy Mastery
            availableSkills.Add(new AlchemySkill
            {
                skillId = "synergy_master",
                skillName = "Synergy Master",
                description = "Increase effectiveness of ingredient synergies",
                category = SkillCategory.Crafting,
                cost = 6,
                prerequisites = new List<string>(),
                effectValue = 0.2f,
                effectType = SkillEffectType.SynergyBonus
            });

            Debug.Log($"🎯 Initialized {availableSkills.Count} alchemy skills");
            LoadPlayerProgress();
        }

        /// <summary>
        /// Load player's skill progress from persistent storage
        /// </summary>
        private void LoadPlayerProgress()
        {
            // In a full implementation, this would load from PlayerPrefs or save system
            // For now, initialize with some test values
            currentSkillPoints = 5; // Give player some starting points for testing
            currentGridSize = baseGridSize;
            
            Debug.Log($"🎯 Loaded player progress: {currentSkillPoints} skill points, {unlockedSkills.Count} skills unlocked");
        }

        /// <summary>
        /// Award skill points to the player
        /// </summary>
        public void AwardSkillPoints(int points, string reason = "")
        {
            if (!enableSkillPointGain) return;

            currentSkillPoints += points;
            totalSkillPointsEarned += points;

            OnSkillPointsChanged?.Invoke(currentSkillPoints);

            Debug.Log($"🎯 Awarded {points} skill points for: {reason}. Total: {currentSkillPoints}");
        }

        /// <summary>
        /// Award skill points based on proficiency grade
        /// </summary>
        public void AwardPointsForProficiency(ProficiencyGrade grade)
        {
            int points = grade.gradeLevel switch
            {
                GradeLevel.F => 0,
                GradeLevel.D => 1,
                GradeLevel.C => 2,
                GradeLevel.B => 3,
                GradeLevel.A => 4,
                GradeLevel.S => 5,
                _ => 1
            };

            if (points > 0)
            {
                AwardSkillPoints(points, $"Proficiency grade {grade.gradeLevel}");
            }
        }

        /// <summary>
        /// Check if a skill is unlocked
        /// </summary>
        public bool IsSkillUnlocked(string skillId)
        {
            return unlockedSkills.Contains(skillId);
        }

        /// <summary>
        /// Check if a skill can be unlocked (has prerequisites and enough points)
        /// </summary>
        public bool CanUnlockSkill(string skillId)
        {
            var skill = availableSkills.FirstOrDefault(s => s.skillId == skillId);
            if (skill == null) return false;

            // Already unlocked
            if (IsSkillUnlocked(skillId)) return false;

            // Check skill points
            if (currentSkillPoints < skill.cost) return false;

            // Check prerequisites
            foreach (var prereq in skill.prerequisites)
            {
                if (!IsSkillUnlocked(prereq)) return false;
            }

            return true;
        }

        /// <summary>
        /// Unlock a skill
        /// </summary>
        public bool TryUnlockSkill(string skillId)
        {
            if (!CanUnlockSkill(skillId)) return false;

            var skill = availableSkills.FirstOrDefault(s => s.skillId == skillId);
            if (skill == null) return false;

            // Spend skill points
            currentSkillPoints -= skill.cost;
            unlockedSkills.Add(skillId);

            // Apply skill effects
            ApplySkillEffect(skill);

            OnSkillUnlocked?.Invoke(skill);
            OnSkillPointsChanged?.Invoke(currentSkillPoints);

            Debug.Log($"🎯 Unlocked skill: {skill.skillName} for {skill.cost} points");

            return true;
        }

        /// <summary>
        /// Apply the effects of a newly unlocked skill
        /// </summary>
        private void ApplySkillEffect(AlchemySkill skill)
        {
            switch (skill.effectType)
            {
                case SkillEffectType.OverlapTiles:
                    maxOverlapTiles = Mathf.Max(maxOverlapTiles, (int)skill.effectValue);
                    Debug.Log($"🎯 Max overlap tiles increased to: {maxOverlapTiles}");
                    break;

                case SkillEffectType.GridSize:
                    int newSize = (int)skill.effectValue;
                    currentGridSize = new Vector2Int(newSize, newSize);
                    Debug.Log($"🎯 Grid size increased to: {currentGridSize.x}x{currentGridSize.y}");
                    
                    // Update the actual grid in GridGameManager
                    UpdateGridSize();
                    break;

                case SkillEffectType.RefundChance:
                    enableIngredientRefund = true;
                    refundChance = skill.effectValue;
                    Debug.Log($"🎯 Ingredient refund chance set to: {refundChance:P}");
                    break;

                case SkillEffectType.PotionStorage:
                    maxPotionsHeld += (int)skill.effectValue;
                    Debug.Log($"🎯 Max potions held increased to: {maxPotionsHeld}");
                    break;

                case SkillEffectType.PurifyCharges:
                    purifyCharges += (int)skill.effectValue;
                    Debug.Log($"🎯 Purify charges increased to: {purifyCharges}");
                    break;

                case SkillEffectType.PotencyBonus:
                case SkillEffectType.SynergyBonus:
                case SkillEffectType.CombatContinuity:
                    // Store value for later retrieval
                    skillValues[skill.skillId] = skill.effectValue;
                    Debug.Log($"🎯 Skill value stored: {skill.skillId} = {skill.effectValue}");
                    break;
            }
        }

        /// <summary>
        /// Update the grid size in GridGameManager
        /// </summary>
        private void UpdateGridSize()
        {
            var gridManager = GridGameManager.Instance;
            if (gridManager != null)
            {
                gridManager.gridWidth = currentGridSize.x;
                gridManager.gridHeight = currentGridSize.y;
                
                // Trigger grid recreation - this would need to be implemented in GridGameManager
                Debug.Log($"🎯 Updated GridGameManager size to {currentGridSize.x}x{currentGridSize.y}");
            }
        }

        /// <summary>
        /// Get the current value of a skill effect
        /// </summary>
        public float GetSkillValue(string skillId)
        {
            return skillValues.ContainsKey(skillId) ? skillValues[skillId] : 0f;
        }

        /// <summary>
        /// Use a purify charge
        /// </summary>
        public bool UsePurifyCharge()
        {
            if (purifyCharges <= 0) return false;

            purifyCharges--;
            OnSkillUsed?.Invoke("Purify");

            Debug.Log($"🎯 Used purify charge. Remaining: {purifyCharges}");
            return true;
        }

        /// <summary>
        /// Check if purify can be used
        /// </summary>
        public bool CanUsePurify()
        {
            return IsSkillUnlocked("purify") && purifyCharges > 0;
        }

        /// <summary>
        /// Get all available skills in a category
        /// </summary>
        public List<AlchemySkill> GetSkillsByCategory(SkillCategory category)
        {
            return availableSkills.Where(s => s.category == category).ToList();
        }

        /// <summary>
        /// Get all unlocked skills
        /// </summary>
        public List<AlchemySkill> GetUnlockedSkills()
        {
            return availableSkills.Where(s => IsSkillUnlocked(s.skillId)).ToList();
        }

        /// <summary>
        /// Get skill tree statistics
        /// </summary>
        public SkillTreeStats GetStats()
        {
            return new SkillTreeStats
            {
                currentSkillPoints = currentSkillPoints,
                totalSkillPointsEarned = totalSkillPointsEarned,
                unlockedSkillCount = unlockedSkills.Count,
                totalSkillCount = availableSkills.Count,
                maxOverlapTiles = maxOverlapTiles,
                currentGridSize = currentGridSize,
                purifyCharges = purifyCharges,
                maxPotionsHeld = maxPotionsHeld
            };
        }

        #region Public API

        /// <summary>
        /// Test the skill tree system
        /// </summary>
        [ContextMenu("Test Skill Tree")]
        public void TestSkillTree()
        {
            Debug.Log("🎯 === TESTING SKILL TREE ===");
            Debug.Log($"🎯 Current Skill Points: {currentSkillPoints}");
            Debug.Log($"🎯 Total Earned: {totalSkillPointsEarned}");
            Debug.Log($"🎯 Skills Unlocked: {unlockedSkills.Count}/{availableSkills.Count}");
            Debug.Log($"🎯 Grid Size: {currentGridSize.x}x{currentGridSize.y}");
            Debug.Log($"🎯 Max Overlap: {maxOverlapTiles}");
            Debug.Log($"🎯 Purify Charges: {purifyCharges}");
            
            Debug.Log("🎯 Available Skills:");
            foreach (var skill in availableSkills)
            {
                string status = IsSkillUnlocked(skill.skillId) ? "✅" : 
                               CanUnlockSkill(skill.skillId) ? "🔓" : "🔒";
                Debug.Log($"   {status} {skill.skillName} ({skill.cost} pts) - {skill.description}");
            }
            
            Debug.Log("🎯 === SKILL TREE TEST COMPLETE ===");
        }

        /// <summary>
        /// Unlock all skills for testing
        /// </summary>
        [ContextMenu("Unlock All Skills (Testing)")]
        public void UnlockAllSkillsForTesting()
        {
            currentSkillPoints = 1000; // Give enough points
            
            foreach (var skill in availableSkills)
            {
                if (!IsSkillUnlocked(skill.skillId))
                {
                    TryUnlockSkill(skill.skillId);
                }
            }
            
            Debug.Log("🎯 All skills unlocked for testing!");
        }

        /// <summary>
        /// Reset skill tree for testing
        /// </summary>
        [ContextMenu("Reset Skill Tree")]
        public void ResetSkillTree()
        {
            unlockedSkills.Clear();
            currentSkillPoints = 5;
            totalSkillPointsEarned = 5;
            skillValues.Clear();
            skillUsageCounts.Clear();
            
            // Reset values to defaults
            maxOverlapTiles = 0;
            currentGridSize = baseGridSize;
            purifyCharges = 0;
            maxPotionsHeld = 3;
            enableIngredientRefund = false;
            refundChance = 0f;
            
            Debug.Log("🎯 Skill tree reset to default state");
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class AlchemySkill
    {
        [Header("Basic Info")]
        public string skillId;
        public string skillName;
        [TextArea(2, 3)]
        public string description;
        public SkillCategory category;

        [Header("Requirements")]
        public int cost = 1;
        public List<string> prerequisites = new List<string>();

        [Header("Effects")]
        public float effectValue;
        public SkillEffectType effectType;
        public bool isPassive = true;
    }

    [System.Serializable]
    public class SkillTreeStats
    {
        public int currentSkillPoints;
        public int totalSkillPointsEarned;
        public int unlockedSkillCount;
        public int totalSkillCount;
        public int maxOverlapTiles;
        public Vector2Int currentGridSize;
        public int purifyCharges;
        public int maxPotionsHeld;
    }

    public enum SkillCategory
    {
        Overlap,
        GridExpansion,
        ResourceManagement,
        ObstacleManagement,
        Combat,
        Crafting
    }

    public enum SkillEffectType
    {
        OverlapTiles,
        GridSize,
        RefundChance,
        PotionStorage,
        PurifyCharges,
        CombatContinuity,
        PotencyBonus,
        SynergyBonus
    }

    #endregion
}
*/