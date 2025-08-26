using FourFatesStudios.ProjectWarden;
using FourFatesStudios.ProjectWarden.GameSystems.AlchemyMenu;
using FourFatesStudios.ProjectWarden.GameSystems.RefinementMenu;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.Setup;
using GameSystems.CraftingMenu.RefinementMenu;
using Setup;
using UnityEngine;
using UnityEngine.UIElements;

namespace Demo
{
    /// <summary>
    /// Complete setup guide and automation for the crafting system demo
    /// This script will walk you through setting up a working crafting system
    /// </summary>
    public class CraftingSystemSetupGuide : MonoBehaviour
    {
        [Header("Auto-Setup Options")]
        [SerializeField] private bool autoCreateUIDocuments = true;
        [SerializeField] private bool autoSetupDemo = true;
        
        [Header("Setup Status")]
        [SerializeField] private bool uiDocumentsCreated;
        [SerializeField] private bool ingredientsCreated;
        [SerializeField] private bool controllersSetup;
        [SerializeField] private bool demoReady;

        private void Start()
        {
            // Check initial status
            CheckSetupStatus();
            
            if (autoSetupDemo)
            {
                PerformFullSetup();
            }
            else
            {
                LogSetupInstructions();
            }
        }

        private void CheckSetupStatus()
        {
            uiDocumentsCreated = CheckUIDocuments();
            ingredientsCreated = CheckIngredients();
            controllersSetup = CheckControllers();
            demoReady = uiDocumentsCreated && ingredientsCreated && controllersSetup;

            if (demoReady)
            {
                Debug.Log("✅ Demo system is ready!");
            }
        }

        private bool CheckUIDocuments()
        {
            return autoCreateUIDocuments && FindObjectsByType<UIDocument>(FindObjectsSortMode.None).Length > 0;
        }

        private bool CheckIngredients()
        {
            return FindFirstObjectByType<ItemSlotContainerHolder>() != null;
        }

        private bool CheckControllers()
        {
            return controllersSetup || FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Length > 5;
        }

        [ContextMenu("Perform Full Setup")]
        public void PerformFullSetup()
        {
            Debug.Log("=== CRAFTING SYSTEM SETUP STARTING ===");
            
            SetupUIHierarchy();
            CreateDemoIngredients();
            SetupControllers();
            ValidateSetup();
            
            Debug.Log("=== CRAFTING SYSTEM SETUP COMPLETE ===");
            LogTestInstructions();
        }

        private void SetupUIHierarchy()
        {
            Debug.Log("Setting up UI hierarchy...");
            
            // Find or create main crafting system
            var craftingSystem = GameObject.Find("CraftingMenuSystem");
            if (craftingSystem == null)
            {
                craftingSystem = new GameObject("CraftingMenuSystem");
                craftingSystem.transform.SetParent(transform);
                
                // Add the main setup component
                if (craftingSystem.GetComponent<CraftingMenuDemoSetup>() == null)
                {
                    craftingSystem.AddComponent<CraftingMenuDemoSetup>();
                }
            }
            
            // Create UI Documents
            CreateUIDocument("PotionCraftingUI", craftingSystem.transform, "Assets/Scripts/UI/CraftingSystem/AlchemySystem/PotionCrafting.uxml");
            CreateUIDocument("GridMinigameUI", craftingSystem.transform, "Assets/Scripts/UI/CraftingSystem/GridMinigame.uxml");
            CreateUIDocument("BulkCraftingUI", craftingSystem.transform, "Assets/Scripts/UI/CraftingSystem/AlchemySystem/BulkCrafting.uxml");
            
            // Create refinement UI parent
            var refinementParent = CreateUIDocument("RefinementUI", craftingSystem.transform, null);
            CreateUIDocument("RoastingMinigameUI", refinementParent.transform, "Assets/Scripts/UI/CraftingSystem/RefinementSystem/RoastingMinigame.uxml");
            CreateUIDocument("DistillationMinigameUI", refinementParent.transform, "Assets/Scripts/UI/CraftingSystem/RefinementSystem/DistillationMinigame.uxml");
            CreateUIDocument("GrindingMinigameUI", refinementParent.transform, "Assets/Scripts/UI/CraftingSystem/RefinementSystem/GrindingMinigame.uxml");
            
            // Create demo inventory
            var inventoryGo = new GameObject("DemoInventory");
            inventoryGo.transform.SetParent(craftingSystem.transform);
            
            var inventoryHolder = inventoryGo.GetComponent<FourFatesStudios.ProjectWarden.ItemSlotContainerHolder>();
            if (inventoryHolder == null)
            {
                inventoryHolder = inventoryGo.AddComponent<FourFatesStudios.ProjectWarden.ItemSlotContainerHolder>();
            }
            
            uiDocumentsCreated = true;
            Debug.Log("✓ UI hierarchy created successfully");
        }

