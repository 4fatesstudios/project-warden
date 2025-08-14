using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.Enums;

namespace FourFatesStudios.ProjectWarden.GameSystems.AlchemyMenu
{
    public class BulkCraftingController : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private ItemSlotContainerHolder ingredientInventoryHolder;

        private DropdownField recipeDropdown;
        private IntegerField quantityField;
        private Button bulkCraftButton;
        private Label costLabel;
        private Label resultLabel;
        private Label statusLabel;

        private List<BulkCraftableRecipe> availableRecipes = new List<BulkCraftableRecipe>();

        [Serializable]
        private class BulkCraftableRecipe
        {
            public string displayName;
            public AlchemyRecipe recipe;
            public List<Ingredient> ingredients;
            public CraftingRank guaranteedRank;
            public bool isGeneric;

            public BulkCraftableRecipe(AlchemyRecipe recipe, List<Ingredient> ingredients, CraftingRank rank)
            {
                this.recipe = recipe;
                this.ingredients = ingredients.ToList();
                this.guaranteedRank = rank;
                this.isGeneric = recipe == null;

                if (recipe != null)
                {
                    displayName = $"{recipe.OutputPotion.ItemName} (Rank: {rank.GetRankDisplayName()})";
                }
                else
                {
                    var effects = GetSharedEffects(ingredients);
                    displayName = $"Generic Potion of {string.Join(", ", effects)} (Rank: {rank.GetRankDisplayName()})";
                }
            }

            private List<string> GetSharedEffects(List<Ingredient> ingredients)
            {
                // This would need to access the actual effect system
                // For now, return a placeholder
                return new List<string> { "Unknown Effect" };
            }
        }

        private void OnEnable()
        {
            var root = uiDocument.rootVisualElement;

            recipeDropdown = root.Q<DropdownField>("RecipeDropdown");
            quantityField = root.Q<IntegerField>("QuantityField");
            bulkCraftButton = root.Q<Button>("BulkCraftButton");
            costLabel = root.Q<Label>("CostLabel");
            resultLabel = root.Q<Label>("ResultLabel");
            statusLabel = root.Q<Label>("StatusLabel");

            if (quantityField != null)
            {
                quantityField.value = 1;
                quantityField.RegisterValueChangedCallback(evt => UpdateCostDisplay());
            }

            recipeDropdown?.RegisterValueChangedCallback(evt => UpdateCostDisplay());
            bulkCraftButton?.RegisterCallback<ClickEvent>(_ => PerformBulkCraft());

            RefreshAvailableRecipes();
            UpdateUI();
        }

        private void RefreshAvailableRecipes()
        {
            availableRecipes.Clear();

            if (AlchemySkillSystem.Instance == null) return;

            var unlockedRecipes = AlchemySkillSystem.Instance.GetUnlockedAutoCraftRecipes();

            foreach (var recipeSkill in unlockedRecipes)
            {
                // Parse the recipe key to get ingredients/recipe
                if (recipeSkill.recipeKey.StartsWith("recipe_"))
                {
                    string recipeName = recipeSkill.recipeKey.Substring(7);
                    var db = Resources.Load<AlchemyRecipeDatabase>("Databases/AlchemyRecipeDatabase");
                    var recipe = db?.Recipes.FirstOrDefault(r => r.name == recipeName);
                    
                    if (recipe != null)
                    {
                        var ingredients = new List<Ingredient>
                        {
                            recipe.InputIngredient1,
                            recipe.InputIngredient2,
                            recipe.InputIngredient3
                        }.Where(i => i != null).ToList();

                        availableRecipes.Add(new BulkCraftableRecipe(recipe, ingredients, recipeSkill.autoCraftRank));
                    }
                }
                else if (recipeSkill.recipeKey.StartsWith("generic_"))
                {
                    // Parse generic recipe ingredients
                    string ingredientNames = recipeSkill.recipeKey.Substring(8);
                    var names = ingredientNames.Split('-');
                    
                    // This would need a way to look up ingredients by name
                    // For now, skip generic recipes in bulk crafting
                }
            }

            UpdateDropdownChoices();
        }

        private void UpdateDropdownChoices()
        {
            if (recipeDropdown == null) return;

            var choices = availableRecipes.Select(r => r.displayName).ToList();
            
            if (choices.Count == 0)
            {
                choices.Add("No recipes available for bulk crafting");
            }

            recipeDropdown.choices = choices;
            
            if (choices.Count > 0 && !choices[0].Contains("No recipes"))
            {
                recipeDropdown.index = 0;
            }
        }

