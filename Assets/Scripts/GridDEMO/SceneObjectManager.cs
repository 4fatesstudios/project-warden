using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using FourFatesStudios.ProjectWarden.GridDemo;
using FourFatesStudios.ProjectWarden.UI;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Manages and coordinates scene objects in the GridDemo scene.
    /// Similar to CraftingModeSelector but focuses on runtime scene management.
    /// </summary>
    public class SceneObjectManager : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private bool autoFindReferences = true;
        [SerializeField] private bool enableDebugLogging = true;

        // Core scene objects - automatically found or manually assigned
        private CraftingUIController craftingUIController;
        private GridCraftingManager gridManager;
        private UIDocument craftingUIDocument;
        private GameObject inventoryObject;
        private UnityEngine.Camera mainCamera;

        // Scene state
        private Dictionary<string, GameObject> sceneObjects = new Dictionary<string, GameObject>();
        private List<MonoBehaviour> managedComponents = new List<MonoBehaviour>();

        public enum SceneState
        {
            Loading,
            Ready,
            Crafting,
            Inventory,
            Paused
        }

        private SceneState currentState = SceneState.Loading;

        #region Unity Lifecycle

        void Awake()
        {
            if (autoFindReferences)
            {
                FindSceneReferences();
            }
            
            InitializeSceneObjects();
        }

        void Start()
        {
            SetupSceneObjectRelationships();
            currentState = SceneState.Ready;
            
            if (enableDebugLogging)
                Debug.Log("🎮 SceneObjectManager: Scene ready");
        }

        #endregion

        #region Scene Object Discovery

        /// <summary>
        /// Automatically find and cache all important scene objects
        /// </summary>
        private void FindSceneReferences()
        {
            if (enableDebugLogging)
                Debug.Log("🔍 SceneObjectManager: Finding scene references...");

            // Find core components
            craftingUIController = FindFirstObjectByType<CraftingUIController>();
            gridManager = FindFirstObjectByType<GridCraftingManager>();
            craftingUIDocument = FindFirstObjectByType<UIDocument>();
            mainCamera = FindFirstObjectByType<UnityEngine.Camera>();

            // Find by GameObject names (fallback method)
            var inventoryGO = GameObject.Find("Inventory");
            if (inventoryGO != null) inventoryObject = inventoryGO;

            // Cache all scene objects by name
            CacheAllSceneObjects();

            if (enableDebugLogging)
            {
                Debug.Log($"✅ Found CraftingUIController: {craftingUIController != null}");
                Debug.Log($"✅ Found GridCraftingManager: {gridManager != null}");
                Debug.Log($"✅ Found UIDocument: {craftingUIDocument != null}");
                Debug.Log($"✅ Found Inventory: {inventoryObject != null}");
                Debug.Log($"✅ Found Camera: {mainCamera != null}");
                Debug.Log($"📦 Cached {sceneObjects.Count} scene objects");
            }
        }

        /// <summary>
        /// Cache all GameObjects in the scene for quick access
        /// </summary>
        private void CacheAllSceneObjects()
        {
            sceneObjects.Clear();
            
            // Get all GameObjects in the scene
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            
            foreach (var obj in allObjects)
            {
                // Only cache root objects to avoid duplicates
                if (obj.transform.parent == null)
                {
                    sceneObjects[obj.name] = obj;
                    
                    // Also cache child objects with full path
                    CacheChildObjects(obj, obj.name);
                }
            }
        }

        /// <summary>
        /// Recursively cache child objects with their full hierarchy path
        /// </summary>
        private void CacheChildObjects(GameObject parent, string parentPath)
        {
            foreach (Transform child in parent.transform)
            {
                string fullPath = $"{parentPath}/{child.name}";
                sceneObjects[fullPath] = child.gameObject;
                
                // Recursively cache children
                if (child.childCount > 0)
                {
                    CacheChildObjects(child.gameObject, fullPath);
                }
            }
        }

        #endregion

        #region Scene Object Management

        /// <summary>
        /// Initialize scene objects and their relationships
        /// </summary>
        private void InitializeSceneObjects()
        {
            // Collect all MonoBehaviour components for management
            managedComponents.Clear();
            
            if (craftingUIController != null) managedComponents.Add(craftingUIController);
            if (gridManager != null) managedComponents.Add(gridManager);

            // Find additional components
            var itemHolders = FindObjectsByType<FourFatesStudios.ProjectWarden.ItemSlotContainerHolder>(FindObjectsSortMode.None);
            managedComponents.AddRange(itemHolders);

            if (enableDebugLogging)
                Debug.Log($"🎮 Managing {managedComponents.Count} components");
        }

        /// <summary>
        /// Setup relationships between scene objects
        /// </summary>
        private void SetupSceneObjectRelationships()
        {
            // Ensure CraftingUIController has proper references
            if (craftingUIController != null)
            {
                if (craftingUIController.gridManager == null && gridManager != null)
                {
                    craftingUIController.gridManager = gridManager;
                    if (enableDebugLogging)
                        Debug.Log("🔗 Connected CraftingUIController to GridManager");
                }

                if (craftingUIController.uiDocument == null && craftingUIDocument != null)
                {
                    craftingUIController.uiDocument = craftingUIDocument;
                    if (enableDebugLogging)
                        Debug.Log("🔗 Connected CraftingUIController to UIDocument");
                }
            }
        }

        #endregion

        #region Public API - Scene Object Access

        /// <summary>
        /// Get a GameObject by name (supports hierarchy paths)
        /// </summary>
        public GameObject GetSceneObject(string name)
        {
            sceneObjects.TryGetValue(name, out GameObject obj);
            return obj;
        }

        /// <summary>
        /// Get a component from a scene object
        /// </summary>
        public T GetSceneComponent<T>(string objectName) where T : Component
        {
            var obj = GetSceneObject(objectName);
            return obj?.GetComponent<T>();
        }

        /// <summary>
        /// Get all objects with a specific component type
        /// </summary>
        public List<T> GetAllComponentsOfType<T>() where T : Component
        {
            return FindObjectsByType<T>(FindObjectsSortMode.None).ToList();
        }

        /// <summary>
        /// Find objects by tag
        /// </summary>
        public List<GameObject> GetObjectsByTag(string tag)
        {
            return GameObject.FindGameObjectsWithTag(tag).ToList();
        }

        /// <summary>
        /// Get the current state of the scene
        /// </summary>
        public SceneState GetCurrentState()
        {
            return currentState;
        }

        /// <summary>
        /// Set the scene state
        /// </summary>
        public void SetSceneState(SceneState newState)
        {
            var oldState = currentState;
            currentState = newState;
            
            if (enableDebugLogging)
                Debug.Log($"🎮 Scene state changed: {oldState} → {newState}");
        }

        #endregion

        #region Public API - Scene Control

        /// <summary>
        /// Enable/disable specific scene objects
        /// </summary>
        public void SetObjectActive(string objectName, bool active)
        {
            var obj = GetSceneObject(objectName);
            if (obj != null)
            {
                obj.SetActive(active);
                if (enableDebugLogging)
                    Debug.Log($"🎮 {objectName} set to {(active ? "active" : "inactive")}");
            }
        }

        /// <summary>
        /// Reset the scene to initial state
        /// </summary>
        public void ResetScene()
        {
            if (enableDebugLogging)
                Debug.Log("🔄 Resetting scene to initial state...");

            // Reset grid if available
            if (gridManager != null)
            {
                gridManager.ClearGrid();
            }

            // Reset UI state
            if (craftingUIController != null)
            {
                // Reset UI through reflection or public methods
                // This depends on your CraftingUIController implementation
            }

            currentState = SceneState.Ready;
            
            if (enableDebugLogging)
                Debug.Log("✅ Scene reset complete");
        }

        /// <summary>
        /// Refresh all scene object references (useful after instantiation)
        /// </summary>
        public void RefreshSceneReferences()
        {
            FindSceneReferences();
            SetupSceneObjectRelationships();
            
            if (enableDebugLogging)
                Debug.Log("🔄 Scene references refreshed");
        }

        #endregion

        #region Debug Tools

        /// <summary>
        /// Print information about all scene objects
        /// </summary>
        [ContextMenu("Debug Scene Objects")]
        public void DebugSceneObjects()
        {
            Debug.Log("🔍 === Scene Object Debug ===");
            Debug.Log($"Scene State: {currentState}");
            Debug.Log($"Cached Objects: {sceneObjects.Count}");
            Debug.Log($"Managed Components: {managedComponents.Count}");
            
            Debug.Log("--- Root Scene Objects ---");
            foreach (var kvp in sceneObjects.Where(x => !x.Key.Contains("/")))
            {
                var active = kvp.Value?.activeInHierarchy ?? false;
                Debug.Log($"  {kvp.Key}: {(active ? "✅" : "❌")}");
            }
            
            Debug.Log("--- Component Status ---");
            Debug.Log($"  CraftingUIController: {craftingUIController != null}");
            Debug.Log($"  GridCraftingManager: {gridManager != null}");
            Debug.Log($"  UIDocument: {craftingUIDocument != null}");
            Debug.Log($"  Inventory: {inventoryObject != null}");
            Debug.Log($"  Main Camera: {mainCamera != null}");
            
            Debug.Log("🔍 === End Debug ===");
        }

        /// <summary>
        /// List all available MonoBehaviour components in the scene
        /// </summary>
        [ContextMenu("List Scene Components")]
        public void ListSceneComponents()
        {
            Debug.Log("🔍 === Scene Components ===");
            
            var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            var componentGroups = allMonoBehaviours
                .GroupBy(mb => mb.GetType().Name)
                .OrderBy(g => g.Key);
                
            foreach (var group in componentGroups)
            {
                Debug.Log($"  {group.Key}: {group.Count()} instances");
                foreach (var component in group.Take(3)) // Show first 3 instances
                {
                    Debug.Log($"    - {component.gameObject.name}");
                }
                if (group.Count() > 3)
                {
                    Debug.Log($"    ... and {group.Count() - 3} more");
                }
            }
            
            Debug.Log("🔍 === End Components ===");
        }

        #endregion
    }
}