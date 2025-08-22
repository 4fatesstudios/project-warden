using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using FourFatesStudios.ProjectWarden.GameSystems.AlchemyMenu;
using FourFatesStudios.ProjectWarden.GameSystems;

namespace FourFatesStudios.ProjectWarden.UI
{
    /// <summary>
    /// Specialized UI Manager for the Crafting System
    /// Manages all crafting-related UI panels in a modular way
    /// </summary>
    public class CraftingUIManager : MonoBehaviour
    {
        [Header("Core UI Settings")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool hideInactivePanels = true;
        
        [Header("UI Panel GameObjects")]
        [SerializeField] private GameObject craftingMenuSystem;
        [SerializeField] private GameObject potionCraftingUI;
        [SerializeField] private GameObject gridMinigameUI;
        [SerializeField] private GameObject bulkCraftingUI;
        [SerializeField] private GameObject refinementUI;
        [SerializeField] private GameObject roastingMinigameUI;
        [SerializeField] private GameObject distillingMinigameUI;
        [SerializeField] private GameObject grindingMinigameUI;
        
        [Header("Core Systems")]
        [SerializeField] private ItemSlotContainerHolder inventoryHolder;
        
        private Dictionary<string, GameObject> uiPanels = new Dictionary<string, GameObject>();
        private Stack<string> panelHistory = new Stack<string>();
        private string currentActivePanel = "";
        
        public static CraftingUIManager Instance { get; private set; }
        
        // Events for UI state changes
        public event Action<string> OnPanelOpened;
        public event Action<string> OnPanelClosed;
        public event Action<string, string> OnPanelSwitched; // from, to
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                InitializeCraftingUI();
            }
            else
            {
                Debug.LogWarning("Multiple CraftingUIManager instances detected. Destroying duplicate.");
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // Auto-discover UI panels if not assigned
            AutoDiscoverUIPanels();
            
            // Set up initial state
            if (hideInactivePanels)
            {
                HideAllPanels();
                ShowPanel("CraftingMenu"); // Start with main menu
            }
            
            // Connect inventory to crafting controllers
            ConnectInventoryToCraftingControllers();
        }
        
        private void InitializeCraftingUI()
        {
            if (enableDebugLogging)
                Debug.Log("🧪 Initializing Crafting UI Manager...");
        }
        
        private void AutoDiscoverUIPanels()
        {
            // Register UI panels
            RegisterPanel("CraftingMenu", craftingMenuSystem);
            RegisterPanel("PotionCrafting", potionCraftingUI);
            RegisterPanel("GridMinigame", gridMinigameUI);
            RegisterPanel("BulkCrafting", bulkCraftingUI);
            RegisterPanel("Refinement", refinementUI);
            RegisterPanel("RoastingMinigame", roastingMinigameUI);
            RegisterPanel("DistillingMinigame", distillingMinigameUI);
            RegisterPanel("GrindingMinigame", grindingMinigameUI);
            
            // Auto-find panels if not assigned
            if (craftingMenuSystem == null)
                craftingMenuSystem = GameObject.Find("CraftingMenuSystem");
            if (potionCraftingUI == null)
                potionCraftingUI = GameObject.Find("PotionCraftingUI");
            if (gridMinigameUI == null)
                gridMinigameUI = GameObject.Find("GridMinigameUI");
            if (bulkCraftingUI == null)
                bulkCraftingUI = GameObject.Find("BulkCraftingUI");
            if (refinementUI == null)
                refinementUI = GameObject.Find("RefinementUI");
            if (roastingMinigameUI == null)
                roastingMinigameUI = GameObject.Find("RoastingMinigameUI");
            if (distillingMinigameUI == null)
                distillingMinigameUI = GameObject.Find("DistillingMinigameUI");
            if (grindingMinigameUI == null)
                grindingMinigameUI = GameObject.Find("GrindingMinigameUI");
            
            // Update registry with found objects
            RegisterPanel("CraftingMenu", craftingMenuSystem);
            RegisterPanel("PotionCrafting", potionCraftingUI);
            RegisterPanel("GridMinigame", gridMinigameUI);
            RegisterPanel("BulkCrafting", bulkCraftingUI);
            RegisterPanel("Refinement", refinementUI);
            RegisterPanel("RoastingMinigame", roastingMinigameUI);
            RegisterPanel("DistillingMinigame", distillingMinigameUI);
            RegisterPanel("GrindingMinigame", grindingMinigameUI);
            
            if (enableDebugLogging)
                Debug.Log($"📋 Registered {uiPanels.Count} UI panels");
        }
        
