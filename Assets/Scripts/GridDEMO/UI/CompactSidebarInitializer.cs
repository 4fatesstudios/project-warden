using UnityEngine;
using FourFatesStudios.ProjectWarden.GridDemo.UI;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Quick initializer to add the missing CompactSidebarFixer component to fix the back button
    /// </summary>
    public class CompactSidebarInitializer : MonoBehaviour
    {
        [Header("Auto-Setup")]
        [SerializeField] private bool fixOnStart = true;
        [SerializeField] private bool debugMode = true;
        
        private void Start()
        {
            if (fixOnStart)
            {
                FixMissingSidebarComponents();
            }
        }
        
        [ContextMenu("Fix Missing Sidebar Components")]
        public void FixMissingSidebarComponents()
        {
            if (debugMode)
                Debug.Log("🔧 CompactSidebarInitializer: Checking for missing components...");
                
            // Check if CompactSidebarFixer exists
            CompactSidebarFixer sidebarFixer = FindFirstObjectByType<CompactSidebarFixer>();
            if (sidebarFixer == null)
            {
                // Add CompactSidebarFixer to GridGameManager
                GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
                if (gridManager != null)
                {
                    sidebarFixer = gridManager.gameObject.AddComponent<CompactSidebarFixer>();
                    Debug.Log("✅ Added CompactSidebarFixer to GridGameManager");
                    
                    // Trigger the fix
                    sidebarFixer.FixCompactSidebar();
                }
                else
                {
                    Debug.LogError("❌ GridGameManager not found! Cannot add CompactSidebarFixer");
                }
            }
            else
            {
                if (debugMode)
                    Debug.Log("✅ CompactSidebarFixer already exists. Triggering fix...");
                    
                // Trigger the fix anyway to ensure back button exists
                sidebarFixer.FixCompactSidebar();
            }
            
            // Check if CompactUIDesigner exists
            CompactUIDesigner compactUI = FindFirstObjectByType<CompactUIDesigner>();
            if (compactUI == null)
            {
                if (debugMode)
                    Debug.Log("⚠️ CompactUIDesigner missing - CompactSidebarFixer will create it");
            }
            else
            {
                if (debugMode)
                    Debug.Log("✅ CompactUIDesigner exists");
            }
        }
        
        private void Update()
        {
            // Quick fix hotkey for testing
            if (Input.GetKeyDown(KeyCode.F6))
            {
                Debug.Log("🔧 F6 pressed - Fixing missing sidebar components...");
                FixMissingSidebarComponents();
            }
        }
    }
}