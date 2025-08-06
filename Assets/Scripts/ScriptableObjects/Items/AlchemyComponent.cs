using ScriptableObjects.Items;
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

        public override int Potency
        {
            get
            {
                if (baseIngredient1 != null && baseIngredient2 != null)
                {
                    // Component potency is average of base ingredients, rounded up
                    return Mathf.CeilToInt((baseIngredient1.Potency + baseIngredient2.Potency) / 2f);
                }
                return base.Potency;
            }
        }

        public override int GridWidth
        {
            get
            {
                if (baseIngredient1 != null && baseIngredient2 != null)
                {
                    // Component size is the larger of the two base ingredients
                    return Mathf.Max(baseIngredient1.GridWidth, baseIngredient2.GridWidth);
                }
                return base.GridWidth;
            }
        }

        public override int GridHeight
        {
            get
            {
                if (baseIngredient1 != null && baseIngredient2 != null)
                {
                    // Component size is the larger of the two base ingredients
                    return Mathf.Max(baseIngredient1.GridHeight, baseIngredient2.GridHeight);
                }
                return base.GridHeight;
            }
        }

#if UNITY_EDITOR
        private new void OnValidate()
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
