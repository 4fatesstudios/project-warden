using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Emergency fix to create back button on demand
    /// </summary>
    public class EmergencyBackButtonFix : MonoBehaviour
    {
        private void Update()
        {
            // Press F12 to instantly fix the back button issue
            if (Input.GetKeyDown(KeyCode.F12))
            {
                Debug.Log("🚨 F12 Emergency Fix: Creating sidebar and back button...");
                FixBackButtonNow();
            }
        }

        [ContextMenu("🚨 Emergency Fix: Create Sidebar and Back Button NOW")]
        public void FixBackButtonNow()
        {
            // Step 1: Ensure CompactUIDesigner exists
            CompactUIDesigner designer = FindFirstObjectByType<CompactUIDesigner>();
            if (designer == null)
            {
                GameObject gridDemoUI = GameObject.Find("GridDemo UI");
                if (gridDemoUI != null)
                {
                    designer = gridDemoUI.AddComponent<CompactUIDesigner>();
                    Debug.Log("✅ Added CompactUIDesigner to GridDemo UI");
                }
                else
                {
                    Debug.LogError("❌ GridDemo UI not found!");
                    return;
                }
            }

            // Step 2: Create the sidebar
            designer.DesignCompactUI();
            Debug.Log("✅ Compact sidebar created");

            // Step 3: Add back button after a short delay
            Invoke(nameof(CreateBackButtonNow), 0.5f);
        }

        private void CreateBackButtonNow()
        {
            GameObject compactSidebar = GameObject.Find("Compact Sidebar");
            if (compactSidebar == null)
            {
                Debug.LogError("❌ Compact Sidebar still not found!");
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
                Debug.LogError("❌ Controls Container not found!");
                return;
            }

            // Check if back button already exists
            Transform existingBackButton = controlsContainer.Find("Back Button");
            if (existingBackButton != null)
            {
                Debug.Log("✅ Back button already exists!");
                return;
            }

            // Create back button
            CreateBackButton(controlsContainer);
            Debug.Log("✅ Emergency back button created successfully!");
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
            buttonImage.color = new Color(0.6f, 0.3f, 0.3f, 1f); // Red-ish color to distinguish emergency fix

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
                Debug.Log("⬅️ Emergency back button clicked! Returning to CraftingModeSelector...");

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
    }
}