        private void UpdateCostDisplay()
        {
            if (costLabel == null || quantityField == null || recipeDropdown == null) return;

            var selectedRecipe = GetSelectedRecipe();
            if (selectedRecipe == null)
            {
                costLabel.text = "No recipe selected";
                return;
            }

            int quantity = Mathf.Max(1, quantityField.value);
            var costs = CalculateIngredientCosts(selectedRecipe, quantity);

            if (costs.Count == 0)
            {
                costLabel.text = "Invalid recipe";
                return;
            }

            var costText = $"Cost for {quantity}x {selectedRecipe.displayName}:\n";
            costText += string.Join("\n", costs.Select(kvp => $"• {kvp.Value}x {kvp.Key.ItemName}"));

            costLabel.text = costText;

            // Check if we have enough ingredients
            bool canCraft = CanCraftQuantity(selectedRecipe, quantity);
            bulkCraftButton?.SetEnabled(canCraft);

            if (!canCraft)
            {
                statusLabel.text = "Insufficient ingredients";
            }
            else
            {
                statusLabel.text = "Ready to craft";
            }
        }

        private BulkCraftableRecipe GetSelectedRecipe()
        {
            if (recipeDropdown == null || recipeDropdown.index < 0 || recipeDropdown.index >= availableRecipes.Count)
                return null;

            return availableRecipes[recipeDropdown.index];
        }

        private Dictionary<Ingredient, int> CalculateIngredientCosts(BulkCraftableRecipe recipe, int quantity)
        {
            var costs = new Dictionary<Ingredient, int>();

            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredient != null)
                {
                    if (costs.ContainsKey(ingredient))
                        costs[ingredient] += quantity;
                    else
                        costs[ingredient] = quantity;
                }
            }

            return costs;
        }

        private bool CanCraftQuantity(BulkCraftableRecipe recipe, int quantity)
        {
            if (ingredientInventoryHolder?.Container == null) return false;

            var costs = CalculateIngredientCosts(recipe, quantity);

            foreach (var cost in costs)
            {
                int available = ingredientInventoryHolder.Container.GetItemCount(cost.Key);
                if (available < cost.Value)
                    return false;
            }

            return true;
        }

        private void PerformBulkCraft()
        {
            var selectedRecipe = GetSelectedRecipe();
            if (selectedRecipe == null) return;

            int quantity = Mathf.Max(1, quantityField.value);
            
            if (!CanCraftQuantity(selectedRecipe, quantity))
            {
                resultLabel.text = "Cannot craft: insufficient ingredients";
                return;
            }

            // Consume ingredients
            var costs = CalculateIngredientCosts(selectedRecipe, quantity);
            foreach (var cost in costs)
            {
                ingredientInventoryHolder.Container.Remove(cost.Key, cost.Value);
            }

            // Create products
            int successfulCrafts = 0;
            for (int i = 0; i < quantity; i++)
            {
                if (AttemptSingleCraft(selectedRecipe))
                    successfulCrafts++;
            }

            resultLabel.text = $"Bulk crafting complete!\nSuccessfully crafted {successfulCrafts}/{quantity} items.";

            // Refresh UI
            UpdateCostDisplay();
            RefreshAvailableRecipes();
        }

        private bool AttemptSingleCraft(BulkCraftableRecipe recipe)
        {
            try
            {
                var rank = AlchemySkillSystem.Instance?.GetAutoCraftRank(recipe.recipe, recipe.ingredients) ?? recipe.guaranteedRank;

                if (recipe.recipe != null)
                {
                    // Create unique potion
                    var potion = ScriptableObject.CreateInstance<Potion>();
                    potion.name = recipe.recipe.OutputPotion.ItemName;

                    // Apply rank modifier to effects
                    var effectsField = typeof(Potion).GetField("potionEffects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    effectsField?.SetValue(potion, recipe.recipe.OutputPotion.PotionEffects.ToList());

                    var upgradedProp = typeof(Potion).GetProperty("Upgraded");
                    upgradedProp?.SetValue(potion, recipe.recipe.OutputPotion.Upgraded);

                    ingredientInventoryHolder.AddItem(potion, 1);
                }
                else
                {
                    // Create generic potion - this would need proper effect resolution
                    var potion = ScriptableObject.CreateInstance<Potion>();
                    potion.name = "Generic Potion";
                    ingredientInventoryHolder.AddItem(potion, 1);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to craft item: {e.Message}");
                return false;
            }
        }

        private void UpdateUI()
        {
            UpdateCostDisplay();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            RefreshAvailableRecipes();
            UpdateUI();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}