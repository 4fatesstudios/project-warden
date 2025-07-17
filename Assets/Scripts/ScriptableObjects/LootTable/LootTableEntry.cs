using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.LootTable
{
    [System.Serializable]
    public class LootTableEntry : MonoBehaviour {
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