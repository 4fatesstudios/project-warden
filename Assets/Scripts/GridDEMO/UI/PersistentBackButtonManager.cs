using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Ensures the back button is always present whenever GridDemo UI becomes active
    /// </summary>
    public class PersistentBackButtonManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool checkOnUpdate = true;
        [SerializeField] private float checkInterval = 2.0f;
        [SerializeField] private bool debugMode = true;

        private float lastCheckTime;
        private bool wasGridDemoUIActive = false;

        private void Update()
        {
            if (!checkOnUpdate)
                return;

            // Check every few seconds and whenever GridDemo UI state changes
            bool shouldCheck = Time.time - lastCheckTime > checkInterval;
            
            // Also check when GridDemo UI becomes active
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            bool isGridDemoUIActive = gridDemoUI != null && gridDemoUI.activeInHierarchy;
            
            if (isGridDemoUIActive && !wasGridDemoUIActive)
            {
                // GridDemo UI just became active
                if (debugMode)
                    Debug.Log("🔍 PersistentBackButtonManager: GridDemo UI became active, checking for back button...");
                shouldCheck = true;
            }
            
            wasGridDemoUIActive = isGridDemoUIActive;

            if (shouldCheck)
            {
                lastCheckTime = Time.time;
                EnsureBackButtonExists();
            }

            // Manual trigger hotkey
            if (Input.GetKeyDown(KeyCode.F11))
            {
                Debug.Log("🔧 F11 pressed - Ensuring back button exists...");
                EnsureBackButtonExists();
            }
        }

        [ContextMenu("Ensure Back Button Exists")]
        public void EnsureBackButtonExists()
        {
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            if (gridDemoUI == null || !gridDemoUI.activeInHierarchy)
            {
                return; // GridDemo UI is not active, no need for back button
            }

            GameObject compactSidebar = GameObject.Find("Compact Sidebar");
            if (compactSidebar == null)
            {
                if (debugMode)
                    Debug.Log("🔍 PersistentBackButtonManager: No Compact Sidebar found, checking if we need to create it...");
                
                // Try to ensure the sidebar exists first
                EnsureSidebarExists();
                return;
            }

            // Find the Controls Container
            Transform controlsContainer = FindChildRecursive(compactSidebar.transform, "Controls Container");
            if (controlsContainer == null)
            {
                if (debugMode)
                    Debug.LogWarning("⚠️ Controls Container not found in Compact Sidebar");
                return;
            }

            // Check if back button already exists
            Transform existingBackButton = controlsContainer.Find("Back Button");
            if (existingBackButton != null)
            {
                return; // Back button already exists
            }

            // Create the back button
            CreateBackButton(controlsContainer);
            
            if (debugMode)
                Debug.Log("✅ PersistentBackButtonManager: Back button created!");
        }

        private void EnsureSidebarExists()
        {
            // Check if there's a CompactUIDesigner that can create the sidebar
            CompactUIDesigner designer = FindFirstObjectByType<CompactUIDesigner>();
            if (designer == null)
            {
                // Try to find GridDemo UI to attach designer to
                GameObject gridDemoUI = GameObject.Find("GridDemo UI");
                if (gridDemoUI != null)
                {
                    designer = gridDemoUI.GetComponent<CompactUIDesigner>();
                    if (designer == null)
                    {
                        designer = gridDemoUI.AddComponent<CompactUIDesigner>();
                        if (debugMode)
                            Debug.Log("🔧 Added CompactUIDesigner to GridDemo UI");
                    }
                }
            }

            if (designer != null)
            {
                // Create the sidebar
                designer.DesignCompactUI();
                
                if (debugMode)
                    Debug.Log("✅ Compact sidebar created, will check for back button on next update");
            }
        }

        private void CreateBackButton(Transform parent)
        {
            // Create back button GameObject
            GameObject backButtonObj = new GameObject("Back Button");
            backButtonObj.transform.SetParent(parent, false);

            // Set up RectTransform
            RectTransform buttonRect = backButtonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(0, 45f);

            // Add Image for background
            Image buttonImage = backButtonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);

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

            // Set up button click functionality
            button.onClick.AddListener(() => {
                if (debugMode)
                    Debug.Log("⬅️ Back button clicked! Returning to CraftingModeSelector...");

                // Hide GridDemo UI
                GameObject gridDemoUI = GameObject.Find("GridDemo UI");
                if (gridDemoUI != null)
                {
                    gridDemoUI.SetActive(false);
                }

                // Show CraftingModeSelector
                GameObject craftingModeSelector = GameObject.Find("CraftingModeSelector");
                if (craftingModeSelector != null)
                {
                    var modeSelector = craftingModeSelector.GetComponent<FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.CraftingModeSelector>();
                    if (modeSelector != null)
                    {
                        modeSelector.ShowModeSelector();
                    }
                    else
                    {
                        craftingModeSelector.SetActive(true);
                    }
                }
            });
        }

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
    }
}