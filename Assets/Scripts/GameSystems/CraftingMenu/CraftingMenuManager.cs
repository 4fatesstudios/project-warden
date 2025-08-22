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
        [SerializeField] private string ingredientsPanel = "InventoryPanel";
        [SerializeField] private string refinementsPanel = "Refinement";
        [SerializeField] private string alchemyPanel = "PotionCrafting";
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
        /// Navigate to ingredients/inventory panel
        /// </summary>
        public void NavigateToIngredients()
        {
            if (useHybridArchitecture && TryNavigateToPanel(ingredientsPanel))
            {
                if (enableDebugLogging)
                    Debug.Log($"🎒 Navigated to ingredients panel: {ingredientsPanel}");
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
        /// Navigate to alchemy/potion crafting panel
        /// </summary>
        public void NavigateToAlchemy()
        {
            if (useHybridArchitecture && TryNavigateToPanel(alchemyPanel))
            {
                if (enableDebugLogging)
                    Debug.Log($"🧪 Navigated to alchemy panel: {alchemyPanel}");
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
                // Try to use the hybrid navigation system
                var craftingManager = CraftingUIManager.Instance;
                var simpleManager = FindFirstObjectByType<SimpleCraftingManager>();

                if (craftingManager != null)
                {
                    craftingManager.GoBack();
                    if (enableDebugLogging)
                        Debug.Log("⬅️ Used CraftingUIManager to go back");
                    return;
                }
                else if (simpleManager != null)
                {
                    simpleManager.ShowPanel("CraftingMenuSystem");
                    if (enableDebugLogging)
                        Debug.Log("⬅️ Used SimpleCraftingManager to go back");
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

            // Try CraftingUIManager first (advanced hybrid system)
            if (CraftingUIManager.Instance != null)
            {
                try
                {
                    // Map common panel names to CraftingUIManager methods
                    switch (panelName.ToLower())
                    {
                        case "inventory":
                        case "inventorypanel":
                        case "ingredients":
                            // For now, show potion crafting which has inventory access
                            CraftingUIManager.Instance.ShowPotionCrafting();
                            break;
                        case "refinement":
                        case "refinements":
                            CraftingUIManager.Instance.ShowRefinement();
                            break;
                        case "alchemy":
                        case "potioncrafting":
                        case "potion":
                            CraftingUIManager.Instance.ShowPotionCrafting();
                            break;
                        case "craftingmenu":
                        case "mainmenu":
                        case "main":
                            CraftingUIManager.Instance.ShowMainMenu();
                            break;
                        default:
                            // Try to show panel by name directly
                            CraftingUIManager.Instance.ShowPanel(panelName);
                            break;
                    }
                    return true;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"⚠️ Failed to navigate with CraftingUIManager: {e.Message}");
                }
            }

            // Try SimpleCraftingManager (simple hybrid system)
            var simpleManager = FindFirstObjectByType<SimpleCraftingManager>();
            if (simpleManager != null)
            {
                try
                {
                    // Map panel names to GameObject names for SimpleCraftingManager
                    string targetPanel = MapPanelNameToGameObject(panelName);
                    simpleManager.ShowPanel(targetPanel);
                    return true;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"⚠️ Failed to navigate with SimpleCraftingManager: {e.Message}");
                }
            }

            return false;
        }

        /// <summary>
        /// Map panel names to actual GameObject names in the scene
        /// </summary>
        private string MapPanelNameToGameObject(string panelName)
        {
            switch (panelName.ToLower())
            {
                case "inventory":
                case "inventorypanel":
                case "ingredients":
                    return "PotionCraftingUI"; // Potion crafting has inventory access
                case "refinement":
                case "refinements":
                    return "RefinementUI";
                case "alchemy":
                case "potioncrafting":
                case "potion":
                    return "PotionCraftingUI";
                case "craftingmenu":
                case "mainmenu":
                case "main":
                    return "CraftingMenuSystem";
                case "bulkcrafting":
                case "bulk":
                    return "BulkCraftingUI";
                case "gridminigame":
                case "grid":
                    return "GridMinigameUI";
                default:
                    return panelName; // Return as-is and hope it matches
            }
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
        /// Check if hybrid architecture is available
        /// </summary>
        public bool IsHybridArchitectureAvailable()
        {
            return CraftingUIManager.Instance != null || FindFirstObjectByType<SimpleCraftingManager>() != null;
        }

        /// <summary>
        /// Get current navigation mode
        /// </summary>
        public string GetNavigationMode()
        {
            if (!useHybridArchitecture)
                return "Legacy Scene Loading";
            
            if (CraftingUIManager.Instance != null)
                return "Advanced Hybrid (CraftingUIManager)";
            
            if (FindFirstObjectByType<SimpleCraftingManager>() != null)
                return "Simple Hybrid (SimpleCraftingManager)";
            
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
            Debug.Log($"✅ Hybrid Available: {IsHybridArchitectureAvailable()}");
            
            if (CraftingUIManager.Instance != null)
                Debug.Log($"📋 Available Panels: {string.Join(", ", CraftingUIManager.Instance.GetAllPanelNames())}");
        }

        #endregion
    }
}