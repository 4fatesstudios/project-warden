// Enhanced RefinementMenuManager with ingredient selection and availability checking
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using FourFatesStudios.ProjectWarden.UI;
using FourFatesStudios.ProjectWarden.GameSystems.RefinementMenu;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using System.Collections.Generic;
using System.Linq;

namespace FourFatesStudios.ProjectWarden.GameSystems
{
    public class RefinementMenuManager : MonoBehaviour
    {
        private UIDocument uiDocument;
        private Button grindingButton;
        private Button distillingButton;
        private Button roastingButton;
        private Button backButton;
        private ScrollView ingredientList;
        private Label selectedIngredientLabel;
        private Label refinementInfoLabel;
        private Label baseSuccessLabel;
        private Label stabilityLabel;
        private Label skillRequiredLabel;
        private Button refreshButton;
        private Button helpButton;
        private VisualElement ingredientIcon;

        [Header("Panel Navigation Targets")]
        [SerializeField] private string grindingPanel = "GrindingMinigame";
        [SerializeField] private string distillingPanel = "DistillingMinigame";
        [SerializeField] private string roastingPanel = "RoastingMinigame";
        [SerializeField] private string backPanel = "CraftingMenu";

        [Header("Hybrid Architecture")]
        [SerializeField] private bool useHybridArchitecture = true;
        [SerializeField] private bool enableDebugLogging = true;

        [Header("Legacy Minigame Scene Names (Fallback)")]
        public string grindingSceneName = "GrindingScene";
        public string distillingSceneName = "DistillingScene";
        public string roastingSceneName = "RoastingScene";
        public string previousSceneName = "MainMenu";

        [Header("Ingredient Selection")]
        [SerializeField] private List<Ingredient> availableIngredients = new List<Ingredient>();
        [SerializeField] private Color enabledButtonColor = new Color(0.2f, 0.6f, 0.2f, 1f);
        [SerializeField] private Color disabledButtonColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        
        private Ingredient selectedIngredient;
        private Dictionary<string, VisualElement> ingredientButtons = new Dictionary<string, VisualElement>();

        void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            var root = uiDocument.rootVisualElement;

            grindingButton = root.Q<Button>("grindingButton");
            distillingButton = root.Q<Button>("distillingButton");
            roastingButton = root.Q<Button>("roastingButton");
            backButton = root.Q<Button>("backButton");
            ingredientList = root.Q<ScrollView>("ingredientList");
            selectedIngredientLabel = root.Q<Label>("selectedIngredientLabel");
            refinementInfoLabel = root.Q<Label>("refinementInfoLabel");
            baseSuccessLabel = root.Q<Label>("baseSuccessLabel");
            stabilityLabel = root.Q<Label>("stabilityLabel");
            skillRequiredLabel = root.Q<Label>("skillRequiredLabel");
            refreshButton = root.Q<Button>("refreshButton");
            helpButton = root.Q<Button>("helpButton");
            ingredientIcon = root.Q<VisualElement>("ingredient-icon");

            grindingButton?.RegisterCallback<ClickEvent>(evt => NavigateToGrinding());
            distillingButton?.RegisterCallback<ClickEvent>(evt => NavigateToDistilling());
            roastingButton?.RegisterCallback<ClickEvent>(evt => NavigateToRoasting());
            backButton?.RegisterCallback<ClickEvent>(evt => NavigateBack());
            refreshButton?.RegisterCallback<ClickEvent>(evt => RefreshAvailableIngredients());
            helpButton?.RegisterCallback<ClickEvent>(evt => ShowHelp());

            // Initialize UI elements if they don't exist
            InitializeUIElements();
            
            // Load available ingredients from inventory or demo data
            LoadAvailableIngredients();
            RefreshIngredientList();
            UpdateMinigameAvailability();

            if (enableDebugLogging)
                Debug.Log("⚗️ Enhanced RefinementMenuManager initialized with ingredient selection");
        }

