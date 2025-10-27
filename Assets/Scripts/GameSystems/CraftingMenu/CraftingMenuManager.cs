using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.UI;

namespace FourFatesStudios.ProjectWarden.GameSystems
{
    /// <summary>
    /// Crafting Menu Manager for Hybrid Architecture
    /// Uses panel switching instead of scene loading
    /// </summary>
    public class CraftingMenuManager : MonoBehaviour
    {
        private UIDocument uiDocument;
        private Button ingredientsButton;
        private Button refinementsButton;
        private Button alchemyButton;
        private Button backButton;

        [Header("Panel Navigation Targets")]
        [SerializeField] private string ingredientsPanel = "AlchemyBookUI";
        [SerializeField] private string refinementsPanel = "Refinement";
        [SerializeField] private string alchemyPanel = "AlchemyMenu";  // Route to potion brewing menu instead of grid minigame
        [SerializeField] private string backPanel = "CraftingMenu";

        [Header("Hybrid Architecture")]
        [SerializeField] private bool useHybridArchitecture = true;
        [SerializeField] private bool enableDebugLogging = true;

        // Fallback scene names for backward compatibility
        [Header("Legacy Scene Navigation (Fallback)")]
        public string ingredientsScene = "IngredientsBookScene";
        public string refinementsScene = "RefinementMenuScene";
        public string alchemyScene = "AlchemyScene";
        public string backScene = "MainMenu";

        void OnEnable()
        {
            uiDocument = GetComponent<UIDocument>();
            var root = uiDocument.rootVisualElement;

            // Find UI buttons
            ingredientsButton = root.Q<Button>("ingredientsButton");
            refinementsButton = root.Q<Button>("refinementsButton");
            alchemyButton = root.Q<Button>("alchemyButton");
            backButton = root.Q<Button>("backButton");

            // Register button callbacks
            ingredientsButton?.RegisterCallback<ClickEvent>(_ => NavigateToIngredients());
            refinementsButton?.RegisterCallback<ClickEvent>(_ => NavigateToRefinements());
            alchemyButton?.RegisterCallback<ClickEvent>(_ => NavigateToAlchemy());
            backButton?.RegisterCallback<ClickEvent>(_ => NavigateBack());

            if (enableDebugLogging)
                Debug.Log("🧪 CraftingMenuManager initialized with hybrid architecture support");
        }

        void OnDisable()
        {
            // Unregister callbacks to prevent memory leaks
            ingredientsButton?.UnregisterCallback<ClickEvent>(_ => NavigateToIngredients());
            refinementsButton?.UnregisterCallback<ClickEvent>(_ => NavigateToRefinements());
            alchemyButton?.UnregisterCallback<ClickEvent>(_ => NavigateToAlchemy());
            backButton?.UnregisterCallback<ClickEvent>(_ => NavigateBack());
        }

        #region Navigation Methods

        /// <summary>
        /// Navigate to ingredients/potion brewing guide panel
        /// </summary>
        public void NavigateToIngredients()
        {
            if (useHybridArchitecture && TryNavigateToPanel(ingredientsPanel))
            {
                if (enableDebugLogging)
                    Debug.Log($"🎒 Navigated to potion brewing guide panel: {ingredientsPanel}");
                return;
            }

            // Fallback to scene loading
            LoadScene(ingredientsScene);
        }

        /// <summary>
        /// Navigate to refinements panel
        /// </summary>
        public void NavigateToRefinements()
        {
            if (useHybridArchitecture && TryNavigateToPanel(refinementsPanel))
            {
                if (enableDebugLogging)
                    Debug.Log($"⚗️ Navigated to refinements panel: {refinementsPanel}");
                return;
            }

            // Fallback to scene loading
            LoadScene(refinementsScene);
        }

        /// <summary>
        /// Navigate to potion brewing/alchemy panel
        /// </summary>
        public void NavigateToAlchemy()
        {
            if (useHybridArchitecture && TryNavigateToPanel(alchemyPanel))
            {
                if (enableDebugLogging)
                    Debug.Log($"🧪 Navigated to potion brewing panel: {alchemyPanel}");
                return;
            }

            // Fallback to scene loading
            LoadScene(alchemyScene);
        }

