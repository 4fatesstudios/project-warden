using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Debug script to inspect the actual UI structure and find missing elements
    /// </summary>
    public class UIStructureDebugger : MonoBehaviour
    {
        [ContextMenu("Debug UI Structure")]
        public void DebugUIStructure()
        {
            Debug.Log("🔍 === UI STRUCTURE DEBUG START ===");
            
            // Find the GridDemo UI
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            if (gridDemoUI == null)
            {
                Debug.LogError("❌ GridDemo UI not found!");
                return;
            }
            
            Debug.Log($"✅ GridDemo UI found: {gridDemoUI.name}");
            Debug.Log($"   Active: {gridDemoUI.activeSelf}");
            Debug.Log($"   Children count: {gridDemoUI.transform.childCount}");
            
            // Check for Compact Sidebar
            Transform compactSidebar = gridDemoUI.transform.Find("Compact Sidebar");
            if (compactSidebar == null)
            {
                Debug.LogError("❌ Compact Sidebar not found!");
                LogAllChildren(gridDemoUI.transform, 0);
                return;
            }
            
            Debug.Log($"✅ Compact Sidebar found: {compactSidebar.name}");
            Debug.Log($"   Active: {compactSidebar.gameObject.activeSelf}");
            Debug.Log($"   Children count: {compactSidebar.childCount}");
            
            // Check for Ingredient Scroll
            Transform ingredientScroll = compactSidebar.Find("Ingredient Scroll");
            if (ingredientScroll == null)
            {
                Debug.LogError("❌ Ingredient Scroll not found in Compact Sidebar!");
                LogAllChildren(compactSidebar, 1);
                return;
            }
            
            Debug.Log($"✅ Ingredient Scroll found: {ingredientScroll.name}");
            Debug.Log($"   Active: {ingredientScroll.gameObject.activeSelf}");
            
            // Check ScrollRect component
            ScrollRect scrollRect = ingredientScroll.GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                Debug.LogError("❌ ScrollRect component missing from Ingredient Scroll!");
            }
            else
            {
                Debug.Log($"✅ ScrollRect found");
                Debug.Log($"   Enabled: {scrollRect.enabled}");
                Debug.Log($"   Viewport: {(scrollRect.viewport != null ? scrollRect.viewport.name : "NULL")}");
                Debug.Log($"   Content: {(scrollRect.content != null ? scrollRect.content.name : "NULL")}");
            }
            
            // Check for Viewport
            Transform viewport = ingredientScroll.Find("Viewport");
            if (viewport == null)
            {
                Debug.LogError("❌ Viewport not found in Ingredient Scroll!");
                LogAllChildren(ingredientScroll, 2);
                return;
            }
            
            Debug.Log($"✅ Viewport found: {viewport.name}");
            Debug.Log($"   Active: {viewport.gameObject.activeSelf}");
            
            // Check for Content
            Transform content = viewport.Find("Content");
            if (content == null)
            {
                Debug.LogError("❌ Content not found in Viewport!");
                LogAllChildren(viewport, 3);
                return;
            }
            
            Debug.Log($"✅ Content found: {content.name}");
            Debug.Log($"   Active: {content.gameObject.activeSelf}");
            
            // Check for Button Container
            Transform buttonContainer = content.Find("Button Container");
            if (buttonContainer == null)
            {
                Debug.LogError("❌ Button Container not found in Content!");
                LogAllChildren(content, 4);
                return;
            }
            
            Debug.Log($"✅ Button Container found: {buttonContainer.name}");
            Debug.Log($"   Active: {buttonContainer.gameObject.activeSelf}");
            Debug.Log($"   Children count (ingredient buttons): {buttonContainer.childCount}");
            
            // Check GridLayoutGroup
            GridLayoutGroup layoutGroup = buttonContainer.GetComponent<GridLayoutGroup>();
            if (layoutGroup == null)
            {
                Debug.LogError("❌ GridLayoutGroup missing from Button Container!");
            }
            else
            {
                Debug.Log($"✅ GridLayoutGroup found");
                Debug.Log($"   Enabled: {layoutGroup.enabled}");
                Debug.Log($"   Cell Size: {layoutGroup.cellSize}");
                Debug.Log($"   Constraint: {layoutGroup.constraint} (Count: {layoutGroup.constraintCount})");
            }
            
            // Check ContentSizeFitter
            ContentSizeFitter sizeFitter = buttonContainer.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                Debug.LogError("❌ ContentSizeFitter missing from Button Container!");
            }
            else
            {
                Debug.Log($"✅ ContentSizeFitter found");
                Debug.Log($"   Enabled: {sizeFitter.enabled}");
                Debug.Log($"   Vertical Fit: {sizeFitter.verticalFit}");
            }
            
            // Log all ingredient buttons
            Debug.Log($"🧩 === INGREDIENT BUTTONS ({buttonContainer.childCount}) ===");
            for (int i = 0; i < buttonContainer.childCount; i++)
            {
                Transform child = buttonContainer.GetChild(i);
                Button button = child.GetComponent<Button>();
                Image image = child.GetComponent<Image>();
                Text text = child.GetComponentInChildren<Text>();
                
                Debug.Log($"   Button {i}: {child.name}");
                Debug.Log($"     Active: {child.gameObject.activeSelf}");
                Debug.Log($"     Button component: {(button != null ? "✅" : "❌")}");
                Debug.Log($"     Image component: {(image != null ? "✅" : "❌")}");
                Debug.Log($"     Text: {(text != null ? text.text : "No text found")}");
                
                RectTransform rect = child.GetComponent<RectTransform>();
                if (rect != null)
                {
                    Debug.Log($"     Position: {rect.localPosition}");
                    Debug.Log($"     Size: {rect.sizeDelta}");
                    Debug.Log($"     Anchors: {rect.anchorMin} to {rect.anchorMax}");
                }
            }
            
            // Check RectTransform sizes
            Debug.Log($"📐 === RECT TRANSFORM SIZES ===");
            RectTransform containerRect = buttonContainer.GetComponent<RectTransform>();
            RectTransform contentRect = content.GetComponent<RectTransform>();
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            
            if (containerRect != null)
            {
                Debug.Log($"Button Container: Position={containerRect.localPosition}, Size={containerRect.sizeDelta}");
            }
            if (contentRect != null)
            {
                Debug.Log($"Content: Position={contentRect.localPosition}, Size={contentRect.sizeDelta}");
            }
            if (viewportRect != null)
            {
                Debug.Log($"Viewport: Position={viewportRect.localPosition}, Size={viewportRect.sizeDelta}");
            }
            
            Debug.Log("🔍 === UI STRUCTURE DEBUG END ===");
        }
        
        private void LogAllChildren(Transform parent, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 2);
            Debug.Log($"{indent}Children of {parent.name}:");
            
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Debug.Log($"{indent}  {i}: {child.name} (Active: {child.gameObject.activeSelf})");
            }
        }
        
        [ContextMenu("Force Create UI")]
        public void ForceCreateUI()
        {
            CompactUIDesigner designer = GetComponent<CompactUIDesigner>();
            if (designer != null)
            {
                Debug.Log("🔧 Force creating UI...");
                designer.DesignCompactUI();
            }
            else
            {
                Debug.LogError("❌ CompactUIDesigner not found!");
            }
        }
        
        [ContextMenu("Check Current Scene")]
        public void CheckCurrentScene()
        {
            Debug.Log("🔍 === SCENE OVERVIEW ===");
            
            // Find all Canvas objects
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            Debug.Log($"Canvases in scene: {canvases.Length}");
            
            foreach (Canvas canvas in canvases)
            {
                Debug.Log($"  Canvas: {canvas.name} (Active: {canvas.gameObject.activeSelf})");
                Debug.Log($"    Children: {canvas.transform.childCount}");
            }
            
            // Find all objects with "Sidebar" in the name
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Sidebar") || obj.name.Contains("Compact"))
                {
                    Debug.Log($"Found object with 'Sidebar/Compact': {obj.name} (Path: {GetFullPath(obj.transform)})");
                }
            }
        }
        
        private string GetFullPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }
    }
}