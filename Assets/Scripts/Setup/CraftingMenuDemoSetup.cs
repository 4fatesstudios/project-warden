using UnityEngine;
using UnityEngine.UIElements;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using FourFatesStudios.ProjectWarden.GameSystems.AlchemyMenu;
using FourFatesStudios.ProjectWarden.GameSystems.RefinementMenu;

namespace FourFatesStudios.ProjectWarden.Setup
{
    /// <summary>
    /// Master setup script for the crafting menu demo system
    /// This script handles initialization and provides a working demo
    /// </summary>
    public class CraftingMenuDemoSetup : MonoBehaviour
    {
        [Header("UI Documents - Assign in Inspector")]
        [SerializeField] private UIDocument potionCraftingUI;
        [SerializeField] private UIDocument gridMinigameUI;
        [SerializeField] private UIDocument roastingMinigameUI;
        [SerializeField] private UIDocument distillationMinigameUI;
        [SerializeField] private UIDocument grindingMinigameUI;
        [SerializeField] private UIDocument bulkCraftingUI;
        
        [Header("Demo Ingredients - Create in Inspector")]
        [SerializeField] private Ingredient[] demoIngredients;
        
        [Header("Controllers - Auto-assigned")]
        [SerializeField] private PotionCraftingController potionController;
        [SerializeField] private GridMinigameController gridController;
        [SerializeField] private RefinementMinigameManager refinementManager;
        [SerializeField] private ItemSlotContainerHolder inventoryHolder;
        
        [Header("Current Menu State")]
        [SerializeField] private CraftingMenuType currentMenu = CraftingMenuType.PotionCrafting;
        
        public enum CraftingMenuType
        {
            PotionCrafting,
            Refinement,
            BulkCrafting,
            AlchemyBook
        }

        private void Awake()
        {
            Debug.Log("=== CRAFTING MENU DEMO SETUP ===");
            
            // Auto-find components if not assigned
            AutoAssignComponents();
            
            // Initialize all systems
            InitializeSystems();
            
            // Show initial menu
            ShowMenu(currentMenu);
            
            LogSetupInstructions();
        }

        private void AutoAssignComponents()
        {
            // Find controllers in children if not assigned
            if (potionController == null)
                potionController = GetComponentInChildren<PotionCraftingController>();
            
            if (gridController == null)
                gridController = GetComponentInChildren<GridMinigameController>();
            
            if (refinementManager == null)
                refinementManager = GetComponentInChildren<RefinementMinigameManager>();
                
            if (inventoryHolder == null)
                inventoryHolder = GetComponentInChildren<ItemSlotContainerHolder>();
            
            // Create inventory if not found
            if (inventoryHolder == null)
            {
                var inventoryGO = new GameObject("Demo Inventory");
                inventoryGO.transform.SetParent(transform);
                inventoryHolder = inventoryGO.AddComponent<ItemSlotContainerHolder>();
                
                // Add demo ingredients to inventory
                if (demoIngredients != null && demoIngredients.Length > 0)
                {
                    var holder = inventoryHolder.GetComponent<ItemSlotContainerHolder>();
                    // TODO: Set demo items in inspector after creating ItemSlotContainerHolder
                    Debug.Log("Demo inventory created - add demo ingredients in inspector");
                }
            }
        }

        private void InitializeSystems()
        {
            // Initialize alchemy skill system
            if (AlchemySkillSystem.Instance == null)
            {
                var skillSystemGO = new GameObject("AlchemySkillSystem");
                skillSystemGO.transform.SetParent(transform);
                skillSystemGO.AddComponent<AlchemySkillSystem>();
                
                // Give some demo skills for testing
                var skillSystem = AlchemySkillSystem.Instance;
                skillSystem.UnlockSkill("ingredient_refund");
                skillSystem.UnlockSkill("enhanced_grid");
                
                Debug.Log("AlchemySkillSystem initialized with demo skills");
            }
            
            // Initialize controllers
            if (potionController != null && potionCraftingUI != null)
            {
                // Connect UI document
                potionController.uiDocument = potionCraftingUI;
                Debug.Log("PotionCraftingController initialized");
            }
            
            if (gridController != null && gridMinigameUI != null)
            {
                gridController.gameObject.SetActive(false); // Start hidden
                Debug.Log("GridMinigameController initialized");
            }
            
            Debug.Log("All systems initialized successfully");
        }