        private void RegisterPanel(string name, GameObject panelObject)
        {
            if (panelObject != null)
            {
                uiPanels[name] = panelObject;
                if (enableDebugLogging)
                    Debug.Log($"✅ Registered panel: {name} → {panelObject.name}");
            }
        }
        
        private void ConnectInventoryToCraftingControllers()
        {
            // Auto-find inventory if not assigned
            if (inventoryHolder == null)
                inventoryHolder = FindFirstObjectByType<ItemSlotContainerHolder>();
            
            if (inventoryHolder == null)
            {
                Debug.LogWarning("⚠️ No ItemSlotContainerHolder found. Creating one...");
                CreateInventorySystem();
            }
            
            // Connect to potion crafting controller
            var potionController = FindFirstObjectByType<PotionCraftingController>();
            if (potionController != null)
            {
                ConnectInventoryToController(potionController, "ingredientInventoryHolder");
            }
            
            // Connect to bulk crafting controller
            var bulkController = FindFirstObjectByType<BulkCraftingController>();
            if (bulkController != null)
            {
                ConnectInventoryToController(bulkController, "ingredientInventoryHolder");
            }
            
            if (enableDebugLogging)
                Debug.Log("🔗 Connected inventory to crafting controllers");
        }
        
        private void ConnectInventoryToController(MonoBehaviour controller, string fieldName)
        {
            var field = controller.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(controller, inventoryHolder);
                if (enableDebugLogging)
                    Debug.Log($"🔗 Connected inventory to {controller.GetType().Name}");
            }
        }
        
        private void CreateInventorySystem()
        {
            var inventoryGO = new GameObject("Inventory");
            inventoryGO.transform.SetParent(transform);
            inventoryHolder = inventoryGO.AddComponent<ItemSlotContainerHolder>();
            
            if (enableDebugLogging)
                Debug.Log("🎒 Created inventory system");
        }
        
        /// <summary>
        /// Shows a specific UI panel and optionally hides others
        /// </summary>
        public void ShowPanel(string panelName, bool hideOthers = true)
        {
            if (!uiPanels.ContainsKey(panelName))
            {
                Debug.LogError($"❌ Panel '{panelName}' not found!");
                return;
            }
            
            var previousPanel = currentActivePanel;
            
            if (hideOthers && hideInactivePanels)
            {
                HideAllPanels();
            }
            
            var panel = uiPanels[panelName];
            if (panel != null)
            {
                panel.SetActive(true);
                currentActivePanel = panelName;
                
                // Add to history if it's a new panel
                if (previousPanel != panelName)
                {
                    if (!string.IsNullOrEmpty(previousPanel))
                        panelHistory.Push(previousPanel);
                    
                    OnPanelSwitched?.Invoke(previousPanel, panelName);
                }
                
                OnPanelOpened?.Invoke(panelName);
                
                if (enableDebugLogging)
                    Debug.Log($"👁️ Showing panel: {panelName}");
            }
        }
        
        /// <summary>
        /// Hides a specific panel
        /// </summary>
        public void HidePanel(string panelName)
        {
            if (uiPanels.TryGetValue(panelName, out GameObject panel))
            {
                panel.SetActive(false);
                
                if (currentActivePanel == panelName)
                    currentActivePanel = "";
                
                OnPanelClosed?.Invoke(panelName);
                
                if (enableDebugLogging)
                    Debug.Log($"🙈 Hiding panel: {panelName}");
            }
        }
        
