using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Databases.Items
{
    [CreateAssetMenu(fileName = "IngredientDatabase", menuName = "Databases/Items/Ingredient Database")]
    public class IngredientDatabase : ItemDatabase<Ingredient> { }
}