using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Simple script to add back button to existing compact sidebar without recreating the entire UI
    /// </summary>
    public class SimpleBackButtonAdder : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool addOnStart = true;
        [SerializeField] private float delaySeconds = 1.5f;
        [SerializeField] private bool debugMode = true;

        private void Start()
        {
            if (addOnStart)
            {
                Invoke(nameof(AddBackButtonToExistingSidebar), delaySeconds);
            }
        }

        [ContextMenu("Add Back Button to Existing Sidebar")]
        public void AddBackButtonToExistingSidebar()
        {
            if (debugMode)
                Debug.Log("🔧 SimpleBackButtonAdder: Looking for existing sidebar...");

            // Find the existing compact sidebar
            GameObject compactSidebar = GameObject.Find("Compact Sidebar");
            if (compactSidebar == null)
            {
                if (debugMode)
                    Debug.LogWarning("⚠️ No Compact Sidebar found! The sidebar may not be created yet.");
                return;
            }

            // Find Controls Container
            Transform controlsContainer = null;
            foreach (Transform child in compactSidebar.GetComponentsInChildren<Transform>())
            {
                if (child.name == "Controls Container")
                {
                    controlsContainer = child;
                    break;
                }
            }

            if (controlsContainer == null)
            {
                if (debugMode)
                    Debug.LogWarning("⚠️ Controls Container not found in Compact Sidebar!");
                return;
            }

            // Check if back button already exists
            Transform existingBackButton = controlsContainer.Find("Back Button");
            if (existingBackButton != null)
            {
                if (debugMode)
                    Debug.Log("✅ Back button already exists!");
                return;
            }

            // Create the back button
            CreateBackButton(controlsContainer);
            
            if (debugMode)
                Debug.Log("✅ Back button added to existing sidebar!");
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

        private void Update()
        {
            // Manual trigger with F9
            if (Input.GetKeyDown(KeyCode.F9))
            {
                Debug.Log("🔧 F9 pressed - Adding back button to existing sidebar...");
                AddBackButtonToExistingSidebar();
            }
        }
    }
}