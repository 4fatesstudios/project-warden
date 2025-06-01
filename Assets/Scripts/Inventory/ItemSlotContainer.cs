using System;
using System.Collections.Generic;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using UnityEngine;

namespace FourFatesStudios.ProjectWarden.Inventory
{
    [System.Serializable]
    public class ItemSlotContainer<T> where T : Item {
        private List<ItemSlot> _items = new List<ItemSlot>();
        private int _maxQuantityPerSlot;
        private const int DefaultMaxQuantity = 499;
        
        public IReadOnlyList<ItemSlot> Slots => _items.AsReadOnly();
        public int Count => _items.Count;
        public ItemSlot this[int index] => _items[index];
        public int MaxQuantityPerSlot => _maxQuantityPerSlot;

        public ItemSlotContainer(int maxQuantityPerSlot = DefaultMaxQuantity) {
            _maxQuantityPerSlot = maxQuantityPerSlot;
        }

        public ItemSlotContainer(ItemSlotContainer<T> itemSlotContainer) {
            _items = new List<ItemSlot>(itemSlotContainer._items);
            _maxQuantityPerSlot = itemSlotContainer.MaxQuantityPerSlot;
        }

        public void Add(T item, int amount = 1) {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "ItemSlotContainer: item cannot be null");
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "ItemSlotContainer: quantity cannot be 0 or negative");

            int lastIndex = -1;

            for (int i = 0; i < _items.Count; i++) {
                var slot = _items[i];
                if (slot.Item.Equals(item)) {
                    lastIndex = i;
                    amount = slot.AddQuantity(amount);
                    if (amount == 0) return;
                }
            }

            while (amount > 0) {
                int insertIndex = (lastIndex >= 0) ? lastIndex + 1 : _items.Count;
                int toAdd = Mathf.Min(amount, _maxQuantityPerSlot);
                var newSlot = new ItemSlot(item, toAdd, _maxQuantityPerSlot);
                _items.Insert(insertIndex, newSlot);
                amount -= toAdd;
                lastIndex = insertIndex;
            }
        }
        
        [System.Serializable]
        public class ItemSlot {

            private T _item;
            private int _quantity;
            private int _maxQuantity;

            public T Item => _item;
            public int Quantity => _quantity;
            public int MaxQuantity => _maxQuantity;

            internal ItemSlot(T item, int quantity = 1, int maxQuantity = DefaultMaxQuantity) {
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

                if (amount > _quantity)
                    return amount - _quantity;

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
