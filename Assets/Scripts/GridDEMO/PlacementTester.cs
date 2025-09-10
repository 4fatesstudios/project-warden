using UnityEngine;
using FourFatesStudios.ProjectWarden.GridDemo;

namespace FourFatesStudios.ProjectWarden.GridDemo
{
    /// <summary>
    /// Runtime testing component to help debug placement issues
    /// </summary>
    public class PlacementTester : MonoBehaviour
    {
        [Header("Test Settings")]
        public KeyCode testKey = KeyCode.T;
        public KeyCode clearKey = KeyCode.C;
        public KeyCode verifyKey = KeyCode.V;
        public bool enableDetailedLogging = true;

        private GridGameManager gridManager;

        void Start()
        {
            gridManager = FindFirstObjectByType<GridGameManager>();
            if (gridManager == null)
            {
                Debug.LogError("PlacementTester: No GridGameManager found!");
                enabled = false;
                return;
            }

            Debug.Log($"PlacementTester: Ready! Press {testKey} to test placement, {clearKey} to clear grid, {verifyKey} to verify occupancy");
        }

        void Update()
        {
            if (Input.GetKeyDown(testKey))
            {
                TestPlacement();
            }

            if (Input.GetKeyDown(clearKey))
            {
                TestClearGrid();
            }

            if (Input.GetKeyDown(verifyKey))
            {
                TestVerifyOccupancy();
            }
        }

        [ContextMenu("Test Placement")]
        public void TestPlacement()
        {
            if (gridManager == null || gridManager.availableIngredients.Count == 0)
            {
                Debug.LogError("❌ No GridGameManager or ingredients available!");
                return;
            }

            var testIngredient = gridManager.availableIngredients[0];
            var testPosition = new Vector2Int(Random.Range(0, gridManager.gridWidth - 1), 
                                            Random.Range(0, gridManager.gridHeight - 1));

            Debug.Log($"🧪 === RUNTIME PLACEMENT TEST ===");
            Debug.Log($"🧪 Testing: {testIngredient.ItemName} at {testPosition}");

            // Check cell state BEFORE placement
            var cellBefore = gridManager.GetCell(testPosition.x, testPosition.y);
            bool wasOccupiedBefore = cellBefore?.IsOccupied ?? false;
            string occupantBefore = cellBefore?.OccupiedByIngredient?.ItemName ?? "None";

            if (enableDetailedLogging)
            {
                Debug.Log($"🧪 BEFORE: Cell ({testPosition.x},{testPosition.y}) - Occupied: {wasOccupiedBefore}, By: {occupantBefore}");
            }

            // Attempt placement
            bool placementSuccess = gridManager.TryPlaceIngredient(testIngredient, testPosition);
            
            // Check cell state AFTER placement
            var cellAfter = gridManager.GetCell(testPosition.x, testPosition.y);
            bool isOccupiedAfter = cellAfter?.IsOccupied ?? false;
            string occupantAfter = cellAfter?.OccupiedByIngredient?.ItemName ?? "None";

            if (enableDetailedLogging)
            {
                Debug.Log($"🧪 AFTER: Cell ({testPosition.x},{testPosition.y}) - Occupied: {isOccupiedAfter}, By: {occupantAfter}");
            }

            // Analyze results
            Debug.Log($"🧪 Placement Result: {(placementSuccess ? "✅ SUCCESS" : "❌ FAILED")}");

            if (placementSuccess && !isOccupiedAfter)
            {
                Debug.LogError("🚨 BUG DETECTED: Placement succeeded but cell not marked as occupied!");
                
                // Additional debugging
                Debug.LogError("🔍 Investigating further...");
                
                // Try to manually call the marking function to see what happens
                var method = typeof(GridGameManager).GetMethod("MarkCellsAsOccupied", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    Debug.Log("🔧 Attempting to manually call MarkCellsAsOccupied...");
                    method.Invoke(gridManager, new object[] { testIngredient, testPosition });
                    
                    // Check again
                    var cellAfterManual = gridManager.GetCell(testPosition.x, testPosition.y);
                    bool isOccupiedAfterManual = cellAfterManual?.IsOccupied ?? false;
                    Debug.Log($"🔧 After manual marking: Cell occupied = {isOccupiedAfterManual}");
                }
            }
            else if (placementSuccess && isOccupiedAfter)
            {
                Debug.Log("✅ Everything working correctly!");
            }
            else if (!placementSuccess && wasOccupiedBefore)
            {
                Debug.Log("✅ Collision detection working correctly - blocked by existing ingredient");
            }
            else if (!placementSuccess && !wasOccupiedBefore)
            {
                Debug.LogWarning("⚠️ Placement failed on empty cell - check bounds or other restrictions");
            }

            // Show final grid state summary
            ShowGridSummary();
        }

