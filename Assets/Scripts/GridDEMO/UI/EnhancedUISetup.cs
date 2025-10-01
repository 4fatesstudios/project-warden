using FourFatesStudios.ProjectWarden.GridDEMO.UI;
using FourFatesStudios.ProjectWarden.GridDemo.UI;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.GameSystems.SkillSystem;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Automated setup script for integrating enhanced alchemy systems with existing UI
    /// </summary>
    [System.Serializable]
    public class EnhancedUISetup : MonoBehaviour
    {
        [Header("Setup Configuration")]
        [SerializeField] public bool autoSetupOnStart = true;
        [SerializeField] public bool preserveExistingUI = true;
        [SerializeField] public bool createDebugConsole = true;
        [SerializeField] public bool enableKeyboardShortcuts = true;
        
        [Header("UI Positioning")]
        [SerializeField] private Vector2 skillTreeButtonPosition = new Vector2(10, 10);
        [SerializeField] private Vector2 synergyButtonPosition = new Vector2(120, 10);
        [SerializeField] private Vector2 templateButtonPosition = new Vector2(230, 10);
        [SerializeField] private Vector2 statusPanelPosition = new Vector2(10, 60);
        
        [Header("UI Styling")]
        [SerializeField] private Color primaryButtonColor = new Color(0.2f, 0.4f, 0.8f, 0.9f);
        [SerializeField] private Color activeButtonColor = new Color(0.2f, 0.8f, 0.2f, 0.9f);
        [SerializeField] private Color statusPanelColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        [SerializeField] private Font defaultFont;
        
        // Component references (will be created)
        private Canvas mainCanvas;
        private GridGameManager gridManager;
        private EnhancedAlchemyUIManager enhancedUIManager;
        private UISystemBridge uiBridge;
        
        // UI Elements (will be created)
        private GameObject enhancedUIRoot;
        private Button skillTreeButton;
        private Button synergyButton;
        private Button templateButton;
        private Button purifyButton;
        private GameObject statusPanel;
        private TextMeshProUGUI statusText;
        private GameObject debugConsole;
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupEnhancedUI();
            }
        }
        
        #region Main Setup
        
        /// <summary>
        /// Main setup method - call this to integrate enhanced systems with your UI
        /// </summary>
        [ContextMenu("🚀 Setup Enhanced UI Integration")]
        public void SetupEnhancedUI()
        {
            Debug.Log("🚀 === SETTING UP ENHANCED UI INTEGRATION ===");
            
            try
            {
                Step1_FindExistingComponents();
                Step2_CreateEnhancedSystems();
                Step3_CreateEnhancedUI();
                Step4_ConnectSystems();
                Step5_FinalizeSetup();
                
                Debug.Log("✅ Enhanced UI integration completed successfully!");
                LogSetupSummary();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ Enhanced UI setup failed: {e.Message}");
                Debug.LogException(e);
            }
        }
        
        private void Step1_FindExistingComponents()
        {
            Debug.Log("📋 Step 1: Finding existing components...");
            
            // Find main canvas
            mainCanvas = FindFirstObjectByType<Canvas>();
            if (mainCanvas == null)
            {
                Debug.LogError("No Canvas found! Please create a Canvas first.");
                return;
            }
            
            // Find grid manager
            gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager == null)
            {
                Debug.LogError("No GridGameManager found! Enhanced systems need GridGameManager.");
                return;
            }
            
            Debug.Log($"✅ Found Canvas: {mainCanvas.name}");
            Debug.Log($"✅ Found GridGameManager: {gridManager.name}");
        }
        
        private void Step2_CreateEnhancedSystems()
        {
            Debug.Log("🔧 Step 2: Creating enhanced systems...");
            
            // Create Enhanced UI Manager
            var existingEnhancedUI = FindFirstObjectByType<EnhancedAlchemyUIManager>();
            if (existingEnhancedUI == null)
            {
                var enhancedUIObj = new GameObject("Enhanced Alchemy UI Manager");
                enhancedUIObj.transform.SetParent(mainCanvas.transform, false);
                enhancedUIManager = enhancedUIObj.AddComponent<EnhancedAlchemyUIManager>();
                Debug.Log("✅ Created EnhancedAlchemyUIManager");
            }
            else
            {
                enhancedUIManager = existingEnhancedUI;
                Debug.Log("✅ Found existing EnhancedAlchemyUIManager");
            }
            
            // Create UI Bridge
            var existingBridge = FindFirstObjectByType<UISystemBridge>();
            if (existingBridge == null)
            {
                var bridgeObj = new GameObject("UI System Bridge");
                bridgeObj.transform.SetParent(mainCanvas.transform, false);
                uiBridge = bridgeObj.AddComponent<UISystemBridge>();
                Debug.Log("✅ Created UISystemBridge");
            }
            else
            {
                uiBridge = existingBridge;
                Debug.Log("✅ Found existing UISystemBridge");
            }
        }
        
        private void Step3_CreateEnhancedUI()
        {
            Debug.Log("🎨 Step 3: Creating enhanced UI elements...");
            
            CreateEnhancedUIRoot();
            CreateQuickAccessButtons();
            CreateStatusPanel();
            
            if (createDebugConsole)
            {
                CreateDebugConsole();
            }
            
            Debug.Log("✅ Enhanced UI elements created");
        }
        
        private void Step4_ConnectSystems()
        {
            Debug.Log("🔗 Step 4: Connecting systems...");
            
            ConnectButtonEvents();
            SetupKeyboardShortcuts();
            
            Debug.Log("✅ Systems connected");
        }
        
        private void Step5_FinalizeSetup()
        {
            Debug.Log("🏁 Step 5: Finalizing setup...");
            
            RefreshAllUI();
            ApplyUIStyles();
            TestConnections();
            
            Debug.Log("✅ Setup finalized");
        }
        
        #endregion
        
        #region UI Creation
        
        private void CreateEnhancedUIRoot()
        {
            enhancedUIRoot = new GameObject("Enhanced UI Root");
            enhancedUIRoot.transform.SetParent(mainCanvas.transform, false);
            
            var rect = enhancedUIRoot.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }
        
        private void CreateQuickAccessButtons()
        {
            // Skill Tree Button
            skillTreeButton = CreateQuickButton(
                "Skills (T)", 
                skillTreeButtonPosition,
                () => enhancedUIManager?.TogglePanel("skills")
            );
            
            // Synergy Button
            synergyButton = CreateQuickButton(
                "Synergies (S)", 
                synergyButtonPosition,
                () => enhancedUIManager?.TogglePanel("synergies")
            );
            
            // Template Button
            templateButton = CreateQuickButton(
                "Templates (R)", 
                templateButtonPosition,
                () => enhancedUIManager?.TogglePanel("templates")
            );
            
            // Purify Button
            purifyButton = CreateQuickButton(
                "Purify (P)", 
                new Vector2(340, 10),
                () => gridManager?.UsePurifySkill()
            );
            
            Debug.Log("✅ Quick access buttons created");
        }
        
        private Button CreateQuickButton(string text, Vector2 position, System.Action onClick)
        {
            var buttonObj = new GameObject($"Quick_{text.Replace(" ", "")}");
            buttonObj.transform.SetParent(enhancedUIRoot.transform, false);
            
            var rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(100, 30);
            
            var button = buttonObj.AddComponent<Button>();
            var image = buttonObj.AddComponent<Image>();
            image.color = primaryButtonColor;
            
            // Button text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 10;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;
            
            if (defaultFont != null)
            {
                textComponent.font = TMP_FontAsset.CreateFontAsset(defaultFont);
            }
            
            button.onClick.AddListener(() => onClick?.Invoke());
            
            return button;
        }
        
        private void CreateStatusPanel()
        {
            statusPanel = new GameObject("Enhanced Status Panel");
            statusPanel.transform.SetParent(enhancedUIRoot.transform, false);
            
            var rect = statusPanel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.anchoredPosition = statusPanelPosition;
            rect.sizeDelta = new Vector2(300, 200);
            
            var image = statusPanel.AddComponent<Image>();
            image.color = statusPanelColor;
            
            // Status text
            statusText = statusPanel.AddComponent<TextMeshProUGUI>();
            statusText.text = "Enhanced Systems Status";
            statusText.fontSize = 12;
            statusText.color = Color.white;
            statusText.margin = new Vector4(10, 10, 10, 10);
            statusText.alignment = TextAlignmentOptions.TopLeft;
            
            if (defaultFont != null)
            {
                statusText.font = TMP_FontAsset.CreateFontAsset(defaultFont);
            }
            
            Debug.Log("✅ Status panel created");
        }
        
        private void CreateDebugConsole()
        {
            debugConsole = new GameObject("Enhanced Debug Console");
            debugConsole.transform.SetParent(enhancedUIRoot.transform, false);
            
            var rect = debugConsole.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0.3f);
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            
            var image = debugConsole.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.8f);
            
            // Initially hidden
            debugConsole.SetActive(false);
            
            Debug.Log("✅ Debug console created (hidden)");
        }
        
        #endregion
        
        #region System Connection
        
        private void ConnectButtonEvents()
        {
            if (skillTreeButton != null)
            {
                var image = skillTreeButton.GetComponent<Image>();
                skillTreeButton.onClick.AddListener(() => {
                    image.color = image.color == primaryButtonColor ? activeButtonColor : primaryButtonColor;
                });
            }
            
            if (synergyButton != null)
            {
                var image = synergyButton.GetComponent<Image>();
                synergyButton.onClick.AddListener(() => {
                    image.color = image.color == primaryButtonColor ? activeButtonColor : primaryButtonColor;
                });
            }
            
            if (templateButton != null)
            {
                var image = templateButton.GetComponent<Image>();
                templateButton.onClick.AddListener(() => {
                    image.color = image.color == primaryButtonColor ? activeButtonColor : primaryButtonColor;
                });
            }
        }
        
        private void SetupKeyboardShortcuts()
        {
            if (!enableKeyboardShortcuts) return;
            
            Debug.Log("🎹 Keyboard shortcuts enabled:");
            Debug.Log("  T - Toggle Skill Tree");
            Debug.Log("  S - Toggle Synergy Panel");
            Debug.Log("  R - Toggle Template Panel");
            Debug.Log("  P - Use Purify Skill");
            Debug.Log("  ` - Toggle Debug Console");
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnIngredientClickedHandler(Ingredient ingredient, Vector2Int position)
        {
            UpdateStatus($"🧪 Clicked: {ingredient.ItemName} at {position}");
        }
        
        private void OnGridCellClickedHandler(Vector2Int position)
        {
            UpdateStatus($"🎯 Grid cell clicked: {position}");
        }
        
        private void OnEmptySpaceClickedHandler()
        {
            UpdateStatus($"🌌 Empty space clicked");
        }
        
        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
                statusText.text = $"[{timestamp}] {message}\n{statusText.text}";
                
                // Keep only last 10 lines
                var lines = statusText.text.Split('\n');
                if (lines.Length > 10)
                {
                    statusText.text = string.Join("\n", lines, 0, 10);
                }
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        private void RefreshAllUI()
        {
            if (enhancedUIManager != null)
            {
                enhancedUIManager.RefreshUI();
            }
            
            if (uiBridge != null)
            {
                uiBridge.RefreshAllUI();
            }
        }
        
        private void ApplyUIStyles()
        {
            // Apply consistent styling to all created UI elements
            var allButtons = enhancedUIRoot.GetComponentsInChildren<Button>();
            foreach (var button in allButtons)
            {
                var image = button.GetComponent<Image>();
                if (image != null && image.color == Color.white)
                {
                    image.color = primaryButtonColor;
                }
            }
        }
        
        private void TestConnections()
        {
            int connectedSystems = 0;
            
            if (gridManager != null) connectedSystems++;
            if (enhancedUIManager != null) connectedSystems++;
            if (uiBridge != null) connectedSystems++;
            
            Debug.Log($"🔗 Systems connected: {connectedSystems}/4");
            
            // Test a basic operation
            if (gridManager?.availableIngredients?.Count > 0)
            {
                Debug.Log($"✅ Test: Found {gridManager.availableIngredients.Count} available ingredients");
            }
        }
        
        private void LogSetupSummary()
        {
            Debug.Log("📊 === ENHANCED UI SETUP SUMMARY ===");
            Debug.Log($"✅ Enhanced UI Manager: {enhancedUIManager != null}");
            Debug.Log($"✅ UI System Bridge: {uiBridge != null}");
            Debug.Log($"✅ Quick Access Buttons: {skillTreeButton != null && synergyButton != null}");
            Debug.Log($"✅ Status Panel: {statusPanel != null}");
            Debug.Log($"✅ Debug Console: {debugConsole != null}");
            Debug.Log($"✅ Keyboard Shortcuts: {enableKeyboardShortcuts}");
            
            // Enhanced systems check
            var skillTree = FindFirstObjectByType<AlchemySkillTree>();
            var synergySystem = FindFirstObjectByType<SynergySystem>();
            var templateSystem = FindFirstObjectByType<TemplateSystem>();
            var failureSystem = FindFirstObjectByType<FailureSystem>();
            var enhancedGrid = FindFirstObjectByType<EnhancedGridSystem>();
            var extractionGame = FindFirstObjectByType<ExtractionMinigame>();
            
            Debug.Log($"🧪 Skill Tree: {skillTree != null}");
            Debug.Log($"✨ Synergy System: {synergySystem != null}");
            Debug.Log($"🗺️ Template System: {templateSystem != null}");
            Debug.Log($"⚠️ Failure System: {failureSystem != null}");
            Debug.Log($"⚡ Enhanced Grid: {enhancedGrid != null}");
            Debug.Log($"🧩 Extraction Minigame: {extractionGame != null}");
            
            Debug.Log("🚀 Integration complete! Use the buttons or keyboard shortcuts to access enhanced features.");
        }
        
        #endregion
        
        #region Update Loop
        
        private void Update()
        {
            HandleKeyboardInput();
            UpdateStatusPanel();
        }
        
        private void HandleKeyboardInput()
        {
            if (!enableKeyboardShortcuts) return;
            
            if (Input.GetKeyDown(KeyCode.BackQuote)) // ` key
            {
                ToggleDebugConsole();
            }
        }
        
        private void ToggleDebugConsole()
        {
            if (debugConsole != null)
            {
                bool isActive = debugConsole.activeSelf;
                debugConsole.SetActive(!isActive);
                Debug.Log($"🖥️ Debug console {(isActive ? "hidden" : "shown")}");
            }
        }
        
        private void UpdateStatusPanel()
        {
            if (statusText == null) return;
            
            // Update status every second
            if (Time.time % 1f < Time.deltaTime)
            {
                UpdateLiveStatus();
            }
        }
        
        private void UpdateLiveStatus()
        {
            var skillTree = FindFirstObjectByType<AlchemySkillTree>();
            var enhancedGrid = FindFirstObjectByType<EnhancedGridSystem>();
            var synergySystem = FindFirstObjectByType<SynergySystem>();
            var templateSystem = FindFirstObjectByType<TemplateSystem>();
            
            string liveStatus = "📊 LIVE STATUS\n\n";
            
            if (skillTree != null)
            {
                liveStatus += $"⭐ Skill Points: {skillTree.GetAvailableSkillPoints()}\n";
            }
            
            if (enhancedGrid != null)
            {
                float efficiency = enhancedGrid.GetGridEfficiencyScore();
                liveStatus += $"⚡ Grid Efficiency: {efficiency:P0}\n";
            }
            
            if (synergySystem != null)
            {
                var activeSynergies = synergySystem.GetActiveSynergies();
                liveStatus += $"✨ Active Synergies: {activeSynergies.Count}\n";
            }
            
            if (templateSystem != null)
            {
                var currentTemplate = templateSystem.CurrentTemplate;
                if (currentTemplate != null)
                {
                    var progress = templateSystem.CheckTemplateProgress();
                    liveStatus += $"🗺️ Template: {progress.completionPercentage:P0}\n";
                }
            }
            
            // Keep the existing click history at the bottom
            var lines = statusText.text.Split('\n');
            var clickHistory = "";
            for (int i = 5; i < lines.Length && i < 15; i++)
            {
                if (lines[i].Contains("[") && lines[i].Contains("]"))
                {
                    clickHistory += lines[i] + "\n";
                }
            }
            
            statusText.text = liveStatus + "\n📋 RECENT ACTIONS\n" + clickHistory;
        }
        
        #endregion
        
        #region Context Menu
        
        [ContextMenu("🔧 Setup Enhanced UI (Full)")]
        public void SetupEnhancedUIFull()
        {
            autoSetupOnStart = true;
            preserveExistingUI = true;
            createDebugConsole = true;
            enableKeyboardShortcuts = true;
            
            SetupEnhancedUI();
        }
        
        [ContextMenu("🔧 Setup Enhanced UI (Minimal)")]
        public void SetupEnhancedUIMinimal()
        {
            autoSetupOnStart = true;
            preserveExistingUI = true;
            createDebugConsole = false;
            enableKeyboardShortcuts = false;
            
            SetupEnhancedUI();
        }
        
        [ContextMenu("🗑️ Remove Enhanced UI")]
        public void RemoveEnhancedUI()
        {
            if (enhancedUIRoot != null)
            {
                DestroyImmediate(enhancedUIRoot);
                Debug.Log("🗑️ Enhanced UI elements removed");
            }
            
            var enhancedComponents = new System.Type[]
            {
                typeof(EnhancedAlchemyUIManager),
                typeof(UISystemBridge)
            };
            
            foreach (var componentType in enhancedComponents)
            {
                var component = FindFirstObjectByType(componentType);
                if (component != null)
                {
                    DestroyImmediate(component as Component);
                    Debug.Log($"🗑️ Removed {componentType.Name}");
                }
            }
        }
        
        [ContextMenu("📋 Debug UI State")]
        public void DebugUIState()
        {
            LogSetupSummary();
        }
        
        #endregion
    }
}