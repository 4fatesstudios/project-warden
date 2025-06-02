using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Items
{
    [CreateAssetMenu(fileName = "NewEquipment", menuName = "Items/Equipment")]
    public class Equipment : Item
    {
        [SerializeField, Tooltip("Equipment type.")]
        private ItemEquipmentType itemEquipmentType;

        public ItemEquipmentType ItemEquipmentType => itemEquipmentType;
    }
}