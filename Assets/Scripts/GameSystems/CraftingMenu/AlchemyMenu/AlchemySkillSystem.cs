using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu
{
    [Serializable]
    public class RecipeSkillData
    {
        public string recipeKey;
        public int sRankCount;
        public bool autoCraftingUnlocked;
        public CraftingRank autoCraftRank = CraftingRank.A; // Default auto-craft rank

        public RecipeSkillData(string recipeKey)
        {
            this.recipeKey = recipeKey;
            this.sRankCount = 0;
            this.autoCraftingUnlocked = false;
        }
    }

    [Serializable]
    public class AlchemySkillData
    {
        public List<RecipeSkillData> recipeSkills = new List<RecipeSkillData>();
        public int totalSRanks;
        public int autoCraftSuccessChance; // 0-100% for S-rank auto-crafts

        [Header("Skill Tree Bonuses")]
        public bool hasIngredientRefund;
        public bool hasOverlapPlacement;
        public bool hasEnhancedGridSize;
        public int bonusGridCells;
    }

    public class AlchemySkillSystem : MonoBehaviour
    {
        private static AlchemySkillSystem instance;
        public static AlchemySkillSystem Instance
        {
            get
            {
                if (instance == null)
                    instance = FindFirstObjectByType<AlchemySkillSystem>();
                return instance;
            }
        }

        [SerializeField] private AlchemySkillData skillData = new AlchemySkillData();

        public AlchemySkillData SkillData => skillData;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                // Only make root GameObjects persistent to avoid warning
                if (transform.parent == null)
                {
                    DontDestroyOnLoad(gameObject);
                }
                LoadSkillData();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void RecordCraftingResult(AlchemyRecipe recipe, List<Ingredient> ingredients, CraftingRank rank)
        {
            string recipeKey = GenerateRecipeKey(recipe, ingredients);
            
            var recipeSkill = GetOrCreateRecipeSkill(recipeKey);

            if (rank == CraftingRank.S)
            {
                recipeSkill.sRankCount++;
                skillData.totalSRanks++;

                // Unlock auto-crafting after first S-rank
                if (!recipeSkill.autoCraftingUnlocked)
                {
                    recipeSkill.autoCraftingUnlocked = true;
                    Debug.Log($"Auto-crafting unlocked for recipe: {recipeKey}");
                }
            }

            SaveSkillData();
        }

        public bool CanAutoCraft(AlchemyRecipe recipe, List<Ingredient> ingredients)
        {
            string recipeKey = GenerateRecipeKey(recipe, ingredients);
            var recipeSkill = GetRecipeSkill(recipeKey);
            return recipeSkill != null && recipeSkill.autoCraftingUnlocked;
        }

        public CraftingRank GetAutoCraftRank(AlchemyRecipe recipe, List<Ingredient> ingredients)
        {
            string recipeKey = GenerateRecipeKey(recipe, ingredients);
            var recipeSkill = GetRecipeSkill(recipeKey);
            
            if (recipeSkill == null || !recipeSkill.autoCraftingUnlocked)
                return CraftingRank.F;

            // Base auto-craft rank is A (100% potency)
            CraftingRank baseRank = recipeSkill.autoCraftRank;

            // Check for skill bonuses that might give S-rank chance
            if (skillData.autoCraftSuccessChance > 0)
            {
                int roll = UnityEngine.Random.Range(0, 100);
                if (roll < skillData.autoCraftSuccessChance)
                {
                    return CraftingRank.S;
                }
            }

            return baseRank;
        }

        public RecipeSkillData GetRecipeSkill(string recipeKey)
        {
            return skillData.recipeSkills.FirstOrDefault(rs => rs.recipeKey == recipeKey);
        }

        private RecipeSkillData GetOrCreateRecipeSkill(string recipeKey)
        {
            var existing = GetRecipeSkill(recipeKey);
            if (existing != null)
                return existing;

            var newSkill = new RecipeSkillData(recipeKey);
            skillData.recipeSkills.Add(newSkill);
            return newSkill;
        }

        private string GenerateRecipeKey(AlchemyRecipe recipe, List<Ingredient> ingredients)
        {
            if (recipe != null)
            {
                // For unique recipes, use the recipe name
                return $"recipe_{recipe.name}";
            }
            else
            {
                // For generic potions, create key from sorted ingredient names
                var sortedNames = ingredients
                    .Where(i => i != null)
                    .Select(i => i.name)
                    .OrderBy(name => name)
                    .ToList();
                return $"generic_{string.Join("-", sortedNames)}";
            }
        }

        public void UnlockSkill(string skillName)
        {
            switch (skillName.ToLower())
            {
                case "ingredient_refund":
                    skillData.hasIngredientRefund = true;
                    break;
                case "overlap_placement":
                    skillData.hasOverlapPlacement = true;
                    break;
                case "enhanced_grid":
                    skillData.hasEnhancedGridSize = true;
                    skillData.bonusGridCells = 4;
                    break;
                case "auto_craft_chance":
                    skillData.autoCraftSuccessChance = Mathf.Min(skillData.autoCraftSuccessChance + 10, 50); // Max 50%
                    break;
            }
            SaveSkillData();
        }

        public bool HasSkill(string skillName)
        {
            return skillName.ToLower() switch
            {
                "ingredient_refund" => skillData.hasIngredientRefund,
                "overlap_placement" => skillData.hasOverlapPlacement,
                "enhanced_grid" => skillData.hasEnhancedGridSize,
                "auto_craft_chance" => skillData.autoCraftSuccessChance > 0,
                _ => false
            };
        }

        public List<RecipeSkillData> GetUnlockedAutoCraftRecipes()
        {
            return skillData.recipeSkills.Where(rs => rs.autoCraftingUnlocked).ToList();
        }

        public int GetSRankCount(string recipeKey)
        {
            var recipeSkill = GetRecipeSkill(recipeKey);
            return recipeSkill?.sRankCount ?? 0;
        }

        private void SaveSkillData()
        {
            // In a real game, this would save to PlayerPrefs or a save file
            string json = JsonUtility.ToJson(skillData, true);
            PlayerPrefs.SetString("AlchemySkillData", json);
            PlayerPrefs.Save();
        }

        private void LoadSkillData()
        {
            if (PlayerPrefs.HasKey("AlchemySkillData"))
            {
                string json = PlayerPrefs.GetString("AlchemySkillData");
                try
                {
                    skillData = JsonUtility.FromJson<AlchemySkillData>(json);
                    if (skillData.recipeSkills == null)
                        skillData.recipeSkills = new List<RecipeSkillData>();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load alchemy skill data: {e.Message}");
                    skillData = new AlchemySkillData();
                }
            }
        }

        public void ResetSkillData()
        {
            skillData = new AlchemySkillData();
            SaveSkillData();
        }

#if UNITY_EDITOR
        [ContextMenu("Reset Skill Data")]
        private void EditorResetSkillData()
        {
            ResetSkillData();
            Debug.Log("Alchemy skill data reset");
        }

        [ContextMenu("Add Test S-Ranks")]
        private void EditorAddTestSRanks()
        {
            var testRecipe = GetOrCreateRecipeSkill("test_recipe");
            testRecipe.sRankCount = 5;
            testRecipe.autoCraftingUnlocked = true;
            skillData.totalSRanks += 5;
            SaveSkillData();
            Debug.Log("Added test S-ranks");
        }
#endif
    }
}