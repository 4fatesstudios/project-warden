using System;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Inventory
{
    [System.Serializable]
    public class ItemSlotContainer<T> where T : Item {
        private List<ItemSlot> _items;
        private int _maxQuantityPerSlot;
        private const int DefaultMaxQuantity = 499;
        private int _maxSlots;
        
        public IReadOnlyList<ItemSlot> Slots => _items.AsReadOnly();
        /// <summary>
        /// Current amount of slots occupied in ItemSlotContainer
        /// </summary>
        public int Count => _items.Count;
        public ItemSlot this[int index] => _items[index];
        /// <summary>
        /// Max items that can go in each slot in ItemSlotContainer
        /// </summary>
        public int MaxQuantityPerSlot => _maxQuantityPerSlot;
        /// <summary>
        /// Maximum size of the ItemSlotContainer, 0 if no limit
        /// </summary>
        public int MaxSlots => _maxSlots;

        public ItemSlotContainer(int maxQuantityPerSlot = DefaultMaxQuantity, int maxSlots = 0) {
            _items = new List<ItemSlot>();
            _maxQuantityPerSlot = maxQuantityPerSlot;
            _maxSlots = maxSlots;
        }

        public ItemSlotContainer(ItemSlotContainer<T> itemSlotContainer) {
            _items = new List<ItemSlot>(itemSlotContainer._items);
            _maxQuantityPerSlot = itemSlotContainer.MaxQuantityPerSlot;
            _maxSlots = itemSlotContainer.MaxSlots;
        }
        
        /// <summary>
        /// Adds a specified amount of the given item to the container.
        /// Attempts to fill existing partial slots first, then adds new slots as needed.
        /// If the container has a maximum slot limit, adding stops when the limit is reached.
        /// </summary>
        /// <param name="item">The item to add. Cannot be null.</param>
        /// <param name="amount">The quantity of the item to add. Must be greater than zero.</param>
        /// <returns>The amount of the item that could not be added due to slot limits or capacity.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided <paramref name="item"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="amount"/> is zero or negative.</exception>
        public int Add(T item, int amount = 1) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "ItemSlotContainer: item cannot be null");
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "ItemSlotContainer: quantity cannot be 0 or negative");

            // Fill existing partial slots first
            for (int i = 0; i < _items.Count && amount > 0; i++) {
                var slot = _items[i];
                if (slot.Item.Equals(item) && slot.Quantity < _maxQuantityPerSlot) {
                    amount = slot.AddQuantity(amount);
                }
            }

            // Add new slots if needed and possible
            while (amount > 0) {
                if (MaxSlots > 0 && Count >= MaxSlots)
                    break;  // Can't add more slots, stop here and return remainder

                int toAdd = Mathf.Min(amount, _maxQuantityPerSlot);
                var newSlot = new ItemSlot(item, toAdd, _maxQuantityPerSlot);
                _items.Add(newSlot);
                amount -= toAdd;
            }

            return amount; // Return leftover amount that couldn't be added
        }
        
        /// <summary>
        /// Attempts to remove a specified amount of the given item from the container.
        /// If the container does not have enough total quantity of the item, no removal occurs.
        /// Removal prioritizes slots with the smallest quantities first. Slots reduced to zero are removed from the container.
        /// The order of remaining slots is preserved.
        /// </summary>
        /// <param name="item">The item to remove. Cannot be null.</param>
        /// <param name="amount">The quantity of the item to remove. Must be greater than zero.</param>
        /// <returns><c>true</c> if the removal was successful; <c>false</c> if the total quantity was insufficient or the item was not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the provided <paramref name="item"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="amount"/> is zero or negative.</exception>
        public int Remove(T item, int amount = 1) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "ItemSlotContainer: item cannot be null");
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "ItemSlotContainer: quantity cannot be 0 or negative");

            // Calculate total quantity of the item in the container
            int totalQuantity = 0;
            foreach (var slot in _items) {
                if (slot.Item.Equals(item)) {
                    totalQuantity += slot.Quantity;
                }
            }

            // If not enough quantity to remove, return full amount (no removal)
            if (totalQuantity < amount) {
                return amount;
            }

            // Get all slots with this item, sorted ascending by quantity
            var slots = new List<ItemSlot>();
            foreach (var slot in _items) {
                if (slot.Item.Equals(item)) {
                    slots.Add(slot);
                }
            }
            slots.Sort((a, b) => a.Quantity.CompareTo(b.Quantity));

            // Remove from smallest slot amounts first
            foreach (var slot in slots) {
                if (amount <= 0) break;

                amount = slot.SubtractQuantity(amount);
                if (slot.Quantity == 0) {
                    _items.Remove(slot);
                }
            }

            return amount; // Should be 0 here, since we already checked totalQuantity
        }



        
        [System.Serializable]
        public class ItemSlot {

            private T _item;
            private int _quantity;
            private int _maxQuantity;

            public T Item => _item;
            public int Quantity => _quantity;
            public int MaxQuantity => _maxQuantity;

            public ItemSlot(T item, int quantity = 1, int maxQuantity = DefaultMaxQuantity) {
                if (item == null)
                    throw new ArgumentNullException(nameof(item), "ItemSlot: item cannot be null");

                _maxQuantity = maxQuantity;
                SetItem(item, quantity);
            }

            public void SetItem(T item, int quantity = 1) {
                if (item == null)
                    throw new ArgumentNullException(nameof(item), "ItemSlot: item cannot be null");

                _item = item;
                _quantity = quantity;
                ClampQuantity();
            }

            public int AddQuantity(int amount) {
                if (amount < 0)
                    throw new ArgumentOutOfRangeException(nameof(amount), "ItemSlot: AddQuantity: Cannot add a negative amount, use SubtractQuantity instead.");

                int spaceLeft = _maxQuantity - _quantity;

                if (amount <= spaceLeft) {
                    _quantity += amount;
                    return 0;
                }

                _quantity = _maxQuantity;
                return amount - spaceLeft;
            }

            public int SubtractQuantity(int amount) {
                if (amount < 0)
                    throw new ArgumentOutOfRangeException(nameof(amount), "ItemSlot: SubtractQuantity: Cannot subtract a negative amount, use AddQuantity instead.");

                if (amount >= _quantity) {
                    int leftover = amount - _quantity;
                    _quantity = 0;
                    return leftover;
                }

                _quantity -= amount;
                return 0;
            }


            private void ClampQuantity() {
                if (_quantity is <= DefaultMaxQuantity and >= 1) return;
                _quantity = Mathf.Clamp(_quantity, 1, DefaultMaxQuantity);
                Debug.LogWarning("ItemSlot: ERROR, attempted to set quantity to invalid range");
            }
        }
    }

    
}
