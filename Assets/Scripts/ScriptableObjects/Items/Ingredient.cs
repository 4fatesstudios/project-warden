using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    [CreateAssetMenu(fileName = "NewIngredient", menuName = "Items/Ingredient")]
    public class Ingredient : Item {
        [SerializeField, Tooltip("Alchemical ingredient type")]
        private IngredientArchetype ingredientArchetype;
        
        public IngredientArchetype IngredientArchetype => ingredientArchetype;
    }
}