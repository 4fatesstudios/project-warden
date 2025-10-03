using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Inventory;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.Characters.Components
{
    public class InventoryComponent : MonoBehaviour
    {
        private ItemSlotContainer<Trinket> trinkets;
        private ItemSlotContainer<Potion> potions;
        private ItemSlotContainer<Ingredient> ingredients;
        private ItemSlotContainer<KeyItem> keyItems;
        private ItemSlotContainer<Note> notes;
        private ItemSlotContainer<Recipe> recipes;
        
        public ItemSlotContainer<Trinket> Trinkets => trinkets;
        public ItemSlotContainer<Potion> Potions => potions;
        public ItemSlotContainer<Ingredient> Ingredients => ingredients;
        public ItemSlotContainer<KeyItem> KeyItems => keyItems;
        public ItemSlotContainer<Note> Notes => notes;
        public ItemSlotContainer<Recipe> Recipes => recipes;

        public void Awake() {
            trinkets = new ItemSlotContainer<Trinket>();
            potions = new ItemSlotContainer<Potion>();
            ingredients = new ItemSlotContainer<Ingredient>();
            keyItems = new ItemSlotContainer<KeyItem>();
            notes = new ItemSlotContainer<Note>();
            recipes = new ItemSlotContainer<Recipe>();
        }

        /// <summary>
        /// Gets all potions in the inventory with their total quantities.
        /// </summary>
        /// <returns>A dictionary mapping each potion to its total quantity across all slots.</returns>
        public Dictionary<Potion, int> GetAllPotionsWithCounts()
        {
            return potions.GetAllItemsWithCounts();
        }

        /// <summary>
        /// Adds a potion to the inventory.
        /// </summary>
        /// <param name="potion">The potion to add</param>
        /// <param name="amount">The amount to add (default: 1)</param>
        /// <returns>The amount that could not be added due to capacity limits</returns>
        public int AddPotion(Potion potion, int amount = 1)
        {
            return potions.Add(potion, amount);
        }

        /// <summary>
        /// Removes a potion from the inventory.
        /// </summary>
        /// <param name="potion">The potion to remove</param>
        /// <param name="amount">The amount to remove (default: 1)</param>
        /// <returns>The amount that could not be removed (if insufficient quantity)</returns>
        public int RemovePotion(Potion potion, int amount = 1)
        {
            return potions.Remove(potion, amount);
        }

        /// <summary>
        /// Gets the total count of a specific potion in the inventory.
        /// </summary>
        /// <param name="potion">The potion to count</param>
        /// <returns>The total quantity of the potion</returns>
        public int GetPotionCount(Potion potion)
        {
            return potions.GetItemCount(potion);
        }
    }
}