        void OnDisable()
        {
            grindingButton?.UnregisterCallback<ClickEvent>(evt => NavigateToGrinding());
            distillingButton?.UnregisterCallback<ClickEvent>(evt => NavigateToDistilling());
            roastingButton?.UnregisterCallback<ClickEvent>(evt => NavigateToRoasting());
            backButton?.UnregisterCallback<ClickEvent>(evt => NavigateBack());
            refreshButton?.UnregisterCallback<ClickEvent>(evt => RefreshAvailableIngredients());
            helpButton?.UnregisterCallback<ClickEvent>(evt => ShowHelp());
        }

        #region UI Initialization

        private void InitializeUIElements()
        {
            var root = uiDocument.rootVisualElement;
            
            // Create ingredient selection panel if it doesn't exist
            if (ingredientList == null)
            {
                var container = root.Q<VisualElement>("menu-container") ?? root;
                
                // Create ingredient selection section
                var ingredientSection = new VisualElement();
                ingredientSection.name = "ingredient-section";
                ingredientSection.style.flexDirection = FlexDirection.Row;
                ingredientSection.style.paddingTop = 10;
                ingredientSection.style.paddingBottom = 10;
                
                // Left side - ingredient list
                var leftPanel = new VisualElement();
                leftPanel.style.width = Length.Percent(40);
                leftPanel.style.paddingRight = 10;
                
                var ingredientHeader = new Label("Select Ingredient:");
                ingredientHeader.style.fontSize = 16;
                ingredientHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
                ingredientHeader.style.marginBottom = 5;
                leftPanel.Add(ingredientHeader);
                
                ingredientList = new ScrollView();
                ingredientList.name = "ingredientList";
                ingredientList.style.height = 200;
                ingredientList.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
                ingredientList.style.borderBottomWidth = 2;
                ingredientList.style.borderTopWidth = 2;
                ingredientList.style.borderLeftWidth = 2;
                ingredientList.style.borderRightWidth = 2;
                ingredientList.style.borderBottomColor = Color.gray;
                ingredientList.style.borderTopColor = Color.gray;
                ingredientList.style.borderLeftColor = Color.gray;
                ingredientList.style.borderRightColor = Color.gray;
                leftPanel.Add(ingredientList);
                
                ingredientSection.Add(leftPanel);
                
                // Right side - refinement buttons and info
                var rightPanel = new VisualElement();
                rightPanel.style.width = Length.Percent(60);
                rightPanel.style.paddingLeft = 10;
                
                selectedIngredientLabel = new Label("No ingredient selected");
                selectedIngredientLabel.name = "selectedIngredientLabel";
                selectedIngredientLabel.style.fontSize = 14;
                selectedIngredientLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                selectedIngredientLabel.style.marginBottom = 10;
                rightPanel.Add(selectedIngredientLabel);
                
                refinementInfoLabel = new Label("Select an ingredient to see available refinement options.");
                refinementInfoLabel.name = "refinementInfoLabel";
                refinementInfoLabel.style.fontSize = 12;
                refinementInfoLabel.style.whiteSpace = WhiteSpace.Normal;
                refinementInfoLabel.style.marginBottom = 15;
                rightPanel.Add(refinementInfoLabel);
                
                // Move existing buttons to right panel
                if (grindingButton?.parent != null)
                {
                    grindingButton.RemoveFromHierarchy();
                    rightPanel.Add(grindingButton);
                }
                if (distillingButton?.parent != null)
                {
                    distillingButton.RemoveFromHierarchy();
                    rightPanel.Add(distillingButton);
                }
                if (roastingButton?.parent != null)
                {
                    roastingButton.RemoveFromHierarchy();
                    rightPanel.Add(roastingButton);
                }
                
                ingredientSection.Add(rightPanel);
                
                // Insert before back button or at the beginning
                if (backButton?.parent != null)
                {
                    backButton.parent.Insert(0, ingredientSection);
                }
                else
                {
                    container.Insert(0, ingredientSection);
                }
            }
        }

        #endregion

        #region Ingredient Management

