using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu;
using FourFatesStudios.ProjectWarden.GameSystems.SkillSystem;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using System.Linq;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Bridges the existing GridDemoUIManager with the new enhanced alchemy systems
    /// </summary>
    public class UISystemBridge : MonoBehaviour
    {
        [Header("UI Integration")]
        [SerializeField] private GridDemoUIManager existingUIManager;
        [SerializeField] private EnhancedAlchemyUIManager enhancedUIManager;
        
        [Header("Enhanced UI Buttons")]
        [SerializeField] private Button enhancedPlacementButton;
        [SerializeField] private Button showSkillTreeButton;
        [SerializeField] private Button showSynergyButton;
        [SerializeField] private Button showTemplateButton;
        [SerializeField] private Button usePurifyButton;
        
        [Header("Status Integration")]
        [SerializeField] private TextMeshProUGUI enhancedStatusText;
        [SerializeField] private bool showEnhancedTooltips = true;
        
        // System references
        private GridGameManager gridManager;
        private AlchemySkillTree skillTree;
        private SynergySystem synergySystem;
        private TemplateSystem templateSystem;
        private FailureSystem failureSystem;
        
        // State tracking
        private bool useEnhancedPlacement = true;
        private Ingredient lastSelectedIngredient;
        
        private void Awake()
        {
            FindComponents();
            SetupSystemReferences();
        }
        
        private void Start()
        {
            IntegrateWithExistingUI();
            CreateEnhancedUIElements();
            SetupEventHandlers();
        }
        
        private void Update()
        {
            UpdateEnhancedStatus();
        }
        
        #region Setup
        
        private void FindComponents()
        {
            if (existingUIManager == null)
            {
                existingUIManager = FindFirstObjectByType<GridDemoUIManager>();
            }
            
            if (enhancedUIManager == null)
            {
                enhancedUIManager = FindFirstObjectByType<EnhancedAlchemyUIManager>();
                if (enhancedUIManager == null)
                {
                    // Create enhanced UI manager if it doesn't exist
                    var enhancedUIObj = new GameObject("Enhanced Alchemy UI Manager");
                    enhancedUIObj.transform.SetParent(transform, false);
                    enhancedUIManager = enhancedUIObj.AddComponent<EnhancedAlchemyUIManager>();
                }
            }
        }
        
        private void SetupSystemReferences()
        {
            gridManager = FindFirstObjectByType<GridGameManager>();
            skillTree = FindFirstObjectByType<AlchemySkillTree>();
            synergySystem = FindFirstObjectByType<SynergySystem>();
            templateSystem = FindFirstObjectByType<TemplateSystem>();
            failureSystem = FindFirstObjectByType<FailureSystem>();
            
            Debug.Log($"🔗 UI Bridge connected to systems: GridManager={gridManager != null}, SkillTree={skillTree != null}");
        }
        
        private void IntegrateWithExistingUI()
        {
            if (existingUIManager == null) return;
            
            // Hook into existing UI events if possible
            var leftPanel = existingUIManager.GetComponentInChildren<CompactUIDesigner>();
            if (leftPanel != null)
            {
                leftPanel.OnIngredientSelected += OnIngredientSelectedEnhanced;
                Debug.Log("✅ Hooked into existing ingredient selection");
            }
        }
        
        private void CreateEnhancedUIElements()
        {
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;
            
            CreateEnhancedControlPanel(canvas);
            CreateEnhancedStatusDisplay(canvas);
        }
        
        private void CreateEnhancedControlPanel(Canvas canvas)
        {
            // Create enhanced control panel
            var controlPanel = new GameObject("Enhanced Control Panel");
            controlPanel.transform.SetParent(canvas.transform, false);
            
            var panelRect = controlPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.7f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0.3f);
            panelRect.sizeDelta = Vector2.zero;
            
            var panelBg = controlPanel.AddComponent<Image>();
            panelBg.color = new Color(0.1f, 0.1f, 0.2f, 0.8f);
            
            // Create buttons
            CreateEnhancedButton(controlPanel, "Enhanced Place", new Vector2(0.1f, 0.8f), OnToggleEnhancedPlacement, out enhancedPlacementButton);
            CreateEnhancedButton(controlPanel, "Skills", new Vector2(0.6f, 0.8f), OnShowSkillTree, out showSkillTreeButton);
            CreateEnhancedButton(controlPanel, "Synergies", new Vector2(0.1f, 0.6f), OnShowSynergies, out showSynergyButton);
            CreateEnhancedButton(controlPanel, "Templates", new Vector2(0.6f, 0.6f), OnShowTemplates, out showTemplateButton);
            CreateEnhancedButton(controlPanel, "Purify", new Vector2(0.35f, 0.4f), OnUsePurify, out usePurifyButton);
            
            // Update enhanced placement button color
            UpdateEnhancedPlacementButtonColor();
        }
        
        private void CreateEnhancedButton(GameObject parent, string text, Vector2 anchorPosition, System.Action onClick, out Button button)
        {
            var buttonObj = new GameObject($"Btn_{text}");
            buttonObj.transform.SetParent(parent.transform, false);
            
            var rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorPosition;
            rect.anchorMax = anchorPosition + new Vector2(0.35f, 0.15f);
            rect.sizeDelta = Vector2.zero;
            
            button = buttonObj.AddComponent<Button>();
            var bg = buttonObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.4f, 0.8f, 0.9f);
            
            // Add text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 12;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;
            
            button.onClick.AddListener(() => onClick?.Invoke());
        }
        
        private void CreateEnhancedStatusDisplay(Canvas canvas)
        {
            var statusObj = new GameObject("Enhanced Status Display");
            statusObj.transform.SetParent(canvas.transform, false);
            
            var statusRect = statusObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.7f, 0.3f);
            statusRect.anchorMax = new Vector2(1f, 0.7f);
            statusRect.sizeDelta = Vector2.zero;
            
            var statusBg = statusObj.AddComponent<Image>();
            statusBg.color = new Color(0.1f, 0.2f, 0.1f, 0.8f);
            
            // Status text
            enhancedStatusText = statusObj.AddComponent<TextMeshProUGUI>();
            enhancedStatusText.text = "Enhanced Systems Status";
            enhancedStatusText.fontSize = 11;
            enhancedStatusText.color = Color.white;
            enhancedStatusText.margin = new Vector4(10, 10, 10, 10);
            enhancedStatusText.alignment = TextAlignmentOptions.TopLeft;
        }
        
        #endregion
        
        #region Event Handlers
        
        private void SetupEventHandlers()
        {
            // Listen to grid manager events if available
            if (gridManager != null)
            {
                // Hook into ingredient placement if the GridGameManager has such events
                Debug.Log("✅ Event handlers setup complete");
            }
        }
        
        private void OnIngredientSelectedEnhanced(Ingredient ingredient)
        {
            lastSelectedIngredient = ingredient;
            
            if (showEnhancedTooltips && ingredient != null)
            {
                ShowEnhancedIngredientTooltip(ingredient);
            }
            
            Debug.Log($"🧪 Enhanced UI: Selected {ingredient.ItemName}");
        }
        
        private void ShowEnhancedIngredientTooltip(Ingredient ingredient)
        {
            string tooltipText = $"🧪 {ingredient.ItemName}\n";
            
            // Add synergy information
            if (synergySystem != null)
            {
                // Get synergies that could potentially involve this ingredient
                var allSynergies = synergySystem.GetActiveSynergies();
                var potentialSynergies = allSynergies.Where(s => 
                    s.synergyData.requiredAspects.Contains(ingredient.IngredientAspect) ||
                    s.synergyData.requiredArchetypes.Contains(ingredient.IngredientArchetype)
                ).ToList();
                
                if (potentialSynergies.Count > 0)
                {
                    tooltipText += "\n✨ Active Synergies:\n";
                    foreach (var synergy in potentialSynergies)
                    {
                        tooltipText += $"  • {synergy.synergyData.synergyName}\n";
                    }
                }
            }
            
            // Add skill-based information
            if (skillTree != null)
            {
                if (skillTree.IsSkillUnlocked("Ingredient Conservation"))
                {
                    float refundChance = skillTree.GetSkillValue("Ingredient Conservation");
                    tooltipText += $"\n💎 Refund chance: {refundChance:P0}";
                }
                
                if (ingredient.UnlocksAdditionalSpace && skillTree.IsSkillUnlocked("Grid Expansion"))
                {
                    tooltipText += $"\n🔄 Can expand grid (+{ingredient.AdditionalSpaceCount})";
                }
            }
            
            // Show in existing tooltip system if available
            if (existingUIManager != null)
            {
                existingUIManager.ShowIngredientTooltip(ingredient, Input.mousePosition);
            }
            
            Debug.Log(tooltipText);
        }
        
        private void OnToggleEnhancedPlacement()
        {
            useEnhancedPlacement = !useEnhancedPlacement;
            UpdateEnhancedPlacementButtonColor();
            
            string mode = useEnhancedPlacement ? "Enhanced" : "Standard";
            Debug.Log($"🔄 Switched to {mode} placement mode");
        }
        
        private void UpdateEnhancedPlacementButtonColor()
        {
            if (enhancedPlacementButton != null)
            {
                var image = enhancedPlacementButton.GetComponent<Image>();
                if (image != null)
                {
                    image.color = useEnhancedPlacement ? 
                        new Color(0.2f, 0.8f, 0.2f, 0.9f) : // Green when enhanced
                        new Color(0.6f, 0.6f, 0.6f, 0.9f);  // Gray when standard
                }
            }
        }
        
        private void OnShowSkillTree()
        {
            if (enhancedUIManager != null)
            {
                enhancedUIManager.TogglePanel("skills");
            }
        }
        
        private void OnShowSynergies()
        {
            if (enhancedUIManager != null)
            {
                enhancedUIManager.TogglePanel("synergies");
            }
        }
        
        private void OnShowTemplates()
        {
            if (enhancedUIManager != null)
            {
                enhancedUIManager.TogglePanel("templates");
            }
        }
        
        private void OnUsePurify()
        {
            if (gridManager != null)
            {
                gridManager.UsePurifySkill();
            }
        }
        
        #endregion
        
        #region Status Updates
        
        private void UpdateEnhancedStatus()
        {
            if (enhancedStatusText == null) return;
            
            string statusText = "📊 ENHANCED STATUS\n\n";
            
            // Skill points
            if (skillTree != null)
            {
                statusText += $"⭐ Skill Points: {skillTree.GetAvailableSkillPoints()}\n";
                var unlockedSkills = skillTree.GetUnlockedSkills();
                statusText += $"🔓 Skills Unlocked: {unlockedSkills.Count}\n";
            }
            
            // Grid efficiency
            var enhancedGrid = FindFirstObjectByType<EnhancedGridSystem>();
            if (enhancedGrid != null)
            {
                float efficiency = enhancedGrid.GetGridEfficiencyScore();
                statusText += $"⚡ Grid Efficiency: {efficiency:P0}\n";
            }
            
            // Active synergies
            if (synergySystem != null)
            {
                var activeSynergies = synergySystem.GetActiveSynergies();
                statusText += $"✨ Active Synergies: {activeSynergies.Count}\n";
            }
            
            // Current template
            if (templateSystem != null)
            {
                var currentTemplate = templateSystem.CurrentTemplate;
                if (currentTemplate != null)
                {
                    var progress = templateSystem.CheckTemplateProgress();
                    statusText += $"🗺️ Template: {currentTemplate.templateName}\n";
                    statusText += $"📈 Progress: {progress.completionPercentage:P0}\n";
                }
                else
                {
                    statusText += "🗺️ Template: Freeform\n";
                }
            }
            
            // Placement mode
            statusText += $"\n🧪 Placement: {(useEnhancedPlacement ? "Enhanced" : "Standard")}\n";
            
            // Last selected ingredient
            if (lastSelectedIngredient != null)
            {
                statusText += $"🎯 Selected: {lastSelectedIngredient.ItemName}\n";
            }
            
            enhancedStatusText.text = statusText;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Handle ingredient placement with enhanced systems
        /// </summary>
        public bool HandleIngredientPlacement(Ingredient ingredient, Vector2Int position)
        {
            if (!useEnhancedPlacement || gridManager == null)
            {
                // Use standard placement
                return gridManager.TryPlaceIngredient(ingredient, position);
            }
            
            // Use enhanced placement
            return gridManager.TryPlaceIngredientEnhanced(ingredient, position);
        }
        
        /// <summary>
        /// Check if enhanced placement is enabled
        /// </summary>
        public bool IsEnhancedPlacementEnabled()
        {
            return useEnhancedPlacement;
        }
        
        /// <summary>
        /// Show failure warning using enhanced UI
        /// </summary>
        public bool ShowFailureWarning(FailureAnalysis analysis)
        {
            if (enhancedUIManager != null)
            {
                return enhancedUIManager.ShowFailureWarning(analysis);
            }
            
            // Fallback to simple dialog
            return UnityEditor.EditorUtility.DisplayDialog(
                "Recipe Conflicts Detected",
                $"This recipe has a {analysis.totalFailureChance:P0} chance of failure due to conflicts.\n\nProceed anyway?",
                "Proceed",
                "Cancel"
            );
        }
        
        /// <summary>
        /// Refresh all UI elements
        /// </summary>
        public void RefreshAllUI()
        {
            if (existingUIManager != null)
            {
                existingUIManager.RefreshUI();
            }
            
            if (enhancedUIManager != null)
            {
                enhancedUIManager.RefreshUI();
            }
            
            UpdateEnhancedStatus();
        }
        
        #endregion
        
        #region Context Menu Testing
        
        [ContextMenu("🔗 Test UI Bridge")]
        public void TestUIBridge()
        {
            Debug.Log("🔗 === TESTING UI BRIDGE ===");
            
            Debug.Log($"Existing UI Manager: {existingUIManager != null}");
            Debug.Log($"Enhanced UI Manager: {enhancedUIManager != null}");
            Debug.Log($"Enhanced Placement: {useEnhancedPlacement}");
            
            if (gridManager?.availableIngredients?.Count > 0)
            {
                var testIngredient = gridManager.availableIngredients[0];
                OnIngredientSelectedEnhanced(testIngredient);
                
                Debug.Log($"✅ Test ingredient selection: {testIngredient.ItemName}");
            }
            
            RefreshAllUI();
            Debug.Log("✅ UI Bridge test completed");
        }
        
        [ContextMenu("🎨 Create Enhanced UI Elements")]
        public void CreateEnhancedUIElementsManual()
        {
            CreateEnhancedUIElements();
            SetupEventHandlers();
            RefreshAllUI();
            Debug.Log("✅ Enhanced UI elements created manually");
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // Clean up event subscriptions
            var leftPanel = existingUIManager?.GetComponentInChildren<CompactUIDesigner>();
            if (leftPanel != null)
            {
                leftPanel.OnIngredientSelected -= OnIngredientSelectedEnhanced;
            }
        }
    }
}