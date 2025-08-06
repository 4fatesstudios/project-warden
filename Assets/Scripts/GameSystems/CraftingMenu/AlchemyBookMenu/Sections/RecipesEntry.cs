using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameSystems.CraftingMenu.AlchemyBookMenu.Sections
{
    [System.Serializable]
    public class RecipeEntry : BaseEntry
    {
        [Header("Recipe Properties")]
        public RecipeType recipeType;
        public List<IngredientRequirement> requiredIngredients = new List<IngredientRequirement>();
        public List<string> infusions = new List<string>();
        public bool isUniquePotionRecipe;
        public List<string> discoveredInfusions = new List<string>(); // Only show if discovered

        public override EntryType GetEntryType() => EntryType.Recipe;

        public override VisualElement CreateEntryVisual()
        {
            var container = CreateBaseVisual();
            if (!isSeen) return container;

            // Recipe type indicator
            var typeContainer = new VisualElement();
            typeContainer.AddToClassList("recipe-type");
            
            var typeLabel = new Label(isUniquePotionRecipe ? "Unique Potion" : "Custom Infusion");
            typeLabel.AddToClassList(isUniquePotionRecipe ? "unique-recipe" : "custom-recipe");
            typeContainer.Add(typeLabel);
            container.Add(typeContainer);

            // Ingredients section
            if (requiredIngredients.Count > 0)
            {
                var ingredientsContainer = new VisualElement();
                ingredientsContainer.AddToClassList("recipe-ingredients");
                
                var ingredientsTitle = new Label("Required Ingredients:");
                ingredientsTitle.AddToClassList("ingredients-title");
                ingredientsContainer.Add(ingredientsTitle);
                
                foreach (var ingredient in requiredIngredients)
                {
                    // Only show if player has discovered this combination
                    if (ingredient.isDiscovered)
                    {
                        var ingredientLabel = new Label($"• {ingredient.ingredientName} x{ingredient.quantity}");
                        ingredientLabel.AddToClassList("ingredient-label");
                        ingredientsContainer.Add(ingredientLabel);
                    }
                    else
                    {
                        var unknownLabel = new Label("• ??? x?");
                        unknownLabel.AddToClassList("ingredient-unknown");
                        ingredientsContainer.Add(unknownLabel);
                    }
                }
                
                container.Add(ingredientsContainer);
            }

            // Infusions section (only show discovered ones)
            if (discoveredInfusions.Count > 0)
            {
                var infusionsContainer = new VisualElement();
                infusionsContainer.AddToClassList("recipe-infusions");
                
                var infusionsTitle = new Label("Discovered Infusions:");
                infusionsTitle.AddToClassList("infusions-title");
                infusionsContainer.Add(infusionsTitle);
                
                foreach (var infusion in discoveredInfusions)
                {
                    var infusionLabel = new Label($"• {infusion}");
                    infusionLabel.AddToClassList("infusion-label");
                    infusionsContainer.Add(infusionLabel);
                }
                
                container.Add(infusionsContainer);
            }

            return container;
        }
    }

    [System.Serializable]
    public class IngredientRequirement
    {
        public string ingredientName;
        public int quantity;
        public bool isDiscovered; // Only show if player has discovered this combination
    }

    public enum RecipeType
    {
        CustomPotion,
        UniquePotion
    }
}
