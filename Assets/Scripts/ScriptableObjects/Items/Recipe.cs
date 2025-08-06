using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.AlchemyRecipes;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    [CreateAssetMenu(fileName = "NewRecipe", menuName = "Items/Recipe")]
    public class Recipe : Item
    {
        [SerializeField, Tooltip("The Alchemy Recipe used to represent this item.")]
        private AlchemyRecipe recipe1;
        public AlchemyRecipe Recipe1 => recipe1;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Recipe1 == null)
                Debug.LogWarning($"[{name}] AlchemyRecipe has not been assigned.");
        }
#endif

    }
}