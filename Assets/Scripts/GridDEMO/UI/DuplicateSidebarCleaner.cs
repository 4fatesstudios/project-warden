using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Cleans up duplicate compact sidebars in the scene
    /// </summary>
    public class DuplicateSidebarCleaner : MonoBehaviour
    {
        private void Update()
        {
            // Press F2 to clean up duplicate sidebars
            if (Input.GetKeyDown(KeyCode.F2))
            {
                CleanupDuplicateSidebars();
            }
        }

        [ContextMenu("🧹 Clean Up Duplicate Sidebars")]
        public void CleanupDuplicateSidebars()
        {
            Debug.Log("🧹 === CLEANING UP DUPLICATE SIDEBARS ===");

            // Step 1: Find all objects with "Compact Sidebar" in name
            List<GameObject> compactSidebars = new List<GameObject>();
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Compact Sidebar") && obj.activeInHierarchy)
                {
                    compactSidebars.Add(obj);
                }
            }

            Debug.Log($"🔍 Found {compactSidebars.Count} active Compact Sidebar objects");

            if (compactSidebars.Count <= 1)
            {
                Debug.Log("✅ No duplicate sidebars found - only 0 or 1 sidebar exists");
                return;
            }

            // Step 2: Determine which sidebar to keep (prefer one with ingredients)
            GameObject sidebarToKeep = null;
            List<GameObject> sidebaresToRemove = new List<GameObject>();

            foreach (GameObject sidebar in compactSidebars)
            {
                bool hasIngredients = CheckSidebarHasIngredients(sidebar);
                Debug.Log($"   Sidebar '{sidebar.name}': Has ingredients = {hasIngredients}");

                if (sidebarToKeep == null || (hasIngredients && !CheckSidebarHasIngredients(sidebarToKeep)))
                {
                    if (sidebarToKeep != null)
                    {
                        sidebaresToRemove.Add(sidebarToKeep);
                    }
                    sidebarToKeep = sidebar;
                }
                else
                {
                    sidebaresToRemove.Add(sidebar);
                }
            }

            // Step 3: Remove duplicate sidebars
            Debug.Log($"📝 Keeping sidebar: '{sidebarToKeep?.name}' (Has ingredients: {CheckSidebarHasIngredients(sidebarToKeep)})");
            Debug.Log($"🗑️ Removing {sidebaresToRemove.Count} duplicate sidebars:");

            foreach (GameObject sidebar in sidebaresToRemove)
            {
                Debug.Log($"   Destroying: '{sidebar.name}'");
                DestroyImmediate(sidebar);
            }

            // Step 4: Clean up orphaned CompactUIDesigner components
            CompactUIDesigner[] designers = FindObjectsByType<CompactUIDesigner>(FindObjectsSortMode.None);
            Debug.Log($"🎨 Found {designers.Length} CompactUIDesigner components");

            List<CompactUIDesigner> designersToRemove = new List<CompactUIDesigner>();
            foreach (CompactUIDesigner designer in designers)
            {
                // Check if this designer's GameObject has the sidebar we're keeping
                if (sidebarToKeep != null && designer.gameObject != sidebarToKeep.transform.parent?.gameObject)
                {
                    // Check if this designer created a sidebar that was removed
                    bool hasOwnedSidebar = false;
                    foreach (Transform child in designer.transform)
                    {
                        if (child.name.Contains("Compact Sidebar"))
                        {
                            hasOwnedSidebar = true;
                            break;
                        }
                    }

                    if (!hasOwnedSidebar)
                    {
                        designersToRemove.Add(designer);
                    }
                }
            }

            if (designersToRemove.Count > 0)
            {
                Debug.Log($"🗑️ Removing {designersToRemove.Count} orphaned CompactUIDesigner components");
                foreach (CompactUIDesigner designer in designersToRemove)
                {
                    Debug.Log($"   Removing CompactUIDesigner from: '{designer.gameObject.name}'");
                    DestroyImmediate(designer);
                }
            }

            Debug.Log("✅ Cleanup complete!");
            Debug.Log("🧹 === CLEANUP FINISHED ===");
        }

        private bool CheckSidebarHasIngredients(GameObject sidebar)
        {
            if (sidebar == null) return false;

            // Look for Ingredient Scroll container
            Transform ingredientScroll = FindChildRecursive(sidebar.transform, "Ingredient Scroll");
            if (ingredientScroll == null) return false;

            // Check if it has content with children
            Transform viewport = ingredientScroll.Find("Viewport");
            Transform content = viewport?.Find("Content");
            
            return content != null && content.childCount > 0;
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