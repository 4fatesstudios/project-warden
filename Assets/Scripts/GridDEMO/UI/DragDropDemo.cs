using UnityEngine;

namespace FourFatesStudios.ProjectWarden.GridDemo.UI
{
    /// <summary>
    /// Demo script that showcases the drag and drop system functionality
    /// Shows how to integrate drag and drop with existing UI systems
    /// </summary>
    public class DragDropDemo : MonoBehaviour
    {
        [Header("Demo Settings")]
        [SerializeField] private bool enableDemoOnStart = true;
        [SerializeField] private bool showInstructions = true;
        
        private IngredientDragDropManager dragDropManager;
        private GridGameManager gridManager;
        private bool demoStarted = false;
        
        private void Start()
        {
            if (enableDemoOnStart)
            {
                StartDemo();
            }
        }
        
        [ContextMenu("Start Drag Drop Demo")]
        public void StartDemo()
        {
            if (demoStarted) return;
            
            Debug.Log("=== DRAG & DROP DEMO STARTING ===");
            
            // Find required components
            gridManager = FindFirstObjectByType<GridGameManager>();
            dragDropManager = FindFirstObjectByType<IngredientDragDropManager>();
            
            if (gridManager == null)
            {
                Debug.LogError("DragDropDemo: GridGameManager not found!");
                return;
            }
            
            // Setup everything automatically
            SetupDragDropSystem();
            
            if (showInstructions)
            {
                ShowInstructions();
            }
            
            demoStarted = true;
            Debug.Log("=== DRAG & DROP DEMO READY ===");
        }
        
        private void SetupDragDropSystem()
        {
            // 1. Create compact UI if not exists
            CompactUIDesigner compactDesigner = FindFirstObjectByType<CompactUIDesigner>();
            if (compactDesigner != null)
            {
                compactDesigner.DesignCompactUI();
                Debug.Log("DragDropDemo: Compact UI created");
            }
            else
            {
                Debug.LogWarning("DragDropDemo: CompactUIDesigner not found - adding one");
                GameObject.FindFirstObjectByType<GridDemoUI>()?.gameObject.AddComponent<CompactUIDesigner>();
            }
            
            Debug.Log("DragDropDemo: All systems setup complete!");
        }
        
        private void ShowInstructions()
        {
            Debug.Log(@"
╔═══════════════════════════════════════════════════════════════╗
║                    DRAG & DROP INSTRUCTIONS                   ║
╠═══════════════════════════════════════════════════════════════╣
║                                                               ║
║ 🖱️  BASIC USAGE:                                              ║
║   • Click ingredients in left panel to select them           ║
║   • Drag ingredients from left panel to grid                 ║
║   • Drop on valid green highlighted cells                    ║
║   • Red highlights show invalid placement                    ║
║                                                               ║
║ 🎯 FEATURES:                                                  ║
║   • Visual feedback during dragging                          ║
║   • Grid cell highlighting (green=valid, red=invalid)        ║
║   • Automatic placement on successful drop                   ║
║   • Click on placed ingredients for right panel details      ║
║                                                               ║
║ ⚡ FALLBACK:                                                  ║
║   • If dragging doesn't work, use click-to-select +          ║
║     click-on-grid for placement                               ║
║                                                               ║
╚═══════════════════════════════════════════════════════════════╝
            ");
        }
        
        private void Update()
        {
            if (!demoStarted) return;
            
            // Monitor drag and drop status
            if (dragDropManager != null && dragDropManager.IsDragging())
            {
                var draggedItem = dragDropManager.GetCurrentDraggedItem();
                if (draggedItem?.AssignedIngredient != null)
                {
                    // Could show UI hints here
                }
            }
        }
        
        [ContextMenu("Show Demo Stats")]
        public void ShowDemoStats()
        {
            Debug.Log("=== DRAG & DROP DEMO STATS ===");
            
            // Count draggable ingredients
            DraggableIngredient[] draggables = FindObjectsByType<DraggableIngredient>(FindObjectsSortMode.None);
            Debug.Log($"Draggable Ingredients: {draggables.Length}");
            
            // Count total ingredients in grid
            if (gridManager != null)
            {
                int placedCount = 0;
                for (int x = 0; x < gridManager.gridWidth; x++)
                {
                    for (int y = 0; y < gridManager.gridHeight; y++)
                    {
                        var cell = gridManager.GetCell(x, y);
                        if (cell != null && cell.IsOccupied)
                        {
                            placedCount++;
                        }
                    }
                }
                Debug.Log($"Placed Ingredients: {placedCount}");
                Debug.Log($"Available Ingredients: {gridManager.availableIngredients?.Count ?? 0}");
            }
            
            // Check systems
            bool hasRightPanelManager = FindFirstObjectByType<RightPanelManager>() != null;
            bool hasDragDropManager = FindFirstObjectByType<IngredientDragDropManager>() != null;
            
            Debug.Log($"RightPanelManager: {(hasRightPanelManager ? "✓" : "✗")}");
            Debug.Log($"DragDropManager: {(hasDragDropManager ? "✓" : "✗")}");
            
            Debug.Log("=== END STATS ===");
        }
        
        [ContextMenu("Reset Demo")]
        public void ResetDemo()
        {
            Debug.Log("DragDropDemo: Resetting demo...");
            
            // Clear grid
            if (gridManager != null)
            {
                gridManager.ClearGrid();
            }
            Debug.Log("DragDropDemo: Demo reset complete!");
        }
        
        
        private bool GetDragEnabledState()
        {
            // Simple check by looking for draggable components
            DraggableIngredient[] draggables = FindObjectsByType<DraggableIngredient>(FindObjectsSortMode.None);
            return draggables.Length > 0;
        }
    }
}