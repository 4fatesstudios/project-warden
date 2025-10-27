using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace FourFatesStudios.ProjectWarden.UI
{
    /// <summary>
    /// Modern UI Manager for Unity 6 using modular panel system
    /// Manages UI panels as prefabs that can be loaded/unloaded dynamically
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI Configuration")]
        [SerializeField] private UIDocument mainUIDocument;
        [SerializeField] private bool enableDebugLogging = true;
        
        [Header("Panel Prefabs")]
        [SerializeField] private GameObject[] panelPrefabs;
        
        private Dictionary<string, GameObject> activePanels = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> prefabRegistry = new Dictionary<string, GameObject>();
        private Stack<string> panelHistory = new Stack<string>();
        
        public static UIManager Instance { get; private set; }
        
        public event Action<string> OnPanelOpened;
        public event Action<string> OnPanelClosed;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeUIManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeUIManager()
        {
            // Register all panel prefabs
            foreach (var prefab in panelPrefabs)
            {
                if (prefab != null)
                {
                    var panelName = prefab.name.Replace("Panel", "").Replace("Menu", "");
                    prefabRegistry[panelName] = prefab;
                    
                    if (enableDebugLogging)
                        Debug.Log($"📋 Registered UI Panel: {panelName}");
                }
            }
            
            // Ensure main UI document exists
            if (mainUIDocument == null)
                mainUIDocument = GetComponent<UIDocument>();
                
            if (enableDebugLogging)
                Debug.Log($"✅ UIManager initialized with {prefabRegistry.Count} panels");
        }
        
        /// <summary>
        /// Opens a UI panel by name, optionally closing others
        /// </summary>
        public void OpenPanel(string panelName, bool closeOthers = true)
        {
            if (string.IsNullOrEmpty(panelName))
            {
                Debug.LogError("Panel name cannot be null or empty");
                return;
            }
            
            // Close other panels if requested
            if (closeOthers)
            {
                CloseAllPanels();
            }
            
            // Check if panel is already active
            if (activePanels.ContainsKey(panelName))
            {
                if (enableDebugLogging)
                    Debug.Log($"📱 Panel '{panelName}' is already open");
                return;
            }
            
            // Load and activate panel
            if (prefabRegistry.TryGetValue(panelName, out GameObject prefab))
            {
                var panelInstance = Instantiate(prefab, transform);
                activePanels[panelName] = panelInstance;
                panelHistory.Push(panelName);
                
                OnPanelOpened?.Invoke(panelName);
                
                if (enableDebugLogging)
                    Debug.Log($"✅ Opened panel: {panelName}");
            }
            else
            {
                Debug.LogError($"❌ Panel prefab not found: {panelName}");
            }
        }
        
        /// <summary>
        /// Closes a specific panel
        /// </summary>
        public void ClosePanel(string panelName)
        {
            if (activePanels.TryGetValue(panelName, out GameObject panel))
            {
                Destroy(panel);
                activePanels.Remove(panelName);
                
                // Remove from history
                var tempStack = new Stack<string>();
                while (panelHistory.Count > 0)
                {
                    var historyPanel = panelHistory.Pop();
                    if (historyPanel != panelName)
                        tempStack.Push(historyPanel);
                }
                
                panelHistory.Clear();
                while (tempStack.Count > 0)
                    panelHistory.Push(tempStack.Pop());
                
                OnPanelClosed?.Invoke(panelName);
                
                if (enableDebugLogging)
                    Debug.Log($"❌ Closed panel: {panelName}");
            }
        }
        
        /// <summary>
        /// Closes all active panels
        /// </summary>
        public void CloseAllPanels()
        {
            var panelsToClose = new List<string>(activePanels.Keys);
            foreach (var panelName in panelsToClose)
            {
                ClosePanel(panelName);
            }
        }
        
        /// <summary>
        /// Goes back to the previous panel in history
        /// </summary>
        public void GoBack()
        {
            if (panelHistory.Count > 1)
            {
                // Remove current panel
                var currentPanel = panelHistory.Pop();
                ClosePanel(currentPanel);
                
                // Open previous panel
                if (panelHistory.Count > 0)
                {
                    var previousPanel = panelHistory.Peek();
                    OpenPanel(previousPanel, true);
                }
            }
        }
        
        /// <summary>
        /// Checks if a panel is currently active
        /// </summary>
        public bool IsPanelOpen(string panelName)
        {
            return activePanels.ContainsKey(panelName);
        }
        
        /// <summary>
        /// Gets the currently active panel names
        /// </summary>
        public List<string> GetActivePanels()
        {
            return new List<string>(activePanels.Keys);
        }
        
        /// <summary>
        /// Switch between panels with navigation support
        /// </summary>
        public void SwitchToPanel(string panelName)
        {
            OpenPanel(panelName, true);
        }
        
        // Keyboard shortcuts for testing
        private void Update()
        {
            if (Application.isPlaying && enableDebugLogging)
            {
                // ESC to go back
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (activePanels.Count > 0)
                        GoBack();
                }
                
                // Number keys for quick panel switching
                if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToPanel("PotionCrafting");
                if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToPanel("Refinement");  
                if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToPanel("BulkCrafting");
                if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchToPanel("AlchemyBook");
                if (Input.GetKeyDown(KeyCode.I)) SwitchToPanel("Inventory");
            }
        }
        
        // Context menu for testing
        [ContextMenu("Debug Panel Status")]
        private void DebugPanelStatus()
        {
            Debug.Log($"🔍 Active Panels: {string.Join(", ", GetActivePanels())}");
            Debug.Log($"📚 Registered Panels: {string.Join(", ", prefabRegistry.Keys)}");
            Debug.Log($"📖 Panel History: {string.Join(" → ", panelHistory)}");
        }
    }
}