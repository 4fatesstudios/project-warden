using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Inventory;

namespace FourFatesStudios.ProjectWarden
{
    /// <summary>
    /// Visual component for ItemSlotContainerHolder that displays inventory slots in UI Toolkit
    /// Positions the inventory at the top-right corner of the screen
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class InventorySlotVisualizer : MonoBehaviour
    {
        [Header("Inventory References")]
        [SerializeField] private ItemSlotContainerHolder inventoryHolder;
        [SerializeField] private VisualTreeAsset inventoryUIAsset;
        
        [Header("Display Settings")]
        [SerializeField] private bool startCollapsed = false;
        
        #pragma warning disable 0414
        [SerializeField] private int maxVisibleRows = 6;
        [SerializeField] private int slotsPerRow = 5;
        #pragma warning restore 0414
        
        [SerializeField] private bool enableAutoRefresh = true;
        [SerializeField] private float autoRefreshInterval = 1f;
        
        // UI Elements
        private UIDocument uiDocument;
        private VisualElement inventoryContainer;
        private VisualElement inventoryGrid;
        private Label slotCountLabel;
        private Button toggleButton;
        private ScrollView inventoryScroll;
        
        // State tracking
        private bool isCollapsed = false;
        private float lastRefreshTime;
        
        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
            
            // Find inventory holder if not assigned
            if (inventoryHolder == null)
            {
                inventoryHolder = FindFirstObjectByType<ItemSlotContainerHolder>();
            }
        }
        
        private void Start()
        {
            InitializeUI();
            SetupEventHandlers();
            
            if (startCollapsed)
            {
                ToggleCollapse();
            }
            
            RefreshInventoryDisplay();
        }
        
        private void Update()
        {
            if (enableAutoRefresh && Time.time - lastRefreshTime > autoRefreshInterval)
            {
                RefreshInventoryDisplay();
                lastRefreshTime = Time.time;
            }
        }
        
        private void InitializeUI()
        {
            if (uiDocument == null)
            {
                Debug.LogError("InventorySlotVisualizer: UIDocument component not found!");
                return;
            }
            
            // Load the inventory UI if asset is assigned
            if (inventoryUIAsset != null)
            {
                uiDocument.visualTreeAsset = inventoryUIAsset;
            }
            
            var root = uiDocument.rootVisualElement;
            
            // Get UI element references
            inventoryContainer = root.Q<VisualElement>("inventory-container");
            inventoryGrid = root.Q<VisualElement>("inventory-grid");
            slotCountLabel = root.Q<Label>("slot-count");
            toggleButton = root.Q<Button>("toggle-button");
            inventoryScroll = root.Q<ScrollView>("inventory-scroll");
            
            if (inventoryContainer == null)
            {
                Debug.LogError("InventorySlotVisualizer: Could not find inventory-container in UI!");
                return;
            }
            
            Debug.Log("✅ InventorySlotVisualizer: UI initialized successfully");
        }
        
        private void SetupEventHandlers()
        {
            if (toggleButton != null)
            {
                toggleButton.clicked += ToggleCollapse;
            }
        }
        
        public void RefreshInventoryDisplay()
        {
            if (inventoryHolder == null || inventoryGrid == null)
            {
                return;
            }
            
            // Clear existing slots
            inventoryGrid.Clear();
            
            var container = inventoryHolder.Container;
            if (container == null)
            {
                UpdateSlotCount(0, 0);
                return;
            }
            
            int filledSlots = 0;
            int totalSlots = container.MaxSlots;
            
            // Create visual slots for all inventory slots
            for (int i = 0; i < totalSlots; i++)
            {
                var slotElement = CreateSlotElement(i);
                inventoryGrid.Add(slotElement);
                
                // Fill slot if it contains an item
                if (i < container.Slots.Count && container.Slots[i].Item != null)
                {
                    FillSlot(slotElement, container.Slots[i]);
                    filledSlots++;
                }
            }
            
            UpdateSlotCount(filledSlots, totalSlots);
        }
        
        private VisualElement CreateSlotElement(int slotIndex)
        {
            var slot = new VisualElement();
            slot.AddToClassList("inventory-slot");
            slot.name = $"slot-{slotIndex}";
            
            // Add tooltip for empty slot
            slot.tooltip = $"Slot {slotIndex + 1}: Empty";
            
            return slot;
        }
        
