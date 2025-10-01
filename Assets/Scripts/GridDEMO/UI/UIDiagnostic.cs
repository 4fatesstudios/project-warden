using UnityEngine;
using UnityEngine.UI;
using FourFatesStudios.ProjectWarden.GridDemo.UI;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Diagnostic script to debug UI creation issues
    /// </summary>
    public class UIDiagnostic : MonoBehaviour
    {
        [Header("Diagnostic Controls")]
        [SerializeField] private bool autoRunDiagnostic = true;
        
        private void Start()
        {
            if (autoRunDiagnostic)
            {
                Invoke(nameof(RunFullDiagnostic), 1f); // Delay to ensure all components are initialized
            }
        }
        
        [ContextMenu("Run Full Diagnostic")]
        public void RunFullDiagnostic()
        {
            Debug.Log("🔍 === UI DIAGNOSTIC START ===");
            
            CheckGridGameManager();
            CheckUIComponents();
            CheckHierarchy();
            CheckIngredients();
            CheckUICreation();
            
            Debug.Log("🔍 === UI DIAGNOSTIC END ===");
        }
        
        private void CheckGridGameManager()
        {
            Debug.Log("📋 Checking GridGameManager...");
            
            GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager == null)
            {
                Debug.LogError("❌ GridGameManager not found!");
                return;
            }
            
            Debug.Log("✅ GridGameManager found");
            Debug.Log($"📊 Available ingredients count: {gridManager.availableIngredients?.Count ?? 0}");
            
            if (gridManager.availableIngredients != null && gridManager.availableIngredients.Count > 0)
            {
                Debug.Log("🧩 Available ingredients:");
                for (int i = 0; i < gridManager.availableIngredients.Count; i++)
                {
                    var ingredient = gridManager.availableIngredients[i];
                    Debug.Log($"  [{i}] {ingredient?.ItemName ?? "NULL"}");
                }
            }
        }
        
        private void CheckUIComponents()
        {
            Debug.Log("🎨 Checking UI Components...");
            
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            if (gridDemoUI == null)
            {
                Debug.LogError("❌ GridDemo UI not found!");
                return;
            }
            
            Debug.Log("✅ GridDemo UI found");
            
            GridDemoUIManager uiManager = gridDemoUI.GetComponent<GridDemoUIManager>();
            CompactUIDesigner compactDesigner = gridDemoUI.GetComponent<CompactUIDesigner>();
            
            Debug.Log($"GridDemoUIManager attached: {uiManager != null}");
            Debug.Log($"CompactUIDesigner attached: {compactDesigner != null}");
            
            if (compactDesigner != null)
            {
                // Check if DesignCompactUI has been called
                Debug.Log("🔧 Manually calling DesignCompactUI...");
                compactDesigner.DesignCompactUI();
            }
        }
        
        private void CheckHierarchy()
        {
            Debug.Log("🌳 Checking UI Hierarchy...");
            
            GameObject sidebar = GameObject.Find("Compact Sidebar");
            Debug.Log($"Compact Sidebar exists: {sidebar != null}");
            
            GameObject buttonContainer = GameObject.Find("Button Container");
            Debug.Log($"Button Container exists: {buttonContainer != null}");
            
            if (buttonContainer != null)
            {
                int buttonCount = buttonContainer.transform.childCount;
                Debug.Log($"🔘 Button count in container: {buttonCount}");
                
                for (int i = 0; i < buttonCount; i++)
                {
                    Transform child = buttonContainer.transform.GetChild(i);
                    Debug.Log($"  Button [{i}]: {child.name} (active: {child.gameObject.activeSelf})");
                    
                    Button btn = child.GetComponent<Button>();
                    if (btn != null)
                    {
                        Debug.Log($"    Button component: {btn.enabled} | Interactable: {btn.interactable}");
                    }
                }
            }
            
            // Check entire hierarchy under GridDemo UI
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            if (gridDemoUI != null)
            {
                Debug.Log("🌲 GridDemo UI hierarchy:");
                LogHierarchy(gridDemoUI.transform, 0);
            }
        }
        
        private void LogHierarchy(Transform parent, int depth)
        {
            string indent = new string(' ', depth * 2);
            Debug.Log($"{indent}├─ {parent.name} (active: {parent.gameObject.activeSelf})");
            
            for (int i = 0; i < parent.childCount; i++)
            {
                LogHierarchy(parent.GetChild(i), depth + 1);
            }
        }
        
        private void CheckIngredients()
        {
            Debug.Log("🧪 Checking Ingredients Resources...");
            
            var ingredients = Resources.LoadAll<Ingredient>("Items/Ingredients");
            Debug.Log($"Found {ingredients.Length} ingredients in Items/Ingredients");
            
            if (ingredients.Length == 0)
            {
                ingredients = Resources.LoadAll<Ingredient>("Ingredients");
                Debug.Log($"Found {ingredients.Length} ingredients in Ingredients");
            }
            
            if (ingredients.Length == 0)
            {
                ingredients = Resources.LoadAll<Ingredient>("");
                Debug.Log($"Found {ingredients.Length} ingredients in root Resources");
            }
            
            foreach (var ingredient in ingredients)
            {
                Debug.Log($"  🧩 {ingredient.ItemName}");
            }
        }
        
        private void CheckUICreation()
        {
            Debug.Log("🏗️ Testing Manual UI Creation...");
            
            GridGameManager gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager == null || gridManager.availableIngredients == null || gridManager.availableIngredients.Count == 0)
            {
                Debug.LogWarning("⚠️ Cannot test UI creation - no ingredients available");
                return;
            }
            
            // Try to create a simple test button
            GameObject testParent = new GameObject("TEST_ButtonContainer");
            testParent.transform.SetParent(transform, false);
            
            RectTransform testRect = testParent.AddComponent<RectTransform>();
            testRect.sizeDelta = new Vector2(200, 300);
            testRect.anchoredPosition = Vector2.zero;
            
            var ingredient = gridManager.availableIngredients[0];
            
            GameObject testButton = new GameObject($"TEST_{ingredient.ItemName}");
            testButton.transform.SetParent(testParent.transform, false);
            
            RectTransform buttonRect = testButton.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(180, 40);
            
            Image buttonImage = testButton.AddComponent<Image>();
            buttonImage.color = Color.yellow;
            
            Button button = testButton.AddComponent<Button>();
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(testButton.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = ingredient.ItemName;
            buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            buttonText.fontSize = 16;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.black;
            
            Debug.Log($"✅ Created test button for {ingredient.ItemName}");
            
            // Destroy test after 5 seconds
            Destroy(testParent, 5f);
        }
        
        [ContextMenu("Force UI Setup")]
        public void ForceUISetup()
        {
            GameObject gridDemoUI = GameObject.Find("GridDemo UI");
            if (gridDemoUI != null)
            {
                CompactUIDesigner designer = gridDemoUI.GetComponent<CompactUIDesigner>();
                if (designer != null)
                {
                    Debug.Log("🔧 Force calling DesignCompactUI...");
                    designer.DesignCompactUI();
                }
                else
                {
                    Debug.LogError("❌ CompactUIDesigner not found on GridDemo UI");
                }
            }
        }
    }
}