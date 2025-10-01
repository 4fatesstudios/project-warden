using UnityEngine;
using System.Collections.Generic;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Detects and reports all compact sidebars in the scene
    /// </summary>
    public class CompactSidebarDetector : MonoBehaviour
    {
        private void Update()
        {
            // Press F1 to scan for compact sidebars
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ScanForCompactSidebars();
            }
        }

        [ContextMenu("🔍 Scan for Compact Sidebars")]
        public void ScanForCompactSidebars()
        {
            Debug.Log("🔍 === COMPACT SIDEBAR SCAN ===");

            // Method 1: Search by name
            List<GameObject> foundByName = new List<GameObject>();
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Compact Sidebar"))
                {
                    foundByName.Add(obj);
                }
            }

            Debug.Log($"📊 Found {foundByName.Count} objects with 'Compact Sidebar' in name:");
            for (int i = 0; i < foundByName.Count; i++)
            {
                GameObject obj = foundByName[i];
                Debug.Log($"   {i+1}. '{obj.name}' at path: {GetGameObjectPath(obj)} (Active: {obj.activeInHierarchy})");
            }

            // Method 2: Search for CompactUIDesigner components
            CompactUIDesigner[] designers = FindObjectsByType<CompactUIDesigner>(FindObjectsSortMode.None);
            Debug.Log($"🎨 Found {designers.Length} CompactUIDesigner components:");
            for (int i = 0; i < designers.Length; i++)
            {
                Debug.Log($"   {i+1}. On GameObject: '{designers[i].gameObject.name}' at path: {GetGameObjectPath(designers[i].gameObject)}");
            }

            // Method 3: Search in GridDemo UI specifically
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            if (gridDemoUI != null)
            {
                Debug.Log("🎮 Scanning GridDemo UI children:");
                int childCount = gridDemoUI.transform.childCount;
                Debug.Log($"   GridDemo UI has {childCount} direct children");
                
                for (int i = 0; i < childCount; i++)
                {
                    Transform child = gridDemoUI.transform.GetChild(i);
                    Debug.Log($"   Child {i+1}: '{child.name}' (Active: {child.gameObject.activeInHierarchy})");
                    
                    if (child.name.Contains("Compact") || child.name.Contains("Sidebar"))
                    {
                        Debug.Log($"      ⚠️ This might be a sidebar!");
                    }
                }
            }
            else
            {
                Debug.Log("❌ GridDemo UI not found");
            }

            Debug.Log("🔍 === SCAN COMPLETE ===");
        }

        private string GetGameObjectPath(GameObject obj)
        {
            if (obj == null) return "null";
            
            string path = obj.name;
            Transform parent = obj.transform.parent;
            
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            
            return "/" + path;
        }
    }
}