        private void FillSlot(VisualElement slotElement, ItemSlotContainer<Item>.ItemSlot slot)
        {
            slotElement.AddToClassList("occupied");
            
            // Create icon element
            var iconElement = new VisualElement();
            iconElement.AddToClassList("slot-icon");
            slotElement.Add(iconElement);
            
            // Set icon from item if available
            if (slot.Item.ItemIcon != null)
            {
                iconElement.style.backgroundImage = new StyleBackground(slot.Item.ItemIcon);
            }
            else
            {
                // Use a default background color for items without icons
                iconElement.style.backgroundColor = GetItemTypeColor(slot.Item);
            }
            
            // Create quantity label if more than 1
            if (slot.Quantity > 1)
            {
                var quantityLabel = new Label(slot.Quantity.ToString());
                quantityLabel.AddToClassList("slot-quantity");
                slotElement.Add(quantityLabel);
            }
            
            // Update tooltip
            string tooltip = $"{slot.Item.ItemName}";
            if (slot.Quantity > 1)
            {
                tooltip += $" x{slot.Quantity}";
            }
            if (!string.IsNullOrEmpty(slot.Item.ItemDescription))
            {
                tooltip += $"\n{slot.Item.ItemDescription}";
            }
            slotElement.tooltip = tooltip;
        }
        
        private Color GetItemTypeColor(Item item)
        {
            // Return different colors based on item type
            switch (item)
            {
                case Potion _:
                    return new Color(0.8f, 0.4f, 0.8f, 0.7f); // Purple for potions
                case Ingredient _:
                    return new Color(0.4f, 0.8f, 0.4f, 0.7f); // Green for ingredients
                default:
                    return new Color(0.6f, 0.6f, 0.6f, 0.7f); // Gray for unknown
            }
        }
        
        private void UpdateSlotCount(int filled, int total)
        {
            if (slotCountLabel != null)
            {
                slotCountLabel.text = $"{filled}/{total}";
            }
        }
        
        private void ToggleCollapse()
        {
            isCollapsed = !isCollapsed;
            
            if (inventoryContainer != null)
            {
                if (isCollapsed)
                {
                    inventoryContainer.AddToClassList("collapsed");
                    if (toggleButton != null) toggleButton.text = "+";
                }
                else
                {
                    inventoryContainer.RemoveFromClassList("collapsed");
                    if (toggleButton != null) toggleButton.text = "−";
                }
            }
            
            Debug.Log($"📦 Inventory {(isCollapsed ? "collapsed" : "expanded")}");
        }
        
        [ContextMenu("🔄 Force Refresh Display")]
        public void ForceRefreshDisplay()
        {
            RefreshInventoryDisplay();
            Debug.Log("📦 Inventory display refreshed manually");
        }
        
        [ContextMenu("🔍 Debug Inventory State")]
        public void DebugInventoryState()
        {
            if (inventoryHolder == null)
            {
                Debug.Log("❌ No inventory holder assigned");
                return;
            }
            
            var container = inventoryHolder.Container;
            if (container == null)
            {
                Debug.Log("❌ Container not initialized");
                return;
            }
            
            Debug.Log($"📦 === INVENTORY DEBUG STATE ===");
            Debug.Log($"📦 Total slots: {container.MaxSlots}");
            Debug.Log($"📦 Used slots: {container.Slots.Count}");
            
            for (int i = 0; i < container.Slots.Count; i++)
            {
                var slot = container.Slots[i];
                if (slot.Item != null)
                {
                    Debug.Log($"📦 Slot {i}: {slot.Item.ItemName} x{slot.Quantity}");
                }
            }
            
            Debug.Log($"📦 === END DEBUG STATE ===");
        }
        
        [ContextMenu("🧪 Test Add Sample Items")]
        public void TestAddSampleItems()
        {
            if (inventoryHolder == null)
            {
                Debug.LogWarning("❌ No inventory holder to test with");
                return;
            }
            
            // Try to find some sample items to add
            var samplePotions = Resources.LoadAll<Potion>("Potions");
            var sampleIngredients = Resources.LoadAll<Ingredient>("Ingredients");
            
            if (samplePotions.Length > 0)
            {
                inventoryHolder.AddItem(samplePotions[0], 1);
                Debug.Log($"🧪 Added test potion: {samplePotions[0].ItemName}");
            }
            
            if (sampleIngredients.Length > 0 && sampleIngredients.Length > 1)
            {
                inventoryHolder.AddItem(sampleIngredients[0], 3);
                inventoryHolder.AddItem(sampleIngredients[1], 2);
                Debug.Log($"🧪 Added test ingredients");
            }
            
            RefreshInventoryDisplay();
        }
        
        private void OnDestroy()
        {
            // Clean up event handlers
            if (toggleButton != null)
            {
                toggleButton.clicked -= ToggleCollapse;
            }
        }
    }
}