        private void LoadAvailableIngredients()
        {
            // Try to get ingredients from inventory system
            var inventoryHolder = FindFirstObjectByType<ItemSlotContainerHolder>();
            if (inventoryHolder != null)
            {
                // This would need to be implemented based on your inventory system
                // For now, load demo ingredients
                LoadDemoIngredients();
            }
            else
            {
                LoadDemoIngredients();
            }
        }

        private void LoadDemoIngredients()
        {
            // Load some demo ingredients for testing
            var allIngredients = Resources.LoadAll<Ingredient>("Ingredients");
            if (allIngredients.Length > 0)
            {
                availableIngredients = allIngredients.ToList();
            }
            else
            {
                if (enableDebugLogging)
                    Debug.LogWarning("No ingredients found in Resources/Ingredients folder. Creating demo data.");
                CreateDemoIngredients();
            }
        }

        private void CreateDemoIngredients()
        {
            // This would create demo ingredients if none exist
            // For development purposes only
            availableIngredients.Clear();
            
            if (enableDebugLogging)
                Debug.Log("Demo ingredients would be created here. Consider creating some Ingredient assets in Resources/Ingredients.");
        }

        private void RefreshIngredientList()
        {
            if (ingredientList == null) return;
            
            ingredientList.Clear();
            ingredientButtons.Clear();
            
            foreach (var ingredient in availableIngredients)
            {
                if (ingredient == null) continue;
                
                var ingredientButton = new Button(() => SelectIngredient(ingredient));
                ingredientButton.text = ingredient.ItemName;
                ingredientButton.AddToClassList("ingredient-button");
                
                // Add ingredient info tooltip
                var tooltipText = $"{ingredient.ItemName}\nType: {ingredient.IngredientArchetype}";
                if (ingredient.CanGrind) tooltipText += "\n• Can be ground";
                if (ingredient.CanDistill) tooltipText += "\n• Can be distilled";
                if (ingredient.CanRoast) tooltipText += "\n• Can be roasted";
                
                ingredientButton.tooltip = tooltipText;
                
                ingredientList.Add(ingredientButton);
                ingredientButtons[ingredient.ItemName] = ingredientButton;
            }
        }

        private void SelectIngredient(Ingredient ingredient)
        {
            selectedIngredient = ingredient;
            
            if (selectedIngredientLabel != null)
            {
                selectedIngredientLabel.text = $"Selected: {ingredient.ItemName}";
            }
            
            // Update ingredient icon
            if (ingredientIcon != null && ingredient.ItemIcon != null)
            {
                ingredientIcon.style.backgroundImage = new StyleBackground(ingredient.ItemIcon);
            }
            
            if (refinementInfoLabel != null)
            {
                var info = $"Type: {ingredient.IngredientArchetype}\n";
                info += $"Potency: {ingredient.Potency}\n";
                info += $"Aspect: {ingredient.IngredientAspect}\n";
                if (ingredient.IsCorrupted)
                    info += "⚠️ Corrupted ingredient\n";
                
                info += "\nAvailable Refinements:\n";
                
                if (ingredient.CanGrind)
                    info += "• Grinding - Crushes ores into powder\n";
                if (ingredient.CanDistill)
                    info += "• Distilling - Extracts essences from herbs\n";
                if (ingredient.CanRoast)
                    info += "• Roasting - Heat-treats organic materials\n";
                
                if (!ingredient.CanGrind && !ingredient.CanDistill && !ingredient.CanRoast)
                    info += "• None - This ingredient cannot be refined\n";
                
                refinementInfoLabel.text = info;
            }
            
            // Update success rate information
            UpdateSuccessRateDisplay(ingredient);
            
            UpdateMinigameAvailability();
            
            // Highlight selected ingredient button
            foreach (var kvp in ingredientButtons)
            {
                var button = kvp.Value as Button;
                if (button != null)
                {
                    if (kvp.Key == ingredient.ItemName)
                    {
                        button.AddToClassList("ingredient-button-selected");
                        button.style.backgroundColor = new Color(0.3f, 0.5f, 0.3f, 1f);
                    }
                    else
                    {
                        button.RemoveFromClassList("ingredient-button-selected");
                        button.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
                    }
                }
            }
            
            if (enableDebugLogging)
                Debug.Log($"🧪 Selected ingredient: {ingredient.ItemName} (Can Grind: {ingredient.CanGrind}, Can Distill: {ingredient.CanDistill}, Can Roast: {ingredient.CanRoast})");
        }
        
