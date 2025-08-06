using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes
{
    [CreateAssetMenu(fileName = "AlchemyRecipeDatabase", menuName = "AlchemyRecipes/Recipe Database")]
    public class AlchemyRecipeDatabase : ScriptableObject
    {
        private static AlchemyRecipeDatabase _instance;
        public static AlchemyRecipeDatabase Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<AlchemyRecipeDatabase>("AlchemyRecipes/AlchemyRecipeDatabase");
                if (_instance == null)
                    Debug.LogError("AlchemyRecipeDatabase asset not found in Resources/AlchemyRecipes/AlchemyRecipeDatabase");
                return _instance;
            }
        }

        [SerializeField]
        private List<AlchemyRecipe> _recipes = new();
        private Dictionary<string, AlchemyRecipe> _recipeLookup;

        public IReadOnlyList<AlchemyRecipe> Recipes => _recipes;

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            _recipeLookup = new Dictionary<string, AlchemyRecipe>();
            foreach (var recipe in _recipes)
            {
                if (recipe == null) continue;

                var inputs = new List<Ingredient>();
                if (recipe.InputIngredient1 != null) inputs.Add(recipe.InputIngredient1);
                if (recipe.InputIngredient2 != null) inputs.Add(recipe.InputIngredient2);
                if (recipe.InputIngredient3 != null) inputs.Add(recipe.InputIngredient3);

                if (inputs.Count < 2) continue;

                // Sort by name to make the key order-insensitive
                inputs.Sort((a, b) => a.name.CompareTo(b.name));
                string key = string.Join("-", inputs.Select(i => i.name));

                if (_recipeLookup.ContainsKey(key))
                {
                    Debug.LogWarning($"Duplicate recipe for key: {key} in {recipe.name}");
                    continue;
                }

                _recipeLookup[key] = recipe;
            }
        }

        /// <summary>
        /// Get a recipe by a set of ingredients (order-insensitive, by name).
        /// </summary>
        public AlchemyRecipe GetRecipeByIngredients(IEnumerable<Ingredient> ingredients)
        {
            if (_recipeLookup == null)
                BuildLookup();

            var validIngredients = ingredients?.Where(i => i != null).ToList();
            if (validIngredients == null || validIngredients.Count < 2)
                return null;

            validIngredients.Sort((a, b) => a.name.CompareTo(b.name));
            string key = string.Join("-", validIngredients.Select(i => i.name));

            _recipeLookup.TryGetValue(key, out var recipe);
            return recipe;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            var seenKeys = new HashSet<string>();

            foreach (var recipe in _recipes)
            {
                if (recipe == null)
                {
                    Debug.LogWarning("Null recipe found in the database.");
                    continue;
                }

                var inputs = new List<Ingredient>();
                if (recipe.InputIngredient1 != null) inputs.Add(recipe.InputIngredient1);
                if (recipe.InputIngredient2 != null) inputs.Add(recipe.InputIngredient2);
                if (recipe.InputIngredient3 != null) inputs.Add(recipe.InputIngredient3);

                if (inputs.Count < 2)
                {
                    Debug.LogWarning($"[{recipe.name}] has fewer than 2 ingredients.");
                    continue;
                }

                if (inputs.Count == 3 && (recipe.InputIngredient1 == null || recipe.InputIngredient2 == null || recipe.InputIngredient3 == null))
                {
                    Debug.LogWarning($"[{recipe.name}] has missing ingredients.");
                    continue;
                }

                inputs.Sort((a, b) => a.name.CompareTo(b.name));
                string key = string.Join("-", inputs.Select(i => i.name));

                if (!seenKeys.Add(key))
                {
                    Debug.LogError($"Duplicate recipe key detected: {key} in [{recipe.name}]");
                }

                if (recipe.OutputPotion == null)
                {
                    Debug.LogWarning($"[{recipe.name}] is missing an output potion.");
                }
            }
        }
#endif
    }
}