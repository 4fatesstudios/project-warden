using UnityEngine;
using FourFatesStudios.ProjectWarden.GridDemo.UI;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Simple runtime script to force UI setup - add this to GridDemo UI temporarily
    /// </summary>
    [System.Serializable]
    public class ForceUISetup : MonoBehaviour
    {
        private void Start()
        {
            // Force setup the UI system
            StartCoroutine(SetupUIAfterDelay());
        }
        
        private System.Collections.IEnumerator SetupUIAfterDelay()
        {
            yield return new WaitForSeconds(0.1f); // Small delay to ensure everything is initialized
            
            Debug.Log("🔧 ForceUISetup: Setting up missing UI components...");
            
            // Add GridDemoUIManager if missing
            GridDemoUIManager uiManager = GetComponent<GridDemoUIManager>();
            if (uiManager == null)
            {
                Debug.Log("🔧 Adding missing GridDemoUIManager...");
                uiManager = gameObject.AddComponent<GridDemoUIManager>();
            }
            
            // Add CompactUIDesigner if missing
            CompactUIDesigner compactDesigner = GetComponent<CompactUIDesigner>();
            if (compactDesigner == null)
            {
                Debug.Log("🔧 Adding missing CompactUIDesigner...");
                compactDesigner = gameObject.AddComponent<CompactUIDesigner>();
            }
            
            // Wait one more frame for components to initialize
            yield return new WaitForEndOfFrame();
            
            // Create the UI
            if (compactDesigner != null)
            {
                Debug.Log("🎨 Creating compact sidebar UI...");
                compactDesigner.DesignCompactUI();
            }
            
            Debug.Log("✅ ForceUISetup: UI setup completed!");
            
            // Remove this script after setup
            Destroy(this);
        }
    }
}