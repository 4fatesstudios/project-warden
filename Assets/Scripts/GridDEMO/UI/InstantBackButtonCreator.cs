using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Instant back button creator - attach to any GameObject and use context menu to instantly create the back button
    /// </summary>
    public class InstantBackButtonCreator : MonoBehaviour
    {
        [ContextMenu("INSTANT: Create Back Button NOW")]
        public void CreateBackButtonNow()
        {
            Debug.Log("🚀 INSTANT: Creating back button right now...");

            // Find Controls Container
            GameObject controlsContainer = null;
            
            // Method 1: Direct find
            controlsContainer = GameObject.Find("Controls Container");
            
            // Method 2: Find via Compact Sidebar
            if (controlsContainer == null)
            {
                GameObject compactSidebar = GameObject.Find("Compact Sidebar");
                if (compactSidebar != null)
                {
                    foreach (Transform child in compactSidebar.GetComponentsInChildren<Transform>())
                    {
                        if (child.name == "Controls Container")
                        {
                            controlsContainer = child.gameObject;
                            break;
                        }
                    }
                }
            }

            if (controlsContainer == null)
            {
                Debug.LogError("❌ INSTANT: Controls Container not found!");
                return;
            }

            // Check if back button already exists
            Transform existingBackButton = controlsContainer.transform.Find("Back Button");
            if (existingBackButton != null)
            {
                Debug.Log("✅ INSTANT: Back button already exists!");
                return;
            }

            // Create the back button immediately
            CreateBackButton(controlsContainer);
        }

        private void CreateBackButton(GameObject parent)
        {
            // Create back button GameObject
            GameObject backButtonObj = new GameObject("Back Button");
            backButtonObj.transform.SetParent(parent.transform, false);

            // Set up RectTransform to match other buttons
            RectTransform buttonRect = backButtonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(0, 45f);
            
            // Add Image for background
            Image buttonImage = backButtonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Slightly lighter gray

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
                Debug.Log("⬅️ INSTANT: Back button clicked!");
                
                // Hide GridDemo UI
                GameObject gridDemoUI = GameObject.Find("GridDemo UI");
                if (gridDemoUI != null)
                {
                    gridDemoUI.SetActive(false);
                    Debug.Log("✅ INSTANT: Hidden GridDemo UI");
                }

                // Show CraftingModeSelector
                GameObject craftingModeSelector = GameObject.Find("CraftingModeSelector");
                if (craftingModeSelector != null)
                {
                    var modeSelector = craftingModeSelector.GetComponent<FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu.CraftingModeSelector>();
                    if (modeSelector != null)
                    {
                        modeSelector.ShowModeSelector();
                        Debug.Log("✅ INSTANT: Showed CraftingModeSelector");
                    }
                    else
                    {
                        craftingModeSelector.SetActive(true);
                        Debug.Log("✅ INSTANT: Activated CraftingModeSelector");
                    }
                }
            });

            Debug.Log("🎉 INSTANT: Back button created successfully!");
        }
    }
}