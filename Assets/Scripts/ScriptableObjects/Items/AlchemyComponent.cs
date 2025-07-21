using UnityEngine;

namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    [CreateAssetMenu(fileName = "NewComponent", menuName = "Items/AlchemyComponent")]
    public class AlchemyComponent : Ingredient
    {
        [SerializeField, Tooltip("The first base ingredient used to create this component.")]
        private Ingredient baseIngredient1;

        [SerializeField, Tooltip("The second base ingredient used to create this component.")]
        private Ingredient baseIngredient2;

        public Ingredient BaseIngredient1 => baseIngredient1;
        public Ingredient BaseIngredient2 => baseIngredient2;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (baseIngredient1 == null || baseIngredient2 == null)
            {
                Debug.LogWarning($"AlchemyComponent [{name}] is missing base ingredients.");
            }
            else
            {
                string[] names = new[] { baseIngredient1.name, baseIngredient2.name };
                System.Array.Sort(names);
                Debug.Log($"AlchemyComponent [{name}] made from: {names[0]} + {names[1]}");
            }
        }
#endif
    }
}