        private GameObject CreateUIDocument(string docName, Transform parent, string uxml)
        {
            var go = new GameObject(docName);
            go.transform.SetParent(parent);
            
            var uiDoc = go.AddComponent<UIDocument>();
            
            if (!string.IsNullOrEmpty(uxml))
            {
                // Try to load the UXML asset
                var uiAsset = Resources.Load<VisualTreeAsset>(uxml.Replace("Assets/Resources/", "").Replace(".uxml", ""));
                if (uiAsset == null)
                {
                    Debug.LogWarning($"Could not load UXML at {uxml} for {docName}. Please assign manually in inspector.");
                }
                else
                {
                    uiDoc.visualTreeAsset = uiAsset;
                }
            }
            
            return go;
        }

        private void CreateDemoIngredients()
        {
            Debug.Log("Creating demo ingredients...");
            
            var creator = FindFirstObjectByType<DemoIngredientCreator>();
            if (creator == null)
            {
                var creatorGo = new GameObject("DemoIngredientCreator");
                creatorGo.transform.SetParent(transform);
                creator = creatorGo.AddComponent<DemoIngredientCreator>();
            }
            
            creator.CreateDemoIngredients();
            ingredientsCreated = true;
            
            Debug.Log("✓ Demo ingredients created");
        }

        private void SetupControllers()
        {
            Debug.Log("Setting up controllers...");
            
            var craftingSystem = GameObject.Find("CraftingMenuSystem");
            if (craftingSystem == null)
            {
                Debug.LogError("CraftingMenuSystem not found! Run SetupUIHierarchy first.");
                return;
            }
            
            // Setup PotionCraftingController
            var potionUI = GameObject.Find("PotionCraftingUI");
            if (potionUI != null)
            {
                var potionController = potionUI.GetComponent<PotionCraftingController>();
                if (potionController == null)
                {
                    potionController = potionUI.AddComponent<PotionCraftingController>();
                }
                potionController.uiDocument = potionUI.GetComponent<UIDocument>();
            }
            
            // Setup GridMinigameController
            var gridUI = GameObject.Find("GridMinigameUI");
            if (gridUI != null)
            {
                var gridController = gridUI.GetComponent<GridMinigameController>();
                if (gridController == null)
                {
                    gridController = gridUI.AddComponent<GridMinigameController>();
                }
            }
            
            // Setup RefinementMinigameManager
            var refinementUI = GameObject.Find("RefinementUI");
            if (refinementUI != null)
            {
                var refinementManager = refinementUI.GetComponent<RefinementMinigameManager>();
                if (refinementManager == null)
                {
                    refinementManager = refinementUI.AddComponent<RefinementMinigameManager>();
                }
            }
            
            // Setup individual refinement controllers
            SetupRefinementController<RoastingMinigameController>("RoastingMinigameUI");
            SetupRefinementController<DistillationMinigameController>("DistillationMinigameUI");
            SetupRefinementController<GrindingMinigameController>("GrindingMinigameUI");
            
            controllersSetup = true;
            Debug.Log("✓ Controllers setup complete");
        }

        private void SetupRefinementController<T>(string gameObjectName) where T : MonoBehaviour
        {
            var go = GameObject.Find(gameObjectName);
            if (go != null)
            {
                var controller = go.GetComponent<T>();
                if (controller == null)
                {
                    go.AddComponent<T>();
                }
            }
        }