        public void ShowMenu(CraftingMenuType menuType)
        {
            currentMenu = menuType;
            
            // Hide all menus first
            HideAllMenus();
            
            // Show requested menu
            switch (menuType)
            {
                case CraftingMenuType.PotionCrafting:
                    ShowPotionCrafting();
                    break;
                    
                case CraftingMenuType.Refinement:
                    ShowRefinement();
                    break;
                    
                case CraftingMenuType.BulkCrafting:
                    ShowBulkCrafting();
                    break;
                    
                case CraftingMenuType.AlchemyBook:
                    ShowAlchemyBook();
                    break;
            }
            
            Debug.Log($"Switched to {menuType} menu");
        }

        private void HideAllMenus()
        {
            if (potionCraftingUI != null)
                potionCraftingUI.gameObject.SetActive(false);
                
            if (gridMinigameUI != null)
                gridMinigameUI.gameObject.SetActive(false);
                
            if (roastingMinigameUI != null)
                roastingMinigameUI.gameObject.SetActive(false);
                
            if (distillationMinigameUI != null)
                distillationMinigameUI.gameObject.SetActive(false);
                
            if (grindingMinigameUI != null)
                grindingMinigameUI.gameObject.SetActive(false);
                
            if (bulkCraftingUI != null)
                bulkCraftingUI.gameObject.SetActive(false);
        }

        private void ShowPotionCrafting()
        {
            if (potionCraftingUI != null)
            {
                potionCraftingUI.gameObject.SetActive(true);
                
                if (potionController != null)
                {
                    potionController.enabled = true;
                }
            }
            else
            {
                Debug.LogWarning("PotionCrafting UI not assigned! Please assign in inspector.");
            }
        }

        private void ShowRefinement()
        {
            if (refinementManager != null)
            {
                refinementManager.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("RefinementManager not found! Please set up refinement system.");
            }
        }

        private void ShowBulkCrafting()
        {
            if (bulkCraftingUI != null)
            {
                bulkCraftingUI.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("BulkCrafting UI not assigned! Please assign in inspector.");
            }
        }

        private void ShowAlchemyBook()
        {
            Debug.Log("Alchemy Book feature coming soon!");
        }

        private void LogSetupInstructions()
        {
            Debug.Log("=== SETUP INSTRUCTIONS ===");
            Debug.Log("1. Create empty GameObjects for each UI system");
            Debug.Log("2. Add UIDocument components and assign UXML files");
            Debug.Log("3. Assign UI references in this script's inspector");
            Debug.Log("4. Create demo Ingredient ScriptableObjects");
            Debug.Log("5. Assign demo ingredients to this script");
            Debug.Log("6. Press Play to test the crafting system!");
            Debug.Log("=== QUICK TEST CONTROLS ===");
            Debug.Log("Press 1: Potion Crafting");
            Debug.Log("Press 2: Refinement");  
            Debug.Log("Press 3: Bulk Crafting");
            Debug.Log("Press 4: Alchemy Book");
        }

        // Quick test controls for demo
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                ShowMenu(CraftingMenuType.PotionCrafting);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                ShowMenu(CraftingMenuType.Refinement);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                ShowMenu(CraftingMenuType.BulkCrafting);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                ShowMenu(CraftingMenuType.AlchemyBook);
        }

        // Helper methods for manual setup
        [ContextMenu("Create Demo Ingredients")]
        public void CreateDemoIngredients()
        {
            Debug.Log("Demo ingredients creation guide:");
            Debug.Log("1. Right-click in Project -> Create -> Items -> Ingredient");
            Debug.Log("2. Create: Fire Claw, Fire Talon, Water Droplet, Earth Shard");
            Debug.Log("3. Set their potency levels (Fire Claw=2, Fire Talon=1, etc.)");
            Debug.Log("4. Set grid sizes (most 1x1, some 2x1 or 2x2)");
            Debug.Log("5. Assign them to the demoIngredients array in inspector");
        }

