using System;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.InputSystem.Controls;


namespace FourFatesStudios.ProjectWarden.Inventory
{
    [System.Serializable]
    public class ItemSlotContainer<T> where T : Item {
        private List<T> _items = new List<T>();
        
        public IReadOnlyList<T> Items => _items.AsReadOnly();
        public int Count => _items.Count;
        public T this[int index] => _items[index];

        public ItemSlotContainer() { }

        public ItemSlotContainer(List<T> items) {
            _items = items;
        }
        
        
    }

    [System.Serializable]
    public class ItemSlot<T> where T : Item {
        private T _item;
        private int _quantity;
        private int _maxQuantity;
        private const int MAX_STACKABLE_QUANTITY = 499;

        public T Item => _item;
        public int Quantity => _quantity;
        public int MaxQuantity => _maxQuantity;

        public ItemSlot(T item, int quantity=1, int maxQuantity=MAX_STACKABLE_QUANTITY) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "ItemSlot: item cannot be null");
            
            SetItem(item, quantity);
        }
        
        /// <summary>
        /// Sets item slot to the specified Item and quantity
        /// </summary>
        /// <param name="item">The item to set the slot to</param>
        /// <param name="quantity">The initial quantity to set the slot to</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> is null</exception>
        public void SetItem(T item, int quantity=1) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "ItemSlot: item cannot be null");
            
            this._item = item;
            this._quantity = quantity;
            ClampQuantity();
        }
    
        /// <summary>
        /// Adds a specified amount from the current quantity.
        /// </summary>
        /// <param name="amount">The amount to add to the quantity</param>
        /// <returns>Returns the amount that could not be added to the item slot</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="amount"/> is negative.</exception>
        public int AddQuantity(int amount) {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "ItemSlot: AddQuantity: Cannot add a negative amount, use SubtractQuantity instead.");

            int spaceLeft = _maxQuantity - _quantity;

            if (amount <= spaceLeft) {
                _quantity += amount;
                return 0;
            }

            _quantity = _maxQuantity;
            return amount - spaceLeft; // remainder
        }
        
        /// <summary>
        /// Attempts to subtract a specified amount from the current quantity.
        /// If there is not enough quantity, the subtraction does not occur.
        /// </summary>
        /// <param name="amount">The amount to subtract from the quantity.</param>
        /// <returns>
        /// Returns <c>0</c> if the subtraction is successful.
        /// Returns the amount that could not be subtracted (i.e., <paramref name="amount"/> - <c>quantity</c>)
        /// if there is not enough quantity available.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="amount"/> is negative.
        /// </exception>
        public int SubtractQuantity(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "ItemSlot: SubtractQuantity: Cannot subtract a negative amount, use AddQuantity instead.");

            if (amount > _quantity)
                return amount - _quantity; // not enough to subtract, return what's missing

            _quantity -= amount;
            return 0;
        }
        
        private void ClampQuantity() {
            if (_quantity is <= MAX_STACKABLE_QUANTITY and >= 1) return;
            _quantity = Mathf.Clamp(_quantity, 1, MAX_STACKABLE_QUANTITY);
            Debug.LogWarning("ItemSlot: ERROR, attempted to set quantity to invalid range");
        }
    }
}