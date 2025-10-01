using UnityEngine;
using FourFatesStudios.ProjectWarden.GridDemo.UI;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Simple script to trigger UI recreation after the fix
    /// </summary>
    public class UIRecreationTrigger : MonoBehaviour
    {
        [Header("Auto-trigger on Start")]
        [SerializeField] private bool triggerOnStart = true;
        [SerializeField] private float delayAfterStart = 1f;
        
        private void Start()
        {
            if (triggerOnStart)
            {
                Invoke(nameof(TriggerUIRecreation), delayAfterStart);
            }
        }
        
        [ContextMenu("Trigger UI Recreation")]
        public void TriggerUIRecreation()
        {
            Debug.Log("🔄 UIRecreationTrigger: Starting UI recreation...");
            
            // Find GridDemo UI
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            if (gridDemoUI == null)
            {
                Debug.LogError("❌ GridDemo UI not found!");
                return;
            }
            
            // Get or add CompactUIDesigner
            CompactUIDesigner designer = gridDemoUI.GetComponent<CompactUIDesigner>();
            if (designer == null)
            {
                Debug.Log("🔧 Adding CompactUIDesigner component...");
                designer = gridDemoUI.AddComponent<CompactUIDesigner>();
            }
            
            // Force recreate the UI
            Debug.Log("🎨 Forcing UI recreation...");
            designer.ForceRecreateUI();
            
            // Verify creation
            StartCoroutine(VerifyUICreation());
            
            Debug.Log("✅ UIRecreationTrigger: Recreation triggered!");
        }
        
        private System.Collections.IEnumerator VerifyUICreation()
        {
            yield return new WaitForSeconds(0.5f);
            
            GameObject compactSidebar = GameObject.Find("Compact Sidebar");
            if (compactSidebar != null)
            {
                Debug.Log("✅ Compact Sidebar successfully created!");
                
                // Check for button container
                Transform buttonContainer = compactSidebar.transform.Find("Ingredient Scroll/Viewport/Content/Button Container");
                if (buttonContainer != null)
                {
                    int buttonCount = buttonContainer.childCount;
                    Debug.Log($"✅ Found {buttonCount} ingredient buttons in the sidebar!");
                    
                    if (buttonCount == 0)
                    {
                        Debug.LogWarning("⚠️ Sidebar created but no buttons found. Triggering button creation...");
                        CompactUIDesigner designer = FindFirstObjectByType<CompactUIDesigner>();
                        if (designer != null)
                        {
                            designer.RefreshUI();
                        }
                    }
                }
                else
                {
                    Debug.LogError("❌ Button container not found in sidebar structure!");
                }
            }
            else
            {
                Debug.LogError("❌ Compact Sidebar creation failed!");
            }
        }
        
        [ContextMenu("Check UI State")]
        public void CheckUIState()
        {
            Debug.Log("📊 === UI STATE CHECK ===");
            
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            GameObject compactSidebar = GameObject.Find("Compact Sidebar");
            
            Debug.Log($"GridDemo UI exists: {gridDemoUI != null}");
            Debug.Log($"Compact Sidebar exists: {compactSidebar != null}");
            
            if (gridDemoUI != null)
            {
                CompactUIDesigner designer = gridDemoUI.GetComponent<CompactUIDesigner>();
                Debug.Log($"CompactUIDesigner attached: {designer != null}");
            }
            
            if (compactSidebar != null)
            {
                Debug.Log($"Compact Sidebar active: {compactSidebar.activeSelf}");
                Debug.Log($"Compact Sidebar children: {compactSidebar.transform.childCount}");
            }
            
            Debug.Log("📊 === END UI STATE CHECK ===");
        }
    }
}