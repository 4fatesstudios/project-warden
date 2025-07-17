using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.LootTable
{
    [CreateAssetMenu(menuName = "Loot Table", fileName = "New Loot Table")]
    public class LootTable : ScriptableObject, IEnumerable<LootTableEntry> {
        [SerializeField] private LootTableEntry[] lootEntries;

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
}