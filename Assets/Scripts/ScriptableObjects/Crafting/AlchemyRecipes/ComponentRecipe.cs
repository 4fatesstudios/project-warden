using System;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes
{
    [CreateAssetMenu(fileName = "NewComponentRecipe", menuName = "AlchemyRecipes/AlchemyComponent Recipe")]
    public class ComponentRecipe : ScriptableObject
    {
        [SerializeField, Tooltip("First ingredient.")]
        private Ingredient inputIngredient1;

        [SerializeField, Tooltip("Second ingredient.")]
        private Ingredient inputIngredient2;

        [SerializeField, Tooltip("Output alchemical component.")]
        private AlchemyComponent outputComponent;

        public Ingredient InputIngredient1 => inputIngredient1;
        public Ingredient InputIngredient2 => inputIngredient2;
        public AlchemyComponent OutputComponent => outputComponent;


#if UNITY_EDITOR
        private void OnValidate()
        {
            if (inputIngredient1 == null)
                Debug.LogWarning($"[{name}] Input ingredient 1 is not assigned.");

            if (inputIngredient2 == null)
                Debug.LogWarning($"[{name}] Input ingredient 2 is not assigned.");

            if (outputComponent == null)
                Debug.LogWarning($"[{name}] Output component is not assigned.");

            if (inputIngredient1 != null && inputIngredient2 != null)
            {
                var sorted = new[] { inputIngredient1.name, inputIngredient2.name };
                Array.Sort(sorted);
                Debug.Log($"[{name}] AlchemyComponent Key: {sorted[0]}, {sorted[1]}");
            }
        }
#endif
    }
}
