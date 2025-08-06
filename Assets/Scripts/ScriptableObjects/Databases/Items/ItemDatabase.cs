using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace FourFatesStudios.ProjectWarden.ScriptableObjects.Databases.Items
{
    public abstract class ItemDatabase : ScriptableObject {
        public abstract IReadOnlyList<Item> Items { get; }
        public abstract System.Type ItemType { get; }
        public abstract void SetItemsRaw(List<Item> items);
    }

    public class ItemDatabase<T> : ItemDatabase where T : Item {
        [SerializeField] private List<T> items;

        public override IReadOnlyList<Item> Items => items;
        public override System.Type ItemType => typeof(T);
        public void SetItems(List<T> newItems) => items = newItems;

        public override void SetItemsRaw(List<Item> rawItems) {
            SetItems(rawItems.Cast<T>().ToList());
        }
    }
}