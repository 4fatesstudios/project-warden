using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using UnityEngine;
using UnityEngine.VFX;


namespace FourFatesStudios.ProjectWarden.Loot
{
    public class Loot : MonoBehaviour
    {
        [SerializeField] private VisualEffect lootVisualEffect;
        [SerializeField] private Item item;

        public void Initialize(Item item) {
            this.item = item;
            UpdateLootColor();
        }
        
        private void UpdateLootColor() {
            if (lootVisualEffect == null || item == null) return;
            lootVisualEffect.SetVector4("Color", RarityColors.Colors[item.ItemRarity]);
        }

        public void LootItem() {
            Debug.Log("Item looted (logic not yet implemented)");
            
            Destroy(gameObject);
        }
    }
}