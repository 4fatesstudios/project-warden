using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    [CreateAssetMenu(fileName = "NewTrinket", menuName = "Items/Trinket")]
    public class Trinket : Item
    {
        [SerializeField, Tooltip("Trinket type.")]
        private ItemTrinketType itemTrinkeyType;

        public ItemTrinketType ItemTrinketType => itemTrinkeyType;
    }
}