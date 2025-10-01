using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Cleanup script to remove duplicate compact sidebars and ensure only one proper sidebar with back button exists
    /// </summary>
    public class CompactSidebarCleanup : MonoBehaviour
    {
        [Header("Cleanup Settings")]
        [SerializeField] private bool cleanupOnStart = true;
        [SerializeField] private bool debugMode = true;

        private void Start()
        {
            if (cleanupOnStart)
            {
                // Add a delay to ensure all UI is created first
                Invoke(nameof(CleanupDuplicateSidebars), 2.0f);
            }
        }

        [ContextMenu("Cleanup Duplicate Sidebars")]
        public void CleanupDuplicateSidebars()
        {
            // DEPRECATED: This functionality is now handled by AutomaticSidebarManager
            Debug.Log("🧹 CompactSidebarCleanup: This cleanup is deprecated - AutomaticSidebarManager handles all sidebar management now!");
            
            // Check if AutomaticSidebarManager exists
            bool foundAutomaticManager = false;
            
            var gridGameManager = FindFirstObjectByType<FourFatesStudios.ProjectWarden.GridDemo.GridGameManager>();
            if (gridGameManager != null)
            {
                var components = gridGameManager.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    if (component.GetType().Name == "AutomaticSidebarManager")
                    {
                        foundAutomaticManager = true;
                        break;
                    }
                }
            }
            
            if (foundAutomaticManager)
            {
                Debug.Log("✅ AutomaticSidebarManager is handling sidebar management automatically.");
            }
            else
            {
                Debug.LogWarning("⚠️ AutomaticSidebarManager not found! It should be added by GridGameManager.");
            }
            /*
            if (debugMode)
                Debug.Log("🧹 CompactSidebarCleanup: Starting cleanup process...");

            // Find all GameObjects named "Compact Sidebar"
            GameObject[] allCompactSidebars = FindAllGameObjectsWithName("Compact Sidebar");
            
            if (debugMode)
                Debug.Log($"🧹 Found {allCompactSidebars.Length} Compact Sidebar GameObjects");

            if (allCompactSidebars.Length <= 1)
            {
                if (debugMode)
                    Debug.Log("✅ No duplicates found, checking for back button...");
                
                if (allCompactSidebars.Length == 1)
                {
                    EnsureBackButtonExists(allCompactSidebars[0]);
                }
                return;
            }

            // Keep the first one, destroy the rest
            GameObject keepSidebar = allCompactSidebars[0];
            for (int i = 1; i < allCompactSidebars.Length; i++)
            {
                if (debugMode)
                    Debug.Log($"🗑️ Destroying duplicate Compact Sidebar #{i}");
                
                DestroyImmediate(allCompactSidebars[i]);
            }

            if (debugMode)
                Debug.Log($"✅ Cleanup complete! Kept 1 sidebar, removed {allCompactSidebars.Length - 1} duplicates");

            // Ensure the remaining sidebar has a back button
            EnsureBackButtonExists(keepSidebar);
            */
        }

        private void EnsureBackButtonExists(GameObject compactSidebar)
        {
            if (debugMode)
                Debug.Log("🔍 Checking if back button exists...");

            // Find Controls Container
            Transform controlsContainer = FindChildRecursive(compactSidebar.transform, "Controls Container");
            
            if (controlsContainer == null)
            {
                if (debugMode)
                    Debug.LogWarning("⚠️ Controls Container not found in sidebar");
                return;
            }

            // Check if back button already exists
            Transform backButton = controlsContainer.Find("Back Button");
            if (backButton != null)
            {
                if (debugMode)
                    Debug.Log("✅ Back button already exists");
                return;
            }

            // Create the back button
            CreateBackButton(controlsContainer.gameObject);
        }

        private void CreateBackButton(GameObject parent)
        {
            if (debugMode)
                Debug.Log("🔧 Creating back button in Controls Container...");

            // Create back button GameObject
            GameObject backButtonObj = new GameObject("Back Button");
            backButtonObj.transform.SetParent(parent.transform, false);

            // Set up RectTransform to match other buttons
            RectTransform buttonRect = backButtonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(0, 45f); // Match other control buttons

            // Add Image for background
            Image buttonImage = backButtonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.4f, 0.4f, 0.4f, 1f); // Gray background

            // Add Button component
            Button button = backButtonObj.AddComponent<Button>();

            // Create text child
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(backButtonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // Add text component
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "← BACK TO MENU";
            buttonText.fontSize = 12f;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            buttonText.fontStyle = FontStyles.Bold;

            // Set up button functionality
            button.onClick.AddListener(() => {
                if (debugMode)
                    Debug.Log("⬅️ Back button clicked! Returning to CraftingModeSelector...");

                // Ensure proficiency display remains visible
                var proficiencyManager = FindFirstObjectByType<FourFatesStudios.ProjectWarden.GridDemo.UI.ProficiencyDisplayManager>();
                if (proficiencyManager != null)
                {
                    // Keep the proficiency manager active so the potion list stays visible
                    proficiencyManager.gameObject.SetActive(true);
                    if (debugMode)
                        Debug.Log("✅ Proficiency display kept active");
                }

                // Hide GridDemo UI
                GameObject gridDemoUI = GameObject.Find("GridDemo UI");
                if (gridDemoUI != null)
                {
                    gridDemoUI.SetActive(false);
                    if (debugMode)
                        Debug.Log("✅ Hidden GridDemo UI");
                }

                // Show CraftingModeSelector
                GameObject craftingModeSelector = GameObject.Find("CraftingModeSelector");
                if (craftingModeSelector != null)
                {
                    var modeSelector = craftingModeSelector.GetComponent<FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.CraftingModeSelector>();
                    if (modeSelector != null)
                    {
                        modeSelector.ShowModeSelector();
                        if (debugMode)
                            Debug.Log("✅ Returned to CraftingModeSelector");
                    }
                    else
                    {
                        craftingModeSelector.SetActive(true);
                        if (debugMode)
                            Debug.Log("✅ Activated CraftingModeSelector");
                    }
                }
                else
                {
                    if (debugMode)
                        Debug.LogWarning("⚠️ CraftingModeSelector not found");
                }
            });

            if (debugMode)
                Debug.Log("✅ Back button created successfully!");
        }

        // DISABLED: Helper method not needed since cleanup is disabled
        /*
        private GameObject[] FindAllGameObjectsWithName(string name)
        {
            // Find all GameObjects in the scene and filter by name
            var allTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            return allTransforms.Where(t => t.name == name).Select(t => t.gameObject).ToArray();
        }
        */

        private Transform FindChildRecursive(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                    return child;

                Transform found = FindChildRecursive(child, childName);
                if (found != null)
                    return found;
            }
            return null;
        }

        private void Update()
        {
            // Manual cleanup hotkey
            if (Input.GetKeyDown(KeyCode.F8))
            {
                Debug.Log("🧹 F8 pressed - Cleaning up duplicate sidebars...");
                CleanupDuplicateSidebars();
            }
        }
    }
}