        /// <summary>
        /// Navigate back to main menu or previous panel
        /// </summary>
        public void NavigateBack()
        {
            if (useHybridArchitecture)
            {
                // Try to use the navigation controller system
                var navigationController = FindFirstObjectByType<CraftingNavigationController>();

                if (navigationController != null)
                {
                    navigationController.GoBack();
                    if (enableDebugLogging)
                        Debug.Log("⬅️ Used CraftingNavigationController to go back");
                    return;
                }
                else if (TryNavigateToPanel(backPanel))
                {
                    if (enableDebugLogging)
                        Debug.Log($"⬅️ Navigated back to panel: {backPanel}");
                    return;
                }
            }

            // Fallback to scene loading
            LoadScene(backScene);
        }

        #endregion

        #region Hybrid Architecture Support

        /// <summary>
        /// Try to navigate to a panel using the hybrid architecture
        /// </summary>
        private bool TryNavigateToPanel(string panelName)
        {
            if (string.IsNullOrEmpty(panelName))
                return false;

            // Try CraftingNavigationController first (navigation system)
            var navigationController = FindFirstObjectByType<CraftingNavigationController>();
            if (navigationController != null)
            {
                try
                {
                    // Map common panel names to CraftingNavigationController methods
                    switch (panelName.ToLower())
                    {
                        case "inventory":
                        case "inventorypanel":
                        case "ingredients":
                        case "alchemybook":
                            // Navigate to potion brewing guide
                            FindFirstObjectByType<CraftingNavigationController>()?.ShowAlchemyBook();
                            break;
                        case "refinement":
                        case "refinements":
                            FindFirstObjectByType<CraftingNavigationController>()?.ShowRefinement();
                            break;
                        case "alchemy":
                        case "alchemymenu":
                        case "alchemy_menu":
                            FindFirstObjectByType<CraftingNavigationController>()?.ShowAlchemyMenu();
                            break;
                        case "potioncrafting":
                        case "potion":
                            FindFirstObjectByType<CraftingNavigationController>()?.ShowAlchemyMenu(); // Redirect to potion brewing menu
                            break;
                        case "gridminigame":
                        case "grid":
                            FindFirstObjectByType<CraftingNavigationController>()?.ShowGridMinigame();
                            break;
                        case "craftingmenu":
                        case "mainmenu":
                        case "main":
                            navigationController.ShowMainMenu();
                            break;
                        default:
                            // Try to show panel by name directly
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

        /// <summary>
        /// Legacy scene loading method (fallback when hybrid architecture fails)
        /// </summary>
        private void LoadScene(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                if (enableDebugLogging)
                    Debug.LogWarning($"⚠️ Falling back to scene loading: {sceneName}");
                    
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning("Scene name not assigned.");
            }
        }

        #endregion

        #region Public Utility Methods

        /// <summary>
        /// Toggle between hybrid and legacy navigation modes
        /// </summary>
        public void SetHybridMode(bool enabled)
        {
            useHybridArchitecture = enabled;
            if (enableDebugLogging)
                Debug.Log($"🔄 Hybrid architecture: {(enabled ? "Enabled" : "Disabled")}");
        }

        /// <summary>
        /// Get current navigation mode
        /// </summary>
        public string GetNavigationMode()
        {
            if (!useHybridArchitecture)
                return "Legacy Scene Loading";
            
            if (FindFirstObjectByType<CraftingNavigationController>() != null)
                return "Navigation Controller (CraftingNavigationController)";
            
            return "Hybrid (No Manager Found)";
        }

        #endregion

        #region Context Menu Debug Methods

        [ContextMenu("Test Ingredients Navigation")]
        private void TestIngredientsNavigation()
        {
            NavigateToIngredients();
        }

        [ContextMenu("Test Refinements Navigation")]
        private void TestRefinementsNavigation()
        {
            NavigateToRefinements();
        }

        [ContextMenu("Test Alchemy Navigation")]
        private void TestAlchemyNavigation()
        {
            NavigateToAlchemy();
        }

        [ContextMenu("Test Back Navigation")]
        private void TestBackNavigation()
        {
            NavigateBack();
        }

        [ContextMenu("Debug Navigation Status")]
        private void DebugNavigationStatus()
        {
            Debug.Log($"🔍 Navigation Mode: {GetNavigationMode()}");
            Debug.Log($"🔄 Hybrid Architecture: {useHybridArchitecture}");
            
            var navigationController = FindFirstObjectByType<CraftingNavigationController>();
            if (navigationController != null)
                Debug.Log($"📋 Available Panels: {string.Join(", ", navigationController.GetAllPanelNames())}");
        }

        #endregion
    }
}