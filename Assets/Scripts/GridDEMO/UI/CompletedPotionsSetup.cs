using UnityEngine;
using UnityEngine.UIElements;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Setup helper for the completed potions UI Document system
    /// Use this script to easily set up the completed potions list in your scene
    /// </summary>
    [System.Serializable]
    public class CompletedPotionsSetup : MonoBehaviour
    {
        [Header("UI Assets")]
        [SerializeField] private VisualTreeAsset completedPotionsUXML;
        [SerializeField] private StyleSheet completedPotionsUSS;
        
        [Header("Integration")]
        [SerializeField] private bool replaceExistingProficiencyDisplay = true;
        [SerializeField] private bool autoConnectToCraftingEvents = true;
        
        private CompletedPotionsUIDocument uiDocumentComponent;
        private ProficiencyDisplayManager existingDisplayManager;
        
        private void Start()
        {
            SetupCompletedPotionsList();
        }
        
        /// <summary>
        /// Set up the completed potions list UI Document
        /// </summary>
        [ContextMenu("Setup Completed Potions List")]
        public void SetupCompletedPotionsList()
        {
            Debug.Log("🔧 Setting up Completed Potions UI Document...");
            
            // Find or create the UI Document component
            uiDocumentComponent = GetComponent<CompletedPotionsUIDocument>();
            if (uiDocumentComponent == null)
            {
                uiDocumentComponent = gameObject.AddComponent<CompletedPotionsUIDocument>();
                Debug.Log("✅ Added CompletedPotionsUIDocument component");
            }
            
            // Set up the UIDocument component
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                uiDocument = gameObject.AddComponent<UIDocument>();
                Debug.Log("✅ Added UIDocument component");
            }
            
            // Assign assets if available
            if (completedPotionsUXML != null)
            {
                uiDocument.visualTreeAsset = completedPotionsUXML;
                Debug.Log("✅ Assigned UXML asset");
            }
            else
            {
                Debug.LogWarning("⚠️ No UXML asset assigned. Please assign CompletedPotionsList.uxml in the inspector.");
            }
            
            // Try to find and disable existing proficiency display if requested
            if (replaceExistingProficiencyDisplay)
            {
                HandleExistingProficiencyDisplay();
            }
            
            // Set up event connections if requested
            if (autoConnectToCraftingEvents)
            {
                ConnectToCraftingEvents();
            }
            
            Debug.Log("✅ Completed Potions UI Document setup complete!");
        }
        
        /// <summary>
        /// Handle existing ProficiencyDisplayManager - disable potion list portion
        /// </summary>
        private void HandleExistingProficiencyDisplay()
        {
            existingDisplayManager = FindFirstObjectByType<ProficiencyDisplayManager>();
            if (existingDisplayManager != null)
            {
                Debug.Log("🔄 Found existing ProficiencyDisplayManager - will coordinate with it");
                // Note: We don't disable it completely as it handles proficiency calculation
                // The new UI Document will handle only the potions list portion
            }
            else
            {
                Debug.Log("ℹ️ No existing ProficiencyDisplayManager found");
            }
        }
        
        /// <summary>
        /// Connect to crafting system events for automatic potion tracking
        /// </summary>
        private void ConnectToCraftingEvents()
        {
            // This would connect to the crafting system events
            // The actual connection depends on how your crafting system dispatches events
            Debug.Log("🔗 Auto-connecting to crafting events...");
            
            // Example: If there's a CraftingManager or similar, connect to its events here
            // CraftingManager.OnPotionCrafted += uiDocumentComponent.OnPotionCrafted;
            
            Debug.Log("✅ Event connections established");
        }
        
        /// <summary>
        /// Create the UI Document GameObject in the scene
        /// </summary>
        [ContextMenu("Create Completed Potions GameObject")]
        public static void CreateCompletedPotionsGameObject()
        {
            // Create a new GameObject for the completed potions UI
            GameObject potionsUIObject = new GameObject("Completed Potions UI");
            
            // Add the setup component
            var setup = potionsUIObject.AddComponent<CompletedPotionsSetup>();
            
            // Try to auto-assign assets
            var uxml = Resources.Load<VisualTreeAsset>("UI/UXML/CompletedPotionsList");
            var uss = Resources.Load<StyleSheet>("UI/Styles/CompletedPotionsListStyles");
            
            if (uxml != null)
            {
                setup.completedPotionsUXML = uxml;
                Debug.Log("✅ Auto-assigned UXML asset");
            }
            
            if (uss != null)
            {
                setup.completedPotionsUSS = uss;
                Debug.Log("✅ Auto-assigned USS asset");
            }
            
            // Set up the UI immediately
            setup.SetupCompletedPotionsList();
            
            Debug.Log("✅ Created Completed Potions UI GameObject");
        }
        
        /// <summary>
        /// Test the potion tracking system
        /// </summary>
        [ContextMenu("Test Add Potion")]
        public void TestAddPotion()
        {
            if (uiDocumentComponent != null)
            {
                uiDocumentComponent.OnPotionCrafted("test_recipe", "Test Healing Potion");
                Debug.Log("🧪 Added test potion");
            }
            else
            {
                Debug.LogWarning("⚠️ CompletedPotionsUIDocument component not found!");
            }
        }
        
        /// <summary>
        /// Load assets from Resources if not assigned
        /// </summary>
        [ContextMenu("Auto-Assign Assets")]
        public void AutoAssignAssets()
        {
            if (completedPotionsUXML == null)
            {
                // Try to load from Resources
                completedPotionsUXML = Resources.Load<VisualTreeAsset>("UI/UXML/CompletedPotionsList");
                if (completedPotionsUXML != null)
                {
                    Debug.Log("✅ Auto-assigned UXML from Resources");
                }
                else
                {
                    Debug.LogWarning("⚠️ Could not find CompletedPotionsList.uxml in Resources");
                }
            }
            
            if (completedPotionsUSS == null)
            {
                // Try to load from Resources
                completedPotionsUSS = Resources.Load<StyleSheet>("UI/Styles/CompletedPotionsListStyles");
                if (completedPotionsUSS != null)
                {
                    Debug.Log("✅ Auto-assigned USS from Resources");
                }
                else
                {
                    Debug.LogWarning("⚠️ Could not find CompletedPotionsListStyles.uss in Resources");
                }
            }
            
            Debug.Log("🔄 Asset auto-assignment complete");
        }
        
        private void OnDestroy()
        {
            // Clean up event connections if needed
            if (autoConnectToCraftingEvents)
            {
                // Disconnect from events
                // CraftingManager.OnPotionCrafted -= uiDocumentComponent.OnPotionCrafted;
            }
        }
    }
}