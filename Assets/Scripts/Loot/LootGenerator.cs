using FourFatesStudios.ProjectWarden.Enums;
using FourFatesStudios.ProjectWarden.Interfaces;
using FourFatesStudios.ProjectWarden.ScriptableObjects.LootTable;
using UnityEngine;
using UnityEngine.VFX;


namespace FourFatesStudios.ProjectWarden.Loot
{
    public class LootGenerator : MonoBehaviour {
        [SerializeField] private LootTable lootTable;
        [SerializeField] private Transform lootGenerationOrigin;
        [SerializeField] private VisualEffect lootVisualEffect;

        private void OnEnable() {
            SetLootColorToHighestRarity();
        }
        
        private void SetLootColorToHighestRarity() {
            if (lootVisualEffect.HasVector4("Color")) {
                lootVisualEffect.SetVector4("Color", RarityColors.Colors[lootTable.GetHighestRarity()]);
            } else {
                Debug.Log("Color property not found in LootOrb");
            }
        }
    }
}