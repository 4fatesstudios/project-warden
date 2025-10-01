using UnityEngine;
using UnityEditor;
using FourFatesStudios.ProjectWarden.GridDemo;
using FourFatesStudios.ProjectWarden.ScriptableObjects.Items;

/// <summary>
/// Editor window to test grid placement and debug occupancy issues
/// </summary>
public class PlacementTestEditor : EditorWindow
{
    [MenuItem("Tools/Placement Test Debug")]
    public static void ShowWindow()
    {
        GetWindow<PlacementTestEditor>("Placement Test");
    }

    private void OnGUI()
    {
        GUILayout.Label("Grid Placement Debug Tests", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("🧪 Test Basic Placement (Position 0,0)"))
        {
            TestBasicPlacement();
        }
        
        if (GUILayout.Button("🔍 Check Current Grid State"))
        {
            CheckGridState();
        }
        
        if (GUILayout.Button("🎯 Force Test Occupancy Manually"))
        {
            ForceTestOccupancy();
        }
        
        if (GUILayout.Button("🧹 Clear Grid and Test Again"))
        {
            ClearAndTest();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("These tests will help identify if the occupancy issue is in:\n" +
                                "1. UI ingredient selection\n" +
                                "2. Grid placement logic\n" +
                                "3. Cell marking logic\n" +
                                "4. Visual feedback", MessageType.Info);
    }

    private void TestBasicPlacement()
    {
        var gridManager = FindFirstObjectByType<GridGameManager>();
        if (gridManager == null)
        {
            Debug.LogError("❌ No GridGameManager found in scene!");
            return;
        }

        if (gridManager.availableIngredients.Count == 0)
        {
            Debug.LogError("❌ No ingredients available in GridGameManager!");
            return;
        }

        var testIngredient = gridManager.availableIngredients[0];
        var testPosition = new Vector2Int(0, 0);
        
        Debug.Log($"🧪 === BASIC PLACEMENT TEST ===");
        Debug.Log($"🧪 Testing ingredient: {testIngredient.ItemName}");
        Debug.Log($"🧪 Testing position: {testPosition}");
        
        // Check if position is clear BEFORE placement
        var cellBefore = gridManager.GetCell(testPosition.x, testPosition.y);
        Debug.Log($"🧪 BEFORE placement - Cell ({testPosition.x},{testPosition.y}): IsOccupied = {cellBefore?.IsOccupied}, OccupiedBy = {cellBefore?.OccupiedByIngredient?.ItemName ?? "None"}");
        
        // Try the placement
        bool success = gridManager.TryPlaceIngredient(testIngredient, testPosition);
        Debug.Log($"🧪 Placement result: {(success ? "✅ SUCCESS" : "❌ FAILED")}");
        
        // Check if position is occupied AFTER placement
        var cellAfter = gridManager.GetCell(testPosition.x, testPosition.y);
        Debug.Log($"🧪 AFTER placement - Cell ({testPosition.x},{testPosition.y}): IsOccupied = {cellAfter?.IsOccupied}, OccupiedBy = {cellAfter?.OccupiedByIngredient?.ItemName ?? "None"}");
        
        if (success && !cellAfter.IsOccupied)
        {
            Debug.LogError("🚨 BUG CONFIRMED: Placement succeeded but cell is NOT marked as occupied!");
            Debug.LogError("🚨 This indicates the MarkCellsAsOccupied method is not working properly!");
        }
        else if (success && cellAfter.IsOccupied)
        {
            Debug.Log("✅ Placement and occupancy marking working correctly!");
        }
    }

    private void CheckGridState()
    {
        var gridManager = FindFirstObjectByType<GridGameManager>();
        if (gridManager == null)
        {
            Debug.LogError("❌ No GridGameManager found!");
            return;
        }

        Debug.Log($"🔍 === CURRENT GRID STATE ===");
        
        int occupiedCount = 0;
        for (int x = 0; x < gridManager.gridWidth; x++)
        {
            for (int y = 0; y < gridManager.gridHeight; y++)
            {
                var cell = gridManager.GetCell(x, y);
                if (cell != null && cell.IsOccupied)
                {
                    occupiedCount++;
                    Debug.Log($"🔍 Occupied cell ({x},{y}): {cell.OccupiedByIngredient?.ItemName ?? "NULL"}");
                }
            }
        }
        
        Debug.Log($"🔍 Total occupied cells: {occupiedCount} out of {gridManager.gridWidth * gridManager.gridHeight}");
        
        if (occupiedCount == 0)
        {
            Debug.LogWarning("⚠️ No cells are marked as occupied! This might indicate the bug.");
        }
    }

    private void ForceTestOccupancy()
    {
        var gridManager = FindFirstObjectByType<GridGameManager>();
        if (gridManager == null || gridManager.availableIngredients.Count == 0)
        {
            Debug.LogError("❌ No GridGameManager or ingredients found!");
            return;
        }

        var testIngredient = gridManager.availableIngredients[0];
        var testPosition = new Vector2Int(1, 1);
        
        Debug.Log($"🎯 === FORCE OCCUPANCY TEST ===");
        Debug.Log($"🎯 Manually calling SetOccupied on cell ({testPosition.x},{testPosition.y})");
        
        var cell = gridManager.GetCell(testPosition.x, testPosition.y);
        if (cell != null)
        {
            Debug.Log($"🎯 BEFORE: Cell IsOccupied = {cell.IsOccupied}");
            
            // Directly call SetOccupied to test if the cell marking works
            cell.SetOccupied(testIngredient);
            
            Debug.Log($"🎯 AFTER: Cell IsOccupied = {cell.IsOccupied}");
            Debug.Log($"🎯 AFTER: Cell OccupiedBy = {cell.OccupiedByIngredient?.ItemName ?? "NULL"}");
            
            if (cell.IsOccupied)
            {
                Debug.Log("✅ SetOccupied method works correctly!");
                Debug.Log("🔍 The issue might be that MarkCellsAsOccupied is not calling SetOccupied properly");
            }
            else
            {
                Debug.LogError("🚨 SetOccupied method itself is broken!");
            }
        }
    }

    private void ClearAndTest()
    {
        var gridManager = FindFirstObjectByType<GridGameManager>();
        if (gridManager == null)
        {
            Debug.LogError("❌ No GridGameManager found!");
            return;
        }

        Debug.Log($"🧹 === CLEAR AND TEST ===");
        
        // Clear the grid
        gridManager.ClearGrid();
        
        // Wait a frame then test placement
        EditorApplication.delayCall += () =>
        {
            TestBasicPlacement();
        };
    }
}