        private void UpdateSuccessRateDisplay(Ingredient ingredient)
        {
            if (baseSuccessLabel != null)
            {
                var successRate = (ingredient.BaseRefiningSuccessRate * 100f).ToString("F0");
                baseSuccessLabel.text = $"Base Success: {successRate}%";
            }
            
            if (stabilityLabel != null)
            {
                var stability = (ingredient.StabilityRating * 100f).ToString("F0");
                stabilityLabel.text = $"Stability: {stability}%";
            }
            
            if (skillRequiredLabel != null)
            {
                skillRequiredLabel.text = $"Skill Required: {ingredient.MinimumRefiningSkill}";
            }
        }

        private void UpdateMinigameAvailability()
        {
            bool canGrind = selectedIngredient != null && selectedIngredient.CanGrind;
            bool canDistill = selectedIngredient != null && selectedIngredient.CanDistill;
            bool canRoast = selectedIngredient != null && selectedIngredient.CanRoast;
            
            UpdateButtonState(grindingButton, canGrind, "Grinding");
            UpdateButtonState(distillingButton, canDistill, "Distilling");
            UpdateButtonState(roastingButton, canRoast, "Roasting");
        }

        private void UpdateButtonState(Button button, bool enabled, string minigameType)
        {
            if (button == null) return;
            
            button.SetEnabled(enabled);
            
            if (enabled)
            {
                button.style.backgroundColor = enabledButtonColor;
                button.style.opacity = 1f;
                button.text = $"Start {minigameType}";
            }
            else
            {
                button.style.backgroundColor = disabledButtonColor;
                button.style.opacity = 0.6f;
                button.text = $"{minigameType} (Not Available)";
            }
        }

        #endregion

        #region Navigation Methods

        public void NavigateToGrinding()
        {
            if (selectedIngredient == null || !selectedIngredient.CanGrind)
            {
                if (enableDebugLogging)
                    Debug.LogWarning("Cannot start grinding: No suitable ingredient selected");
                return;
            }

            if (useHybridArchitecture && TryNavigateToPanel(grindingPanel))
            {
                // Pass selected ingredient to grinding minigame
                PassIngredientToMinigame("grinding", selectedIngredient);
                if (enableDebugLogging)
                    Debug.Log($"🔨 Navigated to grinding panel with ingredient: {selectedIngredient.ItemName}");
                return;
            }

            LoadScene(grindingSceneName);
        }

        public void NavigateToDistilling()
        {
            if (selectedIngredient == null || !selectedIngredient.CanDistill)
            {
                if (enableDebugLogging)
                    Debug.LogWarning("Cannot start distilling: No suitable ingredient selected");
                return;
            }

            if (useHybridArchitecture && TryNavigateToPanel(distillingPanel))
            {
                PassIngredientToMinigame("distilling", selectedIngredient);
                if (enableDebugLogging)
                    Debug.Log($"🧪 Navigated to distilling panel with ingredient: {selectedIngredient.ItemName}");
                return;
            }

            LoadScene(distillingSceneName);
        }

        public void NavigateToRoasting()
        {
            if (selectedIngredient == null || !selectedIngredient.CanRoast)
            {
                if (enableDebugLogging)
                    Debug.LogWarning("Cannot start roasting: No suitable ingredient selected");
                return;
            }

            if (useHybridArchitecture && TryNavigateToPanel(roastingPanel))
            {
                PassIngredientToMinigame("roasting", selectedIngredient);
                if (enableDebugLogging)
                    Debug.Log($"🔥 Navigated to roasting panel with ingredient: {selectedIngredient.ItemName}");
                return;
            }

            LoadScene(roastingSceneName);
        }

