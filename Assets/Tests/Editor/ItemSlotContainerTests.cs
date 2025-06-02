using System;
using NUnit.Framework;
using FourFatesStudios.ProjectWarden.Inventory;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using UnityEngine;

namespace Tests.Editor
{
    public class ItemSlotContainerTests
    {
        private Item _itemA;
        private Item _itemB;

        [SetUp]
        public void Setup()
        {
            // Create items at runtime
            _itemA = ScriptableObject.CreateInstance<Item>();
            _itemA.name = "Test Item A";

            _itemB = ScriptableObject.CreateInstance<Item>();
            _itemB.name = "Test Item B";

            Assert.IsNotNull(_itemA);
            Assert.IsNotNull(_itemB);
        }

        [Test]
        public void Add_FillsPartialThenNewSlot()
        {
            var container = new ItemSlotContainer<Item>(maxQuantityPerSlot: 10);
            container.Add(_itemA, 5);
            container.Add(_itemA, 8); // 5 filled + 5 in 1st, 3 in new slot

            Assert.AreEqual(2, container.Count);
            Assert.AreEqual(10, container[0].Quantity);
            Assert.AreEqual(3, container[1].Quantity);
        }

        [Test]
        public void Add_StopsWhenMaxSlotsReached()
        {
            var container = new ItemSlotContainer<Item>(maxQuantityPerSlot: 5, maxSlots: 2);
            var leftover = container.Add(_itemA, 15);

            Assert.AreEqual(2, container.Count);
            Assert.AreEqual(5, container[0].Quantity);
            Assert.AreEqual(5, container[1].Quantity);
            Assert.AreEqual(5, leftover);
        }

        [Test]
        public void Remove_RemovesFromSmallestQuantityFirst()
        {
            var container = new ItemSlotContainer<Item>(maxQuantityPerSlot: 10);
            container.Add(_itemA, 15); // 10 + 5
            container[1].SubtractQuantity(3); // Make it 2

            var leftover = container.Remove(_itemA, 7); // 2 from 2nd, 5 from 1st

            Assert.AreEqual(1, container.Count);
            Assert.AreEqual(5, container[0].Quantity);
            Assert.AreEqual(0, leftover);
        }

        [Test]
        public void Remove_FailsIfNotEnoughTotal()
        {
            var container = new ItemSlotContainer<Item>(maxQuantityPerSlot: 10);
            container.Add(_itemA, 5);
            var result = container.Remove(_itemA, 10);

            Assert.AreEqual(10, result);
            Assert.AreEqual(1, container.Count);
        }

        [Test]
        public void Add_ThrowsIfNull()
        {
            var container = new ItemSlotContainer<Item>();
            Assert.Throws<ArgumentNullException>(() => container.Add(null));
        }

        [Test]
        public void Remove_ThrowsIfNull()
        {
            var container = new ItemSlotContainer<Item>();
            Assert.Throws<ArgumentNullException>(() => container.Remove(null));
        }

        [Test]
        public void Add_ThrowsIfNegative()
        {
            var container = new ItemSlotContainer<Item>();
            Assert.Throws<ArgumentOutOfRangeException>(() => container.Add(_itemA, -1));
        }

        [Test]
        public void Remove_ThrowsIfNegative()
        {
            var container = new ItemSlotContainer<Item>();
            Assert.Throws<ArgumentOutOfRangeException>(() => container.Remove(_itemA, -5));
        }

        [Test]
        public void AddQuantity_CapsAtMax()
        {
            var slot = new ItemSlotContainer<Item>.ItemSlot(_itemA, 8, 10);
            var leftover = slot.AddQuantity(5);
            Assert.AreEqual(10, slot.Quantity);
            Assert.AreEqual(3, leftover);
        }

        [Test]
        public void SubtractQuantity_BelowZero()
        {
            var slot = new ItemSlotContainer<Item>.ItemSlot(_itemA, 5);
            var leftover = slot.SubtractQuantity(7);
            Assert.AreEqual(0, slot.Quantity);
            Assert.AreEqual(2, leftover);
        }
    }
}
