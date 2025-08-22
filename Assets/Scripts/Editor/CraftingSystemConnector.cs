using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.GameSystems.AlchemyMenu;

namespace FourFatesStudios.ProjectWarden.Editor
{
    /// <summary>
    /// Tool to automatically connect ItemSlotContainerHolder to crafting controllers
    /// </summary>
    public class CraftingSystemConnector : EditorWindow
    {
        [MenuItem("Tools/Connect Crafting System Components")]
        public static void ShowWindow()
        {
            GetWindow<CraftingSystemConnector>("Crafting System Connector");
        }

        private void OnGUI()
        {
            GUILayout.Label("Crafting System Component Connector", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("This tool works in both Edit Mode and Play Mode", MessageType.Info);
            }

            EditorGUILayout.HelpBox("🔧 This tool will automatically connect ItemSlotContainerHolder to crafting controllers", MessageType.None);

            // Find components
            var inventoryHolder = Object.FindFirstObjectByType<ItemSlotContainerHolder>();
            var potionController = Object.FindFirstObjectByType<PotionCraftingController>();
            var bulkController = Object.FindFirstObjectByType<BulkCraftingController>();

            GUILayout.Label("Found Components:", EditorStyles.boldLabel);
            
            string inventoryStatus = inventoryHolder != null ? $"✅ Found: {inventoryHolder.gameObject.name}" : "❌ Not found";
            EditorGUILayout.LabelField($"ItemSlotContainerHolder: {inventoryStatus}");
            
            string potionStatus = potionController != null ? $"✅ Found: {potionController.gameObject.name}" : "❌ Not found";
            EditorGUILayout.LabelField($"PotionCraftingController: {potionStatus}");
            
            string bulkStatus = bulkController != null ? $"✅ Found: {bulkController.gameObject.name}" : "❌ Not found";
            EditorGUILayout.LabelField($"BulkCraftingController: {bulkStatus}");

            GUILayout.Space(10);

            if (inventoryHolder == null)
            {
                EditorGUILayout.HelpBox("❌ No ItemSlotContainerHolder found! Create one first.", MessageType.Error);
                
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Create ItemSlotContainerHolder"))
                {
                    CreateInventoryHolder();
                    inventoryHolder = Object.FindFirstObjectByType<ItemSlotContainerHolder>();
                }
                GUI.backgroundColor = Color.white;
                
                GUILayout.Space(10);
            }

            bool canConnect = inventoryHolder != null && (potionController != null || bulkController != null);
            
            if (canConnect)
            {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("🔗 Connect All Components", GUILayout.Height(40)))
                {
                    ConnectComponents(inventoryHolder, potionController, bulkController);
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                EditorGUILayout.HelpBox("❌ Cannot connect - missing required components", MessageType.Warning);
            }

            GUILayout.Space(10);

            // Individual connection buttons
            if (inventoryHolder != null && potionController != null)
            {
                if (GUILayout.Button("Connect Potion Crafting Only"))
                {
                    ConnectPotionCrafting(inventoryHolder, potionController);
                }
            }

            if (inventoryHolder != null && bulkController != null)
            {
                if (GUILayout.Button("Connect Bulk Crafting Only"))
                {
                    ConnectBulkCrafting(inventoryHolder, bulkController);
                }
            }

            GUILayout.Space(10);
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("🔍 Verify Connections"))
            {
                VerifyConnections();
            }
            GUI.backgroundColor = Color.white;
        }

        private void CreateInventoryHolder()
        {
            var go = new GameObject("Inventory");
            go.layer = LayerMask.NameToLayer("UI");
            
            var holder = go.AddComponent<ItemSlotContainerHolder>();
            
            // Inventory holder created - can be configured in inspector later
            
            Debug.Log("✅ Created ItemSlotContainerHolder on new Inventory GameObject");
            
            Selection.activeGameObject = go;
        }

        private void ConnectComponents(ItemSlotContainerHolder inventory, PotionCraftingController potionController, BulkCraftingController bulkController)
        {
            int connectionsAdded = 0;

            if (potionController != null)
            {
                ConnectPotionCrafting(inventory, potionController);
                connectionsAdded++;
            }

            if (bulkController != null)
            {
                ConnectBulkCrafting(inventory, bulkController);
                connectionsAdded++;
            }

            Debug.Log($"✅ Connected {connectionsAdded} crafting controller(s) to inventory holder");
            
            EditorUtility.DisplayDialog("Connections Complete!", 
                $"Successfully connected {connectionsAdded} crafting controller(s) to the inventory holder.\n\nYour crafting system should now work properly!", 
                "OK");
        }

