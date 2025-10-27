using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.Stats;
using UnityEditor;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    [CreateAssetMenu(fileName = "NewTrinket", menuName = "Items/Trinket")]
    public class Trinket : Item
    {
        [SerializeField, Tooltip("Trinket type.")]
        private ItemTrinketType itemTrinketType;
        
        [SerializeField, Tooltip("Trinket stat modifiers.")]
        private StatModifierList statModifierList;

        public ItemTrinketType ItemTrinketType => itemTrinketType;
        public StatModifierList StatModifierList {
            get => statModifierList;
            set => statModifierList = value;
        }
    }
}