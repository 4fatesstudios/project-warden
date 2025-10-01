using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Inventory;

namespace FourFatesStudios.ProjectWarden
{
    /// <summary>
    /// This is a temporary holder for an ItemSlotContainer that can be used in the Unity Editor.
    /// </summary>
    public class ItemSlotContainerHolder : MonoBehaviour
    {
        public Item[] initialItems;
        public int[] initialQuantities;

        public ItemSlotContainer<Item> Container { get; private set; }

        [SerializeField]
        private int maxQuantityPerSlot = 499;
        [SerializeField]
        private int maxSlots = 30;

        private void Awake()
        {
            Container = new ItemSlotContainer<Item>(maxQuantityPerSlot, maxSlots);

            for (int i = 0; i < Mathf.Min(initialItems.Length, initialQuantities.Length); i++)
            {
                if (initialItems[i] != null)
                {
                    Container.Add(initialItems[i], initialQuantities[i]);
                }
            }
        }

        // Optional helper for debug
        public void AddItem(Item item, int quantity)
        {
            Container.Add(item, quantity);
        }

        public void RemoveItem(Item item, int quantity)
        {
            Container.Remove(item, quantity);
        }


        private void OnGUI()
        {
#if UNITY_EDITOR
            // Set up a rect at the top-right corner of the screen
            float width = 200f;   // how wide you want the panel
            float height = 300f;  // how tall you want it (adjust as needed)
            float x = Screen.width - width - 10f; // 10px padding from right
            float y = 10f; // 10px padding from top

            Rect panelRect = new Rect(x, y, width, height);

            GUILayout.BeginArea(panelRect, GUI.skin.box); // optional box background

            GUILayout.Label("Inventory Contents:");
            foreach (var slot in Container.Slots)
            {
                if (slot.Item != null) // safeguard
                    GUILayout.Label($"{slot.Item.ItemName} x{slot.Quantity}");
            }

            GUILayout.EndArea();
#endif
        }


    }
}
