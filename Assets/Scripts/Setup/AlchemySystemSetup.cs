using UnityEngine;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu;

namespace FourFatesStudios.ProjectWarden.Setup
{
    /// <summary>
    /// Helper script to set up the alchemy system for testing
    /// </summary>
    public class AlchemySystemSetup : MonoBehaviour
    {
        [Header("Auto-Setup")]
        [SerializeField] private bool setupOnStart = true;
        
        [Header("Test Data")]
        [SerializeField] private bool createTestSkillData = false;
        [SerializeField] private bool giveTestSRanks = false;

        private void Start()
        {
            if (setupOnStart)
            {
                SetupAlchemySystem();
            }
        }

        [ContextMenu("Setup Alchemy System")]
        public void SetupAlchemySystem()
        {
            // Ensure AlchemySkillSystem exists
            if (AlchemySkillSystem.Instance == null)
            {
                var skillSystemObject = new GameObject("AlchemySkillSystem");
                skillSystemObject.AddComponent<AlchemySkillSystem>();
                Debug.Log("Created AlchemySkillSystem instance");
            }

            if (createTestSkillData)
            {
                CreateTestSkillData();
            }

            if (giveTestSRanks)
            {
                GiveTestSRanks();
            }
        }

        private void CreateTestSkillData()
        {
            var skillSystem = AlchemySkillSystem.Instance;
            if (skillSystem == null) return;

            // Unlock some test skills
            skillSystem.UnlockSkill("ingredient_refund");
            skillSystem.UnlockSkill("enhanced_grid");
            
            Debug.Log("Created test skill data with ingredient refund and enhanced grid");
        }

        private void GiveTestSRanks()
        {
            var skillSystem = AlchemySkillSystem.Instance;
            if (skillSystem == null) return;

            // Simulate some S-rank achievements
            skillSystem.RecordCraftingResult(null, new System.Collections.Generic.List<FourFatesStudios.ProjectWarden.ScriptableObjects.Items.Ingredient>(), 
                FourFatesStudios.ProjectWarden.Enums.CraftingRank.S);

            Debug.Log("Gave test S-ranks for auto-crafting");
        }

        [ContextMenu("Reset Skill Data")]
        public void ResetSkillData()
        {
            if (AlchemySkillSystem.Instance != null)
            {
                AlchemySkillSystem.Instance.ResetSkillData();
                Debug.Log("Reset alchemy skill data");
            }
        }

        [ContextMenu("Log Current Skill Status")]
        public void LogSkillStatus()
        {
            var skillSystem = AlchemySkillSystem.Instance;
            if (skillSystem == null)
            {
                Debug.Log("No AlchemySkillSystem found");
                return;
            }

            var skillData = skillSystem.SkillData;
            Debug.Log($"Alchemy Skill Status:");
            Debug.Log($"Total S-Ranks: {skillData.totalSRanks}");
            Debug.Log($"Auto-craft success chance: {skillData.autoCraftSuccessChance}%");
            Debug.Log($"Has ingredient refund: {skillData.hasIngredientRefund}");
            Debug.Log($"Has overlap placement: {skillData.hasOverlapPlacement}");
            Debug.Log($"Has enhanced grid: {skillData.hasEnhancedGridSize}");
            Debug.Log($"Unlocked recipes for auto-craft: {skillData.recipeSkills.Count}");
        }
    }
}