        private void PassIngredientToMinigame(string minigameType, Ingredient ingredient)
        {
            // Find the appropriate minigame controller and pass the ingredient
            switch (minigameType.ToLower())
            {
                case "grinding":
                    var grindingController = FindFirstObjectByType<GrindingMinigameController>();
                    if (grindingController != null)
                    {
                        grindingController.SetTargetIngredient(ingredient);
                    }
                    break;
                case "distilling":
                    var distillationController = FindFirstObjectByType<DistillationMinigameController>();
                    if (distillationController != null)
                    {
                        distillationController.SetTargetIngredient(ingredient);
                    }
                    break;
                case "roasting":
                    var roastingController = FindFirstObjectByType<RoastingMinigameController>();
                    if (roastingController != null)
                    {
                        roastingController.SetTargetIngredient(ingredient);
                    }
                    break;
            }
        }

        public void NavigateBack()
        {
            if (useHybridArchitecture)
            {
                var navigationController = FindFirstObjectByType<CraftingNavigationController>();

                if (navigationController != null)
                {
                    navigationController.ShowMainMenu();
                    if (enableDebugLogging)
                        Debug.Log("⬅️ Used CraftingNavigationController to go back to main menu");
                    return;
                }
                else if (TryNavigateToPanel(backPanel))
                {
                    if (enableDebugLogging)
                        Debug.Log($"⬅️ Navigated back to panel: {backPanel}");
                    return;
                }
            }

            LoadScene(previousSceneName);
        }

        #endregion

        #region Hybrid Architecture Support

        private bool TryNavigateToPanel(string panelName)
        {
            if (string.IsNullOrEmpty(panelName))
                return false;

            var navigationController = FindFirstObjectByType<CraftingNavigationController>();
            if (navigationController != null)
            {
                try
                {
                    switch (panelName.ToLower())
                    {
                        case "grinding":
                        case "grindingminigame":
                            navigationController.ShowGrindingMinigame();
                            break;
                        case "distilling":
                        case "distillingminigame":
                            navigationController.ShowDistillingMinigame();
                            break;
                        case "roasting":
                        case "roastingminigame":
                            navigationController.ShowRoastingMinigame();
                            break;
                        case "craftingmenu":
                        case "mainmenu":
                        case "main":
                            navigationController.ShowMainMenu();
                            break;
                        default:
                            navigationController.ShowPanel(panelName);
                            break;
                    }
                    return true;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"⚠️ Failed to navigate with CraftingNavigationController: {e.Message}");
                }
            }

            return false;
        }

        #endregion

        #region Legacy Scene Loading (Fallback)

        private void LoadScene(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                if (enableDebugLogging)
                    Debug.LogWarning($"⚠️ Falling back to scene loading: {sceneName}");
                    
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning("Scene name is empty or null.");
            }
        }

        #endregion

        #region Public Utility Methods

        public void SetHybridMode(bool enabled)
        {
            useHybridArchitecture = enabled;
            if (enableDebugLogging)
                Debug.Log($"🔄 Refinement Menu Hybrid architecture: {(enabled ? "Enabled" : "Disabled")}");
        }

        public bool IsHybridArchitectureAvailable()
        {
            return FindFirstObjectByType<CraftingNavigationController>() != null;
        }

        public void RefreshAvailableIngredients()
        {
            LoadAvailableIngredients();
            RefreshIngredientList();
            UpdateMinigameAvailability();
        }

        public Ingredient GetSelectedIngredient()
        {
            return selectedIngredient;
        }
        
        private void ShowHelp()
        {
            if (enableDebugLogging)
                Debug.Log("📚 Help requested for refinement system");
                
            // This could open a help dialog or show tooltips
            // For now, just log some helpful information
            Debug.Log("🔍 Refinement System Help:\n" +
                     "• Select an ingredient from the left panel\n" +
                     "• Choose an appropriate refinement method\n" +
                     "• Grinding: For ores and minerals\n" +
                     "• Distilling: For herbs and organic materials\n" +
                     "• Roasting: For heat-treating materials\n" +
                     "• Success depends on your skill and the ingredient's stability");
        }

        #endregion
    }
}