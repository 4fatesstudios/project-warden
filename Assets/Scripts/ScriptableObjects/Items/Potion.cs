using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    [CreateAssetMenu(fileName = "NewPotion", menuName = "Items/Potion")]
    public class Potion : Item
    {
        [SerializeField, Tooltip("Potion item type.")]
        private ItemPotionType itemPotionType;
        
        public ItemPotionType ItemPotionType => itemPotionType;
    }
}