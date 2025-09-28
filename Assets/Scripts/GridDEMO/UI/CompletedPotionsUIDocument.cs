using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Characters.Components;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Modern UI Toolkit-based completed potions list manager
    /// Replaces the traditional Unity UI implementation with a cleaner, more performant solution
    /// </summary>
    public class CompletedPotionsUIDocument : MonoBehaviour
    {
        [Header("UI Document Settings")]
        [SerializeField] private VisualTreeAsset potionListUXML;
        [SerializeField] private StyleSheet potionListStyleSheet;
        
        [Header("Positioning")]
        [SerializeField] private Vector2 panelPosition = new Vector2(10, 10);
        [SerializeField] private bool anchorToRight = true;
        [SerializeField] private bool anchorToTop = true;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float entryAnimationDelay = 0.1f;
        
        // UI Elements
        private UIDocument uiDocument;
        private VisualElement rootElement;
        private VisualElement potionsPanel;
        private ScrollView potionsScrollView;
        private VisualElement potionsContent;
        private Label headerLabel;
        private Label footerLabel;
        
        // Data tracking
        private Dictionary<Potion, PotionEntryData> completedPotions = new Dictionary<Potion, PotionEntryData>();
        private AlchemySkillSystem skillSystem;
        private InventoryComponent inventoryComponent;
        private bool isInitialized = false;
        
        // Static reference for external access
        public static CompletedPotionsUIDocument Instance { get; private set; }
        
        /// <summary>
        /// Data structure for tracking potion completion information
        /// </summary>
        [System.Serializable]
        public class PotionEntryData
        {
            public string potionName;
            public string recipeKey;
            public int timesCrafted;
            public int sRankCount;
            public bool isAutoCraftUnlocked;
            public float efficiency;
            
            public PotionEntryData(string name, string key)
            {
                potionName = name;
                recipeKey = key;
                timesCrafted = 1;
                sRankCount = 0;
                isAutoCraftUnlocked = false;
                efficiency = 0f;
            }
        }
        
        /// <summary>
        /// Check if the UI Document has been initialized
        /// </summary>
        public bool IsInitialized => isInitialized;
        
        /// <summary>
        /// Get the current number of completed potions
        /// </summary>
        public int CompletedPotionsCount => completedPotions.Count;
        
        private void Awake()
        {
            Debug.Log("🧪 CompletedPotionsUIDocument: Awake() called");
            
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                Debug.Log("🧪 CompletedPotionsUIDocument: Set as singleton instance");
            }
            else
            {
                Debug.LogWarning("🧪 CompletedPotionsUIDocument: Duplicate instance found, destroying...");
                Destroy(this);
                return;
            }
        }
        
        private void Start()
        {
            Debug.Log("🧪 CompletedPotionsUIDocument: Start() called");
            InitializeUIDocument();
        }
        
        /// <summary>
        /// Initialize the UI Document and set up the potion list interface
        /// </summary>
        private void InitializeUIDocument()
        {
            Debug.Log("🧪 CompletedPotionsUIDocument: Initializing UI Document...");
            
            try
            {
                // Get or create UIDocument component
                uiDocument = GetComponent<UIDocument>();
                if (uiDocument == null)
                {
                    uiDocument = gameObject.AddComponent<UIDocument>();
                    Debug.Log("🧪 Added UIDocument component");
                }
                
                // Load UXML asset if not already set
                if (uiDocument.visualTreeAsset == null)
                {
                    if (potionListUXML != null)
                    {
                        uiDocument.visualTreeAsset = potionListUXML;
                        Debug.Log("🧪 Set UXML asset from serialized field");
                    }
                    else
                    {
                        // Try to load the default UXML file
                        var defaultUXML = Resources.Load<VisualTreeAsset>("UI/UXML/CompletedPotionsList");
                        if (defaultUXML == null)
                        {
                            Debug.LogError("🧪 Could not find CompletedPotionsList.uxml! Please assign the UXML asset in the inspector.");
                            return;
                        }
                        uiDocument.visualTreeAsset = defaultUXML;
                        Debug.Log("🧪 Loaded default UXML asset from Resources");
                    }
                }
                
                // Get root visual element
                rootElement = uiDocument.rootVisualElement;
                if (rootElement == null)
                {
                    Debug.LogError("🧪 Failed to get root visual element from UIDocument!");
                    return;
                }
                
                // Cache UI element references
                CacheUIElements();
                
                // Set up positioning
                SetupPanelPositioning();
                
                // Initialize with alchemy skill system
                InitializeWithSkillSystem();
                
                // Set up initial UI state
                RefreshPotionsList();
                
                isInitialized = true;
                Debug.Log("✅ CompletedPotionsUIDocument: Initialization complete");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"🧪 Failed to initialize CompletedPotionsUIDocument: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Cache references to frequently used UI elements
        /// </summary>
        private void CacheUIElements()
        {
            potionsPanel = rootElement.Q<VisualElement>("completed-potions-panel");
            potionsScrollView = rootElement.Q<ScrollView>("potions-scroll-view");
            potionsContent = rootElement.Q<VisualElement>("potions-content");
            headerLabel = rootElement.Q<Label>("header-label");
            footerLabel = rootElement.Q<Label>("footer-label");
            
            if (potionsPanel == null)
                Debug.LogWarning("🧪 Could not find potions-panel element");
            if (potionsScrollView == null)
                Debug.LogWarning("🧪 Could not find potions-scroll-view element");
            if (potionsContent == null)
                Debug.LogWarning("🧪 Could not find potions-content element");
                
            Debug.Log("🧪 UI elements cached successfully");
        }
        
        /// <summary>
        /// Set up the panel positioning based on anchor settings
        /// </summary>
        private void SetupPanelPositioning()
        {
            if (potionsPanel == null) return;
            
            // Set positioning style
            potionsPanel.style.position = Position.Absolute;
            
            if (anchorToRight)
            {
                potionsPanel.style.right = panelPosition.x;
                potionsPanel.style.left = StyleKeyword.Auto;
            }
            else
            {
                potionsPanel.style.left = panelPosition.x;
                potionsPanel.style.right = StyleKeyword.Auto;
            }
            
            if (anchorToTop)
            {
                potionsPanel.style.top = panelPosition.y;
                potionsPanel.style.bottom = StyleKeyword.Auto;
            }
            else
            {
                potionsPanel.style.bottom = panelPosition.y;
                potionsPanel.style.top = StyleKeyword.Auto;
            }
            
            Debug.Log($"🧪 Panel positioned at {panelPosition} (Right: {anchorToRight}, Top: {anchorToTop})");
        }
        
        /// <summary>
        /// Initialize connection with the alchemy skill system
        /// </summary>
        private void InitializeWithSkillSystem()
        {
            skillSystem = AlchemySkillSystem.Instance;
            if (skillSystem == null)
            {
                Debug.LogWarning("🧪 AlchemySkillSystem not found - running in basic mode");
                SetFooterText("Basic tracking mode");
            }
            else
            {
                Debug.Log("🧪 Connected to AlchemySkillSystem");
                SetFooterText("Track your alchemy progress");
            }
        }
        
        /// <summary>
        /// Set the inventory component for automatic updates
        /// </summary>
        public void SetInventoryComponent(InventoryComponent inventory)
        {
            inventoryComponent = inventory;
            RefreshFromInventory();
            
            Debug.Log($"📦 CompletedPotionsUIDocument: Connected to inventory on {inventory.gameObject.name}");
        }
        
        /// <summary>
        /// Refresh the UI from the current inventory state using ItemSlotContainer data
        /// </summary>
        public void RefreshFromInventory()
        {
            if (inventoryComponent == null) return;
            
            Debug.Log("🔄 Refreshing UI from inventory container data...");
            
            // Get all potions with their quantities from the inventory container
            var potionsWithCounts = inventoryComponent.GetAllPotionsWithCounts();
            
            // Update our tracking with actual inventory quantities
            foreach (var kvp in potionsWithCounts)
            {
                var potion = kvp.Key;
                var quantity = kvp.Value;
                
                if (completedPotions.ContainsKey(potion))
                {
                    // Update existing entry
                    completedPotions[potion].timesCrafted = quantity;
                }
                else
                {
                    // Add new entry
                    var entryData = new PotionEntryData(potion.ItemName, "");
                    entryData.timesCrafted = quantity;
                    completedPotions.Add(potion, entryData);
                }
            }
            
            // Remove potions that are no longer in inventory
            var potionsToRemove = completedPotions.Keys.Where(p => !potionsWithCounts.ContainsKey(p)).ToList();
            foreach (var potion in potionsToRemove)
            {
                completedPotions.Remove(potion);
            }
            
            // Refresh the UI
            if (isInitialized)
            {
                RefreshPotionsList();
            }
            
            Debug.Log($"✅ Refreshed from inventory: {completedPotions.Count} potion types, {potionsWithCounts.Values.Sum()} total count");
        }
        
        /// <summary>
        /// Add a completed potion to the tracking list
        /// </summary>
        public void OnPotionCrafted(Potion potion)
        {
            if (potion == null)
            {
                Debug.LogError("🧪 OnPotionCrafted: Potion parameter is null!");
                return;
            }
            
            if (Instance != this)
            {
                Debug.LogWarning("🧪 OnPotionCrafted called on non-singleton instance, ignoring");
                return;
            }
            
            Debug.Log($"🧪 Potion crafted: {potion.ItemName}");
            
            // Get current quantity from inventory if available
            int currentQuantity = inventoryComponent?.GetPotionCount(potion) ?? 1;
            
            // Update or add potion data
            if (completedPotions.ContainsKey(potion))
            {
                completedPotions[potion].timesCrafted = currentQuantity;
                Debug.Log($"🧪 Updated existing potion: {potion.ItemName}, now has {currentQuantity} in inventory");
            }
            else
            {
                var entryData = new PotionEntryData(potion.ItemName, ""); // Recipe key would need to be passed separately
                entryData.timesCrafted = currentQuantity;
                completedPotions.Add(potion, entryData);
                Debug.Log($"🧪 Added new potion: {potion.ItemName}");
            }
            
            // Update skill system data if available
            UpdatePotionSkillData(potion);
            
            // Refresh the UI
            if (isInitialized)
            {
                RefreshPotionsList();
            }
        }
        
        /// <summary>
        /// Add a completed potion using recipe key and name
        /// </summary>
        public void OnPotionCrafted(string recipeKey, string potionName)
        {
            if (Instance != this)
            {
                Debug.LogWarning("🧪 OnPotionCrafted called on non-singleton instance, ignoring");
                return;
            }
            
            Debug.Log($"🧪 Potion crafted: {potionName} (Recipe: {recipeKey})");
            
            // Find existing potion or create a runtime one
            var existingPotion = completedPotions.Keys.FirstOrDefault(p => p.ItemName == potionName);
            if (existingPotion != null)
            {
                completedPotions[existingPotion].timesCrafted++;
                completedPotions[existingPotion].recipeKey = recipeKey;
                Debug.Log($"🧪 Updated existing potion: {potionName}, crafted {completedPotions[existingPotion].timesCrafted} times");
            }
            else
            {
                // Create runtime potion for tracking
                var runtimePotion = CreateRuntimePotion(potionName, recipeKey);
                if (runtimePotion != null)
                {
                    var entryData = new PotionEntryData(potionName, recipeKey);
                    completedPotions.Add(runtimePotion, entryData);
                    Debug.Log($"🧪 Added new potion: {potionName}");
                }
            }
            
            // Refresh the UI
            if (isInitialized)
            {
                RefreshPotionsList();
            }
        }
        
        /// <summary>
        /// Update potion skill data from the alchemy skill system
        /// </summary>
        private void UpdatePotionSkillData(Potion potion)
        {
            if (skillSystem == null || !completedPotions.ContainsKey(potion)) return;
            
            var entryData = completedPotions[potion];
            var recipeSkill = skillSystem.GetRecipeSkill(entryData.recipeKey);
            
            if (recipeSkill != null)
            {
                entryData.sRankCount = recipeSkill.sRankCount;
                entryData.isAutoCraftUnlocked = recipeSkill.autoCraftingUnlocked;
                entryData.efficiency = CalculateRecipeEfficiency(entryData.recipeKey);
            }
        }
        
        /// <summary>
        /// Calculate efficiency for a recipe based on S-rank performance
        /// </summary>
        private float CalculateRecipeEfficiency(string recipeKey)
        {
            if (skillSystem == null) return 0f;
            
            var recipeSkill = skillSystem.GetRecipeSkill(recipeKey);
            if (recipeSkill == null) return 0f;
            
            // Simple efficiency calculation based on S-ranks
            float baseEfficiency = 0.6f;
            float sRankBonus = Mathf.Min(recipeSkill.sRankCount * 0.05f, 0.3f);
            
            return baseEfficiency + sRankBonus;
        }
        
        /// <summary>
        /// Create a runtime potion instance for tracking purposes
        /// </summary>
        private Potion CreateRuntimePotion(string potionName, string recipeKey)
        {
            try
            {
                var runtimePotion = ScriptableObject.CreateInstance<Potion>();
                
                // Use reflection to set the item name
                var itemNameField = typeof(Item).GetField("itemName", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                if (itemNameField != null)
                {
                    itemNameField.SetValue(runtimePotion, potionName);
                    Debug.Log($"🧪 Created runtime potion: {potionName}");
                    return runtimePotion;
                }
                else
                {
                    Debug.LogWarning("🧪 Could not set potion name via reflection");
                    return runtimePotion;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"🧪 Failed to create runtime potion: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Refresh the potions list UI
        /// </summary>
        public void RefreshPotionsList()
        {
            if (!isInitialized || potionsContent == null)
            {
                Debug.LogWarning("🧪 RefreshPotionsList: Not initialized or content element is null");
                return;
            }
            
            Debug.Log($"🧪 Refreshing potions list with {completedPotions.Count} potions");
            
            // Clear existing entries
            potionsContent.Clear();
            
            // Update header with count
            if (headerLabel != null)
            {
                headerLabel.text = $"COMPLETED POTIONS ({completedPotions.Count})";
            }
            
            // Show empty state if no potions
            if (completedPotions.Count == 0)
            {
                ShowEmptyState();
                return;
            }
            
            // Create entries for each completed potion
            var sortedPotions = completedPotions.OrderByDescending(p => p.Value.sRankCount)
                                                .ThenByDescending(p => p.Value.timesCrafted)
                                                .ThenBy(p => p.Value.potionName);
            
            float delay = 0f;
            foreach (var potionData in sortedPotions)
            {
                CreatePotionEntry(potionData.Key, potionData.Value, delay);
                delay += entryAnimationDelay;
            }
            
            Debug.Log($"✅ Refreshed potions list with {completedPotions.Count} entries");
        }
        
        /// <summary>
        /// Show empty state when no potions have been completed
        /// </summary>
        private void ShowEmptyState()
        {
            var emptyState = new VisualElement();
            emptyState.AddToClassList("empty-state");
            
            var icon = new Label("🧪");
            icon.AddToClassList("empty-state-icon");
            
            var message = new Label("No potions crafted yet.\nStart brewing to track your progress!");
            
            emptyState.Add(icon);
            emptyState.Add(message);
            potionsContent.Add(emptyState);
        }
        
        /// <summary>
        /// Create a UI entry for a completed potion
        /// </summary>
        private void CreatePotionEntry(Potion potion, PotionEntryData data, float animationDelay)
        {
            var entry = new VisualElement();
            entry.AddToClassList("potion-entry");
            entry.AddToClassList("fade-in");
            
            // Add state-based classes
            if (data.timesCrafted > 0)
                entry.AddToClassList("crafted");
            
            if (data.sRankCount > 0)
                entry.AddToClassList("s-rank");
            
            // Potion name
            var nameLabel = new Label(data.potionName);
            nameLabel.AddToClassList("potion-name");
            entry.Add(nameLabel);
            
            // Statistics text
            string statsText = $"Crafted: {data.timesCrafted}x";
            
            if (data.sRankCount > 0)
            {
                statsText += $" | S-Ranks: {data.sRankCount}";
            }
            
            if (data.efficiency > 0)
            {
                statsText += $" | Efficiency: {data.efficiency:P0}";
            }
            
            var statsLabel = new Label(statsText);
            statsLabel.AddToClassList("potion-stats");
            entry.Add(statsLabel);
            
            // Auto-craft indicator
            if (data.isAutoCraftUnlocked)
            {
                var autoCraftLabel = new Label("AUTO-CRAFT UNLOCKED");
                autoCraftLabel.AddToClassList("auto-craft-indicator");
                entry.Add(autoCraftLabel);
            }
            
            // S-Rank badge
            if (data.sRankCount > 0)
            {
                var badge = new Label($"S{data.sRankCount}");
                badge.AddToClassList("potion-badge");
                entry.Add(badge);
            }
            
            // Add tooltip with detailed information
            entry.tooltip = CreatePotionTooltip(data);
            
            // Animation
            if (animationDelay > 0)
            {
                entry.schedule.Execute(() => {
                    entry.AddToClassList("visible");
                }).StartingIn((long)(animationDelay * 1000));
            }
            else
            {
                entry.AddToClassList("visible");
            }
            
            potionsContent.Add(entry);
        }
        
        /// <summary>
        /// Create a detailed tooltip for a potion entry
        /// </summary>
        private string CreatePotionTooltip(PotionEntryData data)
        {
            var tooltip = $"<b>{data.potionName}</b>\n";
            tooltip += $"Recipe: {data.recipeKey}\n";
            tooltip += $"Times Crafted: {data.timesCrafted}\n";
            
            if (data.sRankCount > 0)
            {
                tooltip += $"S-Rank Achievements: {data.sRankCount}\n";
            }
            
            if (data.efficiency > 0)
            {
                tooltip += $"Grid Efficiency: {data.efficiency:P1}\n";
            }
            
            if (data.isAutoCraftUnlocked)
            {
                tooltip += "Auto-Craft: <color=green>Unlocked</color>";
            }
            else
            {
                tooltip += "Auto-Craft: <color=red>Locked</color>";
            }
            
            return tooltip;
        }
        
        /// <summary>
        /// Set the footer text
        /// </summary>
        private void SetFooterText(string text)
        {
            if (footerLabel != null)
            {
                footerLabel.text = text;
            }
        }
        
        /// <summary>
        /// Force refresh of the UI (for debugging)
        /// </summary>
        [ContextMenu("Force Refresh")]
        public void ForceRefresh()
        {
            if (isInitialized)
            {
                RefreshPotionsList();
            }
            else
            {
                InitializeUIDocument();
            }
        }
        
        /// <summary>
        /// Add a test potion entry (for debugging)
        /// </summary>
        [ContextMenu("Add Test Potion")]
        public void AddTestPotion()
        {
            OnPotionCrafted("test_potion", "Test Healing Potion");
        }
        
        private void OnDestroy()
        {
            Debug.Log("🧪 CompletedPotionsUIDocument: OnDestroy() called");
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}