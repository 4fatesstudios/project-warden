using System.Collections;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.Enums;
using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.LootTable
{
    [CreateAssetMenu(menuName = "Loot Table", fileName = "New Loot Table")]
    public class LootTable : ScriptableObject, IEnumerable<LootTableEntry> {
        [SerializeField] private LootTableEntry[] lootEntries;

        public Rarity GetHighestRarity() {
            var highest = Rarity.Common;

            foreach (var entry in lootEntries) {
                if (entry == null || entry.Item == null) continue;
                if (entry.Item.ItemRarity > highest) {
                    highest = entry.Item.ItemRarity;
                }
            }

            return highest;
        }

        private void OnValidate() {
            foreach (var lootEntry in lootEntries) {
                lootEntry?.Validate(this);
            }
        }

        public IEnumerator<LootTableEntry> GetEnumerator() {
            return ((IEnumerable<LootTableEntry>)lootEntries).GetEnumerator();
        }
            
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
    
    [System.Serializable]
    public class LootTableEntry {
        [SerializeField] private int minimumDropAmount;
        [SerializeField] private int maximumDropAmount;
        [SerializeField] private Item item;
        private const int MAX = 100;

        public int MinimumDropAmount => minimumDropAmount;
        public int MaximumDropAmount => maximumDropAmount;
        public Item Item => item;
        
        public void Validate(LootTable parentLootTable) {
            minimumDropAmount = Mathf.Clamp(minimumDropAmount, 0, maximumDropAmount);
            maximumDropAmount = Mathf.Clamp(maximumDropAmount, minimumDropAmount, MAX);
            if (item == null) {
                Debug.LogWarning($"Missing loot in '{parentLootTable.name}'!", parentLootTable);
            }
        }
    }
}