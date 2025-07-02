using System.Collections.Generic;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;


namespace FourFatesStudios.ProjectWarden.GameSystems
{
    public class AlchemyManager : MonoBehaviour
    {
        [SerializeField] private AlchemyRecipeDatabase recipeDatabase;

        private Dictionary<UnorderedIngredientKey, AlchemyRecipe> recipeLookup;

        private void Awake()
        {
            BuildRecipeLookup();
        }

        private void BuildRecipeLookup()
        {
            recipeLookup = new Dictionary<UnorderedIngredientKey, AlchemyRecipe>();

            foreach (var recipe in recipeDatabase.Recipes)
            {
                if (recipe == null) continue;

                var inputs = new List<Ingredient>();

                if (recipe.InputIngredient1 != null) inputs.Add(recipe.InputIngredient1);
                if (recipe.InputIngredient2 != null) inputs.Add(recipe.InputIngredient2);
                if (recipe.InputIngredient3 != null) inputs.Add(recipe.InputIngredient3);

                if (inputs.Count < 2)
                {
                    Debug.LogWarning($"[{recipe.name}] has fewer than 2 ingredients and was skipped.");
                    continue;
                }

                var key = new UnorderedIngredientKey(inputs.ToArray());

                if (!recipeLookup.ContainsKey(key))
                    recipeLookup[key] = recipe;
                else
                    Debug.LogWarning($"Duplicate recipe key in lookup for {recipe.name}: {key}");
            }
        }


        public AlchemyRecipe TryFindRecipe(params Ingredient[] inputs)
        {
            if (inputs.Length < 2 || inputs.Length > 3)
            {
                Debug.LogWarning("You must pass 2 or 3 ingredients to find a recipe.");
                return null;
            }

            var key = new UnorderedIngredientKey(inputs);
            recipeLookup.TryGetValue(key, out var result);
            return result;
        }
    }
}