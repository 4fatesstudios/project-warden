using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FourFatesStudios.ProjectWarden.GridDemo.UI;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Manual fix to directly add the missing back button to the Controls Container
    /// </summary>
    public class ManualBackButtonFixer : MonoBehaviour
    {
        [Header("Manual Back Button Fix")]
        [SerializeField] private bool addBackButtonOnStart = true;
        [SerializeField] private bool debugMode = true;

        private void Start()
        {
            if (addBackButtonOnStart)
            {
                // Add a delay to ensure UI is built
                Invoke(nameof(CreateBackButtonDirectly), 1.0f);
            }
        }

        [ContextMenu("Create Back Button Directly")]
        public void CreateBackButtonDirectly()
        {
            if (debugMode)
                Debug.Log("🔧 ManualBackButtonFixer: Creating back button directly...");

            // Find the Controls Container in the hierarchy
            GameObject controlsContainer = GameObject.Find("Controls Container");
            if (controlsContainer == null)
            {
                // Try alternative paths
                GameObject compactSidebar = GameObject.Find("Compact Sidebar");
                if (compactSidebar != null)
                {
                    controlsContainer = FindChildRecursive(compactSidebar.transform, "Controls Container")?.gameObject;
                }
            }

            if (controlsContainer == null)
            {
                Debug.LogError("❌ Controls Container not found! Cannot add back button.");
                return;
            }

            // Check if back button already exists
            if (controlsContainer.transform.Find("Back Button") != null)
            {
                if (debugMode)
                    Debug.Log("✅ Back button already exists!");
                return;
            }

            // Create back button
            CreateBackButton(controlsContainer);
        }

        private void CreateBackButton(GameObject parent)
        {
            // Create back button GameObject
            GameObject backButtonObj = new GameObject("Back Button");
            backButtonObj.transform.SetParent(parent.transform, false);

            // Set up RectTransform
            RectTransform buttonRect = backButtonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(0, 45f); // Same height as other buttons
            
            // Add Image component for background
            Image buttonImage = backButtonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.4f, 0.4f, 0.4f, 0.9f); // Gray color

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

            // Add TextMeshProUGUI component
            TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "← BACK TO MENU";
            buttonText.fontSize = 12f;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            buttonText.fontStyle = FontStyles.Bold;

            // Set up button click event
            button.onClick.AddListener(() => {
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
                    var modeSelectorComponent = craftingModeSelector.GetComponent<FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.CraftingModeSelector>();
                    if (modeSelectorComponent != null)
                    {
                        modeSelectorComponent.ShowModeSelector();
                        Debug.Log("✅ Returned to CraftingModeSelector");
                    }
                    else
                    {
                        craftingModeSelector.SetActive(true);
                        Debug.Log("✅ Enabled CraftingModeSelector");
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ CraftingModeSelector not found");
                }
            });

            if (debugMode)
                Debug.Log("✅ Back button created successfully in Controls Container!");
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

        private void Update()
        {
            // Quick fix hotkey
            if (Input.GetKeyDown(KeyCode.F7))
            {
                Debug.Log("🔧 F7 pressed - Creating back button directly...");
                CreateBackButtonDirectly();
            }
        }
    }
}