        /// <summary>
        /// Hides all UI panels
        /// </summary>
        public void HideAllPanels()
        {
            foreach (var kvp in uiPanels)
            {
                if (kvp.Value != null)
                    kvp.Value.SetActive(false);
            }
            currentActivePanel = "";
            
            if (enableDebugLogging)
                Debug.Log("🙈 Hidden all panels");
        }
        
        /// <summary>
        /// Goes back to the previous panel
        /// </summary>
        public void GoBack()
        {
            if (panelHistory.Count > 0)
            {
                var previousPanel = panelHistory.Pop();
                ShowPanel(previousPanel);
                
                if (enableDebugLogging)
                    Debug.Log($"⬅️ Going back to: {previousPanel}");
            }
            else
            {
                ShowPanel("CraftingMenu"); // Default back to main menu
            }
        }
        
        /// <summary>
        /// Quick navigation methods for common panels
        /// </summary>
        public void ShowMainMenu() => ShowPanel("CraftingMenu");
        public void ShowPotionCrafting() => ShowPanel("PotionCrafting");
        public void ShowBulkCrafting() => ShowPanel("BulkCrafting");
        public void ShowRefinement() => ShowPanel("Refinement");
        public void ShowGridMinigame() => ShowPanel("GridMinigame");
        public void ShowRoastingMinigame() => ShowPanel("RoastingMinigame");
        public void ShowDistillingMinigame() => ShowPanel("DistillingMinigame");
        public void ShowGrindingMinigame() => ShowPanel("GrindingMinigame");
        
        /// <summary>
        /// Check if a panel is currently active
        /// </summary>
        public bool IsPanelActive(string panelName)
        {
            return currentActivePanel == panelName;
        }
        
        /// <summary>
        /// Get the currently active panel name
        /// </summary>
        public string GetActivePanel()
        {
            return currentActivePanel;
        }
        
        /// <summary>
        /// Get all registered panel names
        /// </summary>
        public List<string> GetAllPanelNames()
        {
            return new List<string>(uiPanels.Keys);
        }
        
        // Keyboard shortcuts for testing
        private void Update()
        {
            if (!enableDebugLogging) return;
            
            // ESC to go back
            if (Input.GetKeyDown(KeyCode.Escape))
                GoBack();
            
            // Number keys for quick panel switching
            if (Input.GetKeyDown(KeyCode.Alpha1)) ShowMainMenu();
            if (Input.GetKeyDown(KeyCode.Alpha2)) ShowPotionCrafting();
            if (Input.GetKeyDown(KeyCode.Alpha3)) ShowBulkCrafting();
            if (Input.GetKeyDown(KeyCode.Alpha4)) ShowRefinement();
            
            // F keys for minigames
            if (Input.GetKeyDown(KeyCode.F1)) ShowGridMinigame();
            if (Input.GetKeyDown(KeyCode.F2)) ShowRoastingMinigame();
            if (Input.GetKeyDown(KeyCode.F3)) ShowDistillingMinigame();
            if (Input.GetKeyDown(KeyCode.F4)) ShowGrindingMinigame();
        }
        
        // Context menu for debugging
        [ContextMenu("Debug Panel Status")]
        private void DebugPanelStatus()
        {
            Debug.Log($"🔍 Current Active Panel: {currentActivePanel}");
            Debug.Log($"📚 Registered Panels: {string.Join(", ", uiPanels.Keys)}");
            Debug.Log($"📖 Panel History: {string.Join(" → ", panelHistory)}");
            
            foreach (var kvp in uiPanels)
            {
                var status = kvp.Value != null ? (kvp.Value.activeInHierarchy ? "✅ Active" : "❌ Inactive") : "🚫 Null";
                Debug.Log($"  • {kvp.Key}: {status}");
            }
        }
        
        [ContextMenu("Show All Panels")]
        private void ShowAllPanels()
        {
            foreach (var kvp in uiPanels)
            {
                if (kvp.Value != null)
                    kvp.Value.SetActive(true);
            }
            Debug.Log("👁️ Showing all panels");
        }
        
        [ContextMenu("Reset to Main Menu")]
        private void ResetToMainMenu()
        {
            panelHistory.Clear();
            ShowMainMenu();
            Debug.Log("🏠 Reset to main menu");
        }
    }
}