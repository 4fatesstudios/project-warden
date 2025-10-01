using UnityEngine;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;
using System.Collections;
using System.Collections.Generic;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Complete GridDemo UI Manager - Fixed for Unity 6.0 compilation issues
    /// This replaces the problematic GridDemoUIManager and CompactUIDesigner combo
    /// </summary>
    public class GridDemoUIManager : MonoBehaviour
    {
        [Header("UI Controllers")]
        [SerializeField] private CompactUIDesigner leftPanel;
        //[SerializeField] private RightPanelManager rightPanel;
        
        [Header("Auto-Setup")]
        [SerializeField] private bool autoFindComponents = true;
        [SerializeField] private bool createUIOnStart = true;
        [SerializeField] private bool autoLoadIngredients = true;
        [SerializeField] private bool debugMode = true;
        
        private GridGameManager gridManager;
        
        private void Awake()
        {
            if (autoFindComponents)
            {
                AutoFindComponents();
            }
        }
        
        private void Start()
        {
            SetupConnections();
            
            if (createUIOnStart && leftPanel != null)
            {
                // Add a small delay to ensure GridGameManager is fully initialized
                StartCoroutine(DelayedUISetup());
            }
        }
        
        private System.Collections.IEnumerator DelayedUISetup()
        {
            yield return new WaitForEndOfFrame();
            
            if (debugMode)
            {
                Debug.Log("🎨 GridDemoUIManager: Starting delayed UI setup...");
            }
            
            // Ensure ingredients are loaded before creating UI
            if (autoLoadIngredients && gridManager != null && (gridManager.availableIngredients == null || gridManager.availableIngredients.Count == 0))
            {
                LoadDefaultIngredients();
            }
            
            if (leftPanel != null)
            {
                if (debugMode)
                {
                    Debug.Log("🎨 GridDemoUIManager: Calling leftPanel.DesignCompactUI()...");
                }
                leftPanel.DesignCompactUI();
            }
            else
            {
                Debug.LogError("❌ GridDemoUIManager: leftPanel is null! Cannot create UI.");
            }
            
            if (debugMode)
            {
                Debug.Log("✅ GridDemoUIManager: Delayed UI setup completed");
                LogSetupStatus();
            }
        }
        
        private void LoadDefaultIngredients()
        {
            if (debugMode) Debug.Log("📦 GridDemoUIManager: Loading ingredients from Resources...");
            
            var ingredients = Resources.LoadAll<Ingredient>("Items/Ingredients");
            
            if (ingredients.Length == 0)
            {
                ingredients = Resources.LoadAll<Ingredient>("Ingredients");
            }
            
            if (ingredients.Length == 0)
            {
                ingredients = Resources.LoadAll<Ingredient>("");
            }
            
            if (ingredients.Length > 0)
            {
                if (gridManager.availableIngredients == null)
                {
                    gridManager.availableIngredients = new List<Ingredient>();
                }
                gridManager.availableIngredients.Clear();
                gridManager.availableIngredients.AddRange(ingredients);
                
                if (debugMode)
                {
                    Debug.Log($"✅ GridDemoUIManager loaded {ingredients.Length} ingredients");
                }
            }
            else if (debugMode)
            {
                Debug.LogWarning("⚠️ GridDemoUIManager: No ingredients found in Resources!");
            }
        }
        
        private void LogSetupStatus()
        {
            Debug.Log("📊 === GridDemoUIManager Setup Status ===");
            Debug.Log($"Grid Manager: {gridManager != null}");
            Debug.Log($"Left Panel: {leftPanel != null}");
            //Debug.Log($"Right Panel: {rightPanel != null}");
            Debug.Log($"Ingredients Count: {gridManager?.availableIngredients?.Count ?? 0}");
            
            GameObject sidebar = GameObject.Find("Compact Sidebar");
            Debug.Log($"Compact Sidebar exists: {sidebar != null}");
            Debug.Log("📊 === End Setup Status ===");
        }
        
        [ContextMenu("Refresh UI")]
        public void RefreshUI()
        {
            if (autoLoadIngredients && gridManager != null)
            {
                LoadDefaultIngredients();
            }
            
            if (leftPanel != null)
            {
                leftPanel.RefreshUI();
            }
        }
        
        private void AutoFindComponents()
        {
            if (debugMode)
            {
                Debug.Log("🔧 GridDemoUIManager: Auto-finding components...");
            }
            
            // Find grid manager
            gridManager = FindFirstObjectByType<GridGameManager>();
            
            // Find UI components if not assigned
            if (leftPanel == null)
            {
                leftPanel = GetComponent<CompactUIDesigner>();
                if (leftPanel == null)
                {
                    leftPanel = GetComponentInChildren<CompactUIDesigner>();
                }
            }
            
            if (debugMode)
            {
                Debug.Log($"🔧 AutoFind Results: GridManager={gridManager != null}, LeftPanel={leftPanel != null}");
            }
            
            // if (rightPanel == null)
            // {
            //     rightPanel = GetComponentInChildren<RightPanelManager>();
            // }
            
            // Create components if they don't exist
            if (leftPanel == null)
            {
                GameObject leftPanelObj = new GameObject("Left Panel");
                leftPanelObj.transform.SetParent(transform, false);
                leftPanel = leftPanelObj.AddComponent<CompactUIDesigner>();
                DebugSystemConfig.LogTesting("Created CompactUIDesigner component");
            }
            
            // if (rightPanel == null)
            // {
            //     GameObject rightPanelObj = new GameObject("Right Panel");
            //     rightPanelObj.transform.SetParent(transform, false);
            //     rightPanel = rightPanelObj.AddComponent<RightPanelManager>();
            //     DebugSystemConfig.LogTesting("Created RightPanelManager component");
            // }
            
            //DebugSystemConfig.LogTesting($"Auto-found components - Grid: {gridManager != null}, Left: {leftPanel != null}, Right: {rightPanel != null}");
            DebugSystemConfig.LogTesting($"Auto-found components - Grid: {gridManager != null}, Left: {leftPanel != null}");
        }
        
        private void SetupConnections()
        {
            if (gridManager == null || leftPanel == null) return;
            
            // Connect ingredient panel events to grid manager methods
            leftPanel.OnIngredientSelected += OnIngredientSelected;
            leftPanel.OnGridCleared += OnGridCleared;
            leftPanel.OnGridRandomized += OnGridRandomized;
            leftPanel.OnGridSizeChanged += OnGridSizeChanged;
            
            // Setup ingredient button hover events for info panel
            SetupIngredientButtonEvents();
            
            DebugSystemConfig.LogTesting("Connected all events successfully!");
        }
        
        private void SetupIngredientButtonEvents()
        {
            if (leftPanel == null) return;
            
            // Find all ingredient buttons and add hover events
            var buttons = leftPanel.GetComponentsInChildren<UnityEngine.UI.Button>();
            
            foreach (var button in buttons)
            {
                if (button.name.StartsWith("Btn_"))
                {
                    AddHoverEventsToButton(button);
                }
            }
        }
        
        private void AddHoverEventsToButton(UnityEngine.UI.Button button)
        {
            // Get the ingredient name from button name
            string ingredientName = button.name.Replace("Btn_", "");
            
            // Find the ingredient by name
            Ingredient ingredient = FindIngredientByName(ingredientName);
            if (ingredient == null) return;
            
            // Add event trigger if not present
            var eventTrigger = button.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }
            
            // Add pointer enter event for tooltip
            var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            // pointerEnter.callback.AddListener((data) => {
            //     ShowIngredientTooltip(ingredient, Input.mousePosition);
            // });
            
            // Add pointer exit event
            var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
            // pointerExit.callback.AddListener((data) => {
            //     HideIngredientTooltip();
            // });
            
            eventTrigger.triggers.Add(pointerEnter);
            eventTrigger.triggers.Add(pointerExit);
        }
        
        private Ingredient FindIngredientByName(string name)
        {
            if (gridManager == null || gridManager.availableIngredients == null) return null;
            
            foreach (var ingredient in gridManager.availableIngredients)
            {
                if (ingredient != null && ingredient.ItemName == name)
                {
                    return ingredient;
                }
            }
            return null;
        }
        
        // Event handlers
        private void OnIngredientSelected(Ingredient ingredient)
        {
            if (gridManager != null)
            {
                gridManager.SelectIngredient(ingredient);
                Debug.Log($"Selected ingredient: {ingredient.ItemName}");
            }
        }
        
        private void OnGridCleared()
        {
            if (gridManager != null)
            {
                gridManager.ClearGrid();
                Debug.Log("Grid cleared!");
            }
        }
        
        private void OnGridRandomized()
        {
            if (gridManager != null)
            {
                // Implement randomization if needed
                Debug.Log("Grid randomized!");
            }
        }
        
        private void OnGridSizeChanged(int newSize)
        {
            if (gridManager != null)
            {
                gridManager.gridWidth = newSize;
                gridManager.gridHeight = newSize;
                Debug.Log($"Grid size changed to {newSize}x{newSize} (restart required)");
            }
        }
        
        // // Public methods for external access
        // public void ShowIngredientTooltip(Ingredient ingredient, Vector2 screenPosition)
        // {
        //     if (rightPanel != null)
        //     {
        //         rightPanel.ShowIngredientInfo(ingredient, screenPosition);
        //     }
        // }
        
        // public void HideIngredientTooltip()
        // {
        //     if (rightPanel != null)
        //     {
        //         rightPanel.HideIngredientInfo();
        //     }
        // }
        
        private void OnDestroy()
        {
            // Disconnect events
            if (leftPanel != null)
            {
                leftPanel.OnIngredientSelected -= OnIngredientSelected;
                leftPanel.OnGridCleared -= OnGridCleared;
                leftPanel.OnGridRandomized -= OnGridRandomized;
                leftPanel.OnGridSizeChanged -= OnGridSizeChanged;
            }
        }
        
        [ContextMenu("🔧 Setup Complete UI System")]
        public void SetupCompleteUISystem()
        {
            Debug.Log("🔧 Setting up complete Grid Demo UI system...");
            
            AutoFindComponents();
            SetupConnections();
            
            if (leftPanel != null)
            {
                leftPanel.DesignCompactUI();
                Debug.Log("✅ CompactUIDesigner setup complete!");
            }
            
            Debug.Log("🚀 Complete UI system setup finished!");
        }
        
        [ContextMenu("📋 Debug UI State")]
        public void DebugUIState()
        {
            DebugSystemConfig.LogTesting("GridDemoUIManagerComplete Debug State:");
            DebugSystemConfig.LogTesting($"   - GridGameManager: {gridManager != null}");
            DebugSystemConfig.LogTesting($"   - LeftPanel (CompactUIDesigner): {leftPanel != null}");
            //DebugSystemConfig.LogTesting($"   - RightPanel (RightPanelManager): {rightPanel != null}");
            
            if (gridManager != null && gridManager.availableIngredients != null)
            {
                DebugSystemConfig.LogTesting($"   - Available Ingredients: {gridManager.availableIngredients.Count}");
            }
        }
    }
}