        [ContextMenu("Setup UI Documents")]
        public void SetupUIDocuments()
        {
            Debug.Log("UI Documents setup guide:");
            Debug.Log("1. Create empty GameObjects for each UI");
            Debug.Log("2. Add UIDocument components");
            Debug.Log("3. Assign UXML files:");
            Debug.Log("   - PotionCrafting.uxml");
            Debug.Log("   - GridMinigame.uxml"); 
            Debug.Log("   - RoastingMinigame.uxml");
            Debug.Log("   - DistillationMinigame.uxml");
            Debug.Log("   - GrindingMinigame.uxml");
            Debug.Log("   - BulkCrafting.uxml");
            Debug.Log("4. Assign UI references in this script's inspector");
        }

        [ContextMenu("Test All Systems")]
        public void TestAllSystems()
        {
            Debug.Log("=== SYSTEM TEST ===");
            
            // Test inventory
            if (inventoryHolder != null)
            {
                var itemCount = inventoryHolder.Container?.GetAllItems()?.Count() ?? 0;
                Debug.Log($"✓ Inventory: {itemCount} item types");
            }
            else
            {
                Debug.LogWarning("✗ Inventory not found");
            }
            
            // Test alchemy system
            if (AlchemySkillSystem.Instance != null)
            {
                Debug.Log("✓ AlchemySkillSystem active");
            }
            else
            {
                Debug.LogWarning("✗ AlchemySkillSystem not found");
            }
            
            // Test controllers
            Debug.Log($"✓ PotionController: {(potionController != null ? "Ready" : "Missing")}");
            Debug.Log($"✓ GridController: {(gridController != null ? "Ready" : "Missing")}");
            Debug.Log($"✓ RefinementManager: {(refinementManager != null ? "Ready" : "Missing")}");
            
            Debug.Log("=== END TEST ===");
        }
    }
}

/* 
SCENE SETUP INSTRUCTIONS:

1. CREATE NEW SCENE
   - File -> New Scene
   - Save as "CraftingMenuDemo.unity"

2. ADD MAIN CAMERA
   - Delete default camera if present
   - Create -> Camera
   - Position: (0, 0, -10)

3. CREATE CRAFTING MENU SYSTEM
   - Create empty GameObject: "CraftingMenuSystem"
   - Add this CraftingMenuDemoSetup script
   - Position: (0, 0, 0)

4. CREATE UI HIERARCHY
   Under CraftingMenuSystem, create:
   
   CraftingMenuSystem/
   ├── PotionCraftingUI (GameObject + UIDocument)
   ├── GridMinigameUI (GameObject + UIDocument) 
   ├── RefinementUI/
   │   ├── RoastingMinigameUI (GameObject + UIDocument)
   │   ├── DistillationMinigameUI (GameObject + UIDocument)
   │   └── GrindingMinigameUI (GameObject + UIDocument)
   ├── BulkCraftingUI (GameObject + UIDocument)
   └── DemoInventory (GameObject + ItemSlotContainerHolder)

5. ASSIGN UXML FILES
   - Assign corresponding .uxml files to each UIDocument
   - Found in: Assets/Scripts/UI/CraftingSystem/

6. ADD CONTROLLERS
   - PotionCraftingUI: Add PotionCraftingController
   - GridMinigameUI: Add GridMinigameController
   - RefinementUI: Add RefinementMinigameManager
   - Each refinement UI: Add respective controller scripts

7. CREATE DEMO INGREDIENTS
   - Right-click Project -> Create -> Items -> Ingredient
   - Create at least 4 different ingredients
   - Set potency, grid size, refinement options
   - Assign to CraftingMenuDemoSetup.demoIngredients array

8. CONNECT REFERENCES
   - In CraftingMenuDemoSetup inspector:
     - Assign all UI Document references
     - Assign demo ingredients array
     - Controllers should auto-assign

9. TEST
   - Press Play
   - Use number keys 1-4 to switch between menus
   - Check console for setup status and instructions

*/