        private void ValidateSetup()
        {
            Debug.Log("Validating setup...");
            
            bool allValid = true;
            
            // Check UI Documents
            if (GameObject.Find("PotionCraftingUI") == null)
            {
                Debug.LogError("✗ PotionCraftingUI not found");
                allValid = false;
            }
            
            // Check Controllers
            var potionController = FindFirstObjectByType<PotionCraftingController>();
            if (potionController == null)
            {
                Debug.LogError("✗ PotionCraftingController not found");
                allValid = false;
            }
            
            // Check Inventory
            var inventory = FindFirstObjectByType<FourFatesStudios.ProjectWarden.ItemSlotContainerHolder>();
            if (inventory == null)
            {
                Debug.LogError("✗ ItemSlotContainerHolder not found");
                allValid = false;
            }
            
            demoReady = allValid;
            
            if (allValid)
            {
                Debug.Log("✓ All systems validated successfully");
            }
            else
            {
                Debug.LogWarning("⚠ Some systems failed validation - check errors above");
            }
        }

        private void LogSetupInstructions()
        {
            Debug.Log("=== MANUAL SETUP INSTRUCTIONS ===");
            Debug.Log("1. Attach this script to an empty GameObject in your scene");
            Debug.Log("2. Run 'Perform Full Setup' from the context menu or check 'Auto Setup Demo'");
            Debug.Log("3. Assign UXML files to UIDocument components in inspector");
            Debug.Log("4. Add demo ingredients to the inventory holder");
            Debug.Log("5. Press Play and test with number keys 1-4");
            Debug.Log("================================");
        }

        private void LogTestInstructions()
        {
            Debug.Log("=== DEMO TEST INSTRUCTIONS ===");
            Debug.Log("✓ Setup complete! You can now:");
            Debug.Log("1. Press PLAY to start the demo");
            Debug.Log("2. Use number keys to switch menus:");
            Debug.Log("   • 1 = Potion Crafting");
            Debug.Log("   • 2 = Refinement");
            Debug.Log("   • 3 = Bulk Crafting");
            Debug.Log("   • 4 = Alchemy Book");
            Debug.Log("3. Click ingredient slots to select ingredients");
            Debug.Log("4. Try manual crafting to launch grid minigame");
            Debug.Log("5. Test refinement minigames from refinement menu");
            Debug.Log("===============================");
            
            if (!demoReady)
            {
                Debug.LogWarning("⚠ Demo setup incomplete - some features may not work");
            }
        }

        // Editor helper methods
#if UNITY_EDITOR
        [ContextMenu("Create Demo Scene")]
        public void CreateDemoScene()
        {
            // Scene creation instructions since we can't create .unity files directly
            Debug.Log("=== DEMO SCENE CREATION GUIDE ===");
            Debug.Log("1. Create new scene: File -> New Scene");
            Debug.Log("2. Save as: CraftingMenuDemo.unity");
            Debug.Log("3. Delete default objects (keep Main Camera)");
            Debug.Log("4. Create empty GameObject named 'CraftingSystemSetup'");
            Debug.Log("5. Add this CraftingSystemSetupGuide script to it");
            Debug.Log("6. Run 'Perform Full Setup' from context menu");
            Debug.Log("7. Press Play to test!");
            Debug.Log("=================================");
        }

        [ContextMenu("Assign Demo Ingredients")]
        public void AssignDemoIngredients()
        {
            var inventory = FindFirstObjectByType<FourFatesStudios.ProjectWarden.ItemSlotContainerHolder>();
            if (inventory == null)
            {
                Debug.LogError("No ItemSlotContainerHolder found! Run setup first.");
                return;
            }
            
            // Load demo ingredients from Resources
            var ingredients = Resources.LoadAll<Ingredient>("Demo/Ingredients");
            
            if (ingredients.Length == 0)
            {
                Debug.LogWarning("No demo ingredients found! Run CreateDemoIngredients first.");
                return;
            }
            
            // Add ingredients to inventory
            foreach (var ingredient in ingredients)
            {
                inventory.AddItem(ingredient, 10);
            }
            
            Debug.Log($"✓ Added {ingredients.Length} demo ingredients to inventory");
        }

        [ContextMenu("Reset Demo")]
        public void ResetDemo()
        {
            // Reset status flags
            uiDocumentsCreated = false;
            ingredientsCreated = false;
            controllersSetup = false;
            demoReady = false;
            
            Debug.Log("Demo status reset. Run 'Perform Full Setup' to recreate.");
        }
#endif
    }
}