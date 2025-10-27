using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden;

namespace FourFatesStudios.ProjectWarden.Editor
{
    public class InventorySystemTool : EditorWindow
    {
        [MenuItem("Tools/Project Warden/Add Inventory System")]
        public static void ShowWindow()
        {
            var window = GetWindow<InventorySystemTool>("Inventory System Tool");
            window.Show();
        }

        [MenuItem("Tools/Project Warden/Quick Add Inventory System")]
        public static void QuickAddInventorySystem()
        {
            AddInventorySystemToScene();
        }

        private void OnGUI()
        {
            GUILayout.Label("Inventory System Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Check current state
            var existingHolder = FindFirstObjectByType<ItemSlotContainerHolder>();
            var existingVisualizer = FindFirstObjectByType<FourFatesStudios.ProjectWarden.InventorySlotVisualizer>();

            if (existingHolder != null)
            {
                EditorGUILayout.HelpBox("✅ ItemSlotContainerHolder already exists in scene", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("❌ ItemSlotContainerHolder not found in scene", MessageType.Warning);
            }

            if (existingVisualizer != null)
            {
                EditorGUILayout.HelpBox("✅ InventorySlotVisualizer already exists in scene", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("❌ InventorySlotVisualizer not found in scene", MessageType.Warning);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Add Complete Inventory System", GUILayout.Height(30)))
            {
                AddInventorySystemToScene();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Add Only ItemSlotContainerHolder"))
            {
                AddInventoryHolder();
            }

            if (GUILayout.Button("Add Only InventorySlotVisualizer"))
            {
                AddInventoryVisualizer();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Test Add Serpent's Dew"))
            {
                TestAddPotion();
            }
        }

        public static void AddInventorySystemToScene()
        {
            Debug.Log("🎒 Creating Inventory System...");

            // Create main inventory GameObject
            GameObject inventoryRoot = new GameObject("Inventory System");
            
            // Add ItemSlotContainerHolder
            var holder = inventoryRoot.AddComponent<ItemSlotContainerHolder>();
            Debug.Log("✅ Added ItemSlotContainerHolder");

            // Create visualizer GameObject
            GameObject visualizerGO = new GameObject("Inventory UI Visualizer");
            visualizerGO.transform.SetParent(inventoryRoot.transform);

            // Add UIDocument component
            var uiDocument = visualizerGO.AddComponent<UnityEngine.UIElements.UIDocument>();

            // Try to load the inventory UI asset
            var inventoryUIAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>("Assets/UI Toolkit/InventorySlots_UI.uxml");
            if (inventoryUIAsset != null)
            {
                uiDocument.visualTreeAsset = inventoryUIAsset;
                Debug.Log("✅ Loaded InventorySlots_UI.uxml");
            }
            else
            {
                Debug.LogWarning("⚠️ Could not find InventorySlots_UI.uxml at Assets/UI Toolkit/InventorySlots_UI.uxml");
            }

            // Add the visualizer component
            var visualizer = visualizerGO.AddComponent<FourFatesStudios.ProjectWarden.InventorySlotVisualizer>();
            Debug.Log("✅ Added InventorySlotVisualizer");

            // Mark scene as dirty
            EditorUtility.SetDirty(inventoryRoot);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("🎉 Inventory System created successfully!");

            // Select the created object
            Selection.activeGameObject = inventoryRoot;
        }

        private static void AddInventoryHolder()
        {
            var existing = FindFirstObjectByType<ItemSlotContainerHolder>();
            if (existing != null)
            {
                Debug.LogWarning("ItemSlotContainerHolder already exists!");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            GameObject holderGO = new GameObject("Inventory Holder");
            holderGO.AddComponent<ItemSlotContainerHolder>();
            
            EditorUtility.SetDirty(holderGO);
            Selection.activeGameObject = holderGO;
            
            Debug.Log("✅ Added ItemSlotContainerHolder");
        }

        private static void AddInventoryVisualizer()
        {
            var existing = FindFirstObjectByType<FourFatesStudios.ProjectWarden.InventorySlotVisualizer>();
            if (existing != null)
            {
                Debug.LogWarning("InventorySlotVisualizer already exists!");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            GameObject visualizerGO = new GameObject("Inventory Visualizer");
            
            // Add UIDocument
            var uiDocument = visualizerGO.AddComponent<UnityEngine.UIElements.UIDocument>();
            var inventoryUIAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>("Assets/UI Toolkit/InventorySlots_UI.uxml");
            if (inventoryUIAsset != null)
            {
                uiDocument.visualTreeAsset = inventoryUIAsset;
            }

            // Add visualizer
            visualizerGO.AddComponent<FourFatesStudios.ProjectWarden.InventorySlotVisualizer>();
            
            EditorUtility.SetDirty(visualizerGO);
            Selection.activeGameObject = visualizerGO;
            
            Debug.Log("✅ Added InventorySlotVisualizer");
        }

        private static void TestAddPotion()
        {
            var holder = FindFirstObjectByType<ItemSlotContainerHolder>();
            if (holder == null)
            {
                Debug.LogError("❌ No ItemSlotContainerHolder found! Add it first.");
                return;
            }

            // Try to find Serpent's Dew
            var serpentsDew = AssetDatabase.LoadAssetAtPath<FourFatesStudios.ProjectWarden.ScriptableObjects.Items.Potion>("Assets/Resources/Items/Potions/Serpent's Dew.asset");
            if (serpentsDew != null)
            {
                if (Application.isPlaying)
                {
                    holder.AddItem(serpentsDew, 1);
                    Debug.Log("🧪 Added Serpent's Dew to inventory!");
                }
                else
                {
                    Debug.LogWarning("⚠️ Can only test adding potions during Play Mode");
                }
            }
            else
            {
                Debug.LogError("❌ Could not find Serpent's Dew potion");
            }
        }
    }
}