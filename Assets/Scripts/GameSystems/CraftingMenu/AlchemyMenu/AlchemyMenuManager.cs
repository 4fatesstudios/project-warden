using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using FourFatesStudios.ProjectWarden.UI;

namespace FourFatesStudios.ProjectWarden.GameSystems
{
    public class AlchemyMenuManager : MonoBehaviour
    {
        private UIDocument uiDocument;
        private Button potionCraftingButton;
        private Button potionEnhancingButton;
        private Button backButton;

        [Header("Panel Navigation Targets")]
        [SerializeField] private string potionCraftingPanel = "GridMinigameUI";
        [SerializeField] private string potionEnhancingPanel = "BulkCrafting";
        [SerializeField] private string backPanel = "CraftingMenu";

        [Header("Hybrid Architecture")]
        [SerializeField] private bool useHybridArchitecture = true;
        [SerializeField] private bool enableDebugLogging = true;

        [Header("Legacy Scene Names (Fallback)")]
        public string potionCraftingSceneName = "PotionCraftingScene";
        public string potionEnhancingSceneName = "PotionEnhancingScene";
        public string previousSceneName = "MainMenu";

        [Header("Placeholder for Unlock Logic")]
        public bool hasEnhancingSkill = false;

        void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            var root = uiDocument.rootVisualElement;

            potionCraftingButton = root.Q<Button>("potionCraftingButton");
            potionEnhancingButton = root.Q<Button>("potionEnhancingButton");
            backButton = root.Q<Button>("backButton");

            potionCraftingButton?.RegisterCallback<ClickEvent>(evt => NavigateToPotionCrafting());
            backButton?.RegisterCallback<ClickEvent>(evt => NavigateBack());

            if (hasEnhancingSkill)
            {
                potionEnhancingButton?.RegisterCallback<ClickEvent>(evt => NavigateToPotionEnhancing());
            }
            else
            {
                potionEnhancingButton?.SetEnabled(false);
                potionEnhancingButton.tooltip = "Unlock the required skill to access bulk crafting.";
            }

            if (enableDebugLogging)
                Debug.Log("AlchemyMenuManager initialized with hybrid architecture support");
        }

        void OnDisable()
        {
            potionCraftingButton?.UnregisterCallback<ClickEvent>(evt => NavigateToPotionCrafting());
            potionEnhancingButton?.UnregisterCallback<ClickEvent>(evt => NavigateToPotionEnhancing());
            backButton?.UnregisterCallback<ClickEvent>(evt => NavigateBack());
        }

        #region Navigation Methods

        public void NavigateToPotionCrafting()
        {
            if (useHybridArchitecture && TryNavigateToPanel(potionCraftingPanel))
            {
                if (enableDebugLogging)
                    Debug.Log($"Navigated to potion crafting panel: {potionCraftingPanel}");
                return;
            }

            LoadScene(potionCraftingSceneName);
        }

        public void NavigateToPotionEnhancing()
        {
            if (useHybridArchitecture && TryNavigateToPanel(potionEnhancingPanel))
            {
                if (enableDebugLogging)
                    Debug.Log($"Navigated to bulk crafting panel: {potionEnhancingPanel}");
                return;
            }

            LoadScene(potionEnhancingSceneName);
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
                        Debug.Log("Used CraftingNavigationController to go back to main menu");
                    return;
                }
                else if (TryNavigateToPanel(backPanel))
                {
                    if (enableDebugLogging)
                        Debug.Log($"Navigated back to panel: {backPanel}");
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
                        case "potioncrafting":
                        case "potion":
                        case "gridminigameui":
                            navigationController.ShowGridMinigame();
                            break;
                        case "gridminigame":
                        case "grid":
                            navigationController.ShowGridMinigame();
                            break;
                        case "bulkcrafting":
                        case "bulk":
                        case "enhancing":
                            navigationController.ShowBulkCrafting();
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
                    Debug.LogWarning($"Failed to navigate with CraftingNavigationController: {e.Message}");
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
                    Debug.LogWarning($"Falling back to scene loading: {sceneName}");
                    
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
                Debug.Log($"Alchemy Menu Hybrid architecture: {(enabled ? "Enabled" : "Disabled")}");
        }

        public bool IsHybridArchitectureAvailable()
        {
            return FindFirstObjectByType<CraftingNavigationController>() != null;
        }

        #endregion
    }
}