        private void ConnectPotionCrafting(ItemSlotContainerHolder inventory, PotionCraftingController controller)
        {
            // Use reflection to set the private field
            var field = typeof(PotionCraftingController).GetField("ingredientInventoryHolder", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(controller, inventory);
                EditorUtility.SetDirty(controller);
                Debug.Log($"🔗 Connected PotionCraftingController to {inventory.gameObject.name}");
            }
            else
            {
                Debug.LogError("❌ Could not find ingredientInventoryHolder field in PotionCraftingController");
            }
        }

        private void ConnectBulkCrafting(ItemSlotContainerHolder inventory, BulkCraftingController controller)
        {
            // Use reflection to set the private field
            var field = typeof(BulkCraftingController).GetField("ingredientInventoryHolder", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(controller, inventory);
                EditorUtility.SetDirty(controller);
                Debug.Log($"🔗 Connected BulkCraftingController to {inventory.gameObject.name}");
            }
            else
            {
                Debug.LogError("❌ Could not find ingredientInventoryHolder field in BulkCraftingController");
            }
        }

        private void VerifyConnections()
        {
            Debug.Log("🔍 Verifying crafting system connections...");
            
            var inventoryHolder = Object.FindFirstObjectByType<ItemSlotContainerHolder>();
            var potionController = Object.FindFirstObjectByType<PotionCraftingController>();
            var bulkController = Object.FindFirstObjectByType<BulkCraftingController>();

            if (inventoryHolder == null)
            {
                Debug.LogError("❌ No ItemSlotContainerHolder found");
                return;
            }

            Debug.Log($"✅ ItemSlotContainerHolder found: {inventoryHolder.gameObject.name}");

            if (potionController != null)
            {
                var field = typeof(PotionCraftingController).GetField("ingredientInventoryHolder", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var connectedInventory = field?.GetValue(potionController) as ItemSlotContainerHolder;
                
                if (connectedInventory == inventoryHolder)
                    Debug.Log("✅ PotionCraftingController properly connected");
                else
                    Debug.LogWarning("⚠️ PotionCraftingController not connected or connected to wrong inventory");
            }

            if (bulkController != null)
            {
                var field = typeof(BulkCraftingController).GetField("ingredientInventoryHolder", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var connectedInventory = field?.GetValue(bulkController) as ItemSlotContainerHolder;
                
                if (connectedInventory == inventoryHolder)
                    Debug.Log("✅ BulkCraftingController properly connected");
                else
                    Debug.LogWarning("⚠️ BulkCraftingController not connected or connected to wrong inventory");
            }

            Debug.Log("🔍 Connection verification complete!");
        }

        [MenuItem("Tools/Quick Connect Crafting System", false, 150)]
        public static void QuickConnect()
        {
            var inventoryHolder = Object.FindFirstObjectByType<ItemSlotContainerHolder>();
            var potionController = Object.FindFirstObjectByType<PotionCraftingController>();
            var bulkController = Object.FindFirstObjectByType<BulkCraftingController>();

            if (inventoryHolder == null)
            {
                EditorUtility.DisplayDialog("Missing Inventory", "No ItemSlotContainerHolder found. Create one first using Tools → Connect Crafting System Components", "OK");
                return;
            }

            int connected = 0;

            if (potionController != null)
            {
                var field = typeof(PotionCraftingController).GetField("ingredientInventoryHolder", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(potionController, inventoryHolder);
                EditorUtility.SetDirty(potionController);
                connected++;
            }

            if (bulkController != null)
            {
                var field = typeof(BulkCraftingController).GetField("ingredientInventoryHolder", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(bulkController, inventoryHolder);
                EditorUtility.SetDirty(bulkController);
                connected++;
            }

            if (connected > 0)
            {
                Debug.Log($"⚡ Quick Connect: Connected {connected} crafting controller(s)");
                EditorUtility.DisplayDialog("Quick Connect Complete!", 
                    $"Connected {connected} crafting controller(s) to inventory.\n\nYour crafting system is now ready!", 
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Nothing to Connect", "No crafting controllers found to connect.", "OK");
            }
        }
    }
}