        [ContextMenu("Test Clear Grid")]
        public void TestClearGrid()
        {
            if (gridManager == null) return;

            Debug.Log($"🧹 === CLEAR GRID TEST ===");
            ShowGridSummary();
            
            gridManager.ClearGrid();
            
            Debug.Log($"🧹 Grid cleared. New state:");
            ShowGridSummary();
        }

        [ContextMenu("Show Grid Summary")]
        public void ShowGridSummary()
        {
            if (gridManager == null) return;

            int occupiedCount = 0;
            int highlightedCount = 0;

            for (int x = 0; x < gridManager.gridWidth; x++)
            {
                for (int y = 0; y < gridManager.gridHeight; y++)
                {
                    var cell = gridManager.GetCell(x, y);
                    if (cell != null)
                    {
                        if (cell.IsOccupied) occupiedCount++;
                        if (cell.IsHighlighted) highlightedCount++;
                    }
                }
            }

            int total = gridManager.gridWidth * gridManager.gridHeight;
            float occupancyPercent = (occupiedCount * 100f) / total;

            Debug.Log($"📊 Grid Summary: {occupiedCount}/{total} cells occupied ({occupancyPercent:F1}%), {highlightedCount} highlighted");
        }

        [ContextMenu("Test Specific Position")]
        public void TestSpecificPosition()
        {
            // Test a specific position that's easy to verify
            if (gridManager == null || gridManager.availableIngredients.Count == 0) return;

            var testIngredient = gridManager.availableIngredients[0];
            var testPosition = new Vector2Int(2, 2); // Center position

            Debug.Log($"🎯 === SPECIFIC POSITION TEST: (2,2) ===");

            // Clear the grid first to ensure clean test
            gridManager.ClearGrid();

            // Test the placement
            bool success = gridManager.TryPlaceIngredient(testIngredient, testPosition);
            
            // Check result
            var cell = gridManager.GetCell(testPosition.x, testPosition.y);
            bool isOccupied = cell?.IsOccupied ?? false;

            Debug.Log($"🎯 Result: Placement={success}, CellOccupied={isOccupied}");

            if (success != isOccupied)
            {
                Debug.LogError("🚨 MISMATCH: Placement success doesn't match cell occupancy!");
            }
        }

        [ContextMenu("Debug Cell Access")]
        public void DebugCellAccess()
        {
            if (gridManager == null) return;

            Debug.Log($"🔧 === CELL ACCESS DEBUG ===");

            // Test if we can get cells properly
            for (int x = 0; x < gridManager.gridWidth && x < 3; x++)
            {
                for (int y = 0; y < gridManager.gridHeight && y < 3; y++)
                {
                    var cell = gridManager.GetCell(x, y);
                    if (cell != null)
                    {
                        Debug.Log($"🔧 Cell ({x},{y}): Position={cell.Position}, Occupied={cell.IsOccupied}");
                    }
                    else
                    {
                        Debug.LogError($"🔧 Cell ({x},{y}): NULL!");
                    }
                }
            }
        }

        [ContextMenu("Test Verify Occupancy")]
        public void TestVerifyOccupancy()
        {
            if (gridManager == null) return;

            Debug.Log($"🔍 === OCCUPANCY VERIFICATION TEST ===");
            gridManager.VerifyAllPlacedIngredients();
            gridManager.FullOccupancyAudit();
        }
    }
}