using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu;
using FourFatesStudios.ProjectWarden.GameSystems.SkillSystem;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using AlchemySkillTree = FourFatesStudios.ProjectWarden.GameSystems.SkillSystem.AlchemySkillTree;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Enhanced UI Manager that integrates all the new alchemy systems with the existing UI
    /// </summary>
    public class EnhancedAlchemyUIManager : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject skillTreePanel;
        [SerializeField] private GameObject synergyPanel;
        [SerializeField] private GameObject templatePanel;
        [SerializeField] private GameObject statusPanel;
        [SerializeField] private GameObject failureWarningPanel;
        [SerializeField] private GameObject extractionMinigamePanel;
        
        [Header("Status Display")]
        [SerializeField] private TextMeshProUGUI skillPointsText;
        [SerializeField] private TextMeshProUGUI gridEfficiencyText;
        [SerializeField] private TextMeshProUGUI templateProgressText;
        [SerializeField] private TextMeshProUGUI synergyStatusText;
        
        [Header("Skill Tree UI")]
        [SerializeField] private Transform skillTreeContent;
        [SerializeField] private Button skillTreeToggleButton;
        [SerializeField] private GameObject skillNodePrefab;
        
        [Header("Template UI")]
        [SerializeField] private Dropdown templateSelector;
        [SerializeField] private Button applyTemplateButton;
        [SerializeField] private Button clearTemplateButton;
        [SerializeField] private TextMeshProUGUI templateDescriptionText;
        
        [Header("Synergy UI")]
        [SerializeField] private Transform synergyListContent;
        [SerializeField] private GameObject synergyItemPrefab;
        [SerializeField] private Button synergyToggleButton;
        
        [Header("Extraction Minigame UI")]
        [SerializeField] private Transform puzzleGrid;
        [SerializeField] private Button startExtractionButton;
        [SerializeField] private TextMeshProUGUI extractionInstructionsText;
        [SerializeField] private TextMeshProUGUI moveCountText;
        [SerializeField] private TextMeshProUGUI extractionTimerText;
        
        [Header("Auto-Setup")]
        [SerializeField] private bool autoCreateUI = true;
        [SerializeField] private bool enableKeyboardShortcuts = true;
        
        // System references
        private GridGameManager gridManager;
        private EnhancedGridSystem enhancedGridSystem;
        private SynergySystem synergySystem;
        private FailureSystem failureSystem;
        private AlchemySkillTree skillTree;
        private TemplateSystem templateSystem;
        private ExtractionMinigame extractionMinigame;
        
        // UI State
        private bool skillTreeVisible = false;
        private bool synergyPanelVisible = false;
        private bool templatePanelVisible = false;
        private bool extractionActive = false;
        
        // GridDemoSkillNode has been replaced with AlchemySkill
        private List<AlchemySkill> lastSkillNodes;
        // IngredientSynergy has been replaced with SynergyInstance
        private List<SynergyInstance> lastActiveSynergies;
        
        private void Awake()
        {
            FindSystemReferences();
            
            if (autoCreateUI)
            {
                CreateUIElements();
            }
        }
        
        private void Start()
        {
            SetupUIConnections();
            SetupKeyboardShortcuts();
            RefreshAllUI();
        }
        
        private void Update()
        {
            if (enableKeyboardShortcuts)
            {
                HandleKeyboardInput();
            }
            
            UpdateStatusDisplay();
        }
        
        #region System Setup
        
        private void FindSystemReferences()
        {
            gridManager = FindFirstObjectByType<GridGameManager>();
            enhancedGridSystem = FindFirstObjectByType<EnhancedGridSystem>();
            synergySystem = FindFirstObjectByType<SynergySystem>();
            failureSystem = FindFirstObjectByType<FailureSystem>();
            skillTree = FindFirstObjectByType<AlchemySkillTree>();
            templateSystem = FindFirstObjectByType<TemplateSystem>();
            extractionMinigame = FindFirstObjectByType<ExtractionMinigame>();
            
            Debug.Log($"🔗 Enhanced UI connected to {CountConnectedSystems()}/7 systems");
        }
        
        private int CountConnectedSystems()
        {
            int count = 0;
            if (gridManager != null) count++;
            if (enhancedGridSystem != null) count++;
            if (synergySystem != null) count++;
            if (failureSystem != null) count++;
            if (skillTree != null) count++;
            if (templateSystem != null) count++;
            if (extractionMinigame != null) count++;
            return count;
        }
        
        private void CreateUIElements()
        {
            if (FindFirstObjectByType<Canvas>() == null)
            {
                Debug.LogWarning("No Canvas found - UI elements will not be visible");
                return;
            }
            
            CreateStatusPanel();
            CreateSkillTreePanel();
            CreateTemplatePanel();
            CreateSynergyPanel();
            CreateExtractionMinigamePanel();
            CreateFailureWarningPanel();
            
            Debug.Log("✅ Enhanced Alchemy UI elements created");
        }
        
        #endregion
        
        #region UI Creation
        
        private void CreateStatusPanel()
        {
            if (statusPanel != null) return;
            
            var canvas = FindFirstObjectByType<Canvas>();
            statusPanel = new GameObject("Enhanced Status Panel");
            statusPanel.transform.SetParent(canvas.transform, false);
            
            var rect = statusPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0.3f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0, 120);
            
            var bg = statusPanel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);
            
            // Create status texts
            CreateStatusText("Skill Points: 0", out skillPointsText);
            CreateStatusText("Grid Efficiency: 0%", out gridEfficiencyText);
            CreateStatusText("Template: None", out templateProgressText);
            CreateStatusText("Synergies: None", out synergyStatusText);
        }
        
        private void CreateStatusText(string initialText, out TextMeshProUGUI textComponent)
        {
            var textObj = new GameObject("Status Text");
            textObj.transform.SetParent(statusPanel.transform, false);
            
            var rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            
            textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = initialText;
            textComponent.fontSize = 14;
            textComponent.color = Color.white;
            textComponent.margin = new Vector4(10, 5, 10, 5);
        }
        
        private void CreateSkillTreePanel()
        {
            if (skillTreePanel != null) return;
            
            var canvas = FindFirstObjectByType<Canvas>();
            skillTreePanel = new GameObject("Skill Tree Panel");
            skillTreePanel.transform.SetParent(canvas.transform, false);
            skillTreePanel.SetActive(false);
            
            var rect = skillTreePanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.2f, 0.2f);
            rect.anchorMax = new Vector2(0.8f, 0.8f);
            rect.sizeDelta = Vector2.zero;
            
            var bg = skillTreePanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            
            // Create scroll view for skill tree content
            CreateScrollView(skillTreePanel, "Skill Tree Content", out skillTreeContent);
            
            // Create toggle button
            CreateToggleButton("Skills (T)", OnToggleSkillTree, out skillTreeToggleButton);
        }
        
        private void CreateTemplatePanel()
        {
            if (templatePanel != null) return;
            
            var canvas = FindFirstObjectByType<Canvas>();
            templatePanel = new GameObject("Template Panel");
            templatePanel.transform.SetParent(canvas.transform, false);
            
            var rect = templatePanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.7f, 0.7f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.sizeDelta = Vector2.zero;
            
            var bg = templatePanel.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.1f, 0.3f, 0.8f);
            
            // Create template controls
            CreateTemplateControls();
        }
        
        private void CreateSynergyPanel()
        {
            if (synergyPanel != null) return;
            
            var canvas = FindFirstObjectByType<Canvas>();
            synergyPanel = new GameObject("Synergy Panel");
            synergyPanel.transform.SetParent(canvas.transform, false);
            synergyPanel.SetActive(false);
            
            var rect = synergyPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.4f);
            rect.anchorMax = new Vector2(0.3f, 0.8f);
            rect.sizeDelta = Vector2.zero;
            
            var bg = synergyPanel.AddComponent<Image>();
            bg.color = new Color(0.3f, 0.2f, 0.1f, 0.9f);
            
            CreateScrollView(synergyPanel, "Synergy Content", out synergyListContent);
            CreateToggleButton("Synergies (S)", OnToggleSynergyPanel, out synergyToggleButton);
        }
        
        private void CreateExtractionMinigamePanel()
        {
            if (extractionMinigamePanel != null) return;
            
            var canvas = FindFirstObjectByType<Canvas>();
            extractionMinigamePanel = new GameObject("Extraction Minigame Panel");
            extractionMinigamePanel.transform.SetParent(canvas.transform, false);
            extractionMinigamePanel.SetActive(false);
            
            var rect = extractionMinigamePanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, 0.1f);
            rect.anchorMax = new Vector2(0.9f, 0.9f);
            rect.sizeDelta = Vector2.zero;
            
            var bg = extractionMinigamePanel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.2f, 0.3f, 0.95f);
            
            CreateExtractionControls();
        }
        
        private void CreateFailureWarningPanel()
        {
            if (failureWarningPanel != null) return;
            
            var canvas = FindFirstObjectByType<Canvas>();
            failureWarningPanel = new GameObject("Failure Warning Panel");
            failureWarningPanel.transform.SetParent(canvas.transform, false);
            failureWarningPanel.SetActive(false);
            
            var rect = failureWarningPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.3f, 0.4f);
            rect.anchorMax = new Vector2(0.7f, 0.6f);
            rect.sizeDelta = Vector2.zero;
            
            var bg = failureWarningPanel.AddComponent<Image>();
            bg.color = new Color(0.8f, 0.2f, 0.2f, 0.9f);
        }
        
        private void CreateScrollView(GameObject parent, string contentName, out Transform content)
        {
            var scrollView = new GameObject("Scroll View");
            scrollView.transform.SetParent(parent.transform, false);
            
            var scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.sizeDelta = Vector2.zero;
            
            var scrollComponent = scrollView.AddComponent<ScrollRect>();
            
            var contentObj = new GameObject(contentName);
            contentObj.transform.SetParent(scrollView.transform, false);
            content = contentObj.transform;
            
            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            
            scrollComponent.content = contentRect;
            scrollComponent.horizontal = false;
            scrollComponent.vertical = true;
        }
        
        private void CreateToggleButton(string buttonText, System.Action onClick, out Button button)
        {
            var canvas = FindFirstObjectByType<Canvas>();
            var buttonObj = new GameObject($"Toggle {buttonText}");
            buttonObj.transform.SetParent(canvas.transform, false);
            
            var rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(100, 50);
            rect.sizeDelta = new Vector2(100, 30);
            
            button = buttonObj.AddComponent<Button>();
            var bg = buttonObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.3f, 0.8f, 0.8f);
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = buttonText;
            text.fontSize = 12;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            
            button.onClick.AddListener(() => onClick?.Invoke());
        }
        
        private void CreateTemplateControls()
        {
            // Template dropdown
            var dropdownObj = new GameObject("Template Dropdown");
            dropdownObj.transform.SetParent(templatePanel.transform, false);
            
            var dropdownRect = dropdownObj.AddComponent<RectTransform>();
            dropdownRect.anchorMin = new Vector2(0.1f, 0.7f);
            dropdownRect.anchorMax = new Vector2(0.9f, 0.9f);
            dropdownRect.sizeDelta = Vector2.zero;
            
            templateSelector = dropdownObj.AddComponent<Dropdown>();
            templateSelector.AddOptions(new List<string> { "Select Template..." });
            
            // Apply button
            var applyObj = new GameObject("Apply Template");
            applyObj.transform.SetParent(templatePanel.transform, false);
            
            var applyRect = applyObj.AddComponent<RectTransform>();
            applyRect.anchorMin = new Vector2(0.1f, 0.5f);
            applyRect.anchorMax = new Vector2(0.4f, 0.65f);
            applyRect.sizeDelta = Vector2.zero;
            
            applyTemplateButton = applyObj.AddComponent<Button>();
            var applyBg = applyObj.AddComponent<Image>();
            applyBg.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
            
            CreateButtonText(applyObj, "Apply");
            applyTemplateButton.onClick.AddListener(OnApplyTemplate);
            
            // Clear button
            var clearObj = new GameObject("Clear Template");
            clearObj.transform.SetParent(templatePanel.transform, false);
            
            var clearRect = clearObj.AddComponent<RectTransform>();
            clearRect.anchorMin = new Vector2(0.6f, 0.5f);
            clearRect.anchorMax = new Vector2(0.9f, 0.65f);
            clearRect.sizeDelta = Vector2.zero;
            
            clearTemplateButton = clearObj.AddComponent<Button>();
            var clearBg = clearObj.AddComponent<Image>();
            clearBg.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
            
            CreateButtonText(clearObj, "Clear");
            clearTemplateButton.onClick.AddListener(OnClearTemplate);
            
            // Description text
            var descObj = new GameObject("Template Description");
            descObj.transform.SetParent(templatePanel.transform, false);
            
            var descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.1f, 0.1f);
            descRect.anchorMax = new Vector2(0.9f, 0.4f);
            descRect.sizeDelta = Vector2.zero;
            
            templateDescriptionText = descObj.AddComponent<TextMeshProUGUI>();
            templateDescriptionText.text = "No template selected";
            templateDescriptionText.fontSize = 10;
            templateDescriptionText.color = Color.white;
        }
        
        private void CreateExtractionControls()
        {
            // Instructions
            var instrObj = new GameObject("Instructions");
            instrObj.transform.SetParent(extractionMinigamePanel.transform, false);
            
            var instrRect = instrObj.AddComponent<RectTransform>();
            instrRect.anchorMin = new Vector2(0.1f, 0.8f);
            instrRect.anchorMax = new Vector2(0.9f, 0.95f);
            instrRect.sizeDelta = Vector2.zero;
            
            extractionInstructionsText = instrObj.AddComponent<TextMeshProUGUI>();
            extractionInstructionsText.text = "Slide ingredients to extract the target to the exit";
            extractionInstructionsText.fontSize = 16;
            extractionInstructionsText.color = Color.white;
            extractionInstructionsText.alignment = TextAlignmentOptions.Center;
            
            // Move counter and timer
            CreateExtractionStatusUI();
            
            // Start button
            var startObj = new GameObject("Start Extraction");
            startObj.transform.SetParent(extractionMinigamePanel.transform, false);
            
            var startRect = startObj.AddComponent<RectTransform>();
            startRect.anchorMin = new Vector2(0.4f, 0.05f);
            startRect.anchorMax = new Vector2(0.6f, 0.15f);
            startRect.sizeDelta = Vector2.zero;
            
            startExtractionButton = startObj.AddComponent<Button>();
            var startBg = startObj.AddComponent<Image>();
            startBg.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
            
            CreateButtonText(startObj, "Start");
            startExtractionButton.onClick.AddListener(OnStartExtraction);
            
            // Puzzle grid container
            var gridObj = new GameObject("Puzzle Grid");
            gridObj.transform.SetParent(extractionMinigamePanel.transform, false);
            
            var gridRect = gridObj.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0.2f, 0.2f);
            gridRect.anchorMax = new Vector2(0.8f, 0.7f);
            gridRect.sizeDelta = Vector2.zero;
            
            puzzleGrid = gridObj.transform;
        }
        
        private void CreateExtractionStatusUI()
        {
            var statusObj = new GameObject("Extraction Status");
            statusObj.transform.SetParent(extractionMinigamePanel.transform, false);
            
            var statusRect = statusObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.1f, 0.7f);
            statusRect.anchorMax = new Vector2(0.9f, 0.8f);
            statusRect.sizeDelta = Vector2.zero;
            
            var moveObj = new GameObject("Move Count");
            moveObj.transform.SetParent(statusObj.transform, false);
            moveCountText = moveObj.AddComponent<TextMeshProUGUI>();
            moveCountText.text = "Moves: 0";
            moveCountText.fontSize = 14;
            moveCountText.color = Color.white;
            
            var timerObj = new GameObject("Timer");
            timerObj.transform.SetParent(statusObj.transform, false);
            extractionTimerText = timerObj.AddComponent<TextMeshProUGUI>();
            extractionTimerText.text = "Time: 0:00";
            extractionTimerText.fontSize = 14;
            extractionTimerText.color = Color.white;
            extractionTimerText.alignment = TextAlignmentOptions.Right;
        }
        
        private void CreateButtonText(GameObject button, string text)
        {
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(button.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 12;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;
        }
        
        #endregion
        
        #region UI Event Handlers
        
        private void SetupUIConnections()
        {
            if (templateSelector != null)
            {
                templateSelector.onValueChanged.AddListener(OnTemplateSelectionChanged);
            }
            
            RefreshTemplateDropdown();
        }
        
        private void SetupKeyboardShortcuts()
        {
            if (!enableKeyboardShortcuts) return;
            
            Debug.Log("🎹 Keyboard shortcuts enabled:");
            Debug.Log("  T - Toggle Skill Tree");
            Debug.Log("  S - Toggle Synergy Panel");
            Debug.Log("  R - Toggle Template Panel");
            Debug.Log("  E - Toggle Extraction Minigame");
            Debug.Log("  P - Use Purify Skill");
        }
        
        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                OnToggleSkillTree();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                OnToggleSynergyPanel();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                OnToggleTemplatePanel();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                OnToggleExtractionMinigame();
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                OnUsePurifySkill();
            }
        }
        
        private void OnToggleSkillTree()
        {
            skillTreeVisible = !skillTreeVisible;
            if (skillTreePanel != null)
            {
                skillTreePanel.SetActive(skillTreeVisible);
                if (skillTreeVisible)
                {
                    RefreshSkillTreeUI();
                }
            }
        }
        
        private void OnToggleSynergyPanel()
        {
            synergyPanelVisible = !synergyPanelVisible;
            if (synergyPanel != null)
            {
                synergyPanel.SetActive(synergyPanelVisible);
                if (synergyPanelVisible)
                {
                    RefreshSynergyUI();
                }
            }
        }
        
        private void OnToggleTemplatePanel()
        {
            templatePanelVisible = !templatePanelVisible;
            if (templatePanel != null)
            {
                templatePanel.SetActive(templatePanelVisible);
                if (templatePanelVisible)
                {
                    RefreshTemplateDropdown();
                }
            }
        }
        
        private void OnToggleExtractionMinigame()
        {
            extractionActive = !extractionActive;
            if (extractionMinigamePanel != null)
            {
                extractionMinigamePanel.SetActive(extractionActive);
            }
        }
        
        private void OnTemplateSelectionChanged(int index)
        {
            if (templateSystem == null || index <= 0) return;
            
            var templates = templateSystem.GetDiscoveredTemplates();
            if (index - 1 < templates.Count)
            {
                var selectedTemplate = templates[index - 1];
                if (templateDescriptionText != null)
                {
                    templateDescriptionText.text = $"{selectedTemplate.description}\n\n" +
                        $"Size: {selectedTemplate.gridSize.x}x{selectedTemplate.gridSize.y}\n" +
                        $"Difficulty: {selectedTemplate.difficulty}\n" +
                        $"Goal: {selectedTemplate.templateName}\n" +
                        $"Bonus: {selectedTemplate.bonusMultiplier:F1}x";
                }
            }
        }
        
        private void OnApplyTemplate()
        {
            if (templateSystem == null || templateSelector == null) return;
            
            var templates = templateSystem.GetDiscoveredTemplates();
            int index = templateSelector.value - 1;
            
            if (index >= 0 && index < templates.Count)
            {
                bool success = templateSystem.ActivateTemplate(templates[index].templateId);
                if (success)
                {
                    Debug.Log($"✅ Applied template: {templates[index].templateName}");
                }
            }
        }
        
        private void OnClearTemplate()
        {
            if (templateSystem != null)
            {
                templateSystem.DeactivateTemplate();
                if (templateDescriptionText != null)
                {
                    templateDescriptionText.text = "Template cleared - freeform mode";
                }
            }
        }
        
        private void OnStartExtraction()
        {
            if (extractionMinigame != null && gridManager?.availableIngredients?.Count > 0)
            {
                var targetIngredient = gridManager.availableIngredients[0];
                extractionMinigame.StartExtractionMinigame(targetIngredient);
            }
        }
        
        private void OnUsePurifySkill()
        {
            if (gridManager != null)
            {
                gridManager.UsePurifySkill();
            }
        }
        
        #endregion
        
        #region UI Updates
        
        private void UpdateStatusDisplay()
        {
            if (skillPointsText != null && skillTree != null)
            {
                skillPointsText.text = $"Skill Points: {skillTree.GetAvailableSkillPoints()}";
            }
            
            if (gridEfficiencyText != null && enhancedGridSystem != null)
            {
                float efficiency = enhancedGridSystem.GetGridEfficiencyScore();
                gridEfficiencyText.text = $"Grid Efficiency: {efficiency:P0}";
            }
            
            if (templateProgressText != null && templateSystem != null)
            {
                var currentTemplate = templateSystem.CurrentTemplate;
                if (currentTemplate != null)
                {
                    var progress = templateSystem.CheckTemplateProgress();
                    templateProgressText.text = $"Template: {currentTemplate.templateName} ({progress.completionPercentage:P0})";
                }
                else
                {
                    templateProgressText.text = "Template: None";
                }
            }
            
            if (synergyStatusText != null && synergySystem != null)
            {
                var activeSynergies = synergySystem.GetActiveSynergies();
                synergyStatusText.text = $"Synergies: {activeSynergies.Count} active";
            }
        }
        
        private void RefreshAllUI()
        {
            RefreshSkillTreeUI();
            RefreshSynergyUI();
            RefreshTemplateDropdown();
            UpdateStatusDisplay();
        }
        
        private void RefreshSkillTreeUI()
        {
            if (skillTree == null || skillTreeContent == null) return;
            
            // Clear existing skill nodes
            foreach (Transform child in skillTreeContent)
            {
                Destroy(child.gameObject);
            }
            
            // Create skill nodes
            var skills = skillTree.GetAllSkills();
            foreach (var skill in skills)
            {
                CreateSkillNode(skill);
            }
        }
        
        private void CreateSkillNode(AlchemySkill skill)
        {
            var nodeObj = new GameObject($"Skill_{skill.skillName}");
            nodeObj.transform.SetParent(skillTreeContent, false);
            
            var rect = nodeObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 60);
            
            var button = nodeObj.AddComponent<Button>();
            var bg = nodeObj.AddComponent<Image>();
            bg.color = skillTree.IsSkillUnlocked(skill.skillId) ? Color.green : Color.gray;
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(nodeObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = $"{skill.skillName}\nCost: {skill.cost}";
            text.fontSize = 10;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            
            button.onClick.AddListener(() => OnSkillNodeClicked(skill));
        }
        
        private void OnSkillNodeClicked(AlchemySkill skill)
        {
            if (skillTree != null && !skillTree.IsSkillUnlocked(skill.skillId))
            {
                bool success = skillTree.TryUnlockSkill(skill.skillId);
                if (success)
                {
                    RefreshSkillTreeUI();
                    Debug.Log($"✅ Unlocked skill: {skill.skillName}");
                }
                else
                {
                    Debug.Log($"❌ Cannot unlock {skill.skillName} - insufficient points or prerequisites not met");
                }
            }
        }
        
        private void RefreshSynergyUI()
        {
            if (synergySystem == null || synergyListContent == null) return;
            
            // Clear existing synergy items
            foreach (Transform child in synergyListContent)
            {
                Destroy(child.gameObject);
            }
            
            // Create synergy items
            var activeSynergies = synergySystem.GetActiveSynergies();
            foreach (var synergy in activeSynergies)
            {
                CreateSynergyItem(synergy);
            }
        }
        
        private void CreateSynergyItem(SynergyInstance synergyInstance)
        {
            var itemObj = new GameObject($"Synergy_{synergyInstance.synergyData.synergyName}");
            itemObj.transform.SetParent(synergyListContent, false);
            
            var rect = itemObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 40);
            
            var bg = itemObj.AddComponent<Image>();
            bg.color = new Color(0.3f, 0.6f, 0.9f, 0.8f);
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(itemObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = $"{synergyInstance.synergyData.synergyName}\n+{synergyInstance.effectivePotencyMultiplier:F1}x potency";
            text.fontSize = 10;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
        }
        
        private void RefreshTemplateDropdown()
        {
            if (templateSystem == null || templateSelector == null) return;
            
            templateSelector.ClearOptions();
            var options = new List<string> { "Select Template..." };
            
            var templates = templateSystem.GetDiscoveredTemplates();
            foreach (var template in templates)
            {
                options.Add($"{template.templateName} ({template.difficulty})");
            }
            
            templateSelector.AddOptions(options);
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Show failure warning dialog
        /// </summary>
        public bool ShowFailureWarning(FailureAnalysis analysis)
        {
            if (failureWarningPanel == null) return true;
            
            failureWarningPanel.SetActive(true);
            
            // In a real implementation, would create a proper dialog
            // For now, just log and return true to proceed
            Debug.Log($"⚠️ Recipe conflicts detected! Failure chance: {analysis.totalFailureChance:P0}");
            
            return true; // Would be based on user choice in real dialog
        }
        
        /// <summary>
        /// Update extraction minigame display
        /// </summary>
        public void UpdateExtractionDisplay(int moveCount, float timeElapsed)
        {
            if (moveCountText != null)
            {
                moveCountText.text = $"Moves: {moveCount}";
            }
            
            if (extractionTimerText != null)
            {
                int minutes = Mathf.FloorToInt(timeElapsed / 60);
                int seconds = Mathf.FloorToInt(timeElapsed % 60);
                extractionTimerText.text = $"Time: {minutes}:{seconds:00}";
            }
        }
        
        /// <summary>
        /// Toggle UI panel visibility
        /// </summary>
        public void TogglePanel(string panelName)
        {
            switch (panelName.ToLower())
            {
                case "skills": case "skilltree": OnToggleSkillTree(); break;
                case "synergy": case "synergies": OnToggleSynergyPanel(); break;
                case "template": case "templates": OnToggleTemplatePanel(); break;
                case "extraction": case "minigame": OnToggleExtractionMinigame(); break;
            }
        }
        
        /// <summary>
        /// Refresh all UI elements
        /// </summary>
        [ContextMenu("Refresh All UI")]
        public void RefreshUI()
        {
            RefreshAllUI();
        }
        
        #endregion
        
        #region Context Menu Testing
        
        [ContextMenu("🎨 Test Enhanced UI")]
        public void TestEnhancedUI()
        {
            Debug.Log("🎨 === TESTING ENHANCED UI ===");
            Debug.Log($"Connected systems: {CountConnectedSystems()}/7");
            
            if (skillTree != null)
            {
                skillTree.AwardSkillPoints(10, "UI Test");
                RefreshSkillTreeUI();
            }
            
            if (templateSystem != null)
            {
                templateSystem.TestTemplateSystem();
                RefreshTemplateDropdown();
            }
            
            if (synergySystem != null)
            {
                synergySystem.TestSynergySystem();
                RefreshSynergyUI();
            }
            
            RefreshAllUI();
            Debug.Log("✅ Enhanced UI test completed");
        }
        
        [ContextMenu("🔧 Create All UI Elements")]
        public void CreateAllUIElements()
        {
            CreateUIElements();
            SetupUIConnections();
            RefreshAllUI();
            Debug.Log("✅ All Enhanced UI elements created and connected");
        }
        
        #endregion
    }
}