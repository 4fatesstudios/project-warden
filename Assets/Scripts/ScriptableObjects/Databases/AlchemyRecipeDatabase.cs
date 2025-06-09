using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes
{
    [CreateAssetMenu(fileName = "AlchemyRecipeDatabase", menuName = "AlchemyRecipes/Recipe Database")]
    public class AlchemyRecipeDatabase : ScriptableObject
    {
        [SerializeField]
        private List<AlchemyRecipe> recipes;

        public List<AlchemyRecipe> Recipes => recipes;

#if UNITY_EDITOR
        private void OnValidate()
        {
            var seenKeys = new HashSet<string>();

            foreach (var recipe in recipes)
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

                if (inputs.Count() < 2)
                {
                    Debug.LogWarning($"[{recipe.name}] has fewer than 2 ingredients.");
                    continue;
                }

                if ((inputs[0] == null || inputs[1] == null || inputs[2] == null) && inputs.Count() == 3)
                {
                    Debug.LogWarning($"[{recipe.name}] has missing ingredients.");
                    continue;
                }

                // Sort by name to make the key order-insensitive
                System.Array.Sort(inputs, (a, b) => a.name.CompareTo(b.name));
                string key = $"{inputs[0].name}-{inputs[1].name}-{inputs[2].name}";

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
