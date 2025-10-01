using UnityEngine;
using System.Linq;
using FourFatesStudios.ProjectWarden.Characters.Components;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Example integration showing how to connect crafting systems with the new inventory UI
    /// Demonstrates best practices for adding potions and updating the UI
    /// </summary>
    public class CraftingIntegrationExample : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private Potion[] testPotions;
        
        private InventoryComponent playerInventory;
        
        private void Start()
        {
            // Find the player inventory
            FindInventoryComponent();
        }
        
        /// <summary>
        /// Find the inventory component (automatically set up by UIStartupManager)
        /// </summary>
        private void FindInventoryComponent()
        {
            // Try to find on player first
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerInventory = player.GetComponent<InventoryComponent>();
            }
            
            // If not found on player, search globally
            if (playerInventory == null)
            {
                playerInventory = FindFirstObjectByType<InventoryComponent>();
            }
            
            if (playerInventory == null)
            {
                Debug.LogWarning("⚠️ CraftingIntegrationExample: No InventoryComponent found. Make sure UIStartupManager is running.");
            }
            else if (enableDebugLogging)
            {
                Debug.Log($"📦 Found inventory on: {playerInventory.gameObject.name}");
            }
        }
        
        /// <summary>
        /// Example: Craft a potion and add it to inventory
        /// This shows the proper way to integrate crafting with the new system
        /// </summary>
        public void CraftPotion(Potion potion, int quantity = 1)
        {
            if (playerInventory == null)
            {
                Debug.LogError("❌ Cannot craft potion: No inventory component available");
                return;
            }
            
            if (potion == null)
            {
                Debug.LogError("❌ Cannot craft potion: Potion is null");
                return;
            }
            
            if (enableDebugLogging)
                Debug.Log($"🧪 Crafting {quantity}x {potion.ItemName}...");
            
            // Add the potion to inventory
            // The inventory will automatically:
            // 1. Add to the appropriate slot or create new slots
            // 2. Fire OnPotionAdded event
            // 3. Update the UI through connected event handlers
            int remainingAmount = playerInventory.AddPotion(potion, quantity);
            
            if (remainingAmount == 0)
            {
                if (enableDebugLogging)
                    Debug.Log($"✅ Successfully crafted {quantity}x {potion.ItemName}");
            }
            else
            {
                int addedAmount = quantity - remainingAmount;
                Debug.LogWarning($"⚠️ Inventory full! Added {addedAmount}x {potion.ItemName}, {remainingAmount}x could not be added");
            }
            
            // Note: UI will update automatically, no manual refresh needed!
        }
        
        /// <summary>
        /// Example: Use a potion from inventory
        /// </summary>
        public void UsePotion(Potion potion, int quantity = 1)
        {
            if (playerInventory == null || potion == null) return;
            
            // Check if we have enough
            int currentCount = playerInventory.GetPotionCount(potion);
            if (currentCount < quantity)
            {
                Debug.LogWarning($"⚠️ Not enough {potion.ItemName} to use {quantity}x (have: {currentCount})");
                return;
            }
            
            if (enableDebugLogging)
                Debug.Log($"🍾 Using {quantity}x {potion.ItemName}...");
            
            // Remove from inventory
            int remainingToRemove = playerInventory.RemovePotion(potion, quantity);
            
            if (remainingToRemove == 0)
            {
                if (enableDebugLogging)
                    Debug.Log($"✅ Used {quantity}x {potion.ItemName}");
                
                // Apply potion effects here
                ApplyPotionEffects(potion, quantity);
            }
            else
            {
                Debug.LogError($"❌ Failed to remove {remainingToRemove}x {potion.ItemName} from inventory");
            }
        }
        
        /// <summary>
        /// Apply potion effects (placeholder for actual game logic)
        /// </summary>
        private void ApplyPotionEffects(Potion potion, int quantity)
        {
            // This is where you'd implement actual potion effects
            // For example: heal player, apply buffs, etc.
            Debug.Log($"💫 Applied effects of {quantity}x {potion.ItemName}");
        }
        
        /// <summary>
        /// Get current inventory status for display
        /// </summary>
        public void DisplayInventoryStatus()
        {
            if (playerInventory == null) return;
            
            Debug.Log("=== Inventory Status ===");
            
            var potionsWithCounts = playerInventory.GetAllPotionsWithCounts();
            
            if (potionsWithCounts.Count == 0)
            {
                Debug.Log("No potions in inventory");
                return;
            }
            
            foreach (var kvp in potionsWithCounts)
            {
                Debug.Log($"📦 {kvp.Key.ItemName}: {kvp.Value}x");
            }
            
            Debug.Log($"Total potion types: {potionsWithCounts.Count}");
            Debug.Log($"Total potion instances: {potionsWithCounts.Values.Sum()}");
        }
        
        // Context menu methods for testing
        
        [ContextMenu("Craft Test Potion")]
        public void CraftTestPotion()
        {
            if (testPotions != null && testPotions.Length > 0)
            {
                var randomPotion = testPotions[Random.Range(0, testPotions.Length)];
                CraftPotion(randomPotion);
            }
            else
            {
                Debug.LogWarning("No test potions assigned");
            }
        }
        
        [ContextMenu("Craft Multiple Test Potions")]
        public void CraftMultipleTestPotions()
        {
            if (testPotions != null && testPotions.Length > 0)
            {
                var randomPotion = testPotions[Random.Range(0, testPotions.Length)];
                int quantity = Random.Range(2, 6);
                CraftPotion(randomPotion, quantity);
            }
            else
            {
                Debug.LogWarning("No test potions assigned");
            }
        }
        
        [ContextMenu("Use Random Potion")]
        public void UseRandomPotion()
        {
            if (playerInventory == null) return;
            
            var potionsWithCounts = playerInventory.GetAllPotionsWithCounts();
            if (potionsWithCounts.Count == 0)
            {
                Debug.Log("No potions to use");
                return;
            }
            
            var randomPotion = potionsWithCounts.Keys.ElementAt(Random.Range(0, potionsWithCounts.Count));
            UsePotion(randomPotion);
        }
        
        [ContextMenu("Display Inventory Status")]
        public void DisplayInventoryStatusCommand()
        {
            DisplayInventoryStatus();
        }
        
        [ContextMenu("Load Test Potions from Resources")]
        public void LoadTestPotionsFromResources()
        {
            testPotions = Resources.LoadAll<Potion>("Potions");
            Debug.Log($"📦 Loaded {testPotions.Length} test potions from Resources/Potions");
        }
    }
}